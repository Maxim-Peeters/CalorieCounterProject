using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class Day
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DayId { get; set; }

        private DateTime _date;
        public DateTime Date
        {
            get => _date.Date; // Only return the date part (no time)
            set => _date = value.Date; // Store only the date, with time set to 00:00:00
        }

        public ICollection<Product>? Products { get; set; }

        public int? TotalCalories => Products.Sum(p => p.TotalCalories);
    }
}
