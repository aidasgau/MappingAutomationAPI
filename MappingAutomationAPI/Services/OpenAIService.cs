using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MappingAutomationAPI.Models;
using System.ClientModel;

namespace MappingAutomationAPI.Services
{
    public class OpenAIService
    {
        private readonly OpenAIClient _client;
        private readonly string _chatModel;
        private readonly string _embeddingModel;

        public OpenAIService(IConfiguration config)
        {
            _client = new OpenAIClient(config["OpenAI:ApiKey"]);
            _chatModel = config["OpenAI:ChatModel"] ?? "gpt-4o";
            _embeddingModel = config["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";
        }

        /// <summary>
        /// Returns the raw ChatCompletion response for downstream unwrapping.
        /// </summary>
        public async Task<ChatCompletion> GenerateTestDescriptionRaw(string testCode)
        {
            return await _client
                .GetChatClient(_chatModel)
                .CompleteChatAsync($@"Analyze this Selenium test and generate a concise description 
                focusing on its purpose and tested functionality:
                {testCode}, Ignore explaining what kind of framework or libraries are used, focus only on providing
                a concise description of what functionality the particular regression test is aiming to verify.");
        }

        /// <summary>
        /// Generates an embedding vector for the given text, using the configured embedding model.
        /// </summary>
        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var embedClient = _client.GetEmbeddingClient(_embeddingModel);

            var embeddingResult = await embedClient.GenerateEmbeddingAsync(text);

            return embeddingResult
                   .Value
                   .ToFloats()
                   .ToArray();
        }

        /// <summary>
        /// Returns the raw ChatCompletion for an automated test description prompt.
        /// </summary>
        public async Task<ChatCompletion> GenerateAutomatedTestDescriptionRaw(
            string issueDesc,
            string module,
            string appName)
        {
            var prompt = $@"
                You are a QA engineer. Given a bug/feature request, describe a concise automated software test
                that should be created in the '{module}' module of the '{appName}' application.
                Focus on the test’s purpose, key steps, and assertions needed to verify the behavior:
                '{issueDesc}'";

            return await _client
                .GetChatClient(_chatModel)
                .CompleteChatAsync(prompt);
        }
    }
}
