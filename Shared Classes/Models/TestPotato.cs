using System.ComponentModel.DataAnnotations;

namespace Shared_Classes.Models
{
    public class TestPotato

    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
