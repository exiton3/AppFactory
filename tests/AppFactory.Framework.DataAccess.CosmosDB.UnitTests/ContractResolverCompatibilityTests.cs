using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace AppFactory.Framework.DataAccess.CosmosDB.UnitTests
{
    // CosmosDbContractResolver replaced a CamelCasePropertyNamesContractResolver subclass.
    // Without polymorphism configured, documents must be written exactly as before so existing data still reads.
    public class ContractResolverCompatibilityTests
    {
        [Fact]
        public void MapToDocument_WithoutPolymorphism_WritesSameJsonAsPreviousResolver()
        {
            var config = new CosmosDbModelConfig<Sample>();
            config.ContainerName("Samples").Id(s => s.Id).PartitionKey(s => s.TenantId);
            var mapper = new ModelMapper<Sample>(config);

            var model = new Sample("S1")
            {
                TenantId = "T1",
                DisplayName = "Sample",
                Status = SampleStatus.InProgress,
                CreatedAt = new DateTime(2026, 10, 19, 12, 30, 0, DateTimeKind.Utc),
                Amount = 12.5m,
                Tags = { "a", "b" },
                Counters = { ["FirstKey"] = 1, ["second_key"] = 2 },
                Child = new SampleChild { ChildName = "child", Nested = new SampleChild { ChildName = "nested" } },
                CustomNamed = "custom"
            };

            var expected = JsonConvert.SerializeObject(model, PreviousSettings());
            var document = mapper.MapToDocument(model);

            // The previous output plus what the mapper adds; the root Id/TenantId removal and id/partition key
            // writing are the same for both, so compare after removing them.
            var previous = Newtonsoft.Json.Linq.JObject.Parse(expected);
            previous.Remove("id");
            previous.Remove("tenantId");
            var current = Newtonsoft.Json.Linq.JObject.Parse(document.ToJson());
            current.Remove("id");
            current.Remove("tenantId");

            Assert.Equal(previous.ToString(Formatting.None), current.ToString(Formatting.None));
        }

        [Fact]
        public void MapFromDocument_PrivateSetters_AreStillRestored()
        {
            var config = new CosmosDbModelConfig<Sample>();
            config.ContainerName("Samples").Id(s => s.Id).PartitionKey(s => s.TenantId);
            var mapper = new ModelMapper<Sample>(config);

            var result = mapper.MapFromDocument(mapper.MapToDocument(new Sample("S1") { TenantId = "T1" }.WithSecret("hidden")));

            Assert.Equal("S1", result.Id);
            Assert.Equal("hidden", result.Secret);
        }

        private static JsonSerializerSettings PreviousSettings() => new JsonSerializerSettings
        {
            ContractResolver = new PreviousResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Converters = { new StringEnumConverter(new CamelCaseNamingStrategy()) }
        };

        // Copy of the resolver used before CosmosDbContractResolver
        private class PreviousResolver : CamelCasePropertyNamesContractResolver
        {
            protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                if (!property.Writable && member is System.Reflection.PropertyInfo prop && prop.GetSetMethod(nonPublic: true) != null)
                {
                    property.Writable = true;
                }

                return property;
            }
        }

        public enum SampleStatus { New, InProgress }

        public class Sample
        {
            [JsonConstructor]
            public Sample(string id)
            {
                Id = id;
            }

            public string Id { get; private set; }
            public string? TenantId { get; set; }
            public string? DisplayName { get; set; }
            public SampleStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public decimal Amount { get; set; }
            public List<string> Tags { get; set; } = new List<string>();
            public Dictionary<string, int> Counters { get; set; } = new Dictionary<string, int>();
            public SampleChild? Child { get; set; }
            public string? Secret { get; private set; }

            [JsonProperty("Custom_Name")]
            public string? CustomNamed { get; set; }

            public Sample WithSecret(string secret)
            {
                Secret = secret;
                return this;
            }
        }

        public class SampleChild
        {
            public string? ChildName { get; set; }
            public SampleChild? Nested { get; set; }
        }
    }
}
