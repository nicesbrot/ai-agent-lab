using System.ClientModel;
using System.Net.WebSockets;
using AgentLab.Api;
using AgentLab.Api.Contracts;
using AgentLab.Api.Endpoints.ChatAgent;
using AgentLab.Api.Endpoints.SimpleChat;
using DotNetEnv;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

//load configuration from .env file
var secrets = Env.Load().ToDictionary();

string apiKey = secrets["OPENAI__APIKEY"];

builder.Services.AddSingleton(new ChatClient("gpt-4.1-nano", apiKey));
builder.Services.AddSingleton(x => new FileService("FAQ.md"));

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapSimpleChat();
app.MapChatAgent();  

app.Run();
