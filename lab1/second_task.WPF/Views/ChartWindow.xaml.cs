using second_task.WPF.Models;
using second_task.WPF.ViewModels;
using System.Windows;

namespace second_task.WPF.Views
{
    public partial class ChartWindow : Window
    {
        public ChartWindow(IEnumerable<Meal> meals)
        {
            InitializeComponent();
            var viewModel = new ChartViewModel();
            viewModel.UpdateData(meals);
            DataContext = viewModel;
        }
    }
}
