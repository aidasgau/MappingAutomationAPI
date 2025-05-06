// --- AstVectorController ---
using MappingAutomationAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/ast-vectors")]
public class AstVectorController : ControllerBase
{
    private readonly OpenAIService _openAIService;
    private readonly VectorDbService _vectorDbService;
    private readonly ILogger<AstVectorController> _logger;
    private readonly string _testDirectory;
    private readonly string _moduleName;

    public AstVectorController(
        OpenAIService openAIService,
        VectorDbService vectorDbService,
        IConfiguration config,
        ILogger<AstVectorController> logger)
    {
        _openAIService = openAIService;
        _vectorDbService = vectorDbService;
        _logger = logger;
        _testDirectory = config["TestSettings:TestDirectory"];

        // Assume TestDirectory ends in "<Module>/Tests"
        var moduleDir = Directory.GetParent(_testDirectory);
        _moduleName = moduleDir?.Name ?? string.Empty;
    }

    [HttpPost("update-ast-vec-db")]
    public async Task<IActionResult> UpdateAstVectorDb()
    {
        if (string.IsNullOrWhiteSpace(_testDirectory) || !Directory.Exists(_testDirectory))
        {
            _logger.LogError("Invalid test directory: {Dir}", _testDirectory);
            return BadRequest(new { Error = "Invalid test directory configuration" });
        }

        try
        {
            var files = Directory.GetFiles(_testDirectory, "*.cs", SearchOption.AllDirectories);
            var updated = 0;

            foreach (var path in files)
            {
                var relativePath = Path.GetRelativePath(_testDirectory, path)
                                       .Replace(Path.DirectorySeparatorChar, '/');
                var parts = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                    continue;

                var app = parts[0];                    
                var fileName = parts.Last();              
                var testName = Path.GetFileNameWithoutExtension(fileName);

                if (await _vectorDbService.ExistsAsync(_moduleName, app, testName))
                    continue;

                var code = await System.IO.File.ReadAllTextAsync(path);
                var clientRes = await _openAIService.GenerateTestDescriptionRaw(code);
                var description = clientRes.Content
                                   .FirstOrDefault()?.Text
                               ?? string.Empty;

                var embedding = await _openAIService.GenerateEmbeddingAsync(description);

                await _vectorDbService.UpsertTestVectorAsync(
                    _moduleName,
                    app,
                    testName,
                    relativePath,
                    description,
                    embedding
                );

                updated++;
            }

            return Ok(new { TotalFiles = files.Length, NewVectorsAdded = updated });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AST vector DB");
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }
}