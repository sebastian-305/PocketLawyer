using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using PocketLawyer.Application.Interfaces;
using Microsoft.Extensions.Options;
using PocketLawyer.Options;
using PocketLawyer.Application.Models;

namespace PocketLawyer.Infrastructure.AiServices.Claude
{
    /// <summary>
    /// Legacy AI service for Claude API integration. This service is deprecated and will be removed in a future version.
    /// Use NebiusService instead for new implementations.
    /// </summary>
    [Obsolete("This service is deprecated. Use NebiusService instead.")]
    public class ClaudeService : IAiClient
    {
        private readonly AnthropicClient _client;
        private readonly ILogger<ClaudeService> _logger;

        public ClaudeService(IOptions<ClaudeOptions> options, ILogger<ClaudeService> logger)
        {
            _client = new AnthropicClient(new APIAuthentication(options.Value.Key ?? ""));
            _logger = logger;
        }



        public async Task<string> SendPrompt(string prompt, object? outputSchema)
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

        public Task<AiResponse> SendPromptWithMetadata(string prompt, object? outputSchema)
        {
            throw new NotImplementedException();
        }
    }
}
