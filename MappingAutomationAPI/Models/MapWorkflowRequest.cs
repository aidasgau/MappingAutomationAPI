namespace MappingAutomationAPI.Models
{
    public class MapWorkflowRequest
    {
        /// <summary>
        /// Either "BUG" or "FR".
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The short title of the BUG/FR.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The detailed description used for mapping.
        /// </summary>
        public string Description { get; set; }
    }
}
