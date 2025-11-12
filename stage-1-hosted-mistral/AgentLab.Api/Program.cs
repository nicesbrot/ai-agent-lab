using AgentLab.Api;
using AgentLab.Contracts;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

//we are running our LLM on localhost
builder.Services.AddHttpClient("Ollama", client =>
{
    client.BaseAddress = new Uri("http://localhost:11434/");
    client.Timeout = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/api/chat", async (ChatRequest request, IHttpClientFactory httpClientFactory) =>
{
    if (request.Messages is null || request.Messages.Count == 0)
    {
        return Results.BadRequest();
    }

    var client = httpClientFactory.CreateClient("Ollama");

    var chatMessages = new List<object>
    {
        new { role = "system", content = AgentLabSystemChatMessage.Message }
    };

    foreach (ChatMessageDto msg in request.Messages)
    {
        string role = msg.Role.Trim().ToLowerInvariant();

        string mappedRule = role switch
        {
            "system" => "system",
            "assistant" => "assistant",
            _ => "user" // default: user
        };

        chatMessages.Add(new
        {
            role = mappedRule,
            content = msg.Content
        });
    }

    var body = new
    {
        model = "mistral",
        messages = chatMessages,
        stream = false,
        //FAQ:❓ What are the available options for customizing responses in Ollama API requests?
        options = new
        {
            num_predict = 128, 
        }
    };

    // send request to Ollama
    var response = await client.PostAsJsonAsync("api/chat", body);

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Ollama API error: {response.StatusCode}");
        return Results.StatusCode((int)response.StatusCode);
    }

    using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

    if (!doc.RootElement.TryGetProperty("message", out var msgElem) ||
        !msgElem.TryGetProperty("content", out var contentElem))
    {
        Console.WriteLine("[Ollama Error] Unexpected response format");
        return Results.StatusCode(StatusCodes.Status502BadGateway);
    }

    var content = contentElem.GetString() ?? string.Empty;

    var result = new ChatResponseDto("assistant", content);

    return Results.Ok(result);
});


app.Run();