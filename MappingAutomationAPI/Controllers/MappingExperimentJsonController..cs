using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MappingAutomationAPI.Models;
using MappingAutomationAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/experiment")]
public class MappingExperimentJsonController : ControllerBase
{
    private readonly IHostEnvironment _env;
    private readonly OpenAIService _openAIService;
    private readonly VectorDbService _vectorDbService;
    private readonly ILogger<MappingExperimentJsonController> _logger;

    private const double SimilarityThreshold = 0.5;
    private const int DefaultTopK = 5;

    public MappingExperimentJsonController(
        IHostEnvironment env,
        OpenAIService openAIService,
        VectorDbService vectorDbService,
        ILogger<MappingExperimentJsonController> logger)
    {
        _env = env;
        _openAIService = openAIService;
        _vectorDbService = vectorDbService;
        _logger = logger;
    }

    /// <summary>
    /// Runs the mapping experiment over the bugs_frs_experiment.json file.
    /// </summary>
    [HttpGet("run-json")]
    public async Task<IActionResult> RunJsonExperiment()
    {
        var path = Path.Combine(_env.ContentRootPath, "bugs_frs_experiment.json");
        if (!System.IO.File.Exists(path))
            return NotFound("Experiment JSON file not found.");

        var json = await System.IO.File.ReadAllTextAsync(path);
        var inputs = JsonSerializer.Deserialize<List<JsonInput>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var results = new List<ExperimentResult>();
        foreach (var item in inputs)
        {
            if (string.IsNullOrWhiteSpace(item.Description))
                continue;

            try
            {
                var vec = await _openAIService.GenerateEmbeddingAsync(item.Description);

                var matches = await _vectorDbService.FindSimilarTestsAsync(vec, DefaultTopK);

                bool requiresNew = matches.All(m => m.Similarity < SimilarityThreshold);

                var result = new ExperimentResult
                {
                    No = item.No,
                    Type = item.Type,
                    Product = item.Product,
                    Title = item.Title,
                    Description = item.Description,
                    RequiresNewTest = requiresNew,
                    Matches = matches.Select(m => new MatchResult
                    {
                        Module = m.Module,
                        App = m.App,
                        TestName = m.TestName,
                        Similarity = m.Similarity
                    }).ToList()
                };

                if (!requiresNew)
                {
                    string decisionRaw = null;
                    var decision = await _openAIService.GenerateMappingDecisionRaw(
                        new MapWorkflowRequest
                        { Type = item.Type, Title = item.Title, Description = item.Description },
                        matches
                    );
                    decisionRaw = decision.Content
                    .FirstOrDefault()?.Text
                        ?? string.Empty;
                    result.MappingDecision = decisionRaw;
                }
                else
                {
                    var completion = await _openAIService.GenerateAutomatedTestDescriptionRaw(
                        item.Description, item.Type, item.Title);
                    result.NewTestScenario = completion.Content.FirstOrDefault()?.Text?.Trim();
                }

                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing item No {No}", item.No);
            }
        }

        return Ok(results);
    }
}

public class JsonInput
{
    public int No { get; set; }
    public string Type { get; set; }
    public string Product { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}

public class ExperimentResult
{
    public int No { get; set; }
    public string Type { get; set; }
    public string Product { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool RequiresNewTest { get; set; }
    public List<MatchResult> Matches { get; set; } = new();
    public string MappingDecision { get; set; }
    public string NewTestScenario { get; set; }
}

public class MatchResult
{
    public string Module { get; set; }
    public string App { get; set; }
    public string TestName { get; set; }
    public double Similarity { get; set; }
}
