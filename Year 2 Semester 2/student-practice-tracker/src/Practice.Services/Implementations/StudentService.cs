using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepo;
        private readonly IInternshipAssignmentRepository _assignRepo;
        private readonly IInternshipTopicRepository _topicRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IReportRepository _reportRepo;
        private readonly IAuditLogRepository _logRepo;
        private readonly IAuditService _auditService;

        public StudentService(
            IStudentRepository studentRepo,
            IInternshipAssignmentRepository assignRepo,
            IInternshipTopicRepository topicRepo,
            ICourseRepository courseRepo,
            IReportRepository reportRepo,
            IAuditLogRepository logRepo,
            IAuditService auditService)
        {
            _studentRepo = studentRepo;
            _assignRepo = assignRepo;
            _topicRepo = topicRepo;
            _courseRepo = courseRepo;
            _reportRepo = reportRepo;
            _logRepo = logRepo;
            _auditService = auditService;
        }

        public async Task<Student?> GetStudentProfileAsync(int userId) => await _studentRepo.GetStudentProfileAsync(userId);

        public async Task<List<Course>> GetEnrolledCoursesAsync(int studentId)
        {
            var student = await _studentRepo.GetByIdAsync(studentId);
            if (student == null) return new List<Course>();
            return await _studentRepo.GetEnrolledCoursesForStudentAsync(studentId, student.GroupId);
        }

        public async Task<InternshipAssignment?> GetAssignmentAsync(int studentId, int courseId) =>
            await _assignRepo.GetByStudentAndCourseAsync(studentId, courseId);

        public async Task<List<InternshipTopic>> GetAvailableTopicsAsync(int disciplineId, int? organizationId)
        {
            var allTopics = await _topicRepo.GetAllAsync();
            return allTopics
                .Where(t => t.IsAvailable && t.DisciplineId == disciplineId && (!organizationId.HasValue || t.OrganizationId == organizationId))
                .ToList();
        }

        public async Task SelectTopicAsync(int studentId, int topicId, int courseId)
        {
            var assignment = await _assignRepo.GetByStudentAndCourseAsync(studentId, courseId);

            if (assignment == null)
            {
                var course = await _courseRepo.GetByIdAsync(courseId);
                var newAssign = new InternshipAssignment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    TopicId = topicId,
                    StatusId = 1,
                    SupervisorId = course?.SupervisorId,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(1)
                };

                await _assignRepo.AddAssignmentWithTopicUpdateAsync(newAssign, topicId);
            }
            else
            {
                await _assignRepo.UpdateAssignmentTopicAsync(assignment, topicId);
            }

            await _assignRepo.SaveAsync();
            await _auditService.LogActionAsync(null, "SelectTopic", $"Студент обрав тему ID: {topicId}", "InternshipAssignment", topicId);
        }

        public async Task SubmitReportAsync(int assignmentId, string comment, string link, List<Attachment> files)
        {
            var report = new Report
            {
                AssignmentId = assignmentId,
                StudentComment = comment,
                SubmissionDate = DateTime.Now,
                StatusId = 1,
                Attachments = new List<Attachment>()
            };
            if (files != null) foreach (var f in files) { f.AttachmentId = 0; report.Attachments.Add(f); }
            if (!string.IsNullOrEmpty(link)) report.Attachments.Add(new Attachment { FilePath = link, FileType = "URL", FileName = "Посилання" });

            _reportRepo.Add(report);
            await _reportRepo.SaveAsync();
        }

        public async Task<List<AuditLog>> GetAssignmentHistoryAsync(int assignmentId) =>
            await _logRepo.GetHistoryForAssignmentAsync(assignmentId);
    }
}