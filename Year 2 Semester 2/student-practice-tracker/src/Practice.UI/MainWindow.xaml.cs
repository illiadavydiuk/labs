using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Services.Implementations;
using Practice.Repositories.Implementations;

namespace Practice.UI
{
    public partial class MainWindow : Window
    {
        private AppDbContext _db;
        private User _user;
        private IdentityService _auth;
        private AuditService _audit;

        public MainWindow()
        {
            InitializeComponent();
            _db = new AppDbContext();
            _db.Database.EnsureCreated(); //

            _audit = new AuditService(new AuditLogRepository(_db));
            _auth = new IdentityService(new UserRepository(_db), new StudentRepository(_db), new SupervisorRepository(_db), _audit);

            CheckFirstRun();
        }

        private void CheckFirstRun()
        {
            // Якщо в базі немає жодного користувача - запускаємо Setup Wizard
            if (!_db.Users.Any())
            {
                WizardOverlay.Visibility = Visibility.Visible;
                LoginOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private bool Validate(string email, string pass)
        {
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Некоректний Email!"); return false;
            }
            if (pass.Length < 6)
            {
                MessageBox.Show("Пароль занадто короткий!"); return false;
            }
            return true;
        }

        // --- КЕЙС: СТВОРЕННЯ СУПЕРАДМІНА (Setup Wizard) ---
        private async void CreateAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate(WizEmail.Text, WizPass.Password)) return;

            var admin = new User { Email = WizEmail.Text, RoleId = 1, FirstName = "System", LastName = "Admin" }; //
            await _auth.RegisterStudentAsync(admin, WizPass.Password, 1, "ADMIN-001");

            WizardOverlay.Visibility = Visibility.Collapsed;
            LoginOverlay.Visibility = Visibility.Visible;
            MessageBox.Show("Адміністратор створений. Увійдіть.");
        }

        // --- КЕЙС: РЕЄСТРАЦІЯ СТУДЕНТА ---
        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate(RegEmail.Text, RegPass.Password)) return;

            var u = new User { Email = RegEmail.Text, FirstName = RegFName.Text, LastName = RegLName.Text, RoleId = 2 }; // Role 2 - Student
            bool res = await _auth.RegisterStudentAsync(u, RegPass.Password, 1, RegRecordBook.Text);

            if (res) { MessageBox.Show("Успіх!"); ToggleAuth_Click(null, null); }
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var u = await _auth.LoginAsync(LogEmail.Text, LogPass.Password);
            if (u != null)
            {
                _user = u;
                LoginOverlay.Visibility = Visibility.Collapsed;
                AdminMenu.Visibility = u.RoleId == 1 ? Visibility.Visible : Visibility.Collapsed;
                StudentMenu.Visibility = u.RoleId == 2 ? Visibility.Visible : Visibility.Collapsed;
                TeacherMenu.Visibility = u.RoleId == 3 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // --- КЕЙС: АУДИТ ТА КОРИСТУВАЧІ ---
        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as FrameworkElement).Tag.ToString();
            AdminUsersGrid.Visibility = AuditGrid.Visibility = StudentReportGrid.Visibility = Visibility.Collapsed;

            if (tag == "AdminUsers") { AdminUsersGrid.Visibility = Visibility.Visible; dgUsers.ItemsSource = _db.Users.ToList(); }
            if (tag == "Audit") { AuditGrid.Visibility = Visibility.Visible; dgAudit.ItemsSource = _db.AuditLogs.ToList(); } //
            if (tag == "StudentReport") StudentReportGrid.Visibility = Visibility.Visible;
        }

        private void ToggleAuth_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = LoginPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            RegPanel.Visibility = RegPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Logout_Click(object sender, RoutedEventArgs e) { _user = null; LoginOverlay.Visibility = Visibility.Visible; }
    }
}