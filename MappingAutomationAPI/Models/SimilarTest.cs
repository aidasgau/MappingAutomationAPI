namespace MappingAutomationAPI.Models
{
    /// <summary>
    /// Represents a candidate test case with its associated similarity score.
    /// </summary>
    public class SimilarTest
    {
        /// <summary>
        /// The module name where the test resides (e.g., "QualityMgmt").
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
        /// The cosine similarity score (0.0–1.0) indicating how closely this test matches the issue description.
        /// </summary>
        public double Similarity { get; set; }
    }
}
