using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Implementations;
using Practice.Services.Implementations;

namespace Practice.Windows
{
    public partial class SupervisorWindow : Window
    {
        private readonly User _currentUser;
        private readonly AppDbContext _context;
        private readonly ReviewService _reviewService;

        // КОНСТРУКТОР З ПАРАМЕТРОМ (Виправляє помилку CS1729)
        public SupervisorWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            TxtName.Text = $"{user.FirstName} {user.LastName}";

            _context = new AppDbContext();

            // Ініціалізація сервісів
            var reportRepo = new ReportRepository(_context);
            var attachRepo = new AttachmentRepository(_context);
            var auditRepo = new AuditLogRepository(_context);
            var auditService = new AuditService(auditRepo);

            _reviewService = new ReviewService(reportRepo, attachRepo, auditService);

            LoadReports();
        }

        private async void LoadReports()
        {
            // Завантажуємо звіти. В ідеалі фільтрувати за SupervisorId, але для демо беремо всі, де є призначення
            // Щоб фільтрувати, треба знайти SupervisorId поточного юзера:
            // var supId = _context.Supervisors.FirstOrDefault(s => s.UserId == _currentUser.UserId)?.SupervisorId;

            var reports = await _context.Reports
                .Include(r => r.InternshipAssignment).ThenInclude(i => i.Student).ThenInclude(s => s.User)
                .Include(r => r.InternshipAssignment).ThenInclude(i => i.InternshipTopic)
                .Include(r => r.ReportStatus)
                .Include(r => r.Attachments)
                .OrderByDescending(r => r.SubmissionDate)
                .ToListAsync();

            GridReports.ItemsSource = reports;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadReports();

        private void GridReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridReports.SelectedItem is Report report)
            {
                TxtStudentComment.Text = report.StudentComment;
                ListFiles.ItemsSource = report.Attachments;
                TxtInfo.Text = $"Обрано звіт ID: {report.ReportId}";
            }
            else
            {
                TxtStudentComment.Clear();
                ListFiles.ItemsSource = null;
                TxtInfo.Text = "";
            }
        }

        private async void BtnApprove_Click(object sender, RoutedEventArgs e) => await ProcessReview(2); // 2 = Approved
        private async void BtnReject_Click(object sender, RoutedEventArgs e) => await ProcessReview(3); // 3 = Rejected

        private async System.Threading.Tasks.Task ProcessReview(int statusId)
        {
            if (GridReports.SelectedItem is Report report)
            {
                await _reviewService.ReviewReportAsync(report.ReportId, statusId, TxtFeedback.Text);
                MessageBox.Show("Статус оновлено!");
                TxtFeedback.Clear();
                LoadReports();
            }
            else
            {
                MessageBox.Show("Оберіть звіт зі списку!");
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}