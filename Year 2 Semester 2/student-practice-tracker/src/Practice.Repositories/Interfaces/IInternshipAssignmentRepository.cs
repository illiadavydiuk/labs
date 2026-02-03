using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Interfaces
{
    public interface IInternshipAssignmentRepository : IRepository<InternshipAssignment>
    {
        Task<InternshipAssignment> GetFullAssignmentDetailsAsync(int assignmentId);
        Task<IEnumerable<InternshipAssignment>> GetAssignmentsByCourseAsync(int courseId);
        Task<InternshipAssignment?> GetActiveAssignmentAsync(int studentId);

        Task<IEnumerable<InternshipAssignment>> GetBySupervisorWithDetailsAsync(int supervisorId);
        Task<InternshipAssignment?> GetByStudentAndCourseWithDetailsAsync(int studentId, int courseId);
        Task<InternshipAssignment?> GetByStudentAndCourseAsync(int studentId, int courseId);
        Task AddAssignmentWithTopicUpdateAsync(InternshipAssignment assignment, int topicId);
        Task UpdateAssignmentTopicAsync(InternshipAssignment assignment, int newTopicId);
        Task<List<InternshipAssignment>> GetByStudentIdAsync(int studentId);
        Task<List<InternshipAssignment>> GetBySupervisorIdAsync(int supervisorId);
    }
}
