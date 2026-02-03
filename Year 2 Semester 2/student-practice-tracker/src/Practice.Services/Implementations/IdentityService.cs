using Practice.Data.Constants;
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
        private readonly IStudentGroupRepository _groupRepo;
        private readonly IDepartmentRepository _deptRepo;
        private readonly IAuditService _auditService;

        public IdentityService(
            IUserRepository userRepo,
            IStudentRepository studentRepo,
            ISupervisorRepository supervisorRepo,
            IStudentGroupRepository groupRepo,
            IDepartmentRepository deptRepo,
            IAuditService auditService)
        {
            _userRepo = userRepo;
            _studentRepo = studentRepo;
            _supervisorRepo = supervisorRepo;
            _groupRepo = groupRepo;
            _deptRepo = deptRepo;
            _auditService = auditService;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _userRepo.GetByEmailAsync(email);

            if (user == null)
            {
                await _auditService.LogActionAsync(null, AuditActions.LoginFailed, $"Невідомий email: {email}");
                return null;
            }

            bool isPasswordValid = false;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            catch (Exception)
            {
                isPasswordValid = false;
            }

            if (!isPasswordValid)
            {
                await _auditService.LogActionAsync(user.UserId, AuditActions.LoginFailed, "Невірний пароль");
                return null;
            }

            string roleName = user.Role?.RoleName ?? "Unknown";
            await _auditService.LogActionAsync(user.UserId, AuditActions.LoginSuccess, $"Роль: {roleName}");
            return user;
        }

    public async Task<bool> RegisterStudentAsync(User user, string password, int groupId, string recordBook)
        {
            if (string.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains("@"))
                throw new ArgumentException("Некоректний Email");

            var existingUser = await _userRepo.GetByEmailAsync(user.Email);
            if (existingUser != null)
                throw new ArgumentException($"Користувач з email {user.Email} вже існує!");


            try
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                user.IsPasswordChangeRequired = true;
                _userRepo.Add(user);
                await _userRepo.SaveAsync(); 

                var student = new Student
                {
                    UserId = user.UserId,
                    GroupId = groupId,
                    RecordBookNumber = recordBook ?? "" 
                };
                _studentRepo.Add(student);
                await _studentRepo.SaveAsync();

                await _auditService.LogActionAsync(user.UserId, "Register", $"Студент: {user.Email}", "Student", student.StudentId);

                return true;
            }
            catch (Exception)
            {
                if (user.UserId > 0)
                {
                    _userRepo.Delete(user);
                    await _userRepo.SaveAsync();
                }
                throw;
            }
        }

        public async Task<bool> RegisterSupervisorAsync(User user, string? password, int? deptId, int? posId, string? phone)
        {
            if (!string.IsNullOrEmpty(password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                user.IsPasswordChangeRequired = true; // Це активує вікно зміни пароля
            }

            _userRepo.Add(user);
            await _userRepo.SaveAsync(); 

            var supervisor = new Supervisor
            {
                UserId = user.UserId,
                Phone = phone, 
                DepartmentId = deptId ?? 0,
                PositionId = posId
            };

            _supervisorRepo.Add(supervisor);
            await _supervisorRepo.SaveAsync();

            await _auditService.LogActionAsync(user.UserId, AuditActions.UserCreated, $"Викладач: {user.Email}", "Supervisor", supervisor.SupervisorId);
            return true;
        }

        public async Task<IEnumerable<StudentGroup>> GetAllGroupsAsync()
        {
            return await _groupRepo.GetAllAsync();
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            return await _deptRepo.GetAllAsync();
        }

        public async Task<StudentGroup> CreateGroupAsync(string code, int specialtyId, int year)
        {
            var group = new StudentGroup { GroupCode = code, SpecialtyId = specialtyId, EntryYear = year };
            _groupRepo.Add(group);
            await _groupRepo.SaveAsync();
            return group;
        }

        public async Task<Department> CreateDepartmentAsync(string name)
        {
            var dept = new Department { DepartmentName = name };
            _deptRepo.Add(dept);
            await _deptRepo.SaveAsync();
            return dept; 
        }
    }
}