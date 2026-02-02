using Practice.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Interfaces
{
    public interface ISupervisorService
    {
        Task<Supervisor?> GetSupervisorProfileAsync(int userId);
        Task<List<InternshipAssignment>> GetStudentsForSupervisorAsync(int supervisorId);
        Task<InternshipAssignment?> GetAssignmentDetailsAsync(int assignmentId);
        Task SaveAssessmentAsync(int assignmentId, string feedback, int? finalGrade, int? reportStatusId, int? companyGrade, string companyFeedback);
    }
}