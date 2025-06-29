using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using PocketLawyer.Service;
using Microsoft.Extensions.Options;

namespace PocketLawyer.Infrastructure.AiServices.Claude
{
    public class ClaudeService : IAiService
    {
        private readonly AnthropicClient _client;
        private readonly ILogger<ClaudeService> _logger;

        public ClaudeService(IOptions<ClaudeOptions> options, ILogger<ClaudeService> logger)
        {
            _client = new AnthropicClient(new APIAuthentication(options.Value.Key ?? ""));
            _logger = logger;
        }



        public async Task<string> SendPrompt(string prompt, object? outputSchema, bool useAltKey = false)
        {
            _logger.LogDebug("Entering {Method}", nameof(SendPrompt));

            SystemMessage systemPrompt = new(@"Du antwortest immer in gültigem JSON Format.
        Verwende keine Markdown-Formatierung, nur reines JSON.");

            var messages = new List<Message>
        {
            new Message(RoleType.User, $"{prompt}\n\nAntwort als JSON:")
        };

            var parameters = new MessageParameters
            {
                Messages = messages,
                MaxTokens = 1000,
                Model = AnthropicModels.Claude3Haiku,
                System = new List<SystemMessage>() { systemPrompt },
                Temperature = 0.1m
            };

            var response = await _client.Messages.GetClaudeMessageAsync(parameters);
            var jsonContent = response.FirstMessage.Text;

            _logger.LogDebug("Claude response: {Response}", jsonContent);
            return jsonContent;
        }
    }
}
