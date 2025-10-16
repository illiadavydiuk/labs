using CsvHelper.Configuration.Attributes;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace second_task.Data.Dtos
{
    public class MealRawDto
    {
        [Name("meal_id")]
        [JsonPropertyName("meal_id")]
        [XmlElement("meal_id")]
        public int MealId { get; set; }

        [Name("meal_name")]
        [JsonPropertyName("meal_name")]
        [XmlElement("meal_name")]
        public string MealName { get; set; } = string.Empty;

        [Name("cuisine")]
        [JsonPropertyName("cuisine")]
        [XmlElement("cuisine")]
        public string Cuisine { get; set; } = string.Empty;

        [Name("meal_type")]
        [JsonPropertyName("meal_type")]
        [XmlElement("meal_type")]
        public string MealType { get; set; } = string.Empty;

        [Name("diet_type")]
        [JsonPropertyName("diet_type")]
        [XmlElement("diet_type")]
        public string DietType { get; set; } = string.Empty;

        [Name("calories")]
        [JsonPropertyName("calories")]
        [XmlElement("calories")]
        public int Calories { get; set; }

        [Name("protein_g")]
        [JsonPropertyName("protein_g")]
        [XmlElement("protein_g")]
        public double ProteinG { get; set; }

        [Name("carbs_g")]
        [JsonPropertyName("carbs_g")]
        [XmlElement("carbs_g")]
        public double CarbsG { get; set; }

        [Name("fat_g")]
        [JsonPropertyName("fat_g")]
        [XmlElement("fat_g")]
        public double FatG { get; set; }

        [Name("rating")]
        [JsonPropertyName("rating")]
        [XmlElement("rating")]
        public double Rating { get; set; }

        [Name("is_healthy")]
        [JsonPropertyName("is_healthy")]
        [XmlElement("is_healthy")]
        public bool IsHealthy { get; set; }
    }
}
