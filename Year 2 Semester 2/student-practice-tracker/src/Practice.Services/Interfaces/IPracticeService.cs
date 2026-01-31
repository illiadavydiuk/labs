using Practice.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Interfaces
{
    public interface IPracticeService
    {
        Task<IEnumerable<InternshipTopic>> GetAvailableTopicsAsync();
        Task<bool> AddTopicAsync(InternshipTopic topic);
        Task<bool> AssignTopicAsync(int studentId, int topicId, int courseId, int supervisorId, string individualTask);
        Task<InternshipAssignment?> GetStudentAssignmentAsync(int studentId);

        // Методи для Організацій
        Task<IEnumerable<Organization>> GetAllOrganizationsAsync();
        Task<Organization> CreateOrganizationAsync(string name, string address, string type);

        // Бекап
        void CreateBackup(string destinationPath);
    }
}