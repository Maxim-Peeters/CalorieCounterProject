using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CalorieCounter.Models
{
    public class Day : ObservableObject
    {
        private int dayId;
        private DateTime date;
        private List<Product> products;
        private int calories;

        public int Id
        {
            get => dayId;
            set => SetProperty(ref dayId, value);
        }
        public DateTime Date
        {
            get => date;
            set => SetProperty(ref date, value);
        }
        public List<Product> Products
        {
            get => products;
            set => SetProperty(ref products, value);
        }
        public int Calories
        {
            get => calories;
            set => SetProperty(ref calories, value);
        }

        public Day(int dayId, DateTime date, List<Product> products, int calories)
        {
            Id = dayId;
            Date = date;
            Products = products;
            Calories = calories;
        }
        public Day(){}
    }
}
