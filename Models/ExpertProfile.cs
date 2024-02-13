using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kheti.Models
{
    public class ExpertProfile
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public KhetiApplicationUser User { get; set; }
        [Required]
        public string FieldOfExpertise { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage ="Years of experience must be a non-negative integer.")]
        public int YearsOfExperience { get; set; }        
        
    }
}
