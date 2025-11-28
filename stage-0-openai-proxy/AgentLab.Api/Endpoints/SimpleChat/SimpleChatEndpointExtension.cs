using AgentLab.Api.Contracts;
using OpenAI.Chat;
using System.ClientModel;

namespace AgentLab.Api.Endpoints.SimpleChat
{
    public static class SimpleChatEndpointExtension
    {
        public static RouteHandlerBuilder MapSimpleChat(this IEndpointRouteBuilder builder)
        {
            return builder.MapPost("/api/simple-chat", async (ChatRequest request, ChatClient chatClient) =>
            {
                if (request.Messages is null || request.Messages.Count == 0)
                {
                    return Results.BadRequest(new { error = "messages[] is required" });
                }
                var chatMessages = new List<ChatMessage>()
                {
                    new SystemChatMessage(SimpleChatSystemPrompt.Message)
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
            .WithName("Simple Chat");
        }
    }
}
