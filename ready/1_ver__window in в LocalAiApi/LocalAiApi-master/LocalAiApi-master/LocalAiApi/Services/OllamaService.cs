
using System.Net.Http.Json;
using LocalAiApi.Models;

namespace LocalAiApi.Services
{
    public class OllamaService(HttpClient httpClient)
    {
        public async Task<string> ChatAsync(string message, string prompt, CancellationToken cancellationToken = default)
        {
            var request = new  // запрос к ИИ 
            {
                model = "qwen3:8b",
                stream = false,  // чтобы получить полный ответ, а не частичный 
                messages = new[]
                {
                    new { role = "system", content = prompt },
                    new { role = "user", content = message }
                }
            };

            using var response = await httpClient.PostAsJsonAsync("api/chat", request, cancellationToken);  // отправка post 

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaRespose>(cancellationToken: cancellationToken);

            if (string.IsNullOrWhiteSpace(result?.Message.Content))
            {
                throw new InvalidOperationException("Ollama вернула пустой ответ.");
            }

            return result.Message.Content;
        }
    }
}
