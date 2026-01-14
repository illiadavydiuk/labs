using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TaskManager.Data.Entities;
using TaskManager.Services;
using Task = TaskManager.Data.Entities.Task;

namespace TaskManager.UI
{
    public partial class MainWindow : Window
    {
        private readonly ProjectService _pService;
        private readonly DeveloperService _dService;
        private readonly TaskService _tService;

        // Зберігаємо ID замість об'єктів, щоб уникнути помилок stale data
        private int? _selectedDevId;
        private int? _selectedProjId;
        private int? _selectedTaskId;

        public MainWindow(ProjectService p, DeveloperService d, TaskService t)
        {
            InitializeComponent();
            _pService = p;
            _dService = d;
            _tService = t;

            RefreshGlobalData();
        }

        private void RefreshGlobalData()
        {
            GridDevs.ItemsSource = _dService.GetAll().ToList();

            var projects = _pService.GetAllProjects().ToList();
            GridProjects.ItemsSource = projects;
            ComboProjectsForTasks.ItemsSource = projects;

            var devs = _dService.GetAll().ToList();
            ComboDevsForProj.ItemsSource = devs;
            ComboDevsForTask.ItemsSource = devs;
        }

        // ======================= TAB 1: DEVELOPERS =======================
        private void GridDevs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ЗАХИСТ ВІД NULL
            if (GridDevs.SelectedItem is Developer dev)
            {
                _selectedDevId = dev.Id;
                TxtDevName.Text = dev.FullName;
                TxtDevEmail.Text = dev.Email;
            }
            else
            {
                _selectedDevId = null;
                TxtDevName.Clear();
                TxtDevEmail.Clear();
            }
        }

