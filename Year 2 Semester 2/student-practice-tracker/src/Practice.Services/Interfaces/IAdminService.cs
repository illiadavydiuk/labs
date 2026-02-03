using Practice.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Interfaces
{
    public interface IAdminService
    {
        // --- Студенти ---
        Task<List<Student>> GetStudentsByGroupAsync(int? groupId);
        Task<Student?> GetStudentByIdAsync(int studentId);
        Task UpdateStudentAsync(int studentId, string firstName, string lastName, string email, string recordBook, int groupId);
        Task DeleteStudentAsync(int userId);

        // --- Викладачі ---
        Task<List<Supervisor>> GetSupervisorsByDeptAsync(int? deptId);
        Task<Supervisor?> GetSupervisorByIdAsync(int supervisorId);
        Task UpdateSupervisorAsync(int supervisorId, string firstName, string lastName, string email, string phone, int deptId, int? posId);
        Task DeleteSupervisorAsync(int userId);

        // --- Спеціальності ---
        Task<IEnumerable<Specialty>> GetAllSpecialtiesAsync();
        Task AddSpecialtyAsync(Specialty specialty);
        Task UpdateSpecialtyAsync(Specialty specialty);
        Task DeleteSpecialtyAsync(int id);

        // --- Групи ---
        Task<IEnumerable<StudentGroup>> GetAllGroupsAsync();
        Task AddGroupAsync(StudentGroup group);
        Task UpdateGroupAsync(StudentGroup group);
        Task DeleteGroupAsync(int id);

        // --- Кафедри ---
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();
        Task AddDepartmentAsync(Department department);
        Task UpdateDepartmentAsync(Department department);
        Task DeleteDepartmentAsync(int id);

        // --- Посади ---
        Task<IEnumerable<Position>> GetAllPositionsAsync();
        Task AddPositionAsync(Position position);
        Task UpdatePositionAsync(Position position); 
        Task DeletePositionAsync(int id);

        // --- Дисципліни ---
        Task<IEnumerable<Discipline>> GetAllDisciplinesAsync();
        Task UpdateDisciplineAsync(Discipline discipline);
        Task DeleteDisciplineAsync(int id);

        // --- Організації ---
        Task UpdateOrganizationAsync(Organization org);
        Task DeleteOrganizationAsync(int id);
    }
}