using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kheti.Models
{
    public class Report
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string IssueType { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string SeverityLevel { get; set; }
        [Required]
        public string ReportImageUrl { get; set; }
        public string? AdditionalInformation { get; set; }
        public string? ContactEmail { get; set; }
        public string? ReportStatus { get; set; }
        public DateTime CreatedAt{ get; set; }
        public String UserId { get; set; }
        [ForeignKey("UserId")]
        public KhetiApplicationUser User;


    }
}
