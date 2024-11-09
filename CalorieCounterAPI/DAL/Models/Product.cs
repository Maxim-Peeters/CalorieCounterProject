using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DAL.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        public string Name { get; set; }

        public int CaloriesPer100g { get; set; }  // Calories per 100 grams

        public int AmountInGrams { get; set; }   // Actual amount in grams consumed

        [NotMapped]
        public int TotalCalories => (CaloriesPer100g * AmountInGrams) / 100;

        [MaxLength(50)]
        public string Barcode { get; set; }

        public Category Category { get; set; }  // Enum type used here

        [JsonIgnore]
        public Day? Day { get; set; }
    }
}
