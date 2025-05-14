using MappingAutomationAPI.Services;
using MappingAutomationAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/ast-mapping")]
public class AstMappingController : ControllerBase
{
    private readonly OpenAIService _openAIService;
    private readonly VectorDbService _vectorDbService;
    private readonly ILogger<AstMappingController> _logger;

    private const double SimilarityThreshold = 0.7;
    private const int DefaultTopK = 5;

    public AstMappingController(
        OpenAIService openAIService,
        VectorDbService vectorDbService,
        ILogger<AstMappingController> logger)
    {
        _openAIService = openAIService;
        _vectorDbService = vectorDbService;
        _logger = logger;
    }

    [HttpPost("map-workflow-to-ast")]
    public async Task<IActionResult> MapWorkflowToAst([FromBody] MapWorkflowRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Description)
         || string.IsNullOrWhiteSpace(req.Type)
         || string.IsNullOrWhiteSpace(req.Title))
        {
            return BadRequest(new { Error = "Type, Title and Description are all required." });
        }

        try
        {
            float[] issueVector = await _openAIService.GenerateEmbeddingAsync(req.Description);

            var matches = await _vectorDbService.FindSimilarTestsAsync(issueVector, DefaultTopK);

            bool requiresNewTest = matches.All(m => m.Similarity < SimilarityThreshold);

            var response = new
            {
                Matches = matches,
                RequiresNewTest = requiresNewTest,
                MappingDecision = (string?)null,
                NewTestScenario = (string?)null
            };

            if (!requiresNewTest)
            {
                string mappingDecision = null;
                var decision = await _openAIService.GenerateMappingDecisionRaw(req, matches);
                mappingDecision = decision.Content
                                    .FirstOrDefault()?.Text
                                ?? string.Empty;
        
                return Ok(new
                {
                    Matches = matches,
                    RequiresNewTest = false,
                    MappingDecision = mappingDecision,
                    NewTestScenario = (string?)null
                });
            }
            else
            {
                var completion = await _openAIService.GenerateAutomatedTestDescriptionRaw(
                    req.Description,
                    req.Type,
                    req.Title
                );
                var newScenario = completion.Content.FirstOrDefault()?.Text?.Trim()
                                  ?? "<no output>";
                return Ok(new
                {
                    Matches = matches,
                    RequiresNewTest = true,
                    MappingDecision = (string?)null,
                    NewTestScenario = newScenario
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping workflow");
            return StatusCode(500, new { Error = "Mapping failed", Details = ex.Message });
        }
    }
}
