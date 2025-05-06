namespace MappingAutomationAPI.Models
{
    public class WorkflowTask
    {
        public Guid TaskId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Module { get; set; }
        public string Description { get; set; }
        public string TestScenario { get; set; }
        public string Priority { get; set; }
    }
}
