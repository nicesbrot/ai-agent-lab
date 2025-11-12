using AgentLab.Contracts;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;

internal class Program
{
    private static async Task Main(string[] args)
    {
        List<ChatMessageDto> chatMessages = [];
        while (true)
        {
            Console.WriteLine("Ask something about this project: ");
            string? userInput = Console.ReadLine();
            if (string.IsNullOrEmpty(userInput))
            {
                continue;
            }

            // Handler mit SSL-Ausnahme (lokale Zertifikate erlauben)
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            using var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost:7113"),
                Timeout = TimeSpan.FromMinutes(5)
            };

            var newChatMessage = new ChatMessageDto("user", userInput);
            chatMessages.Add(newChatMessage);

            var requestBody = new
            {
                messages = chatMessages
            };

            try
            {
                Console.WriteLine("\nSending request to your API...");

                var response = await httpClient.PostAsJsonAsync("/api/chat", requestBody);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API-Error: {response.StatusCode}");
                    Console.WriteLine(responseText);
                    continue;
                }

                // JSON deserialisieren
                var chatResponse = JsonSerializer.Deserialize<ChatResponseDto>(responseText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (chatResponse != null)
                {
                    Console.WriteLine($"\nAI: {chatResponse.Content}");
                    var chatMessageFromChatResponse = new ChatMessageDto(
                        chatResponse.Role,
                        chatResponse.Content);
                    chatMessages.Add(chatMessageFromChatResponse);
                }
                else
                {
                    Console.WriteLine("Invalid response!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }

    }
}