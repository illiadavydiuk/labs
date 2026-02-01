using Practice.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Interfaces
{
    public interface IAdminService
    {
        Task<List<Student>> GetStudentsByGroupAsync(int? groupId);
        Task<Student?> GetStudentByIdAsync(int studentId);
        Task UpdateStudentAsync(int studentId, string firstName, string lastName, string email, string recordBook, int groupId);
        Task DeleteStudentAsync(int userId); 

        Task<List<Supervisor>> GetSupervisorsByDeptAsync(int? deptId);
        Task<Supervisor?> GetSupervisorByIdAsync(int supervisorId);
        Task UpdateSupervisorAsync(int supervisorId, string firstName, string lastName, string email, string phone, int deptId, int? posId);
        Task DeleteSupervisorAsync(int userId);

        Task<IEnumerable<Specialty>> GetAllSpecialtiesAsync();
        Task AddSpecialtyAsync(Specialty specialty);
        Task UpdateSpecialtyAsync(Specialty specialty);
        Task DeleteSpecialtyAsync(int id);

        Task<IEnumerable<StudentGroup>> GetAllGroupsAsync();
        Task AddGroupAsync(StudentGroup group);
        Task UpdateGroupAsync(StudentGroup group);
        Task DeleteGroupAsync(int id);

        Task<IEnumerable<Department>> GetAllDepartmentsAsync();
        Task AddDepartmentAsync(Department department);
        Task UpdateDepartmentAsync(Department department);
        Task DeleteDepartmentAsync(int id);

        Task<IEnumerable<Position>> GetAllPositionsAsync();
        Task AddPositionAsync(Position position);
        Task DeletePositionAsync(int id);

        Task<IEnumerable<Discipline>> GetAllDisciplinesAsync(); 
        Task DeleteDisciplineAsync(int id);

        Task UpdateOrganizationAsync(Organization org);
        Task DeleteOrganizationAsync(int id);
    }
}