        private void BtnAddDev_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtDevName.Text))
            {
                _dService.Add(TxtDevName.Text, TxtDevEmail.Text);
                RefreshGlobalData();
                TxtDevName.Clear(); TxtDevEmail.Clear();
            }
        }

        private void BtnUpdateDev_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDevId.HasValue)
            {
                _dService.Update(_selectedDevId.Value, TxtDevName.Text, TxtDevEmail.Text);
                RefreshGlobalData();
            }
        }

        private void BtnDelDev_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDevId.HasValue)
            {
                _dService.Delete(_selectedDevId.Value);
                RefreshGlobalData();
                TxtDevName.Clear(); TxtDevEmail.Clear();
                _selectedDevId = null;
            }
        }

        // ======================= TAB 2: PROJECTS =======================
        private void GridProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridProjects.SelectedItem is Project p)
            {
                _selectedProjId = p.Id;

                var fullProject = _pService.GetProjectDetails(p.Id);

                TxtProjName.Text = fullProject.Name;
                TxtProjDesc.Text = fullProject.Description;

                ListProjTeam.ItemsSource = fullProject.Developers;
            }
            else
            {
                _selectedProjId = null;
                TxtProjName.Clear();
                TxtProjDesc.Clear();
                ListProjTeam.ItemsSource = null;
            }
        }

        private void UpdateProjectDetailsUI()
        {
            if (_selectedProjId.HasValue)
            {
                var fullProject = _pService.GetProjectDetails(_selectedProjId.Value);
                if (fullProject != null)
                {
                    ListProjTeam.ItemsSource = null; 
                    ListProjTeam.ItemsSource = fullProject.Developers; 
                }
            }
        }

        private void BtnCreateProj_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtProjName.Text))
            {
                _pService.CreateProject(TxtProjName.Text, TxtProjDesc.Text);
                RefreshGlobalData();
            }
        }

        private void BtnUpdateProj_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProjId.HasValue)
            {
                _pService.UpdateProject(_selectedProjId.Value, TxtProjName.Text, TxtProjDesc.Text);
                RefreshGlobalData();
            }
        }

        private void BtnDelProj_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProjId.HasValue)
            {
                _pService.DeleteProject(_selectedProjId.Value);
                RefreshGlobalData(); 
            }
        }

        private void BtnAddToTeam_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProjId.HasValue && ComboDevsForProj.SelectedItem is Developer dev)
            {
                _pService.AddDeveloperToProject(_selectedProjId.Value, dev.Id);
                UpdateProjectDetailsUI();
            }
        }

        private void BtnRemoveFromTeam_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProjId.HasValue && ListProjTeam.SelectedItem is Developer dev)
            {
                _pService.RemoveDeveloperFromProject(_selectedProjId.Value, dev.Id);
                UpdateProjectDetailsUI(); // Оновлюємо тільки список команди
            }
        }

        // ======================= TAB 3: TASKS =======================
        private void ComboProjectsForTasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshTasksList();
        }

        private void RefreshTasksList()
        {
            if (ComboProjectsForTasks.SelectedItem is Project p)
            {

                GridTasks.ItemsSource = _tService.GetTasksForProject(p.Id).ToList();


                var fullProject = _pService.GetProjectDetails(p.Id);

                if (fullProject != null)
                {

                    ComboDevsForTask.ItemsSource = fullProject.Developers;
                }
                else
                {
                    ComboDevsForTask.ItemsSource = null;
                }

                TxtTaskTitle.Clear();
                ListTaskDevs.ItemsSource = null;
                _selectedTaskId = null;
            }
        }

        private void GridTasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = GridTasks.SelectedItem as TaskManager.Data.Entities.Task;

            if (selectedItem != null)
            {
                _selectedTaskId = selectedItem.Id;

                try
                {
                    var fullTask = _tService.GetTaskDetails(_selectedTaskId.Value);

                    if (fullTask != null)
                    {
                        TxtTaskTitle.Text = fullTask.Title;

                        // Оновлюємо список виконавців
                        ListTaskDevs.ItemsSource = null; // Скидаємо старе
                        ListTaskDevs.ItemsSource = fullTask.Developers;
                    }
                }
                catch
                {

                }
            }
            else
            {
                // Якщо виділення знято
                _selectedTaskId = null;
                TxtTaskTitle.Clear();
                ListTaskDevs.ItemsSource = null;
            }
        }

        private void UpdateTaskDetailsUI()
        {
            if (_selectedTaskId.HasValue)
            {
                var fullTask = _tService.GetTaskDetails(_selectedTaskId.Value);
                if (fullTask != null)
                {
                    ListTaskDevs.ItemsSource = null;
                    ListTaskDevs.ItemsSource = fullTask.Developers;
                }
            }
        }

        private void BtnAddTask_Click(object sender, RoutedEventArgs e)
        {
            if (ComboProjectsForTasks.SelectedItem is not Project p)
            {
                MessageBox.Show("Будь ласка, спочатку оберіть проєкт зі списку зліва зверху (Current Context)!",
                                "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Перевірка: чи введено назву завдання?
            if (string.IsNullOrWhiteSpace(TxtTaskTitle.Text))
            {
                MessageBox.Show("Введіть назву завдання!",
                                "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _tService.CreateTask(TxtTaskTitle.Text, p.Id);

                RefreshTasksList();
                TxtTaskTitle.Clear();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Помилка при створенні завдання:\n{ex.Message}\n{ex.InnerException?.Message}",
                                "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelTask_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTaskId.HasValue)
            {
                _tService.DeleteTask(_selectedTaskId.Value);
                RefreshTasksList();
            }
        }

        private void BtnToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTaskId.HasValue)
            {
                var task = _tService.GetTaskDetails(_selectedTaskId.Value);
                _tService.ToggleStatus(task);
                RefreshTasksList();
            }
        }

        private void BtnAssignDev_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTaskId.HasValue && ComboDevsForTask.SelectedItem is Developer dev)
            {
                _tService.AssignDeveloper(_selectedTaskId.Value, dev.Id);
                UpdateTaskDetailsUI(); 
            }
        }

        private void BtnRemoveFromTask_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTaskId.HasValue && ListTaskDevs.SelectedItem is Developer dev)
            {
                _tService.RemoveDeveloperFromTask(_selectedTaskId.Value, dev.Id);
                UpdateTaskDetailsUI();
            }
        }
    }
}