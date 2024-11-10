using System;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace CalorieCounter.Models
{
    public class Product : ObservableObject
    {
        [JsonPropertyName("productId")]  // Ensure the mapping from API to model property
        private int productId;
        private string name;
        private int caloriesPer100g;
        private int amountInGrams;
        private string barcode;
        private Category category;  // Assuming Category is an Enum
        private Day? day;  // Link to Day, nullable in case it's not set

        public int ProductId
        {
            get => productId;
            set => SetProperty(ref productId, value);
        }

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public int CaloriesPer100g
        {
            get => caloriesPer100g;
            set => SetProperty(ref caloriesPer100g, value);
        }

        public int AmountInGrams
        {
            get => amountInGrams;
            set => SetProperty(ref amountInGrams, value);
        }

        [NotMapped]  // Not mapped to the database (if using Entity Framework)
        public int TotalCalories => (CaloriesPer100g * AmountInGrams) / 100;

        public string Barcode
        {
            get => barcode;
            set => SetProperty(ref barcode, value);
        }

        public Category Category
        {
            get => category;
            set => SetProperty(ref category, value);
        }

        public Day? Day
        {
            get => day;
            set => SetProperty(ref day, value);
        }

        public Product() { }

        public Product(int productId, string name, int caloriesPer100g, int amountInGrams, string barcode, Category category, Day? day = null)
        {
            ProductId = productId;
            Name = name;
            CaloriesPer100g = caloriesPer100g;
            AmountInGrams = amountInGrams;
            Barcode = barcode;
            Category = category;
            Day = day;
        }
    }
}
