using System;
using System.Text.Json.Serialization;

namespace StorageFunc.Interfaces
{
    public class FileResponse
    {
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;

        [JsonPropertyName("validUntil")]
        public DateTimeOffset ValidUntil { get; set; }
    }
}
