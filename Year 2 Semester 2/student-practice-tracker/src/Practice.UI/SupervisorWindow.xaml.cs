using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                if (_currentSupervisor == null) { MessageBox.Show("Профіль не знайдено."); Close(); return; }

                TxtSupervisorName.Text = $"{_currentSupervisor.User.LastName} {_currentSupervisor.User.FirstName.FirstOrDefault()}.";
                await LoadData();
            }
            catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
        }

        private async Task LoadData()
        {
            _allAssignments = await _supervisorService.GetStudentsForSupervisorAsync(_currentSupervisor.SupervisorId);

            CmbFilterCourse.ItemsSource = _allAssignments.Select(a => a.Course).DistinctBy(c => c.CourseId).ToList();
            CmbFilterGroup.ItemsSource = _allAssignments.Select(a => a.Student.StudentGroup).DistinctBy(g => g.GroupId).ToList();

            RenderList();
        }

        private void RenderList()
        {
            var filtered = _allAssignments.AsEnumerable();
            if (CmbFilterCourse.SelectedValue is int cid) filtered = filtered.Where(a => a.CourseId == cid);
            if (CmbFilterGroup.SelectedValue is int gid) filtered = filtered.Where(a => a.Student.GroupId == gid);

            ListStudents.ItemsSource = filtered.Select(a => new {
                a.AssignmentId,
                StudentName = $"{a.Student.User.LastName} {a.Student.User.FirstName}",
                GroupCode = a.Student.StudentGroup.GroupCode,
                CourseName = a.Course.Name,
                TopicTitle = a.InternshipTopic?.Title ?? "Тема не обрана",

                ReportStatus = GetStatusText(a),
                StatusColor = GetStatusColor(a)
            }).ToList();
        }

        private string GetStatusText(InternshipAssignment a)
        {
            var lastReport = a.Reports?.OrderByDescending(r => r.SubmissionDate).FirstOrDefault();

            if (lastReport == null) return "Без звіту";

            if (lastReport.StatusId == 2) return "Повернуто";
            if (lastReport.StatusId == 1) return "На перевірці";

            if (a.FinalGrade.HasValue || lastReport.StatusId == 3) return "Оцінено";

            return "В процесі";
        }

        private string GetStatusColor(InternshipAssignment a)
        {
            var lastReport = a.Reports?.OrderByDescending(r => r.SubmissionDate).FirstOrDefault();

            if (lastReport == null) return "#9E9E9E"; // Сірий

            if (lastReport.StatusId == 2) return "#F44336";

            // Помаранчевий для "На перевірці"
            if (lastReport.StatusId == 1) return "#FF9800";

            // Зелений для "Оцінено/Прийнято"
            if (a.FinalGrade.HasValue || lastReport.StatusId == 3) return "#4CAF50";

            return "#2196F3"; // Синій (дефолт)
        }

        private void StudentCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is int aid)
            {
                _selectedAssignment = _allAssignments.FirstOrDefault(a => a.AssignmentId == aid);
                if (_selectedAssignment != null) ShowDetails(_selectedAssignment);
            }
        }

        private void ShowDetails(InternshipAssignment a)
        {
            PanelEmpty.Visibility = Visibility.Collapsed;
            PanelContent.Visibility = Visibility.Visible;

            TxtActiveStudent.Text = $"{a.Student.User.LastName} {a.Student.User.FirstName}";
            TxtActiveTopic.Text = a.InternshipTopic?.Title ?? "-";
            TxtActiveOrg.Text = a.InternshipTopic?.Organization?.Name ?? "-";

            var report = a.Reports?.OrderByDescending(r => r.SubmissionDate).FirstOrDefault();

            var historyList = new List<dynamic>();

            if (a.Reports != null)
            {
                foreach (var r in a.Reports.OrderByDescending(x => x.SubmissionDate))
                {
                    historyList.Add(new
                    {
                        TimeStamp = r.SubmissionDate,
                        Action = "Студент надіслав звіт",
                        Details = string.IsNullOrWhiteSpace(r.StudentComment) ? "Без коментаря" : r.StudentComment
                    });

                    if (r.ReviewDate.HasValue)
                    {
                        string statusText = r.StatusId == 2 ? "Повернуто на доопрацювання" : (r.StatusId == 3 ? "Прийнято" : "Перевірено");
                        historyList.Add(new
                        {
                            TimeStamp = r.ReviewDate.Value,
                            Action = statusText,
                            Details = r.SupervisorFeedback ?? "-"
                        });
                    }
                }
            }
            ListHistory.ItemsSource = historyList.OrderByDescending(x => x.TimeStamp).ToList();


            if (report != null)
            {
                TxtReportDate.Text = report.SubmissionDate.ToString("dd.MM.yyyy HH:mm");
                TxtStudentComment.Text = report.StudentComment;

                var linkAttach = report.Attachments?.FirstOrDefault(f => f.FileType == "URL");
                TxtStudentLink.Text = linkAttach != null ? linkAttach.FilePath : "-";

                ListAttachments.ItemsSource = report.Attachments?.Where(f => f.FileType != "URL").ToList();
                TxtSupervisorFeedback.Text = report.SupervisorFeedback;

                CmbReportStatus.SelectedIndex = -1;
                foreach (ComboBoxItem item in CmbReportStatus.Items)
                {
                    if (item.Tag != null && item.Tag.ToString() == report.StatusId.ToString())
                    {
                        CmbReportStatus.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                TxtReportDate.Text = "Не подано";
                TxtStudentComment.Text = "-";
                TxtStudentLink.Text = "-";
                ListAttachments.ItemsSource = null;
                TxtSupervisorFeedback.Clear();
                CmbReportStatus.SelectedIndex = -1;
            }

            TxtFinalGrade.Text = a.FinalGrade?.ToString() ?? "";
            TxtCompanyGrade.Text = a.CompanyGrade?.ToString() ?? "";
            TxtCompanyFeedback.Text = a.CompanyFeedback ?? "";

            UpdateTimeline(true, report != null, a.FinalGrade.HasValue);
        }

        private void UpdateTimeline(bool s1, bool s2, bool s3)
        {
            var g = (Brush)new BrushConverter().ConvertFrom("#4CAF50");
            var gr = (Brush)new BrushConverter().ConvertFrom("#DDD");
            Step1Circle.Background = s1 ? g : gr; Line1.Fill = s1 ? g : gr;
            Step2Circle.Background = s2 ? g : gr; Line2.Fill = s2 ? g : gr;
            Step3Circle.Background = s3 ? g : gr;
        }

        private async void BtnSaveAssessment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAssignment == null) return;
            try
            {
                string feedback = TxtSupervisorFeedback.Text;
                string companyFeedback = TxtCompanyFeedback.Text;

                int? finalGrade = int.TryParse(TxtFinalGrade.Text, out int fg) ? fg : (int?)null;
                int? companyGrade = int.TryParse(TxtCompanyGrade.Text, out int cg) ? cg : (int?)null;

                int? statusId = null;
                if (CmbReportStatus.SelectedItem is ComboBoxItem item && item.Tag != null)
                {
                    statusId = int.Parse(item.Tag.ToString());
                }

                await _supervisorService.SaveAssessmentAsync(
                    _selectedAssignment.AssignmentId,
                    feedback,
                    finalGrade,
                    statusId,
                    companyGrade,
                    companyFeedback
                );

                MessageBox.Show("Зміни збережено!");

                await LoadData();

                _selectedAssignment = _allAssignments.First(a => a.AssignmentId == _selectedAssignment.AssignmentId);
                ShowDetails(_selectedAssignment);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка збереження: " + ex.Message);
            }
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
        private void BtnResetFilters_Click(object sender, RoutedEventArgs e) 
        { 
            CmbFilterCourse.SelectedIndex = -1; 
            CmbFilterGroup.SelectedIndex = -1; 
            RenderList(); 
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e) 
        { 
            new LoginWindow().Show(); 
            Close(); 
        }
    }
}