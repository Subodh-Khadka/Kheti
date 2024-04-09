using System.ComponentModel.DataAnnotations;

namespace Kheti.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}