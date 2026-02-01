using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Practice.Data.Entities;
using Practice.Services.Interfaces;

namespace Practice.Windows
{
    public partial class SupervisorWindow : Window
    {
        private readonly User _currentUser;
        private readonly ISupervisorService _supervisorService;

        private Supervisor _currentSupervisor;
        private List<InternshipAssignment> _allAssignments = new List<InternshipAssignment>();
        private InternshipAssignment _selectedAssignment;

        public SupervisorWindow(User user, ISupervisorService supervisorService)
        {
            InitializeComponent();
            _currentUser = user;
            _supervisorService = supervisorService;

            Init();
        }

        private async void Init()
        {
            try
            {
                _currentSupervisor = await _supervisorService.GetSupervisorProfileAsync(_currentUser.UserId);
                if (_currentSupervisor == null)
                {
                    MessageBox.Show("Профіль викладача не знайдено.");
                    Close();
                    return;
                }

                TxtSupervisorName.Text = $"{_currentSupervisor.User.LastName} {_currentSupervisor.User.FirstName.FirstOrDefault()}.";
                await LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка ініціалізації: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            _allAssignments = await _supervisorService.GetStudentsForSupervisorAsync(_currentSupervisor.SupervisorId);

            CmbFilterCourse.ItemsSource = _allAssignments.Select(a => a.Course).DistinctBy(c => c.CourseId).ToList();
            CmbFilterGroup.ItemsSource = _allAssignments.Select(a => a.Student.StudentGroup).DistinctBy(g => g.GroupId).ToList();

            RenderList();
        }

        private void RenderList()
        {
            var filtered = _allAssignments.AsEnumerable();

            if (CmbFilterCourse.SelectedValue is int courseId)
                filtered = filtered.Where(a => a.CourseId == courseId);

            if (CmbFilterGroup.SelectedValue is int groupId)
                filtered = filtered.Where(a => a.Student.GroupId == groupId);

            var uiList = filtered.Select(a => new
            {
                AssignmentId = a.AssignmentId,
                StudentName = $"{a.Student.User.LastName} {a.Student.User.FirstName}",
                GroupCode = a.Student.StudentGroup.GroupCode,
                CourseName = a.Course.Name,
                TopicTitle = a.InternshipTopic?.Title ?? "Тема не обрана",
                ReportStatus = GetStatusText(a),
                StatusColor = GetStatusColor(a)
            }).ToList();

            ListStudents.ItemsSource = uiList;
        }

        private string GetStatusText(InternshipAssignment a)
        {
            if (a.FinalGrade.HasValue) return "Оцінено";
            var report = a.Reports.OrderByDescending(r => r.SubmissionDate).FirstOrDefault();
            if (report == null) return "Без звіту";
            return report.StatusId switch { 1 => "На перевірці", 2 => "Повернуто", 3 => "Прийнято", _ => "В процесі" };
        }

        private string GetStatusColor(InternshipAssignment a)
        {
            if (a.FinalGrade.HasValue) return "#4CAF50";
            var report = a.Reports.OrderByDescending(r => r.SubmissionDate).FirstOrDefault();
            if (report == null) return "#9E9E9E";
            if (report.StatusId == 1) return "#FF9800";
            if (report.StatusId == 2) return "#F44336";
            return "#2196F3";
        }

        private void StudentCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is int assignmentId)
            {
                _selectedAssignment = _allAssignments.FirstOrDefault(a => a.AssignmentId == assignmentId);
                if (_selectedAssignment != null)
                {
                    ShowDetails(_selectedAssignment);
                }
            }
        }

        private void ShowDetails(InternshipAssignment a)
        {
            PanelEmpty.Visibility = Visibility.Collapsed;
            PanelContent.Visibility = Visibility.Visible;

            TxtActiveStudent.Text = $"{a.Student.User.LastName} {a.Student.User.FirstName}";
            TxtActiveTopic.Text = a.InternshipTopic?.Title ?? "-";
            TxtActiveOrg.Text = a.InternshipTopic?.Organization?.Name ?? "-";

            var report = a.Reports.OrderByDescending(r => r.SubmissionDate).FirstOrDefault();

            if (report != null)
            {
                TxtReportDate.Text = "Дата: " + report.SubmissionDate.ToString("dd.MM.yyyy HH:mm");
                TxtStudentComment.Text = report.StudentComment;
                TxtStudentLink.Text = report.Attachments.FirstOrDefault(at => at.FileType == "URL")?.FilePath ?? "Немає посилання";

                ListAttachments.ItemsSource = report.Attachments.Where(at => at.FileType != "URL").ToList();
                TxtSupervisorFeedback.Text = report.SupervisorFeedback;

                foreach (ComboBoxItem item in CmbReportStatus.Items)
                {
                    if (item.Tag.ToString() == report.StatusId.ToString())
                    {
                        CmbReportStatus.SelectedItem = item;
                        break;
                    }
                }
                UpdateTimeline(true, true, a.FinalGrade.HasValue);
            }
            else
            {
                TxtReportDate.Text = "Звіт ще не подано";
                TxtStudentComment.Text = "-";
                TxtStudentLink.Text = "-";
                ListAttachments.ItemsSource = null;
                TxtSupervisorFeedback.Text = "";
                CmbReportStatus.SelectedIndex = -1;
                UpdateTimeline(true, false, false);
            }

            TxtFinalGrade.Text = a.FinalGrade?.ToString() ?? "";
        }

        private void UpdateTimeline(bool step1, bool step2, bool step3)
        {
            var green = (Brush)new BrushConverter().ConvertFrom("#4CAF50");
            var gray = (Brush)new BrushConverter().ConvertFrom("#DDD");

            Step1Circle.Background = step1 ? green : gray;
            Line1.Fill = step1 ? green : gray;
            Step2Circle.Background = step2 ? green : gray;
            Line2.Fill = step2 ? green : gray;
            Step3Circle.Background = step3 ? green : gray;
        }

        private async void BtnSaveAssessment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAssignment == null) return;
            try
            {
                string feedback = TxtSupervisorFeedback.Text;
                int? grade = null;
                if (int.TryParse(TxtFinalGrade.Text, out int g)) grade = g;

                int? statusId = null;
                if (CmbReportStatus.SelectedItem is ComboBoxItem item)
                    statusId = int.Parse(item.Tag.ToString());

                await _supervisorService.SaveAssessmentAsync(_selectedAssignment.AssignmentId, feedback, grade, statusId);
                MessageBox.Show("Зміни збережено!");
                await LoadData();

                _selectedAssignment = _allAssignments.First(a => a.AssignmentId == _selectedAssignment.AssignmentId);
                ShowDetails(_selectedAssignment);
            }
            catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
        }

        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string path)
            {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = path, UseShellExecute = true }); }
                catch { MessageBox.Show("Не вдалося відкрити файл."); }
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e) => RenderList();
        private void BtnResetFilters_Click(object sender, RoutedEventArgs e) { CmbFilterCourse.SelectedIndex = -1; CmbFilterGroup.SelectedIndex = -1; RenderList(); }
        private void BtnLogout_Click(object sender, RoutedEventArgs e) { new LoginWindow().Show(); Close(); }
    }
}