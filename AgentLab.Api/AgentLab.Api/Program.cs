using System.ClientModel;
using AgentLab.Api;
using AgentLab.Api.Contracts;
using DotNetEnv;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

//load configuration from .env file
var secrets = Env.Load().ToDictionary();

string apiKey = secrets["OPENAI__APIKEY"];

builder.Services.AddSingleton(new ChatClient("gpt-4.1-nano", apiKey));

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapPost("/api/chat", async (ChatRequest request, ChatClient chatClient) =>
{
    if (request.Messages is null || request.Messages.Count == 0)
    {
        return Results.BadRequest(new { error = "messages[] is required" });
    }

    var chatMessages = new List<ChatMessage>()
    {
        new SystemChatMessage(AgentLabSystemChatMessage.Message)
    };

    foreach (ChatMessageDto msg in request.Messages)
    {
        string role = msg.Role.Trim().ToLowerInvariant();

        //FAQ: ❓ What is the difference between the system, user, and assistant roles?
        ChatMessage chatMessage = role switch
        {
            "system" => new SystemChatMessage(msg.Content),
            "assistant" => new AssistantChatMessage(msg.Content),
            _ => new UserChatMessage(msg.Content) // default: user
        };

        chatMessages.Add(chatMessage);
    }

    // Call OpenAI via ChatClient
    ClientResult<ChatCompletion> completion = await chatClient.CompleteChatAsync(chatMessages);

    string content = completion.Value.Content.FirstOrDefault()?.Text ?? string.Empty;

    ChatResponseDto response = new ChatResponseDto("assistant", content);

    return Results.Ok(response);
})
    .WithName("Chat");

app.Run();
