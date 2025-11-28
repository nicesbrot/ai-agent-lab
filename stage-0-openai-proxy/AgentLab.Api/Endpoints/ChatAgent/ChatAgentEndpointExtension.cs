using AgentLab.Api.Contracts;
using AgentLab.Api.Endpoints.ChatAgent.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace AgentLab.Api.Endpoints.ChatAgent
{
    public static class ChatAgentEndpointExtension
    {
        public static RouteHandlerBuilder MapChatAgent(this IEndpointRouteBuilder builder)
        {
            return builder.MapPost("/api/chat-agent", async (ChatRequest request, FileService fileService, ChatClient chatClient) =>
            {
                Console.Clear();
                if (request.Messages is null || request.Messages.Count == 0)
                {
                    Console.WriteLine("[Agent] Error: messages[] is missing.");
                    return Results.BadRequest(new { error = "messages[] is required" });
                }

                Console.WriteLine("[Agent] Starting agent processing.");

                var chatMessages = new List<ChatMessage>()
                {
                    new SystemChatMessage(ChatAgentDefinition.Message)
                };

                // Add user messages
                foreach (ChatMessageDto msg in request.Messages)
                {
                    string role = msg.Role.Trim().ToLowerInvariant();
                    ChatMessage chatMessage = role switch
                    {
                        "system" => new SystemChatMessage(msg.Content),
                        "assistant" => new AssistantChatMessage(msg.Content),
                        _ => new UserChatMessage(msg.Content)
                    };

                    Console.WriteLine($"[Agent] Added message: role={role}, content={msg.Content}");
                    chatMessages.Add(chatMessage);
                }

                const int maxSteps = 5;

                for (int step = 0; step < maxSteps; step++)
                {
                    Console.WriteLine($"[Agent] Step {step + 1}/{maxSteps}: Sending conversation to the model.");

                    ClientResult<ChatCompletion> completion = await chatClient.CompleteChatAsync(chatMessages);

                    string assistantRaw = completion.Value.Content.FirstOrDefault()?.Text ?? string.Empty;

                    Console.WriteLine("[Agent] Raw model output:");
                    Console.WriteLine(assistantRaw);

                    // Try parsing model output into AgentAction
                    AgentAction? action = null;
                    try
                    {
                        action = JsonSerializer.Deserialize<AgentAction>(
                            assistantRaw,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                        Console.WriteLine("[Agent] Parsed AgentAction:");
                        Console.WriteLine(JsonSerializer.Serialize(action));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Agent] Warning: Could not parse model output as AgentAction. {ex.Message}");
                    }

                    // If no valid action → treat as normal chat
                    if (action == null || string.IsNullOrWhiteSpace(action.Type))
                    {
                        Console.WriteLine("[Agent] No action detected. Returning raw model response.");
                        var fallbackResponse = new ChatResponseDto("assistant", assistantRaw);
                        return Results.Ok(fallbackResponse);
                    }

                    // Handle final answer
                    if (string.Equals(action.Type, "final_answer", StringComparison.OrdinalIgnoreCase))
                    {
                        string answer = action.Content ?? string.Empty;
                        Console.WriteLine("[Agent] Final answer returned by the model.");
                        var response = new ChatResponseDto("assistant", answer);
                        return Results.Ok(response);
                    }

                    // Handle tool call
                    if (string.Equals(action.Type, "tool_call", StringComparison.OrdinalIgnoreCase))
                    {
                        string toolName = action.Tool?.Trim().ToLowerInvariant() ?? string.Empty;

                        Console.WriteLine($"[Agent] Tool call detected: {toolName}");
                        Console.WriteLine("[Agent] Adding tool-call message to conversation memory.");

                        chatMessages.Add(new AssistantChatMessage(assistantRaw));

                        ReadFileArgs? toolArgs = null;
                        try
                        {
                            toolArgs = action.Arguments.Deserialize<ReadFileArgs>(
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            Console.WriteLine("[Agent] Parsed tool arguments:");
                            Console.WriteLine(JsonSerializer.Serialize(toolArgs));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Agent] Error parsing tool arguments: {ex.Message}");
                            chatMessages.Add(new AssistantChatMessage("Tool arguments could not be parsed."));
                            continue;
                        }

                        string toolResultText;

                        switch (toolName)
                        {
                            case "read_faq":
                                Console.WriteLine("[Agent] Executing tool: read_faq");
                                toolResultText = fileService.LoadFaqContent();
                                chatMessages.Add(new AssistantChatMessage(
                                    $"Tool read_faq result:\n{toolResultText}"));
                                break;

                            default:
                                Console.WriteLine($"[Agent] Unknown tool requested: {toolName}");
                                chatMessages.Add(new AssistantChatMessage(
                                    $"Requested tool is not available: {toolName}"));
                                break;
                        }

                        continue;
                    }

                    // Unknown action type
                    Console.WriteLine($"[Agent] Unknown action type: {action.Type}");
                    chatMessages.Add(new AssistantChatMessage(
                        $"Unknown action type: {action.Type}"));
                }

                // Maximum steps exceeded
                Console.WriteLine("[Agent] Error: Maximum reasoning steps exceeded.");
                var fallback = new ChatResponseDto(
                    "assistant",
                    "Error: Maximum reasoning steps exceeded without reaching a final answer.");

                return Results.Ok(fallback);
            })
            .WithName("Chat Agent");
        }
    }
}
