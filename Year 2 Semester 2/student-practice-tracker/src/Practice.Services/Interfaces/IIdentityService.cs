using Practice.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Interfaces
{
    public interface IIdentityService
    {
        Task<User?> LoginAsync(string email, string password);

        // Реєстрація з валідацією
        Task<bool> RegisterStudentAsync(User user, string password, int groupId, string recordBook);
        Task<bool> RegisterSupervisorAsync(User user, string password, int? departmentId, int? positionId, string? phone);

        // Методи для довідників (Dropdowns)
        Task<IEnumerable<StudentGroup>> GetAllGroupsAsync();
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();

        // Методи адміністрування
        Task<StudentGroup> CreateGroupAsync(string code, int specialtyId, int year);
        Task<Department> CreateDepartmentAsync(string name);
    }
}