using Microsoft.EntityFrameworkCore;
using Pgvector;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MappingAutomationAPI.Models
{
    public class TestVectorEntity
    {
        public Guid Id { get; set; }
        public string Module { get; set; }
        public string App { get; set; }
        public string TestName { get; set; }
        public string RelativePath { get; set; } 
        public string Description { get; set; }
        public Vector Embedding { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [NotMapped]
        public decimal SimilarityScore { get; set; }
    }
}
