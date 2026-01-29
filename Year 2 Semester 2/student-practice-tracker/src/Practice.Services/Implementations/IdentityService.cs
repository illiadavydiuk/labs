using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Services.Implementations
{
    public class IdentityService : IIdentityService
    {
        private readonly IUserRepository _userRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly ISupervisorRepository _supervisorRepo;
        private readonly IAuditService _auditService;

        public IdentityService(
            IUserRepository userRepo,
            IStudentRepository studentRepo,
            ISupervisorRepository supervisorRepo,
            IAuditService auditService)
        {
            _userRepo = userRepo;
            _studentRepo = studentRepo;
            _supervisorRepo = supervisorRepo;
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
    }
}
