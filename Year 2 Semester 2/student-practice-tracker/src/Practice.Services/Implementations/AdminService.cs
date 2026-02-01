using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IAuditService _auditService;

        public AdminService(IAuditService auditService)
        {
            _auditService = auditService;
        }


        public async Task<List<Student>> GetStudentsByGroupAsync(int? groupId)
        {
            using var context = new AppDbContext();
            var query = context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.StudentGroup)
                .AsQueryable();

            if (groupId.HasValue && groupId.Value > 0)
                query = query.Where(s => s.GroupId == groupId.Value);

            return await query.ToListAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            using var context = new AppDbContext();
            return await context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.StudentGroup)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public async Task UpdateStudentAsync(int studentId, string firstName, string lastName, string email, string recordBook, int groupId)
        {
            using var context = new AppDbContext();
            var student = await context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student == null) throw new Exception("Студента не знайдено");

            student.User.FirstName = firstName;
            student.User.LastName = lastName;
            student.User.Email = email;
            student.RecordBookNumber = recordBook;
            student.GroupId = groupId;

            await context.SaveChangesAsync();
            await _auditService.LogActionAsync(null, "Update", $"Оновлено студента: {email}", "Student", studentId);
        }

        public async Task DeleteStudentAsync(int userId)
        {
            using var context = new AppDbContext();
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                string email = user.Email;
                context.Users.Remove(user);
                await context.SaveChangesAsync();
                await _auditService.LogActionAsync(null, "Delete", $"Видалено студента: {email}", "User", userId);
            }
        }


        public async Task<List<Supervisor>> GetSupervisorsByDeptAsync(int? deptId)
        {
            using var context = new AppDbContext();
            var query = context.Supervisors
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Department)
                .Include(s => s.Position)
                .AsQueryable();

            if (deptId.HasValue && deptId.Value > 0)
                query = query.Where(s => s.DepartmentId == deptId.Value);

            return await query.ToListAsync();
        }

        public async Task<Supervisor?> GetSupervisorByIdAsync(int supervisorId)
        {
            using var context = new AppDbContext();
            return await context.Supervisors
                .AsNoTracking()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SupervisorId == supervisorId);
        }

        public async Task UpdateSupervisorAsync(int supervisorId, string firstName, string lastName, string email, string phone, int deptId, int? posId)
        {
            using var context = new AppDbContext();
            var sup = await context.Supervisors.Include(s => s.User).FirstOrDefaultAsync(s => s.SupervisorId == supervisorId);
            if (sup == null) throw new Exception("Керівника не знайдено");

            sup.User.FirstName = firstName;
            sup.User.LastName = lastName;
            sup.User.Email = email;
            sup.Phone = phone;
            sup.DepartmentId = deptId;
            sup.PositionId = posId;

            await context.SaveChangesAsync();
            await _auditService.LogActionAsync(null, "Update", $"Оновлено керівника: {email}", "Supervisor", supervisorId);
        }

        public async Task DeleteSupervisorAsync(int userId)
        {
            using var context = new AppDbContext();
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                string email = user.Email;
                context.Users.Remove(user);
                await context.SaveChangesAsync();
                await _auditService.LogActionAsync(null, "Delete", $"Видалено керівника: {email}", "User", userId);
            }
        }

        public async Task<IEnumerable<Specialty>> GetAllSpecialtiesAsync()
        {
            using var context = new AppDbContext();
            return await context.Specialties.AsNoTracking().Include(s => s.Department).ToListAsync();
        }

        public async Task AddSpecialtyAsync(Specialty specialty)
        {
            if (string.IsNullOrWhiteSpace(specialty.Name) || string.IsNullOrWhiteSpace(specialty.Code))
                throw new ArgumentException("Назва та Код обов'язкові!");

            using var context = new AppDbContext();
            context.Specialties.Add(specialty);
            await context.SaveChangesAsync();
            await _auditService.LogActionAsync(null, "Create", $"Створено спец.: {specialty.Code}", "Specialty", specialty.SpecialtyId);
        }

        public async Task UpdateSpecialtyAsync(Specialty specialty)
        {
            using var context = new AppDbContext();
            var existing = await context.Specialties.FindAsync(specialty.SpecialtyId);
            if (existing != null)
            {
                existing.Name = specialty.Name;
                existing.Code = specialty.Code;
                existing.DepartmentId = specialty.DepartmentId;

                await context.SaveChangesAsync();
                await _auditService.LogActionAsync(null, "Update", $"Оновлено спец.: {specialty.Code}", "Specialty", specialty.SpecialtyId);
            }
        }

        public async Task DeleteSpecialtyAsync(int id)
        {
            using var context = new AppDbContext();
            var item = await context.Specialties.FindAsync(id);
            if (item != null)
            {
                context.Specialties.Remove(item);
                await context.SaveChangesAsync();
                await _auditService.LogActionAsync(null, "Delete", $"Видалено спец. ID: {id}", "Specialty", id);
            }
        }

        public async Task<IEnumerable<StudentGroup>> GetAllGroupsAsync()
        {
            using var context = new AppDbContext();
            return await context.StudentGroups
                .AsNoTracking()
                .Include(g => g.Specialty) 
                .ToListAsync();
        }

        public async Task AddGroupAsync(StudentGroup group)
        {
            if (string.IsNullOrWhiteSpace(group.GroupCode)) throw new ArgumentException("Шифр групи обов'язковий!");
            using var context = new AppDbContext();
            context.StudentGroups.Add(group);
            await context.SaveChangesAsync();
            await _auditService.LogActionAsync(null, "Create", $"Створено групу: {group.GroupCode}", "StudentGroup", group.GroupId);
        }

        public async Task UpdateGroupAsync(StudentGroup group)
        {
            using var context = new AppDbContext();
            var existing = await context.StudentGroups.FindAsync(group.GroupId);
            if (existing != null)
            {
                existing.GroupCode = group.GroupCode;
                existing.EntryYear = group.EntryYear;
                existing.SpecialtyId = group.SpecialtyId; 

                await context.SaveChangesAsync();
                await _auditService.LogActionAsync(null, "Update", $"Оновлено групу: {group.GroupCode}", "StudentGroup", group.GroupId);
            }
        }

        public async Task DeleteGroupAsync(int id)
        {
            using var context = new AppDbContext();
            var group = await context.StudentGroups.FindAsync(id);
            if (group != null)
            {
                string code = group.GroupCode;
                context.StudentGroups.Remove(group);
                await context.SaveChangesAsync();
                await _auditService.LogActionAsync(null, "Delete", $"Видалено групу: {code}", "StudentGroup", id);
            }
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            using var context = new AppDbContext();
            return await context.Departments.AsNoTracking().ToListAsync();
        }

        public async Task AddDepartmentAsync(Department department)
        {
            if (string.IsNullOrWhiteSpace(department.DepartmentName)) throw new ArgumentException("Назва кафедри обов'язкова!");
            using var context = new AppDbContext();
            context.Departments.Add(department);
            await context.SaveChangesAsync();
            await _auditService.LogActionAsync(null, "Create", $"Створено каф.: {department.DepartmentName}", "Department", department.DepartmentId);
        }

        public async Task UpdateDepartmentAsync(Department department)
        {
            using var context = new AppDbContext();
            var existing = await context.Departments.FindAsync(department.DepartmentId);
            if (existing != null)
            {
                existing.DepartmentName = department.DepartmentName;
                await context.SaveChangesAsync();
                await _auditService.LogActionAsync(null, "Update", $"Оновлено каф.: {department.DepartmentName}", "Department", department.DepartmentId);
            }
        }

        public async Task DeleteDepartmentAsync(int id)
        {
            using var context = new AppDbContext();
            var item = await context.Departments.FindAsync(id);
            if (item != null)
            {
                string name = item.DepartmentName;
                context.Departments.Remove(item);
                await context.SaveChangesAsync();
                await _auditService.LogActionAsync(null, "Delete", $"Видалено каф.: {name}", "Department", id);
            }
        }

        public async Task<IEnumerable<Position>> GetAllPositionsAsync()
        {
            using var context = new AppDbContext();
            return await context.Positions.AsNoTracking().ToListAsync();
        }

        public async Task AddPositionAsync(Position position)
        {
            if (string.IsNullOrWhiteSpace(position.PositionName)) throw new ArgumentException("Назва обов'язкова!");
            using var context = new AppDbContext();
            context.Positions.Add(position);
            await context.SaveChangesAsync();
            await _auditService.LogActionAsync(null, "Create", $"Створено посаду: {position.PositionName}", "Position", position.PositionId);
        }

        public async Task DeletePositionAsync(int id)
        {
            using var context = new AppDbContext();
            var item = await context.Positions.FindAsync(id);
            if (item != null)
            {
                context.Positions.Remove(item);
                await context.SaveChangesAsync();
                await _auditService.LogActionAsync(null, "Delete", $"Видалено посаду ID: {id}", "Position", id);
            }
        }

        public async Task<IEnumerable<Discipline>> GetAllDisciplinesAsync()
        {
            using var context = new AppDbContext();
            return await context.Disciplines.AsNoTracking().ToListAsync();
        }

        public async Task DeleteDisciplineAsync(int id)
        {
            using var context = new AppDbContext();
            var item = await context.Disciplines.FindAsync(id);
            if (item != null)
            {
                context.Disciplines.Remove(item);
                await context.SaveChangesAsync();
                await _auditService.LogActionAsync(null, "Delete", $"Видалено дисц. ID: {id}", "Discipline", id);
            }
        }

        public async Task UpdateOrganizationAsync(Organization org)
        {
            using var context = new AppDbContext();
            var existing = await context.Organizations.FindAsync(org.OrganizationId);
            if (existing != null)
            {
                existing.Name = org.Name;
                existing.Address = org.Address;
                existing.Type = org.Type;
                await context.SaveChangesAsync();
                await _auditService.LogActionAsync(null, "Update", $"Оновлено орг.: {org.Name}", "Organization", org.OrganizationId);
            }
        }

        public async Task DeleteOrganizationAsync(int id)
        {
            using var context = new AppDbContext();
            var item = await context.Organizations.FindAsync(id);
            if (item != null)
            {
                string name = item.Name;
                context.Organizations.Remove(item);
                await context.SaveChangesAsync();
                await _auditService.LogActionAsync(null, "Delete", $"Видалено орг. ID: {id}", "Organization", id);
            }
        }
    }
}