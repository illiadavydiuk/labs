using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class IdentityService : IIdentityService
    {
        private readonly IUserRepository _userRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly ISupervisorRepository _supervisorRepo;
        private readonly IStudentGroupRepository _groupRepo;       // Додано
        private readonly IDepartmentRepository _departmentRepo;     // Додано
        private readonly IAuditService _auditService;

        public IdentityService(
            IUserRepository userRepo,
            IStudentRepository studentRepo,
            ISupervisorRepository supervisorRepo,
            IStudentGroupRepository groupRepo,
            IDepartmentRepository departmentRepo,
            IAuditService auditService)
        {
            _userRepo = userRepo;
            _studentRepo = studentRepo;
            _supervisorRepo = supervisorRepo;
            _groupRepo = groupRepo;
            _departmentRepo = departmentRepo;
            _auditService = auditService;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                await _auditService.LogActionAsync(null, "Auth_Failed", $"Спроба для: {email}", "User", 0);
                return null;
            }
            await _auditService.LogActionAsync(user.UserId, "Auth", "Успішний вхід", "User", user.UserId);
            return user;
        }

        public async Task<bool> RegisterStudentAsync(User user, string password, int groupId, string recordBook)
        {
            // Валідація
            if (string.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains("@"))
                throw new ArgumentException("Некоректний Email");
            if (string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.LastName))
                throw new ArgumentException("Ім'я та прізвище обов'язкові");
            if (password.Length < 4)
                throw new ArgumentException("Пароль занадто короткий");

            var group = await _groupRepo.GetByIdAsync(groupId);
            if (group == null) throw new ArgumentException("Обрана група не існує");

            // Створення
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            _userRepo.Add(user);
            await _userRepo.SaveAsync();

            var student = new Student
            {
                UserId = user.UserId,
                GroupId = groupId,
                RecordBookNumber = recordBook
            };
            _studentRepo.Add(student);
            await _studentRepo.SaveAsync();

            await _auditService.LogActionAsync(user.UserId, "Register", "Новий студент", "Student", user.UserId);
            return true;
        }

        public async Task<bool> RegisterSupervisorAsync(User user, string password, int? departmentId, int? positionId, string? phone)
        {
            // Валідація
            if (string.IsNullOrWhiteSpace(user.Email)) throw new ArgumentException("Email обов'язковий");
            if (departmentId.HasValue)
            {
                var dept = await _departmentRepo.GetByIdAsync(departmentId.Value);
                if (dept == null) throw new ArgumentException("Кафедра не знайдена");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            _userRepo.Add(user);
            await _userRepo.SaveAsync();

            var supervisor = new Supervisor
            {
                UserId = user.UserId,
                DepartmentId = departmentId,
                PositionId = positionId,
                Phone = phone
            };

            _supervisorRepo.Add(supervisor);
            await _supervisorRepo.SaveAsync();

            return true;
        }

        public async Task<IEnumerable<StudentGroup>> GetAllGroupsAsync()
        {
            return await _groupRepo.GetAllAsync();
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            return await _departmentRepo.GetAllAsync();
        }

        public async Task<StudentGroup> CreateGroupAsync(string code, int specialtyId, int year)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Код групи не може бути пустим");

            var group = new StudentGroup
            {
                GroupCode = code,
                SpecialtyId = specialtyId,
                EntryYear = year
            };
            _groupRepo.Add(group);
            await _groupRepo.SaveAsync();
            return group;
        }

        public async Task<Department> CreateDepartmentAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Назва кафедри обов'язкова");

            var dept = new Department { DepartmentName = name };
            _departmentRepo.Add(dept);
            await _departmentRepo.SaveAsync();
            return dept;
        }
    }
}