using second_task.Data.Dtos;
using second_task.WPF.ViewModels;
using System.Windows;

namespace second_task.WPF.Views
{
    public partial class FilePreviewWindow : Window
    {
        public bool LoadConfirmed { get; private set; }
        
        public FilePreviewWindow(string filePath, IEnumerable<MealRawDto> previewData)
        {
            InitializeComponent();
            var viewModel = new FilePreviewViewModel();
            viewModel.UpdatePreview(filePath, previewData);
            DataContext = viewModel;
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            LoadConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            LoadConfirmed = false;
            DialogResult = false;
            Close();
        }
    }
}
