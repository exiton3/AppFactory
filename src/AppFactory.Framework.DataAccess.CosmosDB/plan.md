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

## Step 2 – `ModelMapRegistry`: build config and mapper once per model (F4)  `[ ]`

**Changes**
- New `CosmosDbModelMap<TModel>` holding `Config` and `Mapper` (internal members).
- New `IModelMapRegistry` / `ModelMapRegistry` (singleton):
  - `Get<TModel>()` builds lazily from `IModelConfig<TModel>` resolved from `IServiceProvider`.
  - `ConcurrentDictionary<Type, Lazy<object>>` so `Configure()` runs exactly once even under concurrency.
- Register it in `RegisterCosmosDbPersistence`.
- `RepositoryBase<TModel>`:
  - New constructor `(ICosmosDbClientFactory, ILogger, IModelMapRegistry)`.
  - Existing constructor `(…, IModelConfig<TModel>)` kept and marked `[Obsolete]` for one version.
- Document that the config and mapper are shared across requests and threads, so anything
  plugged into them (resolvers, converters) must be stateless and thread-safe.
- Optional: eager validation of all registered configs at startup.

**Tests**
- `Get<TModel>()` returns the same instance on repeated and concurrent calls.
- `Configure()` is invoked once.
- Repository via new constructor behaves identically to the old one.

**Versioning:** non-breaking (old constructor still works). Note in `CHANGELOG.md`.

---

## Step 3 – Instance-cached contract resolver  `[ ]`

**Why:** removes the hidden dependency on `CamelCasePropertyNamesContractResolver`'s static cache
(keyed by resolver CLR type). Only safe **after Step 2**: before it, every request would rebuild contracts.

**Note after Step 1:** the only remaining resolver (`PrivateSetterCamelCaseContractResolver`) holds no
per-model configuration, so sharing its static cache is harmless today. This step becomes required
once Step 4 puts per-model polymorphism config into the resolver – implement it together with Step 4.

**Changes**
- `PrivateSetterCamelCaseContractResolver` derives from
  `DefaultContractResolver` with `NamingStrategy = new CamelCaseNamingStrategy()`
  (must also camel-case dictionary keys to match current output).

**Tests**
- Existing serialization output unchanged (golden JSON comparison).
- Two configs of the same `TModel` with different ignore lists produce different output.

---

## Step 4 – Polymorphism support (F1, F2)  `[ ]`

**Decisions**
- Discriminator is a normal property chosen by the config (e.g. `"type"`), **not** `$type`.
  This avoids F2, keeps CLR type names out of stored data (refactor-safe, no type-name
  security risk) and keeps queries simple: `WHERE c.person.type = 'manager'`.
- Configured only in `IModelConfig<TModel>`. No attributes on domain classes
  (`AppFactory.Framework.Domain` has no Newtonsoft dependency).
- **Type-level registration is the default**: a hierarchy is declared once and applies everywhere it
  appears in the model graph (single property, `List<T>`, nested at any depth).
- **Property-level mapping is an optional override** for a specific property.

**API (proposed)**
```csharp
// Type-level – applies to every Person in the Department graph
config.Polymorphic<Person>("type")
      .WithValue<Person>("person")     // concrete base types need a value too
      .WithValue<Manager>("manager")
      .DefaultTo<Person>();            // documents stored before the discriminator existed

// Property-level override
config.Map(x => x.Persons)
      .DiscriminateOn("personType")
      .WithValue<Person>("person")
      .WithValue<Manager>("manager");
```

**Implementation**
- Write: contract resolver adds the discriminator property with a constant value to each
  registered subtype's contract (for property-level: converter on `JsonProperty.Converter` /
  `ItemConverter` writes it).
- Read: `JsonConverter` per registered base type, `CanConvert(t) => t == baseType`,
  `CanWrite = false`. Loads `JObject`, reads the discriminator and deserializes into the subtype.
  Property-level uses `JsonProperty.Converter` (single value) / `ItemConverter` (collections, dictionaries).
- Property-level matching by `MemberInfo`, not name (same rule as Step 1).
- Unknown discriminator value or unregistered runtime type → clear exception naming the model, property and value.

**Tests**
- Abstract property, concrete-base property, `List<Base>`, nested hierarchy (`Manager.Reports`).
- Property-level override wins over type-level.
- Old documents without a discriminator load via `DefaultTo<T>()`.
- Unknown discriminator / unregistered subtype → descriptive exception.

---

## Step 5 – Partition key resolvers (F5, F6) – deferred until the tenant feature  `[ ]`

`UseResolver` is not used in the codebase today. Until this step is done, mark it in the
README/XML docs as not supported for `GetById`/`Delete` (or `[Obsolete]`), so nobody writes data
they can't read back.

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

## Progress

| Step | Status | PR |
|------|--------|----|
| 1. Nested ignore fix | Done | |
| 1b. Id belongs to the configured property | Done | |
| 2. ModelMapRegistry | Not started | |
| 3. Instance-cached resolvers | Not started | |
| 4. Polymorphism | Not started | |
| 5. Partition key resolvers | Deferred | |
