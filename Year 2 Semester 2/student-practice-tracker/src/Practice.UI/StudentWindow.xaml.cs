using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using Practice.Data.Entities;
using Practice.Services.Interfaces;

namespace Practice.Windows
{
    public partial class StudentWindow : Window
    {
        private readonly User _currentUser;
        private readonly IStudentService _studentService;

        private Student _currentStudent;
        private Course _selectedCourse;
        private InternshipAssignment _currentAssignment;

        private ObservableCollection<Attachment> _tempAttachments = new ObservableCollection<Attachment>();
        private List<InternshipTopic> _allCachedTopics = new List<InternshipTopic>();
        private List<string> _tempFilePaths = new List<string>();

        public StudentWindow(User user, IStudentService studentService)
        {
            InitializeComponent();
            _currentUser = user;
            _studentService = studentService;

            ListAttachments.ItemsSource = _tempAttachments;
            InitStudent();
        }

        private async void InitStudent()
        {
            try
            {
                _currentStudent = await _studentService.GetStudentProfileAsync(_currentUser.UserId);
                if (_currentStudent == null)
                {
                    MessageBox.Show("Помилка: Профіль студента не знайдено.");
                    Close();
                    return;
                }

                TxtStudentName.Text = $"{_currentStudent.User.LastName} {_currentStudent.User.FirstName} ({_currentStudent.StudentGroup.GroupCode})";
                LoadCourses();
            }
            catch (Exception ex) { MessageBox.Show("Помилка ініціалізації: " + ex.Message); }
        }

        private async void LoadCourses()
        {
            var courses = await _studentService.GetEnrolledCoursesAsync(_currentStudent.StudentId);
            CmbCurrentCourse.ItemsSource = courses;
            if (courses.Any()) CmbCurrentCourse.SelectedIndex = 0;
        }

        private void CmbCurrentCourse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentAssignment = null;
            _selectedCourse = CmbCurrentCourse.SelectedItem as Course;
            RefreshAllData();
        }

        private async void RefreshAllData()
        {
            if (_selectedCourse == null) return;
            try
            {
                _currentAssignment = await _studentService.GetAssignmentAsync(_currentStudent.StudentId, _selectedCourse.CourseId);
                UpdateUIState();
            }
            catch (Exception ex) { MessageBox.Show("Помилка оновлення даних: " + ex.Message); }
        }

