using Practice.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Interfaces
{
    public interface IAdminService
    {
        // Спеціальності
        Task<IEnumerable<Specialty>> GetAllSpecialtiesAsync();
        Task AddSpecialtyAsync(Specialty specialty);
        Task UpdateSpecialtyAsync(Specialty specialty);
        Task DeleteSpecialtyAsync(int id);

        // Групи (CRUD)
        Task<IEnumerable<StudentGroup>> GetAllGroupsAsync();
        Task AddGroupAsync(StudentGroup group);
        Task UpdateGroupAsync(StudentGroup group);
        Task DeleteGroupAsync(int id);

        // Кафедри
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();
        Task AddDepartmentAsync(Department department);
        Task UpdateDepartmentAsync(Department department);
        Task DeleteDepartmentAsync(int id);

        // Позиції (Посади)
        Task<IEnumerable<Position>> GetAllPositionsAsync();
        Task AddPositionAsync(Position position);
        Task DeletePositionAsync(int id);

        // Дисципліни
        Task<IEnumerable<Discipline>> GetAllDisciplinesAsync();
        Task AddDisciplineAsync(Discipline discipline);
        Task DeleteDisciplineAsync(int id);

        // Організації (Повний CRUD)
        Task<IEnumerable<Organization>> GetAllOrganizationsAsync();
        Task AddOrganizationAsync(Organization org);
        Task UpdateOrganizationAsync(Organization org);
        Task DeleteOrganizationAsync(int id);
        
        
        Task<List<Student>> GetStudentsByGroupAsync(int? groupId);
        Task<List<Supervisor>> GetSupervisorsByDeptAsync(int? deptId);
    }
}