using System.ComponentModel.DataAnnotations;

namespace AgentLab.Api.Contracts
{
    public record ChatRequest(
        [Required] List<ChatMessageDto> Messages);
}
