using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class SupervisorService : ISupervisorService
    {
        private readonly ISupervisorRepository _supervisorRepo;
        private readonly IInternshipAssignmentRepository _assignRepo;
        private readonly IReportRepository _reportRepo;
        private readonly AppDbContext _context;

        public SupervisorService(
            ISupervisorRepository supervisorRepo,
            IInternshipAssignmentRepository assignRepo,
            IReportRepository reportRepo,
            AppDbContext context)
        {
            _supervisorRepo = supervisorRepo;
            _assignRepo = assignRepo;
            _reportRepo = reportRepo;
            _context = context;
        }

        public async Task<Supervisor?> GetSupervisorProfileAsync(int userId)
        {
            return await _supervisorRepo.GetSupervisorProfileAsync(userId);
        }

        public async Task<List<InternshipAssignment>> GetStudentsForSupervisorAsync(int supervisorId)
        {
            return await _context.InternshipAssignments
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.Student).ThenInclude(s => s.StudentGroup)
                .Include(a => a.Course)
                .Include(a => a.InternshipTopic).ThenInclude(t => t.Organization)
                .Include(a => a.Reports)
                .Where(a => a.SupervisorId == supervisorId || a.Course.SupervisorId == supervisorId)
                .ToListAsync();
        }

        public async Task<InternshipAssignment?> GetAssignmentDetailsAsync(int assignmentId)
        {
            return await _context.InternshipAssignments
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.Student).ThenInclude(s => s.StudentGroup)
                .Include(a => a.Course)
                .Include(a => a.InternshipTopic).ThenInclude(t => t.Organization)
                .Include(a => a.Reports)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);
        }

        public async Task SaveAssessmentAsync(int assignmentId, string feedback, int? finalGrade, int? reportStatusId, int? companyGrade, string companyFeedback)
        {
            var assignment = await _assignRepo.GetByIdAsync(assignmentId);
            if (assignment == null) return;

            assignment.FinalGrade = finalGrade;
            assignment.CompanyGrade = companyGrade;
            assignment.CompanyFeedback = companyFeedback;

            if (reportStatusId.HasValue)
            {
                var reports = await _reportRepo.GetAllAsync();
                var lastReport = reports.Where(r => r.AssignmentId == assignmentId).OrderByDescending(r => r.SubmissionDate).FirstOrDefault();

                if (lastReport != null)
                {
                    lastReport.StatusId = reportStatusId.Value;
                    lastReport.SupervisorFeedback = feedback;
                    lastReport.ReviewDate = DateTime.Now;
                    _reportRepo.Update(lastReport);
                }
            }

            _assignRepo.Update(assignment);
            await _assignRepo.SaveAsync();
        }
    }
}