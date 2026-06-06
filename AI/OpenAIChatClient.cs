using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinformsVibes.AI;

public class OpenAIChatClient
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly Uri _baseUrl;

    public OpenAIChatClient(string apiKey, string model, string baseUrl = "http://192.168.2.15:8888/v1")
    {
        _baseUrl = new Uri(baseUrl.TrimEnd('/'));
        _model = model;
        _http = new HttpClient { BaseAddress = _baseUrl };
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<string> ChatAsync(string message, string systemPrompt = "You are a helpful assistant.")
    {
        var payload = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = message },
            },
        };

        var response = await _http.PostAsJsonAsync("/chat/completions", payload);
        var body = await response.Content.ReadFromJsonAsync<ChatResponse>()
                   ?? throw new InvalidOperationException("Empty response from endpoint.");

        return body.Choices[0].Message.Content;
    }

    public void Dispose() => _http.Dispose();

    // --- DTOs ---

    sealed class ChatResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; } = [];
    }

    sealed class Choice
    {
        [JsonPropertyName("message")]
        public ChatMessage Message { get; set; } = new();
    }

    sealed class ChatMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
