using second_task.WPF.Models;
using second_task.Data.Dtos;
using second_task.Data.Providers;
using System.IO;

namespace second_task.WPF.Services
{
    public class DataService
    {
        private readonly CsvDataProvider _csvProvider = new();
        private readonly JsonDataProvider _jsonProvider = new();
        private readonly XmlDataProvider _xmlProvider = new();
        private readonly XlsxDataProvider _xlsxProvider = new();

        public IEnumerable<MealRawDto> LoadPreviewData(string filePath, object settings)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            IEnumerable<MealRawDto> rawData;
            
            switch (extension)
            {
                case ".csv":
                    rawData = _csvProvider.ReadRawData(filePath, settings);
                    break;
                case ".json":
                    rawData = _jsonProvider.ReadRawData(filePath, settings);
                    break;
                case ".xml":
                    rawData = _xmlProvider.ReadRawData(filePath, settings);
                    break;
                case ".xlsx":
                    rawData = _xlsxProvider.ReadRawData(filePath, settings);
                    break;
                default:
                    throw new NotSupportedException($"File format {extension} is not supported");
            }

            return rawData.Take(100); // Return first 100 rows for preview
        }

        public IEnumerable<Meal> LoadData(string filePath, object settings)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            IEnumerable<MealRawDto> rawData;
            
            switch (extension)
            {
                case ".csv":
                    rawData = _csvProvider.ReadRawData(filePath, settings);
                    break;
                case ".json":
                    rawData = _jsonProvider.ReadRawData(filePath, settings);
                    break;
                case ".xml":
                    rawData = _xmlProvider.ReadRawData(filePath, settings);
                    break;
                case ".xlsx":
                    rawData = _xlsxProvider.ReadRawData(filePath, settings);
                    break;
                default:
                    throw new NotSupportedException($"File format {extension} is not supported");
            }

            return ConvertToMeals(rawData);
        }

        public void SaveData(string filePath, IEnumerable<Meal> meals, object settings)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            var rawData = ConvertToRawData(meals);

            switch (extension)
            {
                case ".csv":
                    _csvProvider.WriteRawData(filePath, rawData, settings);
                    break;
                case ".json":
                    _jsonProvider.WriteRawData(filePath, rawData, settings);
                    break;
                case ".xml":
                    _xmlProvider.WriteRawData(filePath, rawData, settings);
                    break;
                case ".xlsx":
                    _xlsxProvider.WriteRawData(filePath, rawData, settings);
                    break;
                default:
                    throw new NotSupportedException($"File format {extension} is not supported");
            }
        }

        private IEnumerable<Meal> ConvertToMeals(IEnumerable<MealRawDto> rawData)
        {
            var cuisines = new Dictionary<string, Cuisine>();
            var diets = new Dictionary<string, Diet>();
            var meals = new List<Meal>();

            int cuisineIdCounter = 1;
            int dietIdCounter = 1;

            foreach (var raw in rawData)
            {
                var cuisineKey = string.IsNullOrWhiteSpace(raw.Cuisine) ? "Unknown" : raw.Cuisine.Trim();
                var dietKey = string.IsNullOrWhiteSpace(raw.DietType) ? "Unknown" : raw.DietType.Trim();

                if (!cuisines.ContainsKey(cuisineKey))
                {
                    cuisines[cuisineKey] = new Cuisine { Id = cuisineIdCounter++, Name = cuisineKey };
                }

                if (!diets.ContainsKey(dietKey))
                {
                    diets[dietKey] = new Diet { Id = dietIdCounter++, Name = dietKey };
                }

                var cuisine = cuisines[cuisineKey];
                var diet = diets[dietKey];

                var meal = new Meal
                {
                    MealId = raw.MealId,
                    MealName = raw.MealName,
                    CuisineId = cuisine.Id,
                    Cuisine = cuisine,
                    DietId = diet.Id,
                    Diet = diet,
                    MealType = raw.MealType,
                    Calories = raw.Calories,
                    ProteinG = raw.ProteinG,
                    CarbsG = raw.CarbsG,
                    FatG = raw.FatG,
                    Rating = raw.Rating,
                    IsHealthy = raw.IsHealthy
                };
                meals.Add(meal);
            }

            return meals;
        }

        private IEnumerable<MealRawDto> ConvertToRawData(IEnumerable<Meal> meals)
        {
            return meals.Select(m => new MealRawDto
            {
                MealId = m.MealId,
                MealName = m.MealName,
                Cuisine = m.Cuisine?.Name ?? "Unknown",
                DietType = m.Diet?.Name ?? "Unknown",
                MealType = m.MealType,
                Calories = m.Calories,
                ProteinG = m.ProteinG,
                CarbsG = m.CarbsG,
                FatG = m.FatG,
                Rating = m.Rating,
                IsHealthy = m.IsHealthy
            });
        }
    }
}
