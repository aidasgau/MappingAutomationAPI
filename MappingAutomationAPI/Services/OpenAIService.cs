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
                a concise description of what functionality the particular regression test is aiming to verify.
                Use this as an example: Probabilities.cs - Includes 4 tests - Verifies that Inserting, Updating, Deleting 
                records in the probabilities grid functions correctly and that Generating defaults is working as expected.
                Ignore the fact that each AST logs and captures a screenshot as it's not really relevant.");
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
                '{issueDesc}'
                As an example: A new AST should be created for '{module}' of the '{appName}' that verifies the following functionality: ...";

            return await _client
                .GetChatClient(_chatModel)
                .CompleteChatAsync(prompt);
        }

        /// <summary>
        /// Uses a second AI layer to choose the best existing test or indicate none fit.
        /// Returns a ChatCompletion where the content is "<choice> – <justification>".
        /// </summary>
        public async Task<ChatCompletion> GenerateMappingDecisionRaw(
            MapWorkflowRequest req,
            List<SimilarTest> candidates)
        {
            var header = $@"
                Type: {req.Type}
                Title: {req.Title}
                Description: {req.Description}
                ".Trim();

            var list = string.Join("\n", candidates.Select((c, i) =>
                $"{i + 1}. {c.TestName} (Module={c.Module}, App={c.App}, Similarity={c.Similarity:F2})"));

            var prompt = $@"
                You are a QA assistant. Given the following reported issue and existing test cases,
                choose the single best test case number that matches the issue description, or reply 'None' if no existing test applies.

                {header}

                Existing Tests:
                {list}

                Respond with the test number (1-{candidates.Count}) or 'None', and a brief justification.
                ".Trim();

            return await _client
                .GetChatClient(_chatModel)
                .CompleteChatAsync(prompt);
        }

    }
}
