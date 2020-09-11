using System.Text.Json.Serialization;

namespace StorageFunc.Interfaces
{
    public class FileRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("file")]
        public byte[] File { get; set; }
    }
}
