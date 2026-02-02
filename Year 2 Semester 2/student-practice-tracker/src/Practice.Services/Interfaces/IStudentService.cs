using Practice.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Interfaces
{
    public interface IStudentService
    {
        Task<Student?> GetStudentProfileAsync(int userId);
        Task<List<Course>> GetEnrolledCoursesAsync(int studentId);
        Task<InternshipAssignment?> GetAssignmentAsync(int studentId, int courseId);
        Task<List<InternshipTopic>> GetAvailableTopicsAsync(int disciplineId, int? orgId);

        Task SelectTopicAsync(int studentId, int topicId, int courseId);

        Task SubmitReportAsync(int assignmentId, string comment, string link, List<Attachment> files);

        Task<List<AuditLog>> GetAssignmentHistoryAsync(int assignmentId);
    }
}