using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class SupervisorService : ISupervisorService
    {
        private readonly AppDbContext _context;

        public SupervisorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<InternshipAssignment>> GetStudentsForSupervisorAsync(int supervisorId)
        {
            using var context = new AppDbContext();

            return await context.InternshipAssignments
                .AsNoTracking()
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.Student).ThenInclude(s => s.StudentGroup)
                .Include(a => a.InternshipTopic).ThenInclude(t => t.Organization)
                .Include(a => a.Course)
                .Include(a => a.Reports)
                .Where(a => a.SupervisorId == supervisorId)
                .ToListAsync();
        }

        public async Task<InternshipAssignment?> GetAssignmentDetailsAsync(int assignmentId)
        {
            using var context = new AppDbContext();
            return await context.InternshipAssignments
                .AsNoTracking()
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.InternshipTopic)
                .Include(a => a.Reports).ThenInclude(r => r.Attachments)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);
        }

        public async Task<Supervisor?> GetSupervisorProfileAsync(int userId)
        {
            using var context = new AppDbContext();
            return await context.Supervisors
                .AsNoTracking()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task SaveAssessmentAsync(int assignmentId, string feedback, int? grade, int? statusId)
        {
            using var context = new AppDbContext();

            var assignment = await context.InternshipAssignments
                .Include(a => a.Reports)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null) throw new Exception("Призначення не знайдено");

            if (grade.HasValue) assignment.FinalGrade = grade.Value;
            if (statusId.HasValue) assignment.StatusId = statusId.Value;

            var lastReport = assignment.Reports.OrderByDescending(r => r.SubmissionDate).FirstOrDefault();

            if (lastReport != null)
            {
                if (!string.IsNullOrEmpty(feedback))
                {
                    lastReport.SupervisorFeedback = feedback;
                }

                if (statusId == 3)
                {
                    lastReport.StatusId = 3; // Прийнято
                }
            }

            await context.SaveChangesAsync();
        }
    }
}