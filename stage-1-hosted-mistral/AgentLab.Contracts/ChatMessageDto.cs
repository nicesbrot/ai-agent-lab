using System.ComponentModel.DataAnnotations;

namespace AgentLab.Contracts
{
    public record ChatMessageDto(
        [Required] string Role,
        [Required] string Content);
}
