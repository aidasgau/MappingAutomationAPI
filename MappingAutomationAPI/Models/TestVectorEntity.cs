using Microsoft.EntityFrameworkCore;
using Pgvector;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MappingAutomationAPI.Models
{
    /// <summary>
    /// Entity representing a test's description and its embedding vector stored in the database.
    /// </summary>
    public class TestVectorEntity
    {
        /// <summary>
        /// Primary key for the test vector record.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The module to which this test belongs (e.g., "QualityMgmt").
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// The application or sub-component name containing the test (e.g., "ITPRegister").
        /// </summary>
        public string App { get; set; }

        /// <summary>
        /// The name of the test case (without the .cs extension).
        /// </summary>
        public string TestName { get; set; }

        /// <summary>
        /// Relative file path under the tests directory for locating the source code.
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// The natural-language description generated for the test scenario.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The embedding vector produced by OpenAI for the test description.
        /// Stored as a 1536-dimensional pgvector field.
        /// </summary>
        public Vector Embedding { get; set; }

        /// <summary>
        /// Timestamp indicating when this record was created in the database.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp indicating the last time this record was updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Computed similarity score (0.0–1.0). Not mapped to the database.
        /// </summary>
        [NotMapped]
        public decimal SimilarityScore { get; set; }
    }
}
