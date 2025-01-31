using LuxImport.Utils;
using Newtonsoft.Json;

namespace LuxImport.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="CustomGuidConverter"/> class.
    /// </summary>
    public class CustomGuidConverterTests
    {
        private readonly JsonSerializerSettings _jsonSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomGuidConverterTests"/> class.
        /// Sets up JSON serializer settings with the custom GUID converter.
        /// </summary>
        public CustomGuidConverterTests()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                Converters = { new CustomGuidConverter() }
            };
        }

        /// <summary>
        /// Tests whether a valid GUID is correctly deserialized from JSON.
        /// </summary>
        [Fact]
        public void ReadJson_ShouldDeserializeValidGuid()
        {
            var validGuid = Guid.NewGuid();
            string json = $"\"{validGuid}\"";

            var deserializedGuid = JsonConvert.DeserializeObject<Guid>(json, _jsonSettings);

            Assert.Equal(validGuid, deserializedGuid);
        }

        /// <summary>
        /// Tests whether an invalid GUID string is deserialized as <see cref="Guid.Empty"/>.
        /// </summary>
        [Fact]
        public void ReadJson_ShouldReturnGuidEmpty_ForInvalidGuid()
        {
            string invalidJson = "\"Invalid-GUID-Value\"";

            var deserializedGuid = JsonConvert.DeserializeObject<Guid>(invalidJson, _jsonSettings);

            Assert.Equal(Guid.Empty, deserializedGuid);
        }

        /// <summary>
        /// Tests whether a valid GUID is correctly serialized to JSON.
        /// </summary>
        [Fact]
        public void WriteJson_ShouldSerializeGuidToString()
        {
            var guid = Guid.NewGuid();
            string expectedJson = $"\"{guid}\"";

            string serializedJson = JsonConvert.SerializeObject(guid, _jsonSettings);

            Assert.Equal(expectedJson, serializedJson);
        }
    }
}
