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
        private readonly ILogger<ClaudeService> _logger;

        public ClaudeService(IConfiguration configuration, ILogger<ClaudeService> logger)
        {
            _client = new AnthropicClient(new APIAuthentication(configuration["Claude:Key"] ?? ""));
            _logger = logger;
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
            _logger.LogDebug("Entering {Method}", nameof(SendPrompt));

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

            _logger.LogDebug("Claude response: {Response}", jsonContent);
            return jsonContent;
        }
    }
}
