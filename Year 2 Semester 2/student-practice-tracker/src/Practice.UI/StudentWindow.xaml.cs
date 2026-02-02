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
            TxtReportComment.Clear();
            TxtReportLink.Clear();
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

                if (lastReport == null)
                {
                    TxtInfoStatus.Text = "Очікується звіт";
                    StatusBadge.Background = gray;
                    SetEditMode(true);
                }
                else
                {
                    TxtReportComment.Text = lastReport.StudentComment;
                    TxtFeedback.Text = lastReport.SupervisorFeedback ?? "Звіт надіслано. Очікуйте на перевірку викладачем.";

                    var link = lastReport.Attachments?.FirstOrDefault(a => a.FileType == "URL");
                    if (link != null) TxtReportLink.Text = link.FilePath;

                    if (lastReport.StatusId == 2) // Повернуто (Червоний)
                    {
                        TxtInfoStatus.Text = "⚠️ ПОВЕРНУТО";
                        StatusBadge.Background = red;
                        SetEditMode(true); // Дозволяємо редагувати для виправлення
                    }
                    else if (lastReport.StatusId == 1) // На перевірці (Помаранчевий)
                    {
                        TxtInfoStatus.Text = "⏳ НА ПЕРЕВІРЦІ";
                        StatusBadge.Background = orange;
                        SetEditMode(false); // Блокуємо, поки не перевірять
                    }
                    else if (lastReport.StatusId == 3) // Прийнято (Зелений)
                    {
                        TxtInfoStatus.Text = "✅ ПРИЙНЯТО";
                        StatusBadge.Background = green;
                        SetEditMode(false);
                    }
                }

                TxtFinalGrade.Text = _currentAssignment.FinalGrade?.ToString() ?? "-";
                TxtCompanyGrade.Text = _currentAssignment.CompanyGrade?.ToString() ?? "Не виставлено";
                TxtCompanyFeedback.Text = _currentAssignment.CompanyFeedback ?? "Відгук від організації відсутній";
                TxtFinalStatusText.Text = _currentAssignment.FinalGrade.HasValue ? "ПРАКТИКУ ЗАВЕРШЕНО" : "В ПРОЦЕСІ";

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
                historyItems.Add(new
                {
                    TimeStamp = r.SubmissionDate,
                    Action = "📤 Ви надіслали звіт",
                    Details = string.IsNullOrWhiteSpace(r.StudentComment) ? "Без коментаря" : r.StudentComment
                });

                if (r.ReviewDate.HasValue)
                {
                    string status = r.StatusId == 2 ? "⚠️ ВИКЛАДАЧ ПОВЕРНУВ РОБОТУ" : "✅ ВИКЛАДАЧ ПРИЙНЯВ ЗВІТ";
                    historyItems.Add(new
                    {
                        TimeStamp = r.ReviewDate.Value,
                        Action = status,
                        Details = r.SupervisorFeedback ?? "Коментар відсутній"
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

        private void BtnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Multiselect = true };
            if (dlg.ShowDialog() == true)
            {
                foreach (string f in dlg.FileNames)
                {
                    _tempAttachments.Add(new Attachment
                    {
                        FileName = System.IO.Path.GetFileName(f),
                        FilePath = f,
                        FileType = "FILE"
                    });
                }
            }
        }

        private async void BtnSubmitReport_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAssignment == null) return;
            try
            {
                await _studentService.SubmitReportAsync(
                    _currentAssignment.AssignmentId,
                    TxtReportComment.Text,
                    TxtReportLink.Text,
                    _tempAttachments.ToList()
                );

                MessageBox.Show("Звіт успішно відправлено на перевірку!");
                _tempAttachments.Clear();
                RefreshAllData();
            }
            catch (Exception ex) { MessageBox.Show("Помилка відправки: " + ex.Message); }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}