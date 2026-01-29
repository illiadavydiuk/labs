using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Services.Interfaces
{
    public interface IPracticeService
    {
        Task<IEnumerable<InternshipTopic>> GetAvailableTopicsAsync();

        Task<bool> AddTopicAsync(InternshipTopic topic);

        Task<bool> AssignTopicAsync(int studentId, int topicId, int courseId, int supervisorId, string individualTask);

        Task<InternshipAssignment?> GetStudentAssignmentAsync(int studentId);
    }
}
