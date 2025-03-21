namespace CSharpProxy
{
    using System.Text.Json.Serialization;

    public class AuthResponse
    {
        [JsonPropertyName("session")]
        public required Session Session { get; set; }
    }

    public class Session
    {
        [JsonPropertyName("valid")]
        public required bool Valid { get; set; }

        [JsonPropertyName("totp")]
        public required bool Totp { get; set; }

        [JsonPropertyName("sid")]
        public required string Sid { get; set; }

        [JsonPropertyName("csrf")]
        public required string Csrf { get; set; }

        [JsonPropertyName("validity")]
        public required int Validity { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }
    }

}
