using second_task.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace second_task.WPF.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainViewModel();
            DataContext = viewModel;
            
            // Subscribe to RecentFiles changes
            viewModel.RecentFiles.CollectionChanged += (s, e) => UpdateRecentFilesMenu(viewModel);
            UpdateRecentFilesMenu(viewModel);
        }

        private void UpdateRecentFilesMenu(MainViewModel viewModel)
        {
            RecentFilesMenu.Items.Clear();
            
            if (!viewModel.RecentFiles.Any())
            {
                var noFilesItem = new MenuItem { Header = "No recent files", IsEnabled = false };
                RecentFilesMenu.Items.Add(noFilesItem);
                return;
            }

            foreach (var filePath in viewModel.RecentFiles)
            {
                var menuItem = new MenuItem 
                { 
                    Header = System.IO.Path.GetFileName(filePath),
                    ToolTip = filePath
                };
                menuItem.Click += (s, e) => viewModel.OpenRecentFileCommand.Execute(filePath);
                RecentFilesMenu.Items.Add(menuItem);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("dataset_program\n\nVersion 2.0\n© Chicago + Claude 4.0", 
                          "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void DataGrid_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
        {
            var dataGrid = sender as System.Windows.Controls.DataGrid;
            var column = e.Column;
            
            // Check if column is already sorted in ascending order
            if (column.SortDirection == System.ComponentModel.ListSortDirection.Ascending)
            {
                // If Ctrl is pressed, clear sorting
                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || 
                    System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl))
                {
                    e.Handled = true;
                    column.SortDirection = null;
                    
                    // Clear all sorting
                    if (dataGrid != null)
                    {
                        dataGrid.Items.SortDescriptions.Clear();
                    }
                    
                    // Reset to default order
                    var viewModel = DataContext as MainViewModel;
                    viewModel?.ClearSortingCommand.Execute(null);
                    
                    return;
                }
            }
        }
    }
}
