using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using PocketLawyer.Service;

namespace PocketLawyer.Infrastructure.AiServices.Claude
{
    public class ClaudeService : IAiService
    {
        private readonly AnthropicClient _client;

        public ClaudeService(IConfiguration configuration)
        {
            _client = new AnthropicClient(new APIAuthentication(configuration["Claude:Key"] ?? ""));
        }

        public string PromptBuilder(string filename, string filecontent)
        {
            throw new NotImplementedException();
        }

        public Task<string> SendContext()
        {
            throw new NotImplementedException();
        }

        public async Task<string> SendPrompt(string prompt, object? outputSchema, bool sendAltKey = false)
        {
            SystemMessage systemPrompt = new (@"Du antwortest immer in gültigem JSON Format. 
        Verwende keine Markdown-Formatierung, nur reines JSON.");

            var messages = new List<Message>
        {
            new Message(RoleType.User, $"{prompt}\n\nAntwort als JSON:")
        };

            var parameters = new MessageParameters
            {
                Messages = messages,
                MaxTokens = 4000,
                Model = AnthropicModels.Claude35Sonnet,
                System = new List<SystemMessage>() { systemPrompt }
            };

            var response = await _client.Messages.GetClaudeMessageAsync(parameters);
            var jsonContent = response.FirstMessage.Text;

            return jsonContent;
        }
    }

    public class ResponseFormat
    {
        public int Population { get; set; }
        public int Flaeche { get; set; }

        public string[] Bundeslaender { get; set; }
    }
}
