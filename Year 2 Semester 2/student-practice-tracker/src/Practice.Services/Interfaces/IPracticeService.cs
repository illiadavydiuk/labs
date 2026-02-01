using Practice.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Interfaces
{
    public interface IPracticeService
    {
        Task<IEnumerable<InternshipTopic>> GetAvailableTopicsAsync();
        Task<bool> AddTopicAsync(InternshipTopic topic);

        Task UpdateTopicAsync(InternshipTopic topic);
        Task DeleteTopicAsync(int topicId);

        Task<bool> AssignTopicAsync(int studentId, int topicId, int courseId, int supervisorId, string task);
        Task<InternshipAssignment?> GetStudentAssignmentAsync(int studentId);

        Task<IEnumerable<Organization>> GetAllOrganizationsAsync();
        Task<Organization> CreateOrganizationAsync(string name, string address, string type);
        Task DeleteOrganizationAsync(int id); 

        void CreateBackup(string path);
    }
}