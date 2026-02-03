using Practice.Data.Entities;
using System.Collections;
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

        
        Task SubmitAssignmentAsync(int assignmentId, string comment, List<string> filePaths);

        Task<List<AuditLog>> GetAssignmentHistoryAsync(int assignmentId);
        Task DeleteAttachmentAsync(int attachmentId);
        Task<List<Attachment>> GetAttachmentsByAssignmentIdAsync(int assignmentId);
    }
}