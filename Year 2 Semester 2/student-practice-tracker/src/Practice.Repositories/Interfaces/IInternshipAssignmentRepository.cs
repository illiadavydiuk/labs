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
    }
}
