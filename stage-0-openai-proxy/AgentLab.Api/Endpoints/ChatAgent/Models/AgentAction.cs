using System.Text.Json;

namespace AgentLab.Api.Endpoints.ChatAgent.Models
{
    public class AgentAction
    {
        public string? Type { get; set; }     
        public string? Tool { get; set; }        
        public JsonElement Arguments { get; set; } 
        public string? Content { get; set; }       
    }
}
