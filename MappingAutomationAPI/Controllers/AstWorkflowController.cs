using MappingAutomationAPI.Models;
using MappingAutomationAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/ast-workflows")]
public class AstWorkflowController : ControllerBase
{
    private readonly OpenAIService _openAIService;
    private readonly ILogger<AstWorkflowController> _logger;

    public AstWorkflowController(
        OpenAIService openAIService,
        ILogger<AstWorkflowController> logger)
    {
        _openAIService = openAIService;
        _logger = logger;
    }

    /// <summary>
    /// Generate a new Selenium test idea from a reported issue that lacks coverage.
    /// </summary>
    [HttpPost("new-ast-workflow")]
    public async Task<IActionResult> CreateNewAstWorkflow([FromBody] NewASTRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.IssueDescription) || string.IsNullOrWhiteSpace(request.Module))
            {
                return BadRequest(new { Error = "Missing required fields: IssueDescription and Module." });
            }

            var testScenario = await _openAIService.GenerateAutomatedTestDescriptionRaw(
                request.IssueDescription,
                request.Module,
                request.AppName
            );

            var newTask = new WorkflowTask
            {
                TaskId = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                Module = request.Module,
                Description = $"New test required for: {request.IssueDescription}",
                TestScenario = testScenario.Content.FirstOrDefault()?.Text
            };

            return Ok(newTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating new test scenario");
            return StatusCode(500, new { Error = "Workflow creation failed", Details = ex.Message });
        }
    }
}
