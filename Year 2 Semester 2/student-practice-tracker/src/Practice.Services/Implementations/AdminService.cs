using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context; // Використовуємо контекст напряму для адмінки для швидкості CRUD
        private readonly IAuditService _auditService;

        public AdminService(AppDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // --- SPECIALTIES ---
        public async Task<IEnumerable<Specialty>> GetAllSpecialtiesAsync() => await _context.Specialties.ToListAsync();

        public async Task AddSpecialtyAsync(Specialty specialty)
        {
            if (string.IsNullOrWhiteSpace(specialty.Name) || string.IsNullOrWhiteSpace(specialty.Code))
                throw new ArgumentException("Назва та Код спеціальності обов'язкові!");

            _context.Specialties.Add(specialty);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSpecialtyAsync(Specialty specialty)
        {
            _context.Specialties.Update(specialty);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSpecialtyAsync(int id)
        {
            var item = await _context.Specialties.FindAsync(id);
            if (item != null)
            {
                _context.Specialties.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        // --- GROUPS ---
        public async Task<IEnumerable<StudentGroup>> GetAllGroupsAsync() =>
            await _context.StudentGroups.Include(g => g.Specialty).ToListAsync();

        public async Task AddGroupAsync(StudentGroup group)
        {
            if (string.IsNullOrWhiteSpace(group.GroupCode)) throw new ArgumentException("Шифр групи обов'язковий!");
            if (group.SpecialtyId == 0) throw new ArgumentException("Оберіть спеціальність!");

            _context.StudentGroups.Add(group);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGroupAsync(StudentGroup group)
        {
            _context.StudentGroups.Update(group);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGroupAsync(int id)
        {
            var group = await _context.StudentGroups.FindAsync(id);
            if (group != null)
            {
                _context.StudentGroups.Remove(group);
                await _context.SaveChangesAsync();
            }
        }

        // --- DEPARTMENTS ---
        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync() => await _context.Departments.ToListAsync();

        public async Task AddDepartmentAsync(Department department)
        {
            if (string.IsNullOrWhiteSpace(department.DepartmentName)) throw new ArgumentException("Назва кафедри обов'язкова!");
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDepartmentAsync(Department department)
        {
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDepartmentAsync(int id)
        {
            var item = await _context.Departments.FindAsync(id);
            if (item != null)
            {
                _context.Departments.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        // --- POSITIONS ---
        public async Task<IEnumerable<Position>> GetAllPositionsAsync() => await _context.Positions.ToListAsync();

        public async Task AddPositionAsync(Position position)
        {
            if (string.IsNullOrWhiteSpace(position.PositionName)) throw new ArgumentException("Назва посади обов'язкова!");
            _context.Positions.Add(position);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePositionAsync(int id)
        {
            var item = await _context.Positions.FindAsync(id);
            if (item != null)
            {
                _context.Positions.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        // --- DISCIPLINES ---
        public async Task<IEnumerable<Discipline>> GetAllDisciplinesAsync() => await _context.Disciplines.ToListAsync();

        public async Task AddDisciplineAsync(Discipline discipline)
        {
            if (string.IsNullOrWhiteSpace(discipline.DisciplineName)) throw new ArgumentException("Назва дисципліни обов'язкова!");
            _context.Disciplines.Add(discipline);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDisciplineAsync(int id)
        {
            var item = await _context.Disciplines.FindAsync(id);
            if (item != null)
            {
                _context.Disciplines.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        // --- ORGANIZATIONS ---
        public async Task<IEnumerable<Organization>> GetAllOrganizationsAsync() => await _context.Organizations.ToListAsync();

        public async Task AddOrganizationAsync(Organization org)
        {
            if (string.IsNullOrWhiteSpace(org.Name)) throw new ArgumentException("Назва організації обов'язкова!");
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrganizationAsync(Organization org)
        {
            _context.Organizations.Update(org);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrganizationAsync(int id)
        {
            var item = await _context.Organizations.FindAsync(id);
            if (item != null)
            {
                _context.Organizations.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Student>> GetStudentsByGroupAsync(int? groupId)
        {
            var query = _context.Students
                .Include(s => s.User)
                .Include(s => s.StudentGroup)
                .AsQueryable();

            if (groupId.HasValue && groupId.Value > 0)
            {
                query = query.Where(s => s.GroupId == groupId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<List<Supervisor>> GetSupervisorsByDeptAsync(int? deptId)
        {
            var query = _context.Supervisors
                .Include(s => s.User)
                .Include(s => s.Department)
                .AsQueryable();

            if (deptId.HasValue && deptId.Value > 0)
            {
                query = query.Where(s => s.DepartmentId == deptId.Value);
            }

            return await query.ToListAsync();
        }
    }
}