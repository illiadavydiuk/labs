using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IAuditService _auditService;
        private readonly IStudentRepository _studentRepo;
        private readonly IUserRepository _userRepo;
        private readonly ISupervisorRepository _supervisorRepo;
        private readonly ISpecialtyRepository _specialtyRepo;
        private readonly IStudentGroupRepository _groupRepo;
        private readonly IDepartmentRepository _deptRepo;
        private readonly IPositionRepository _posRepo;
        private readonly IDisciplineRepository _discRepo;
        private readonly IOrganizationRepository _orgRepo;

        public AdminService(
            IAuditService auditService,
            IStudentRepository studentRepo,
            IUserRepository userRepo,
            ISupervisorRepository supervisorRepo,
            ISpecialtyRepository specialtyRepo,
            IStudentGroupRepository groupRepo,
            IDepartmentRepository deptRepo,
            IPositionRepository posRepo,
            IDisciplineRepository discRepo,
            IOrganizationRepository orgRepo)
        {
            _auditService = auditService;
            _studentRepo = studentRepo;
            _userRepo = userRepo;
            _supervisorRepo = supervisorRepo;
            _specialtyRepo = specialtyRepo;
            _groupRepo = groupRepo;
            _deptRepo = deptRepo;
            _posRepo = posRepo;
            _discRepo = discRepo;
            _orgRepo = orgRepo;
        }

        // --- Students ---
        public async Task<List<Student>> GetStudentsByGroupAsync(int? groupId)
        {
            var students = await _studentRepo.GetStudentsByGroupAsync(groupId);
            return new List<Student>(students);
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            return await _studentRepo.GetStudentDetailsAsync(studentId);
        }

        public async Task UpdateStudentAsync(int studentId, string firstName, string lastName, string email, string recordBook, int groupId)
        {
            var student = await _studentRepo.GetStudentDetailsAsync(studentId);
            if (student == null) throw new Exception("Студента не знайдено");

            student.User.FirstName = firstName;
            student.User.LastName = lastName;
            student.User.Email = email;
            student.RecordBookNumber = recordBook;
            student.GroupId = groupId;

            _studentRepo.Update(student);
            await _studentRepo.SaveAsync();
            await _auditService.LogActionAsync(null, "Update", $"Оновлено студента: {email}", "Student", studentId);
        }

        public async Task DeleteStudentAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user != null)
            {
                string email = user.Email;
                _userRepo.Delete(user);
                await _userRepo.SaveAsync();
                await _auditService.LogActionAsync(null, "Delete", $"Видалено студента: {email}", "User", userId);
            }
        }

        // --- Supervisors ---
        public async Task<List<Supervisor>> GetSupervisorsByDeptAsync(int? deptId)
        {
            var sups = await _supervisorRepo.GetSupervisorsByDepartmentAsync(deptId);
            return new List<Supervisor>(sups);
        }

        public async Task<Supervisor?> GetSupervisorByIdAsync(int supervisorId)
        {
            return await _supervisorRepo.GetSupervisorDetailsAsync(supervisorId);
        }

        public async Task UpdateSupervisorAsync(int supervisorId, string firstName, string lastName, string email, string phone, int deptId, int? posId)
        {
            var sup = await _supervisorRepo.GetSupervisorDetailsAsync(supervisorId);
            if (sup == null) throw new Exception("Керівника не знайдено");

            sup.User.FirstName = firstName;
            sup.User.LastName = lastName;
            sup.User.Email = email;
            sup.Phone = phone;
            sup.DepartmentId = deptId;
            sup.PositionId = posId;

            _supervisorRepo.Update(sup);
            await _supervisorRepo.SaveAsync();
            await _auditService.LogActionAsync(null, "Update", $"Оновлено керівника: {email}", "Supervisor", supervisorId);
        }

        public async Task DeleteSupervisorAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user != null)
            {
                string email = user.Email;
                _userRepo.Delete(user);
                await _userRepo.SaveAsync();
                await _auditService.LogActionAsync(null, "Delete", $"Видалено керівника: {email}", "User", userId);
            }
        }

        // --- Organization Update ---
        public async Task UpdateOrganizationAsync(Organization org)
        {
            var existing = await _orgRepo.GetByIdAsync(org.OrganizationId);
            if (existing != null)
            {
                existing.Name = org.Name;
                existing.Address = org.Address;
                existing.Type = org.Type;
                existing.ContactEmail = org.ContactEmail;

                _orgRepo.Update(existing);
                await _orgRepo.SaveAsync();
                await _auditService.LogActionAsync(null, "Update", $"Оновлено орг.: {org.Name}", "Organization", org.OrganizationId);
            }
        }

        public async Task DeleteOrganizationAsync(int id)
        {
            var ex = await _orgRepo.GetByIdAsync(id);
            if (ex != null) { _orgRepo.Delete(ex); await _orgRepo.SaveAsync(); }
        }

        // --- Dictionaries ---
        public async Task<IEnumerable<Specialty>> GetAllSpecialtiesAsync() => await _specialtyRepo.GetAllAsync();
        public async Task AddSpecialtyAsync(Specialty s) { _specialtyRepo.Add(s); await _specialtyRepo.SaveAsync(); }
        public async Task UpdateSpecialtyAsync(Specialty s)
        {
            var ex = await _specialtyRepo.GetByIdAsync(s.SpecialtyId);
            if (ex != null) { ex.Name = s.Name; ex.Code = s.Code; ex.DepartmentId = s.DepartmentId; _specialtyRepo.Update(ex); await _specialtyRepo.SaveAsync(); }
        }
        public async Task DeleteSpecialtyAsync(int id) { var item = await _specialtyRepo.GetByIdAsync(id); if (item != null) { _specialtyRepo.Delete(item); await _specialtyRepo.SaveAsync(); } }

        public async Task<IEnumerable<StudentGroup>> GetAllGroupsAsync() => await _groupRepo.GetAllAsync();
        public async Task AddGroupAsync(StudentGroup g) { _groupRepo.Add(g); await _groupRepo.SaveAsync(); }
        public async Task UpdateGroupAsync(StudentGroup g)
        {
            var ex = await _groupRepo.GetByIdAsync(g.GroupId);
            if (ex != null) { ex.GroupCode = g.GroupCode; ex.EntryYear = g.EntryYear; ex.SpecialtyId = g.SpecialtyId; _groupRepo.Update(ex); await _groupRepo.SaveAsync(); }
        }
        public async Task DeleteGroupAsync(int id) { var ex = await _groupRepo.GetByIdAsync(id); if (ex != null) { _groupRepo.Delete(ex); await _groupRepo.SaveAsync(); } }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync() => await _deptRepo.GetAllAsync();
        public async Task AddDepartmentAsync(Department d) 
        { 
            _deptRepo.Add(d); 
            await _deptRepo.SaveAsync(); 
        }
        public async Task UpdateDepartmentAsync(Department d)
        {
            var ex = await _deptRepo.GetByIdAsync(d.DepartmentId); 
            if (ex != null) 
            { 
                ex.DepartmentName = d.DepartmentName; 
                _deptRepo.Update(ex); 
                await _deptRepo.SaveAsync(); 
            } 
        }
        public async Task DeleteDepartmentAsync(int id)
        { 
            var ex = await _deptRepo.GetByIdAsync(id); 
            if (ex != null) 
            { 
                _deptRepo.Delete(ex); 
                await _deptRepo.SaveAsync(); 
            } 
        }

        public async Task<IEnumerable<Position>> GetAllPositionsAsync() => await _posRepo.GetAllAsync();
        public async Task AddPositionAsync(Position p) 
        { 
            _posRepo.Add(p); 
            await _posRepo.SaveAsync(); 
        }
        public async Task DeletePositionAsync(int id) 
        { 
            var ex = await _posRepo.GetByIdAsync(id); 
            if (ex != null) 
            { _posRepo.Delete(ex); 
                await _posRepo.SaveAsync(); 
            } 
        }

        public async Task<IEnumerable<Discipline>> GetAllDisciplinesAsync() => await _discRepo.GetAllAsync();
        public async Task DeleteDisciplineAsync(int id) 
        { 
            var ex = await _discRepo.GetByIdAsync(id); 
            if (ex != null) 
            { 
                _discRepo.Delete(ex); 
                await _discRepo.SaveAsync(); 
            } 
        }
    }
}