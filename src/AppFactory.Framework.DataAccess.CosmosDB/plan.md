# CosmosDB Data Access – Improvement Plan

Goal: support storing and restoring entities whose properties use abstract or base
classes and their derived types (polymorphism), configured only in `IModelConfig<TModel>`.
Along the way, fix the issues found while analysing the current mapping pipeline.

Steps are ordered by priority and are meant to be implemented and merged one at a time.
Each step must keep all existing tests green and adds its own regression tests to
`tests/AppFactory.Framework.DataAccess.CosmosDB.UnitTests`.

---

## Background: current pipeline

**Write:** `TModel` → JSON (`ModelMapper._serializerSettings`) → `CosmosDbDocument`
(`Dictionary<string, object>`, built via `JsonConvert` with default settings) →
`CosmosNewtonsoftJsonSerializer` → Cosmos.

**Read:** Cosmos → `Dictionary<string, object>` → JSON → `TModel` (`ModelMapper._serializerSettings`).

Findings that shaped this plan:

| # | Finding | Impact |
|---|---------|--------|
| F1 | No type information is written for abstract/base properties. | Reading fails with *"Could not create an instance of type X. Type is an interface or abstract class"*. |
| F2 | Newtonsoft drops `$type` (and other `$` metadata) when deserializing into `Dictionary<string, object>` with default `MetadataPropertyHandling`. | Even `TypeNameHandling.Auto` would not survive the round trip. Avoided by using a non-`$` discriminator name. |
| F3 | `IgnorePropertiesResolver` matches ignored properties by **name** across the whole object graph. | Any nested object property named like the `Id` or a partition key (e.g. `Department.Persons[i].Id`) is silently **not stored**. Verified against the real `ModelMapper`. |
| F4 | `RepositoryBase` is Scoped, so the constructor runs `Configure()`, recompiles expressions and creates a new `ModelMapper` on every request. | Wasted work per request. Contracts are only built once because of `CamelCasePropertyNamesContractResolver`'s hidden static cache. |
| F5 | `GetDocumentKey(object id)` ignores `UseResolver` partition key parts. | `GetById`/`Delete` cannot find documents written with a resolver partition key. **`UseResolver` is currently not used in the codebase** – latent only. |
| F6 | `UseResolver<T>() where T : new()` – resolvers cannot receive DI services. | A real tenant resolver can't inject a tenant accessor. |
| F7 | `MapBackId` writes the document `id` back into the model's `Id` property, not into the property configured with `Id(...)`. | With `Id(u => u.UserId)`, `UserId` is `null` after reading (unless it is also a partition key), and a separate `Id` property is overwritten with the `UserId` value. |

---

## Step 1 – Fix nested properties being ignored (F3)  `[x]`

**Why first:** it affects data being stored today.

**Implemented**
- Matching by `MemberInfo` in the contract resolver was rejected: contracts are resolved per **type**,
  not per position in the graph, so a nested object of the same type as the root (or sharing a base
  class that declares `Id`, e.g. `Order` and `OrderLine` both deriving from `EntityBase`) would still lose its `Id`.
- Instead, the model is serialized in full and `ModelMapper.RemoveIgnoredRootProperties` removes
  the ignored names from the **top level of the document only**, before `id` and partition keys are written.
- `IgnorePropertiesResolver` was removed. The serializer settings no longer depend on the model config.
- Root model behaviour is unchanged (same document output).

**Tests** – `NestedPropertyMappingTests` (4 of 5 fail on the old code, all pass now; existing 23 tests unchanged)
- Root `Id` and partition key source are still excluded from the document.
- Nested object and `List<T>` keep `Id` / partition-key-named properties.
- Round trip, including nested objects sharing a base class with the root and a nested object of the root's own type.

---

## Step 1b – The Cosmos `id` belongs to the property configured with `Id(...)` (F7)  `[x]`

**Rule:** the document `id` belongs to the property configured with `Id(...)`. Everything else follows from it.

