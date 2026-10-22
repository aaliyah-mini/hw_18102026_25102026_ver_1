using System.Text.Json.Serialization;

namespace LocalAiApi.Models
{
    public class OllamaRespose
    {
        [JsonPropertyName("message")]
        public OllamaMessage Message { get; set; }  // нам нужет только Message в ответе от ИИ (там находится контент)
    }
}
