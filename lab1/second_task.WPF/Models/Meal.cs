using System.ComponentModel;

namespace second_task.WPF.Models
{
    public class Meal : IDataErrorInfo
    {
        public int MealId { get; set; }
        public string MealName { get; set; } = string.Empty;
        public int CuisineId { get; set; }
        public Cuisine? Cuisine { get; set; } // Навігаційна властивість
        public int DietId { get; set; }
        public Diet? Diet { get; set; } // Навігаційна властивість
        public string MealType { get; set; } = string.Empty;
        public int Calories { get; set; }
        public double ProteinG { get; set; }
        public double CarbsG { get; set; }
        public double FatG { get; set; }
        public double Rating { get; set; }
        public bool IsHealthy { get; set; }

        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                string? result = null;
                switch (columnName)
                {
                    case nameof(Calories):
                        if (Calories < 0) result = "Calories cannot be negative.";
                        break;
                    case nameof(ProteinG):
                        if (ProteinG < 0) result = "Protein cannot be negative.";
                        break;
                    case nameof(CarbsG):
                        if (CarbsG < 0) result = "Carbs cannot be negative.";
                        break;
                    case nameof(FatG):
                        if (FatG < 0) result = "Fat cannot be negative.";
                        break;
                    case nameof(Rating):
                        if (Rating < 0 || Rating > 5) result = "Rating must be between 0 and 5.";
                        break;
                    case nameof(MealName):
                        if (string.IsNullOrWhiteSpace(MealName)) result = "Meal name is required.";
                        break;
                }
                return result ?? string.Empty;
            }
        }
    }
}