**Problem (before)** – with `Id(e => e.EmployeeNumber)`:
- Read: the document `id` (prefix stripped) was deserialized into the model's `Id` property, never into
  `EmployeeNumber`, so `EmployeeNumber` came back `null` unless it was also a partition key.
- Write: if the model also had its own `Id` with a different value, it was silently overwritten and never stored.

**Implemented**
- `CosmosDbModelConfig.Id(...)` compiles a converting setter as well as the getter and keeps the CLR
  property name (`IdPropertyName`, `GetIdPropertyValue`, `SetIdPropertyValue`). Get-only or nested
  expressions get no setter (same behaviour as before for those).
- `ModelMapper.MapFromDocument` restores the prefix-stripped `id` into the configured property. The `id`
  also stays in the deserialized JSON, so a model `Id` sharing the field gets the same value.
- `ModelMapper.MapToDocument` throws `InvalidOperationException` when another model property serialized as
  `id` holds a **different, non-null** value than the configured Id (would be lost). Equal or null is allowed.
- `PropertyExpressionHelper.ConvertValue<T>` is shared by Id and partition key setters
  (direct, `Guid`, enum, `Nullable<T>`, `Convert.ChangeType` with invariant culture).
  Partition keys previously threw for `null` into a value type; they now get `default`.
- Stored documents are unchanged, so existing data reads correctly.

| Case | Before | After |
|---|---|---|
| `Id(e => e.Id)` | works | works (unchanged) |
| `Id(e => e.EmployeeNumber)`, no other `Id` | `EmployeeNumber` = null | restored |
| `Id(e => e.EmployeeNumber)`, `Id == EmployeeNumber` | `EmployeeNumber` = null | both restored |
| `Id(e => e.EmployeeNumber)`, `Id` null | `EmployeeNumber` = null | both restored with the Id value |
| `Id(e => e.EmployeeNumber)`, `Id != EmployeeNumber` | `Id` value silently lost | exception on write |

If storing a different `Id` value is ever needed, add an explicit rename via the Step 4 `Map(...)` API
(e.g. `Map(e => e.Id, "technicalId")`) rather than an implicit field name.

**Tests** – `IdPropertyMappingTests` (7 tests, all fail on the old code)
- Non-`Id` property round trip, both properties with equal value, model `Id` null, different values throw,
  `Guid` and `int` conversion, private setter.
- `ModelMapperTests`: 3 tests used `Id = "123"` with `Id(u => u.UserId)` (value was silently lost, not asserted).
  Test data changed to `Id = "userID1"`; `MapBack_IdWithCustomNames` now also asserts `Id` is restored.

---

## Step 2 – `ModelMapRegistry`: build config and mapper once per model (F4)  `[x]`

**Implemented**
- `Mapping/CosmosDbModelMap<TModel>` (public, sealed): holds the built `Config` and `Mapper` (internal members).
  `Create(IModelConfig<TModel>)` runs `Configure()` and creates the `ModelMapper`.
- `Mapping/IModelMapRegistry` (public) / `ModelMapRegistry` (internal, singleton):
  - `Get<TModel>()` builds lazily from `IModelConfig<TModel>` resolved from `IServiceProvider`.
  - `ConcurrentDictionary<Type, Lazy<object>>`, so `Configure()` runs exactly once even under concurrency.
  - Missing `IModelConfig<TModel>` registration → `InvalidOperationException` from DI on first use.
- `RegisterCosmosDbPersistence` registers it with `TryAddSingleton` (safe to call twice).
  Covers `RegisterCosmosDbDataAccess` and `AddCosmosRepository` too.
- `RepositoryBase<TModel>`:
  - New constructor `(ICosmosDbClientFactory, ILogger, IModelMapRegistry)`.
  - Old constructor `(…, IModelConfig<TModel>)` is `[Obsolete]`; it still works and builds the map per instance.
  - Both delegate to one private constructor taking `CosmosDbModelMap<TModel>`.
- README: repository examples use the new constructor; documents that the mapping is shared across
  requests/threads and plugged-in resolvers must be stateless. The `GetByEmail` example now uses a parameterized query.
- Not done: eager validation of all configs at startup (optional, can be added later).

