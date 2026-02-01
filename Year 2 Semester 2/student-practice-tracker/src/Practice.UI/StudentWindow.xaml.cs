using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private List<InternshipTopic> _allCachedTopics = new List<InternshipTopic>(); // Кеш тем

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
                if (_currentStudent == null) { MessageBox.Show("Помилка профілю."); Close(); return; }

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

            CmbFilterOrg.ItemsSource = null;
            TxtFeedback.Text = "Завантаження...";

            RefreshAllData();
        }

        private async void RefreshAllData()
        {
            if (_selectedCourse == null) return;

            try
            {
                _currentAssignment = await _studentService.GetAssignmentAsync(_currentStudent.StudentId, _selectedCourse.CourseId);
                UpdateUIState();
                LoadHistory();
            }
            catch (Exception ex) { MessageBox.Show("Помилка оновлення даних: " + ex.Message); }
        }

        private async void LoadHistory()
        {
            if (_currentAssignment != null)
            {
                ListHistory.ItemsSource = await _studentService.GetAssignmentHistoryAsync(_currentAssignment.AssignmentId);
            }
            else
            {
                ListHistory.ItemsSource = null;
            }
        }

        private void UpdateUIState()
        {
            _tempAttachments.Clear();
            TxtReportComment.Clear();
            TxtReportLink.Clear();
            TxtFeedback.Text = "Відгук відсутній";

            var gray = (Brush)new BrushConverter().ConvertFrom("#DDD");
            var green = (Brush)new BrushConverter().ConvertFrom("#4CAF50");
            var blue = (Brush)new BrushConverter().ConvertFrom("#2196F3");
            var red = (Brush)new BrushConverter().ConvertFrom("#F44336");

            if (_currentAssignment == null)
            {
                TabReport.IsEnabled = false;
                TabResults.IsEnabled = false;

                GridTopics.Visibility = Visibility.Visible;
                PanelFilters.Visibility = Visibility.Visible;
                PanelTopicAlreadySelected.Visibility = Visibility.Collapsed;

                Step1Circle.Fill = gray; Step2Circle.Fill = gray;

                LoadTopicsForCourse();
            }
            else
            {
                TabReport.IsEnabled = true;
                TabResults.IsEnabled = true;

                GridTopics.Visibility = Visibility.Collapsed;
                PanelFilters.Visibility = Visibility.Collapsed;
                PanelTopicAlreadySelected.Visibility = Visibility.Visible;

                Step1Circle.Fill = green;

                // Керівник
                TxtInfoSupervisor.Text = _currentAssignment.Supervisor != null
                    ? $"{_currentAssignment.Supervisor.User.LastName} {_currentAssignment.Supervisor.User.FirstName}"
                    : "Призначається...";

                // Статус звіту
                var report = _currentAssignment.Reports?.OrderByDescending(r => r.SubmissionDate).FirstOrDefault();

                if (report == null)
                {
                    Step2Circle.Fill = gray;
                    TxtInfoStatus.Text = "Подайте звіт";
                    BtnSubmitReport.Visibility = Visibility.Visible;
                }
                else
                {
                    TxtReportComment.Text = report.StudentComment;
                    TxtFeedback.Text = report.SupervisorFeedback ?? "На перевірці...";

                    if (report.Attachments != null)
                        foreach (var a in report.Attachments)
                            if (a.FileType != "URL") _tempAttachments.Add(a);
                            else TxtReportLink.Text = a.FilePath;

                    if (report.StatusId == 1) { Step2Circle.Fill = blue; BtnSubmitReport.Visibility = Visibility.Collapsed; }
                    else if (report.StatusId == 2) { Step2Circle.Fill = red; BtnSubmitReport.Visibility = Visibility.Visible; }
                    else if (report.StatusId == 3) { Step2Circle.Fill = green; BtnSubmitReport.Visibility = Visibility.Collapsed; }
                }

                TxtFinalGrade.Text = _currentAssignment.FinalGrade?.ToString() ?? "-";
            }
        }

        private async void LoadTopicsForCourse()
        {
            if (_selectedCourse == null || _selectedCourse.DisciplineId == 0) return;

            _allCachedTopics = await _studentService.GetAvailableTopicsAsync(_selectedCourse.DisciplineId, null);

            var orgs = _allCachedTopics.Select(t => t.Organization).Where(o => o != null)
                        .GroupBy(o => o.OrganizationId).Select(g => g.First()).ToList();

            CmbFilterOrg.ItemsSource = orgs;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allCachedTopics.AsEnumerable();
            if (CmbFilterOrg.SelectedValue is int orgId) filtered = filtered.Where(t => t.OrganizationId == orgId);
            GridTopics.ItemsSource = filtered.ToList();
        }

        private void CmbFilterOrg_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private void BtnResetFilter_Click(object sender, RoutedEventArgs e) { CmbFilterOrg.SelectedIndex = -1; ApplyFilters(); }


        private async void BtnSelectTopic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int topicId)
            {
                if (MessageBox.Show("Обрати цю тему?", "Підтвердження", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        // ВИКЛИКАЄМО СЕРВІС (не ліземо в базу напряму!)
                        await _studentService.SelectTopicAsync(_currentStudent.StudentId, topicId, _selectedCourse.CourseId);

                        RefreshAllData();
                        MainTabControl.SelectedIndex = 1;
                        MessageBox.Show("Тему успішно закріплено!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Помилка: " + ex.Message);
                    }
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
                    _tempAttachments.Add(new Attachment { FileName = System.IO.Path.GetFileName(f), FilePath = f, FileType = "FILE" });
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

                MessageBox.Show("Звіт відправлено!");
                RefreshAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка відправки: " + ex.Message);
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e) { new LoginWindow().Show(); Close(); }
    }
}