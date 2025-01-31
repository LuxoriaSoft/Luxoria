using LuxImport.Utils;
using Newtonsoft.Json;

namespace LuxImport.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="CustomLuxCfgResolver"/> class.
    /// </summary>
    public class CustomLuxCfgResolverTests
    {
        /// <summary>
        /// Sample class to test the custom resolver.
        /// </summary>
        private class TestLuxCfg
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = "";
        }

        private readonly JsonSerializerSettings _jsonSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomLuxCfgResolverTests"/> class.
        /// Sets up JSON serializer settings with the custom resolver.
        /// </summary>
        public CustomLuxCfgResolverTests()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CustomLuxCfgResolver(),
                Formatting = Formatting.Indented
            };
        }

        /// <summary>
        /// Tests whether the "Id" property remains writable during deserialization.
        /// </summary>
        [Fact]
        public void Deserialize_ShouldAllowSettingIdProperty()
        {
            var expectedId = Guid.NewGuid();
            string json = $"{{ \"Id\": \"{expectedId}\", \"Name\": \"TestObject\" }}";

            var deserializedObject = JsonConvert.DeserializeObject<TestLuxCfg>(json, _jsonSettings);

            Assert.NotNull(deserializedObject);
            Assert.Equal(expectedId, deserializedObject.Id);
            Assert.Equal("TestObject", deserializedObject.Name);
        }

        /// <summary>
        /// Tests whether the object is correctly serialized with the "Id" property.
        /// </summary>
        [Fact]
        public void Serialize_ShouldIncludeIdProperty()
        {
            var obj = new TestLuxCfg
            {
                Id = Guid.NewGuid(),
                Name = "TestObject"
            };

            string json = JsonConvert.SerializeObject(obj, _jsonSettings);

            Assert.Contains($"\"Id\": \"{obj.Id}\"", json);
            Assert.Contains("\"Name\": \"TestObject\"", json);
        }
    }
}