**Tests** – `ModelMapRegistryTests` (7 tests)
- Same map instance on repeated and concurrent (64 parallel) calls; `Configure()` called once.
- Missing model config throws.
- Repository resolved in 3 scopes → `Configure()` called once; obsolete constructor → once per instance.
- New and obsolete constructors write identical documents.
- `RegisterCosmosDbPersistence` called twice → one singleton registration.

**Versioning:** non-breaking (old constructor still works, compiler warning CS0618 guides migration).

---

## Step 3 – Instance-cached contract resolver  `[x]`

**Implemented** (together with Step 4, which needs per-model resolver configuration)
- `Mapping/CosmosDbContractResolver` replaces the private `PrivateSetterCamelCaseContractResolver`.
  It derives from `DefaultContractResolver` (contracts cached per instance) with
  `CamelCaseNamingStrategy { ProcessDictionaryKeys = true, OverrideSpecifiedNames = true }`,
  the same naming as `CamelCasePropertyNamesContractResolver`. Private setter support is unchanged.
- One resolver per `ModelMapper`, which `ModelMapRegistry` builds once per model, so contracts are built once.
  (With the obsolete `RepositoryBase` constructor they are rebuilt per repository instance.)

**Tests** – `ContractResolverCompatibilityTests`
- Golden comparison against a copy of the previous resolver (enums, dates, decimals, dictionary keys,
  `[JsonProperty]` names, nested objects): identical JSON. Verified that it fails when the naming settings differ.
- Private setters still restored.
- `PolymorphicMappingTests.SameModelConfiguredDifferently_EachMapperUsesItsOwnConfiguration`: two configs of the
  same model no longer share contracts.

---

## Step 4 – Polymorphism support (F1, F2)  `[x]`

**Decisions**
- Discriminator is a normal property chosen by the config (e.g. `"type"`), **not** `$type`.
  This avoids F2, keeps CLR type names out of stored data (refactor-safe, no type-name
  security risk) and keeps queries simple: `WHERE c.person.type = 'manager'`.
- Configured only in `IModelConfig<TModel>`. No attributes on domain classes
  (`AppFactory.Framework.Domain` has no Newtonsoft dependency).
- **Type-level registration is the default**: a hierarchy is declared once and applies everywhere it
  appears in the model graph (single property, array, collection, dictionary value, nested at any depth).
- **Property-level mapping is an optional override** for a specific property.
- The converter is attached to **properties** (`JsonProperty.Converter` / `ItemConverter`), also for type-level
  registration, never to a type contract. A contract converter on a concrete base type (`Person` registered as
  `"person"`) would call itself again when deserializing into `Person`.

**API (implemented)**
```csharp
config.Polymorphic<Person>()
      .DiscriminateOn("type")
      .WithValue<Person>("person")
      .WithValue<Manager>("manager")
      .DefaultTo<Person>();

config.Map(x => x.Persons)
      .DiscriminateOn("personType")
      .WithValue<Person>("person")
      .WithValue<Manager>("manager");
```

**Implemented**
- `IModelConfigOptions<TModel>`: `Polymorphic<TBase>()` and `Map(expression)` (also on `PartitionKeyConfigBuilder`).
  Adding members to this public interface breaks external implementations of it (none known).
- `Configuration/IPolymorphicConfigOptions.cs`: `IPolymorphicConfigOptions<TBase>.DiscriminateOn(name)` →
  `IDiscriminatorConfigOptions<TBase>.WithValue<T>(value)` / `DefaultTo<T>()`.
  `Map(...)` uses `TBase = object` and checks registered types against the property (item) type at configuration time.
- `Configuration/DiscriminatorConfig` + `DiscriminatorConfigBuilder`; `Configuration/TypeHelper` (collection item type, inheritance depth).
- `Mapping/DiscriminatorConverter`: write adds the discriminator as the first property; read picks the type,
  removes the discriminator and deserializes into the concrete type.
