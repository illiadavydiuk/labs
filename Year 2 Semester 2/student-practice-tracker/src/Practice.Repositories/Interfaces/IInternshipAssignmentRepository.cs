using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Interfaces
{
    public interface IInternshipAssignmentRepository : IRepository<InternshipAssignment>
    {
        // Отримати призначення студента з усіма деталями (тема, керівник, курс)
        Task<InternshipAssignment> GetFullAssignmentDetailsAsync(int assignmentId);

        // Список усіх призначень для конкретного курсу (для відомості)
        Task<IEnumerable<InternshipAssignment>> GetAssignmentsByCourseAsync(int courseId);
    }
}
