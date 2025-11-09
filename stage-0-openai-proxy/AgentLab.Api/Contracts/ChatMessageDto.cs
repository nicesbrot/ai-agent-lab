using System.ComponentModel.DataAnnotations;

namespace AgentLab.Api.Contracts
{
    public record ChatMessageDto(
        [Required] string Role,
        [Required] string Content);
}
