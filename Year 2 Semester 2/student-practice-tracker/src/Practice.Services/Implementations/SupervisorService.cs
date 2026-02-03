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
                .AsNoTracking() 
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.Student).ThenInclude(s => s.StudentGroup)
                .Include(a => a.Course)
                .Include(a => a.InternshipTopic).ThenInclude(t => t.Organization)
                .Include(a => a.Reports)
                    .ThenInclude(r => r.Attachments)
                .Where(a => a.SupervisorId == supervisorId || a.Course.SupervisorId == supervisorId)
                .ToListAsync();
        }

        public async Task<InternshipAssignment?> GetAssignmentDetailsAsync(int assignmentId)
        {
            return await _context.InternshipAssignments
                .AsNoTracking()
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.Student).ThenInclude(s => s.StudentGroup)
                .Include(a => a.Course)
                .Include(a => a.InternshipTopic).ThenInclude(t => t.Organization)
                .Include(a => a.Reports)
                    .ThenInclude(r => r.Attachments)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);
        }
        public async Task SaveAssessmentAsync(int assignmentId, string feedback, int? finalGrade, int? reportStatusId, int? companyGrade, string companyFeedback)
        {
            var assignment = await _context.InternshipAssignments
                .Include(a => a.Reports)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null) return;

            assignment.FinalGrade = finalGrade;
            assignment.CompanyGrade = companyGrade;
            assignment.CompanyFeedback = companyFeedback;

            if (reportStatusId.HasValue)
            {
                var lastReport = assignment.Reports?
                    .OrderByDescending(r => r.SubmissionDate)
                    .FirstOrDefault();

                if (lastReport != null)
                {
                    lastReport.StatusId = reportStatusId.Value;
                    lastReport.SupervisorFeedback = feedback;
                    lastReport.ReviewDate = DateTime.Now;

                    if (reportStatusId.Value == 3)
                    {
                        assignment.StatusId = 3;
                    }
                    else if (reportStatusId.Value == 2)
                    {
                        assignment.StatusId = 2;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<List<InternshipAssignment>> GetMyStudentsAssignmentsAsync(int supervisorId)
        {
            return await _assignRepo.GetBySupervisorIdAsync(supervisorId);
        }
    }
}