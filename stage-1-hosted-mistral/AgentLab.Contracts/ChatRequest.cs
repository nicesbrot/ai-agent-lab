using System.ComponentModel.DataAnnotations;

namespace AgentLab.Contracts
{
    public record ChatRequest(
        [Required] List<ChatMessageDto> Messages);
}
