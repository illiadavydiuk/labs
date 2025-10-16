using System.Windows;
using second_task.WPF.Models;

namespace second_task.WPF.Views
{
    public partial class MealEditWindow : Window
    {
        public Meal? Result { get; private set; }
        private readonly Meal? _originalMeal;

        public MealEditWindow(Meal? meal = null)
        {
            InitializeComponent();
            _originalMeal = meal;
            InitializeComboBoxes();
            
            if (meal != null)
            {
                Title = "Edit Meal";
                LoadMealData(meal);
            }
            else
            {
                Title = "Add New Meal";
                SetDefaultValues();
            }
            
            MealNameTextBox.Focus();
        }

        private void InitializeComboBoxes()
        {
            // Cuisine options
            CuisineComboBox.Items.Add("Mediterranean");
            CuisineComboBox.Items.Add("Asian");
            CuisineComboBox.Items.Add("Italian");
            CuisineComboBox.Items.Add("American");
            CuisineComboBox.Items.Add("Japanese");
            CuisineComboBox.Items.Add("Mexican");
            CuisineComboBox.Items.Add("Health");
            CuisineComboBox.Items.Add("Dessert");
            
            // Diet options
            DietComboBox.Items.Add("Regular");
            DietComboBox.Items.Add("Vegetarian");
            DietComboBox.Items.Add("Vegan");
            
            // Meal Type options
            MealTypeComboBox.Items.Add("Breakfast");
            MealTypeComboBox.Items.Add("Lunch");
            MealTypeComboBox.Items.Add("Dinner");
            MealTypeComboBox.Items.Add("Dessert");
        }

        private void LoadMealData(Meal meal)
        {
            MealNameTextBox.Text = meal.MealName;
            CuisineComboBox.Text = meal.Cuisine?.Name ?? "";
            DietComboBox.Text = meal.Diet?.Name ?? "";
            MealTypeComboBox.Text = meal.MealType;
            CaloriesTextBox.Text = meal.Calories.ToString();
            ProteinTextBox.Text = meal.ProteinG.ToString("F1");
            CarbsTextBox.Text = meal.CarbsG.ToString("F1");
            FatTextBox.Text = meal.FatG.ToString("F1");
            RatingTextBox.Text = meal.Rating.ToString("F1");
            IsHealthyCheckBox.IsChecked = meal.IsHealthy;
        }

        private void SetDefaultValues()
        {
            MealNameTextBox.Text = "";
            CuisineComboBox.Text = "Regular";
            DietComboBox.Text = "Regular";
            MealTypeComboBox.Text = "Lunch";
            CaloriesTextBox.Text = "0";
            ProteinTextBox.Text = "0.0";
            CarbsTextBox.Text = "0.0";
            FatTextBox.Text = "0.0";
            RatingTextBox.Text = "0.0";
            IsHealthyCheckBox.IsChecked = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                Result = CreateMealFromInput();
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            ErrorTextBlock.Text = "";
            var errors = new List<string>();

            // Validate meal name
            if (string.IsNullOrWhiteSpace(MealNameTextBox.Text))
                errors.Add("Meal name is required.");

            // Validate cuisine
            if (string.IsNullOrWhiteSpace(CuisineComboBox.Text))
                errors.Add("Cuisine is required.");

            // Validate diet
            if (string.IsNullOrWhiteSpace(DietComboBox.Text))
                errors.Add("Diet type is required.");

            // Validate meal type
            if (string.IsNullOrWhiteSpace(MealTypeComboBox.Text))
                errors.Add("Meal type is required.");

            // Validate numeric fields
            if (!int.TryParse(CaloriesTextBox.Text, out var calories) || calories < 0)
                errors.Add("Calories must be a valid non-negative number.");

            if (!double.TryParse(ProteinTextBox.Text, out var protein) || protein < 0)
                errors.Add("Protein must be a valid non-negative number.");

            if (!double.TryParse(CarbsTextBox.Text, out var carbs) || carbs < 0)
                errors.Add("Carbs must be a valid non-negative number.");

            if (!double.TryParse(FatTextBox.Text, out var fat) || fat < 0)
                errors.Add("Fat must be a valid non-negative number.");

            if (!double.TryParse(RatingTextBox.Text, out var rating) || rating < 0 || rating > 5)
                errors.Add("Rating must be a number between 0 and 5.");

            if (errors.Any())
            {
                ErrorTextBlock.Text = string.Join("\n", errors);
                return false;
            }

            return true;
        }

        private Meal CreateMealFromInput()
        {
            return new Meal
            {
                MealId = _originalMeal?.MealId ?? 0, // Will be set by caller for new meals
                MealName = MealNameTextBox.Text.Trim(),
                Cuisine = new Cuisine { Name = CuisineComboBox.Text.Trim() },
                Diet = new Diet { Name = DietComboBox.Text.Trim() },
                MealType = MealTypeComboBox.Text.Trim(),
                Calories = int.Parse(CaloriesTextBox.Text),
                ProteinG = double.Parse(ProteinTextBox.Text),
                CarbsG = double.Parse(CarbsTextBox.Text),
                FatG = double.Parse(FatTextBox.Text),
                Rating = double.Parse(RatingTextBox.Text),
                IsHealthy = IsHealthyCheckBox.IsChecked == true
            };
        }
    }
}
