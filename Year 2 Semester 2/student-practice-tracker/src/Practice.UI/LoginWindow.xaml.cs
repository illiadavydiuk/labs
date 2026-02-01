using System.Windows;
using Practice.Data.Context;
using Practice.Repositories.Implementations;
using Practice.Services.Implementations;
using Practice.Windows;

namespace Practice
{
    public partial class LoginWindow : Window
    {
        private readonly IdentityService _identityService;
        private readonly AppDbContext _context;

        public LoginWindow()
        {
            InitializeComponent();

            // Ініціалізація (Manual DI)
            _context = new AppDbContext();
            Practice.Data.DbInitializer.Initialize(_context); 

            var userRepo = new UserRepository(_context);
            var studentRepo = new StudentRepository(_context);
            var supervisorRepo = new SupervisorRepository(_context);
            var groupRepo = new StudentGroupRepository(_context);
            var deptRepo = new DepartmentRepository(_context);
            var auditRepo = new AuditLogRepository(_context);
            var auditService = new AuditService(auditRepo);

            _identityService = new IdentityService(userRepo, studentRepo, supervisorRepo, groupRepo, deptRepo, auditService);
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var user = await _identityService.LoginAsync(TxtEmail.Text, TxtPassword.Password);

                if (user != null)
                {
                    if (user.IsPasswordChangeRequired)
                    {
                        var changePassWin = new ChangePasswordWindow(user, _identityService);
                        if (changePassWin.ShowDialog() != true)
                        {
                            return;
                        }
                    }

                    Window nextWindow = null;
                    switch (user.Role.RoleName)
                    {
                        case "Admin":
                            nextWindow = new AdminWindow(user);
                            break;
                        case "Student":
                            nextWindow = new StudentWindow(user);
                            break;
                        case "Supervisor":
                            nextWindow = new SupervisorWindow(user);
                            break;
                        default:
                            MessageBox.Show("Роль не визначена");
                            return;
                    }

                    nextWindow.Show();
                    this.Close();
                }
                else
                {
                    TxtError.Text = "Невірний логін або пароль";
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Помилка: " + ex.Message);
            }
        }
    }
}