        private void UpdateUIState()
        {
            _tempAttachments.Clear();
            TxtFeedback.Text = "Відгук відсутній";

            var gray = (Brush)new BrushConverter().ConvertFrom("#9E9E9E");
            var green = (Brush)new BrushConverter().ConvertFrom("#4CAF50");
            var orange = (Brush)new BrushConverter().ConvertFrom("#FF9800");
            var red = (Brush)new BrushConverter().ConvertFrom("#F44336");

            if (_currentAssignment == null)
            {
                TabReport.IsEnabled = false;
                TabResults.IsEnabled = false;
                GridTopics.Visibility = Visibility.Visible;
                PanelFilters.Visibility = Visibility.Visible;
                PanelTopicAlreadySelected.Visibility = Visibility.Collapsed;
                LoadTopicsForCourse();
            }
            else
            {
                TabReport.IsEnabled = true;
                TabResults.IsEnabled = true;
                GridTopics.Visibility = Visibility.Collapsed;
                PanelFilters.Visibility = Visibility.Collapsed;
                PanelTopicAlreadySelected.Visibility = Visibility.Visible;

                TxtCurrentTopicTitle.Text = _currentAssignment.InternshipTopic?.Title ?? "Тема не вказана";
                TxtInfoSupervisor.Text = _currentAssignment.Supervisor != null
                    ? $"{_currentAssignment.Supervisor.User.LastName} {_currentAssignment.Supervisor.User.FirstName}"
                    : "Призначається...";

                var lastReport = _currentAssignment.Reports?.OrderByDescending(r => r.SubmissionDate).FirstOrDefault();
                int reportStatusId = lastReport?.StatusId ?? 0;

                if (_currentAssignment.StatusId == 3) // Якщо в таблиці Assignment статус "Завершено"
                {
                    TxtInfoStatus.Text = "✅ ЗАРАХОВАНО";
                    StatusBadge.Background = green;
                    SetEditMode(false); // Блокуємо
                }
                else if (reportStatusId == 2) // Якщо останній звіт має статус "Повернуто"
                {
                    TxtInfoStatus.Text = "⚠️ ПОВЕРНУТО";
                    StatusBadge.Background = red;
                    SetEditMode(true); // Дозволяємо редагувати для повторної здачі
                }
                else if (reportStatusId == 1) // Якщо останній звіт "На перевірці"
                {
                    TxtInfoStatus.Text = "⏳ НА ПЕРЕВІРЦІ";
                    StatusBadge.Background = orange;
                    SetEditMode(false); // Чекаємо
                }
                else
                {
                    TxtInfoStatus.Text = "📝 В ПРОЦЕСІ";
                    StatusBadge.Background = gray;
                    SetEditMode(true);
                }

                if (lastReport != null)
                {
                    if (string.IsNullOrEmpty(TxtReportComment.Text))
                        TxtReportComment.Text = lastReport.StudentComment;

                    TxtFeedback.Text = !string.IsNullOrEmpty(lastReport.SupervisorFeedback)
                        ? lastReport.SupervisorFeedback
                        : "Очікуйте перевірки.";
                }

                TxtFinalGrade.Text = _currentAssignment.FinalGrade.HasValue
                    ? _currentAssignment.FinalGrade.ToString()
                    : "-";

                TxtCompanyGrade.Text = _currentAssignment.CompanyGrade.HasValue
                    ? _currentAssignment.CompanyGrade.ToString()
                    : "-";

                TxtCompanyFeedback.Text = !string.IsNullOrEmpty(_currentAssignment.CompanyFeedback)
                    ? _currentAssignment.CompanyFeedback
                    : "Відгук від організації відсутній";

                if (_currentAssignment.FinalGrade.HasValue)
                {
                    TxtFinalStatusText.Text = "ОЦІНЕНО";
                    TxtFinalStatusText.Foreground = green;
                }
                else
                {
                    TxtFinalStatusText.Text = "Не оцінено";
                    TxtFinalStatusText.Foreground = gray;
                }

                LoadAttachments(_currentAssignment.AssignmentId);
                LoadFormattedHistory();
            }
        }

        private void SetEditMode(bool canEdit)
        {
            TxtReportComment.IsReadOnly = !canEdit;
            TxtReportLink.IsReadOnly = !canEdit;
            BtnSubmitReport.IsEnabled = canEdit;
            BtnSubmitReport.Opacity = canEdit ? 1.0 : 0.6;
        }

        private void LoadFormattedHistory()
        {
            if (_currentAssignment?.Reports == null) return;

            var historyItems = new List<dynamic>();

            foreach (var r in _currentAssignment.Reports)
            {
                // 1. Подія: Студент здав роботу
                historyItems.Add(new
                {
                    TimeStamp = r.SubmissionDate,
                    Action = "📤 Ви надіслали звіт",
                    Details = string.IsNullOrWhiteSpace(r.StudentComment) ? "Без коментаря" : r.StudentComment
                });

                // 2. Подія: Викладач перевірив (Якщо є дата перевірки або коментар)
                if (r.ReviewDate.HasValue || !string.IsNullOrEmpty(r.SupervisorFeedback))
                {
                    string status;
                    string details = r.SupervisorFeedback ?? "Коментар відсутній";

                    if (r.StatusId == 2)
                    {
                        status = "⚠️ ПОВЕРНУТО НА ДООПРАЦЮВАННЯ";
                    }
                    else if (r.StatusId == 3)
                    {
                        status = "✅ ЗАРАХОВАНО / ПРИЙНЯТО";
                    }
                    else
                    {
                        // Якщо статус "На перевірці", але є коментар — пишемо "Новий коментар"
                        status = "💬 КОМЕНТАР ВИКЛАДАЧА";
                    }

                    historyItems.Add(new
                    {
                        TimeStamp = r.ReviewDate ?? r.SubmissionDate.AddMinutes(5), // Якщо дати немає, беремо приблизну
                        Action = status,
                        Details = details
                    });
                }
            }

            ListHistory.ItemsSource = historyItems.OrderByDescending(x => x.TimeStamp).ToList();
        }

