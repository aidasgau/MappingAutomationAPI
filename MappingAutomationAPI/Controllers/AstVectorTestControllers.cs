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
public class AstVectorTestController : ControllerBase
{
    private readonly OpenAIService _openAIService;
    private readonly VectorDbService _vectorDbService;
    private readonly ILogger _logger;
    private readonly string _testDirectory;
    private readonly string _moduleName;

    public AstVectorTestController(
        OpenAIService openAIService,
        VectorDbService vectorDbService,
        IConfiguration config,
        ILogger<AstVectorTestController> logger)
    {
        _openAIService = openAIService;
        _vectorDbService = vectorDbService;
        _logger = logger;
        _testDirectory = config["TestSettings:TestDirectory"];

        // Peel off the module name from the path:
        var moduleDir = Directory.GetParent(_testDirectory);
        _moduleName = moduleDir?.Name ?? "UnknownModule";
    }

    /// <summary>
    /// TAKE the VERY FIRST .cs under _testDirectory, generate a description+embedding and upsert it.
    /// </summary>
    [HttpPost("test-first-file")]
    public async Task<IActionResult> TestFirstFile()
    {
        // locate first test file
        var firstPath = Directory
            .GetFiles(_testDirectory, "*.cs", SearchOption.AllDirectories)
            .FirstOrDefault();

        if (firstPath == null)
            return NotFound(new { Error = "No .cs files found under TestDirectory." });

        var relativePath = Path
            .GetRelativePath(_testDirectory, firstPath)
            .Replace(Path.DirectorySeparatorChar, '/');
        var parts = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var app = parts.Length > 1 ? parts[0] : "UnknownApp";
        var fileName = parts.Last();
        var testName = Path.GetFileNameWithoutExtension(fileName);

        var code = await System.IO.File.ReadAllTextAsync(firstPath);

        var clientRes = await _openAIService.GenerateTestDescriptionRaw(code);
        var description = clientRes.Content.FirstOrDefault()?.Text ?? "";

        var embedding = await _openAIService.GenerateEmbeddingAsync(description);

        bool alreadyExisted = await _vectorDbService.ExistsAsync(_moduleName, app, testName);
        await _vectorDbService.UpsertTestVectorAsync(
            _moduleName, app, testName, relativePath, description, embedding
        );

        return Ok(new
        {
            File = relativePath,
            AlreadyInDb = alreadyExisted,
            description = description,
            EmbeddingDimension = embedding.Length
        });
    }
}
