namespace MappingAutomationAPI.Models
{
    public class NewASTRequest
    {
        /// <summary>
        /// The natural‑language description of the bug or feature request.
        /// </summary>
        public string IssueDescription { get; set; }

        /// <summary>
        /// The module where this test should live (e.g. “Login”, “SettingsPage”).
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// How “novel” this request is compared to existing tests (0.0–1.0).
        /// Used by the workflow controller to set priority.
        /// </summary>
        public float SimilarityScore { get; set; }

        /// <summary>
        /// (Optional) The application name or context, if you have more than one app.
        /// </summary>
        public string AppName { get; set; }
    }
}
