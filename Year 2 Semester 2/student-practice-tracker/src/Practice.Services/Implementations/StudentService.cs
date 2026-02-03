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
        private readonly IInternshipAssignmentRepository _assignmentRepo;
        private readonly IInternshipTopicRepository _topicRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IReportRepository _reportRepo;
        private readonly IAuditLogRepository _logRepo;
        private readonly IAuditService _auditService;
        private readonly IRepository<Attachment> _attachmentRepo;

        public StudentService(
            IStudentRepository studentRepo,
            IInternshipAssignmentRepository assignRepo,
            IInternshipTopicRepository topicRepo,
            ICourseRepository courseRepo,
            IReportRepository reportRepo,
            IAuditLogRepository logRepo,
            IAuditService auditService,
            IRepository<Attachment> attachmentRepo)
        {
            _studentRepo = studentRepo;
            _assignmentRepo = assignRepo;
            _topicRepo = topicRepo;
            _courseRepo = courseRepo;
            _reportRepo = reportRepo;
            _logRepo = logRepo;
            _auditService = auditService;
            _attachmentRepo = attachmentRepo;
        }

        public async Task<Student?> GetStudentProfileAsync(int userId) => await _studentRepo.GetStudentProfileAsync(userId);
        public async Task<Student> GetStudentByEmailAsync(string email) => await _studentRepo.GetByEmailAsync(email);
        public async Task<List<InternshipAssignment>> GetStudentAssignmentsAsync(int studentId) => await _assignmentRepo.GetByStudentIdAsync(studentId);

        public async Task<List<Course>> GetEnrolledCoursesAsync(int studentId)
        {
            var student = await _studentRepo.GetByIdAsync(studentId);
            if (student == null) return new List<Course>();
            return await _studentRepo.GetEnrolledCoursesForStudentAsync(studentId, student.GroupId);
        }

        public async Task<InternshipAssignment?> GetAssignmentAsync(int studentId, int courseId) =>
            await _assignmentRepo.GetByStudentAndCourseAsync(studentId, courseId);

        public async Task<List<InternshipTopic>> GetAvailableTopicsAsync(int disciplineId, int? organizationId)
        {
            var allTopics = await _topicRepo.GetAvailableTopicsAsync();
            return allTopics.Where(t => t.DisciplineId == disciplineId && (!organizationId.HasValue || t.OrganizationId == organizationId)).ToList();
        }

        public async Task SelectTopicAsync(int studentId, int topicId, int courseId)
        {
            var assignment = await _assignmentRepo.GetByStudentAndCourseAsync(studentId, courseId);
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
                await _assignmentRepo.AddAssignmentWithTopicUpdateAsync(newAssign, topicId);
            }
            else
            {
                await _assignmentRepo.UpdateAssignmentTopicAsync(assignment, topicId);
            }
            await _assignmentRepo.SaveAsync();
            await _auditService.LogActionAsync(studentId, "SelectTopic", $"Студент обрав тему ID: {topicId}", "InternshipAssignment", topicId);
        }

        public async Task<List<AuditLog>> GetAssignmentHistoryAsync(int assignmentId) => await _logRepo.GetHistoryForAssignmentAsync(assignmentId);

        public async Task DeleteAttachmentAsync(int attachmentId)
        {
            var attachment = await _attachmentRepo.GetByIdAsync(attachmentId);
            if (attachment != null)
            {
                if (System.IO.File.Exists(attachment.FilePath)) try { System.IO.File.Delete(attachment.FilePath); } catch { }
                _attachmentRepo.Delete(attachment);
                await _attachmentRepo.SaveAsync();
            }
        }

        public async Task<List<Attachment>> GetAttachmentsByAssignmentIdAsync(int assignmentId)
        {
            var reports = await _reportRepo.GetReportsByAssignmentIdAsync(assignmentId);
            var attachments = new List<Attachment>();
            if (reports != null)
            {
                foreach (Report report in reports)
                {
                    if (report.Attachments != null) attachments.AddRange(report.Attachments);
                }
            }
            return attachments;
        }

        public async Task SubmitAssignmentAsync(int assignmentId, string comment, List<string> filePaths)
        {
            var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
            if (assignment == null) throw new Exception("Завдання не знайдено");

            assignment.StatusId = 2; // Здано
            _assignmentRepo.Update(assignment);

            var newReport = new Report
            {
                AssignmentId = assignmentId,
                StudentComment = comment,
                SubmissionDate = DateTime.Now,
                StatusId = 1,
                Attachments = new List<Attachment>()
            };

            if (filePaths != null && filePaths.Count > 0)
            {
                foreach (var path in filePaths)
                {
                    var attach = new Attachment
                    {
                        FileName = System.IO.Path.GetFileName(path),
                        FilePath = path,
                        FileType = System.IO.Path.GetExtension(path),
                        UploadedAt = DateTime.Now
                    };
                    newReport.Attachments.Add(attach);
                }
            }

            await _reportRepo.AddAsync(newReport);
            await _auditService.LogActionAsync(assignment.StudentId, "Student.Report.Submitted", $"Здано роботу ID {assignmentId} з {newReport.Attachments.Count} файлами", "Assignment", assignmentId);
            await _assignmentRepo.SaveAsync();
        }
    }
}