        private async void LoadTopicsForCourse()
        {
            if (_selectedCourse == null) return;
            _allCachedTopics = await _studentService.GetAvailableTopicsAsync(_selectedCourse.DisciplineId, null);

            var orgs = _allCachedTopics.Select(t => t.Organization).Where(o => o != null)
                                       .DistinctBy(o => o.OrganizationId).ToList();
            CmbFilterOrg.ItemsSource = orgs;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allCachedTopics.AsEnumerable();
            if (CmbFilterOrg.SelectedValue is int orgId)
                filtered = filtered.Where(t => t.OrganizationId == orgId);

            GridTopics.ItemsSource = filtered.ToList();
        }

        private void CmbFilterOrg_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private void BtnResetFilter_Click(object sender, RoutedEventArgs e) { CmbFilterOrg.SelectedIndex = -1; ApplyFilters(); }

        private async void BtnSelectTopic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int topicId)
            {
                var res = MessageBox.Show("Ви впевнені, що хочете обрати цю тему?", "Підтвердження", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _studentService.SelectTopicAsync(_currentStudent.StudentId, topicId, _selectedCourse.CourseId);
                        RefreshAllData();
                        MessageBox.Show("Тему успішно обрано!");
                    }
                    catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
                }
            }
        }

        private void BtnGoToReport_Click(object sender, RoutedEventArgs e) => MainTabControl.SelectedIndex = 1;



        private async void BtnSubmitReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _studentService.SubmitAssignmentAsync(
                    _currentAssignment.AssignmentId,
                    TxtReportComment.Text,
                    _tempFilePaths
                );

                MessageBox.Show("Роботу успішно здано!");

                _tempFilePaths.Clear();

                RefreshAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка: " + ex.Message);
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string path)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося відкрити файл: {ex.Message}");
                }
            }
        }

        private async void BtnDeleteFile_Click(object sender, RoutedEventArgs e)
        {

            if (_currentAssignment.StatusId == 2 || _currentAssignment.StatusId == 4)
            {
                MessageBox.Show("Ви не можете видаляти файли, поки робота на перевірці або вже оцінена.");
                return;
            }

            if (sender is Button btn && btn.Tag is int attachId)
            {
                if (MessageBox.Show("Видалити цей файл?", "Підтвердження", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _studentService.DeleteAttachmentAsync(attachId);

                        await LoadAttachments(_currentAssignment.AssignmentId);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Помилка видалення: " + ex.Message);
                    }
                }
            }
        }

        private async Task LoadAttachments(int assignmentId)
        {
            try
            {
                var files = await _studentService.GetAttachmentsByAssignmentIdAsync(assignmentId);
                ListAttachments.ItemsSource = files;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не вдалося завантажити файли: " + ex.Message);
            }
        }
        private void BtnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();

            if (dlg.ShowDialog() == true)
            {
                string path = dlg.FileName;

                _tempFilePaths.Add(path);

                var currentList = ListAttachments.ItemsSource as List<Attachment>;
                if (currentList == null) currentList = new List<Attachment>();

                currentList.Add(new Attachment
                {
                    AttachmentId = 0, 
                    FileName = System.IO.Path.GetFileName(path),
                    FilePath = path
                });

                ListAttachments.ItemsSource = null;
                ListAttachments.ItemsSource = currentList;
            }
        }
    }
}