- Lookup per property: property-level (by declaring type + member name) → most specific type-level base type.
- Missing discriminator on read: `DefaultTo<T>()`, else the declared property type if concrete, else exception.
- Validation when the mapper is built: `DiscriminateOn` required, at least one `WithValue`, discriminator name must
  not clash with a property of a registered type. At configuration time: concrete types only, assignable to the base,
  no duplicate values/types, `Polymorphic<T>()` / `Map(x)` once each.
- README: new "Polymorphic Properties" section.
- Not supported: polymorphic root model (`RepositoryBase<Payment>`), nested collections (`List<List<Person>>`),
  properties typed `object`.

**Tests** – `PolymorphicMappingTests` (19 tests, round trips go through JSON text like Cosmos)
- Abstract property, concrete base property, `List<T>`, array, dictionary values, `IList<T>`.
- Nested hierarchy (`Manager.Reports`, `Person.Mentor`) at several levels.
- Property-level override wins for that property only; property-level without type-level.
- Stored without discriminator: `DefaultTo<T>()`, concrete property type, abstract without default throws.
- Unknown value, unregistered runtime type, value mapping to a type not assignable to the property: descriptive exceptions.
- Nested `Id`/partition-key-named properties kept inside polymorphic objects (Step 1 still applies).
- Configuration validation errors.

---

## Step 5 – Partition key resolvers (F5, F6) – deferred until the tenant feature  `[ ]`

`UseResolver` is not used anywhere (only two mapper unit tests) and is not documented in the README.
Decision: leave it as is and do this whole step together with the tenant feature (no interim warning).

**Why reads need the resolver:** the resolver never populates the entity (the entity has no `TenantId`;
`MapBackPartitionKeys` skips resolver parts). It is needed to *locate* the document: Cosmos point reads and
deletes require `id` + full partition key, and for `GetById(id)` / `Delete(id)` the only source of the tenant
partition key value is the resolver. This also gives tenant isolation: a `GetById` from tenant `T2` looks in
partition `T2` and cannot read `T1`'s documents.

**Changes**
- `UseResolver<TResolver>()` stores the resolver **type**. `ModelMapRegistry` creates it via
  `ActivatorUtilities.CreateInstance` so it can inject services.
- Resolvers are singletons. They depend on accessor-style services (`IHttpContextAccessor`,
  `ITenantContextAccessor`, AsyncLocal-based) and read the value inside `GetValue()` on every call.
- `GetDocumentKey(object id)` uses `Resolver.GetValue()` for resolver parts.
- Remove unused `PartitionKeyConfig.GetPropertiesWithValues` (would throw on resolver parts).

**Tests**
- `Add` → `GetById` → `Delete` round trip with a resolver partition key.
- Resolver value changes between calls (simulated tenant switch) are honoured.

---

## Release

The package is published once Step 4 (polymorphism) is done, not per step. **Done: CHANGELOG `[10.8.0]` entry added (2026-10-19). The package version comes from the release tag (`v10.8.0`), the csproj `<Version>` is not changed.**
At that point: release as a minor version via the tag (e.g. `v10.8.0`; the old `RepositoryBase`
constructor stays `[Obsolete]`, so nothing breaks), and add one `CHANGELOG.md` entry that covers all steps, including these behaviour changes:
- Nested properties named like the Id or a partition key are now stored (Step 1).
- The Cosmos `id` is restored into the property configured with `Id(...)`. A write throws when another property
  sharing `id` has a different value (Step 1b).
- Partition key setters assign `default` for `null` into value types instead of throwing (Step 1b).
- New: `IModelMapRegistry` and the `RepositoryBase` constructor taking it; the `IModelConfig<TModel>` constructor is obsolete (Step 2).
- New: `Polymorphic<TBase>()` and `Map(...)` on `IModelConfigOptions<TModel>` (Step 4). Custom implementations
  of that interface, if any exist, must add them.

---

## Progress

| Step | Status | PR |
|------|--------|----|
| 1. Nested ignore fix | Done | |
| 1b. Id belongs to the configured property | Done | |
| 2. ModelMapRegistry | Done | |
| 3. Instance-cached resolver | Done | |
| 4. Polymorphism | Done | |
| 5. Partition key resolvers | Deferred | |
