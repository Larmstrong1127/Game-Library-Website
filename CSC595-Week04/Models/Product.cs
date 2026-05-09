using System.ComponentModel.DataAnnotations;

namespace CSC595_Week04.Models
{
    public class Product
    {
        public int Id { get; set; }

        [UniqueProductName]
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [Range(0.01, 9999.99)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(30)]
        public string Genre { get; set; } = "Other";

        [Display(Name = "Product photo")]
        public byte[]? ImageDataForProduct { get; set; }

        // Predefined genre options used in Add/Edit dropdowns
        public static readonly string[] Genres = new[]
        {
            "Action", "Adventure", "Fighting", "Horror",
            "Platform", "Puzzle", "Racing", "RPG",
            "Shooter", "Simulation", "Sports", "Strategy", "Other"
        };
    }
}
