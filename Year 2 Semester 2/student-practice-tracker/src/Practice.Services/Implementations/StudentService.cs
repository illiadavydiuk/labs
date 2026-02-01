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
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;

        public StudentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Student?> GetStudentProfileAsync(int userId)
        {
            using var context = new AppDbContext();
            return await context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.StudentGroup)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<List<Course>> GetEnrolledCoursesAsync(int studentId)
        {
            using var context = new AppDbContext();
            return await context.CourseEnrollments
                .AsNoTracking()
                .Where(e => e.StudentId == studentId)
                .Select(e => e.Course)
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<InternshipAssignment?> GetAssignmentAsync(int studentId, int courseId)
        {
            using var context = new AppDbContext();
            return await context.InternshipAssignments
                .AsNoTracking()
                .Include(a => a.Supervisor).ThenInclude(s => s.User)
                .Include(a => a.InternshipTopic).ThenInclude(t => t.Organization)
                .Include(a => a.Reports).ThenInclude(r => r.Attachments)
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.CourseId == courseId);
        }

        public async Task<List<InternshipTopic>> GetAvailableTopicsAsync(int? disciplineId, int? organizationId = null)
        {
            using var context = new AppDbContext();
            var query = context.InternshipTopics
                .AsNoTracking()
                .Include(t => t.Organization)
                .Where(t => t.IsAvailable == true)
                .AsQueryable();

            if (disciplineId.HasValue && disciplineId.Value > 0)
                query = query.Where(t => t.DisciplineId == disciplineId.Value);

            if (organizationId.HasValue)
                query = query.Where(t => t.OrganizationId == organizationId.Value);

            return await query.ToListAsync();
        }

        public async Task SelectTopicAsync(int studentId, int topicId, int courseId)
        {
            using var context = new AppDbContext();

            // 1. Перевірка
            bool exists = await context.InternshipAssignments.AnyAsync(a => a.StudentId == studentId && a.CourseId == courseId);
            if (exists) throw new Exception("Ви вже обрали тему на цьому курсі.");

            // 2. Тема
            var topic = await context.InternshipTopics.FindAsync(topicId);
            if (topic == null || !topic.IsAvailable) throw new Exception("Тема недоступна або вже зайнята.");

            // 3. Курс (для керівника)
            var course = await context.Courses.FindAsync(courseId);
            if (course == null) throw new Exception("Курс не знайдено.");

            var assignment = new InternshipAssignment
            {
                StudentId = studentId,
                CourseId = courseId,
                TopicId = topicId,
                SupervisorId = course.SupervisorId, 
                StartDate = DateTime.UtcNow,
                StatusId = 2 // "В процесі"
            };

            topic.IsAvailable = false;
            context.InternshipTopics.Update(topic);
            context.InternshipAssignments.Add(assignment);

            await context.SaveChangesAsync();


            try
            {
                await LogHistoryAsync(context, assignment.AssignmentId, studentId, "Topic Selected", $"Обрано тему: {topic.Title}");
            }
            catch { }
        }

        public async Task SubmitReportAsync(int assignmentId, string comment, string link, List<Attachment> uiAttachments)
        {
            using var context = new AppDbContext();

            var assignment = await context.InternshipAssignments
                .Include(a => a.Reports).ThenInclude(r => r.Attachments)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null) throw new Exception("Призначення не знайдено");

            var report = assignment.Reports.OrderByDescending(r => r.SubmissionDate).FirstOrDefault();
            bool isNew = false;

            if (report == null || report.StatusId == 3) // Новий звіт
            {
                isNew = true;
                report = new Report
                {
                    AssignmentId = assignmentId,
                    StudentComment = comment,
                    SubmissionDate = DateTime.UtcNow,
                    StatusId = 1,
                    Attachments = new List<Attachment>()
                };
                context.Reports.Add(report);
            }
            else 
            {
                report.StudentComment = comment;
                report.SubmissionDate = DateTime.UtcNow;
                report.StatusId = 1;

                var oldFiles = report.Attachments.Where(a => a.FileType != "URL").ToList();
                context.Attachments.RemoveRange(oldFiles);
            }

            foreach (var att in uiAttachments)
            {
                report.Attachments.Add(new Attachment
                {
                    FileName = att.FileName,
                    FilePath = att.FilePath,
                    FileType = att.FileType
                });
            }

            if (!string.IsNullOrEmpty(link))
            {
                var existLink = report.Attachments.FirstOrDefault(a => a.FileType == "URL");
                if (existLink != null) existLink.FilePath = link;
                else report.Attachments.Add(new Attachment { FileName = "Link", FilePath = link, FileType = "URL" });
            }

            await context.SaveChangesAsync();

            try
            {
                string action = isNew ? "Report Submitted" : "Report Updated";
                await LogHistoryAsync(context, assignmentId, assignment.StudentId, action, "Студент надіслав звіт.");
            }
            catch { }
        }

        public async Task<List<AuditLog>> GetAssignmentHistoryAsync(int assignmentId)
        {
            using var context = new AppDbContext();
            return await context.AuditLogs
                .AsNoTracking()
                .Where(l => l.EntityName == "Assignment" && l.EntityId == assignmentId)
                .OrderByDescending(l => l.TimeStamp)
                .ToListAsync();
        }

        private async Task LogHistoryAsync(AppDbContext context, int assignmentId, int userId, string action, string details)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                Details = details,
                EntityName = "Assignment",
                EntityId = assignmentId,
                TimeStamp = DateTime.UtcNow
            };
            context.AuditLogs.Add(log);
            await context.SaveChangesAsync();
        }
    }
}