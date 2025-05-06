using MappingAutomationAPI.Data;
using MappingAutomationAPI.Models;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MappingAutomationAPI.Services
{
    public class VectorDbService
    {
        private readonly VectorDbContext _dbContext;

        public DbSet<SimilarTest> SimilarTests => _dbContext.SimilarTests;

        public VectorDbService(VectorDbContext dbContext)
            => _dbContext = dbContext;

        public Task<bool> ExistsAsync(string module, string app, string testName)
            => _dbContext.TestVectors
                         .AnyAsync(t => t.Module == module
                                     && t.App == app
                                     && t.TestName == testName);

        public async Task UpsertTestVectorAsync(
            string module,
            string app,
            string testName,
            string relativePath,
            string description,
            float[] embedding)
        {
            var entity = await _dbContext.TestVectors
                .FirstOrDefaultAsync(t => t.Module == module
                                       && t.App == app
                                       && t.TestName == testName);

            if (entity != null)
            {
                entity.Description = description;
                entity.RelativePath = relativePath;
                entity.Embedding = new Vector(embedding);
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _dbContext.TestVectors.Add(new TestVectorEntity
                {
                    Id = Guid.NewGuid(),
                    Module = module,
                    App = app,
                    TestName = testName,
                    RelativePath = relativePath,
                    Description = description,
                    Embedding = new Vector(embedding),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _dbContext.SaveChangesAsync();
        }

        public Task<List<SimilarTest>> FindSimilarTestsAsync(float[] issueVector, int topK = 5)
        {
            var literal = "[" + string.Join(",", issueVector) + "]";

            var sql = $@"
                SELECT
                  ""Module"",
                  ""App"",
                  ""TestName"",
                  1 - (""Embedding"" <=> '{literal}') AS ""Similarity""
                FROM public.""TestVectors""
                ORDER BY ""Embedding"" <=> '{literal}'
                LIMIT {topK};";

            return _dbContext.SimilarTests
                             .FromSqlRaw(sql)
                             .ToListAsync();
        }
    }
}
