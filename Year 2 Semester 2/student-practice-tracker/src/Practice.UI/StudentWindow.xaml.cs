using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Implementations;
using Practice.Services.Implementations;

namespace Practice.Windows
{
    public partial class StudentWindow : Window
    {
        private readonly User _currentUser;
        private readonly AppDbContext _context;
        private readonly PracticeService _practiceService;
        private readonly ReviewService _reviewService;

        private string _selectedFilePath;

        public StudentWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            TxtStudentName.Text = $"{user.FirstName} {user.LastName}";

            _context = new AppDbContext();
            var topicRepo = new InternshipTopicRepository(_context);
            var assignRepo = new InternshipAssignmentRepository(_context);
            var statusRepo = new AssignmentStatusRepository(_context);
            var orgRepo = new OrganizationRepository(_context);
            var reportRepo = new ReportRepository(_context);
            var attachRepo = new AttachmentRepository(_context);
            var auditRepo = new AuditLogRepository(_context);
            var auditService = new AuditService(auditRepo);

            _practiceService = new PracticeService(topicRepo, assignRepo, statusRepo, orgRepo, auditService);
            _reviewService = new ReviewService(reportRepo, attachRepo, auditService);

            LoadTopics();
            LoadCurrentAssignment();
        }

        private async void LoadTopics()
        {
            GridTopics.ItemsSource = await _practiceService.GetAvailableTopicsAsync();
        }

        private async void LoadCurrentAssignment()
        {

            int studentId = _currentUser.Student?.StudentId ?? 0;
            if (studentId == 0)
            {
                var student = await new StudentRepository(_context).GetByUserIdAsync(_currentUser.UserId);
                if (student != null) studentId = student.StudentId;
            }

            if (studentId != 0)
            {
                var assignment = await _practiceService.GetStudentAssignmentAsync(studentId);
                if (assignment != null)
                {
                    TxtCurrentAssignment.Text = assignment.InternshipTopic.Title;
                    TxtSupervisorName.Text = assignment.Supervisor != null
                        ? $"Керівник: {assignment.Supervisor.User.LastName} {assignment.Supervisor.User.FirstName}"
                        : "Керівник не призначений";
                }
            }
        }

        private void BtnRefreshTopics_Click(object sender, RoutedEventArgs e) => LoadTopics();

        private async void BtnSelectTopic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int topicId)
            {
                // Для демо: CourseId=1, SupervisorId=1 (Якщо треба логіка розподілу - це окрема історія)
                bool success = await _practiceService.AssignTopicAsync(_currentUser.UserId, topicId, 1, 1, "Завдання");
                if (success)
                {
                    MessageBox.Show("Тему обрано!");
                    LoadCurrentAssignment();
                    LoadTopics();
                }
            }
        }

        private void BtnBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                _selectedFilePath = dlg.FileName;
                TxtSelectedFile.Text = Path.GetFileName(_selectedFilePath);
            }
        }

        private async void BtnSubmitReport_Click(object sender, RoutedEventArgs e)
        {
            // Отримуємо актуальний StudentId
            var student = await new StudentRepository(_context).GetByUserIdAsync(_currentUser.UserId);
            if (student == null) { MessageBox.Show("Помилка профілю студента"); return; }

            var assignment = await _practiceService.GetStudentAssignmentAsync(student.StudentId);
            if (assignment == null) { MessageBox.Show("Спочатку оберіть тему!"); return; }
            if (string.IsNullOrEmpty(_selectedFilePath)) { MessageBox.Show("Оберіть файл!"); return; }

            // Копіювання файлу
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StudentPracticePlatform", "Reports");
            Directory.CreateDirectory(appDataPath);
            string destFile = Path.Combine(appDataPath, $"{DateTime.Now.Ticks}_{Path.GetFileName(_selectedFilePath)}");
            File.Copy(_selectedFilePath, destFile, true);

            var attachment = new Attachment
            {
                FileName = Path.GetFileName(destFile),
                FilePath = destFile,
                FileType = Path.GetExtension(destFile)
            };

            await _reviewService.SubmitReportAsync(assignment.AssignmentId, TxtReportComment.Text, new List<Attachment> { attachment });
            MessageBox.Show("Звіт відправлено!");
            TxtReportComment.Clear();
            TxtSelectedFile.Text = "Файл не обрано";
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}