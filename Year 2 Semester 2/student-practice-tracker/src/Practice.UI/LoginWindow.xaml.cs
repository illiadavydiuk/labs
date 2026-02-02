using System.Windows;
using Practice.Data.Context;
using Practice.Repositories.Implementations;
using Practice.Services.Implementations;
using Practice.Services.Interfaces;
using Practice.Windows;
using Practice.Data;

namespace Practice
{
    public partial class LoginWindow : Window
    {
        private readonly AppDbContext _context;
        private readonly IIdentityService _identityService;
        private readonly IAdminService _adminService;
        private readonly ICourseService _courseService;
        private readonly IPracticeService _practiceService;
        private readonly IStudentService _studentService;
        private readonly ISupervisorService _supervisorService;
        private readonly IAuditService _auditService;

        public LoginWindow()
        {
            InitializeComponent();

            _context = new AppDbContext();
            _context.Database.EnsureCreated();
            DbInitializer.Initialize(_context);

            var userRepo = new UserRepository(_context);
            var studentRepo = new StudentRepository(_context);
            var supervisorRepo = new SupervisorRepository(_context);
            var groupRepo = new StudentGroupRepository(_context);
            var deptRepo = new DepartmentRepository(_context);
            var auditLogRepo = new AuditLogRepository(_context); 
            var auditRepo = new AuditLogRepository(_context);   
            var topicRepo = new InternshipTopicRepository(_context);
            var assignRepo = new InternshipAssignmentRepository(_context);
            var statusRepo = new AssignmentStatusRepository(_context);
            var orgRepo = new OrganizationRepository(_context);
            var courseRepo = new CourseRepository(_context);
            var discRepo = new DisciplineRepository(_context);
            var enrollRepo = new CourseEnrollmentRepository(_context);
            var reportRepo = new ReportRepository(_context);
            var attachRepo = new AttachmentRepository(_context);
            var specRepo = new SpecialtyRepository(_context);
            var posRepo = new PositionRepository(_context);

            _auditService = new AuditService(auditRepo);

            _identityService = new IdentityService(userRepo, studentRepo, supervisorRepo, groupRepo, deptRepo, _auditService);

            _adminService = new AdminService(_auditService, studentRepo, userRepo, supervisorRepo, specRepo, groupRepo, deptRepo, posRepo, discRepo, orgRepo);

            _courseService = new CourseService(courseRepo, discRepo, enrollRepo, _auditService);

            _practiceService = new PracticeService(topicRepo, assignRepo, statusRepo, orgRepo, _auditService);

            _studentService = new StudentService(
                studentRepo,
                assignRepo,
                topicRepo,
                courseRepo,
                reportRepo,
                auditLogRepo,
                _auditService
            );

            _supervisorService = new SupervisorService(supervisorRepo, assignRepo, reportRepo, _context);
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = TxtEmail.Text.Trim();
                string password = TxtPassword.Password.Trim();

                var user = await _identityService.LoginAsync(email, password);

                if (user != null)
                {
                    Window nextWindow = null;
                    switch (user.Role.RoleName)
                    {
                        case "Admin":
                            nextWindow = new AdminWindow(user, _adminService, _identityService, _practiceService, _courseService, _auditService);
                            break;
                        case "Student":
                            nextWindow = new StudentWindow(user, _studentService);
                            break;
                        case "Supervisor":
                            nextWindow = new SupervisorWindow(user, _supervisorService);
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
                var inner = ex.InnerException != null ? "\n" + ex.InnerException.Message : "";
                MessageBox.Show($"Помилка входу: {ex.Message}{inner}");
            }
        }
    }
}