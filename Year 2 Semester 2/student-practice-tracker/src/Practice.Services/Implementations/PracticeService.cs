using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class PracticeService : IPracticeService
    {
        private readonly IInternshipTopicRepository _topicRepo;
        private readonly IInternshipAssignmentRepository _assignmentRepo;
        private readonly IAssignmentStatusRepository _statusRepo;
        private readonly IOrganizationRepository _orgRepo; 
        private readonly IAuditService _auditService;

        public PracticeService(
            IInternshipTopicRepository topicRepo,
            IInternshipAssignmentRepository assignmentRepo,
            IAssignmentStatusRepository statusRepo,
            IOrganizationRepository orgRepo,
            IAuditService auditService)
        {
            _topicRepo = topicRepo;
            _assignmentRepo = assignmentRepo;
            _statusRepo = statusRepo;
            _orgRepo = orgRepo;
            _auditService = auditService;
        }

        public async Task<IEnumerable<InternshipTopic>> GetAvailableTopicsAsync()
        {
            return await _topicRepo.GetAvailableTopicsAsync();
        }

        public async Task<bool> AddTopicAsync(InternshipTopic topic)
        {
            if (string.IsNullOrWhiteSpace(topic.Title)) throw new ArgumentException("Назва теми обов'язкова");

            topic.IsAvailable = true;
            _topicRepo.Add(topic);
            await _topicRepo.SaveAsync();
            return true;
        }

        public async Task<bool> AssignTopicAsync(int studentId, int topicId, int courseId, int supervisorId, string individualTask)
        {
            // Спроба знайти статус 'Assigned', якщо ні - беремо перший
            var status = await _statusRepo.GetByNameAsync("Assigned") ?? await _statusRepo.GetByIdAsync(1);

            var assignment = new InternshipAssignment
            {
                StudentId = studentId,
                TopicId = topicId,
                CourseId = courseId,
                StatusId = status?.StatusId ?? 1,
                SupervisorId = supervisorId,
                IndividualTask = individualTask,
                StartDate = DateTime.UtcNow
            };

            var topic = await _topicRepo.GetByIdAsync(topicId);
            if (topic != null)
            {
                topic.IsAvailable = false;
                _topicRepo.Update(topic);
            }

            _assignmentRepo.Add(assignment);
            await _assignmentRepo.SaveAsync();
            await _topicRepo.SaveAsync();
            await _auditService.LogActionAsync(null, "Assign", "Призначення теми практики", "InternshipAssignment", assignment.AssignmentId);
            return true;
        }

        public async Task<InternshipAssignment?> GetStudentAssignmentAsync(int studentId)
        {
            return await _assignmentRepo.GetActiveAssignmentAsync(studentId);
        }

        public async Task<IEnumerable<Organization>> GetAllOrganizationsAsync()
        {
            return await _orgRepo.GetAllAsync();
        }

        public async Task<Organization> CreateOrganizationAsync(string name, string address, string type)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Назва організації обов'язкова");

            var org = new Organization
            {
                Name = name,
                Address = address ?? "",
                Type = type ?? "Company",
                ContactEmail = ""
            };
            _orgRepo.Add(org);
            await _orgRepo.SaveAsync();
            return org;
        }

        public void CreateBackup(string destinationPath)
        {
            string folderName = "StudentPracticePlatform";
            string dbPath = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderName, "practice_platform.db");
            }
            else
            {
                dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share", folderName, "practice_platform.db");
            }

            if (File.Exists(dbPath))
            {
                File.Copy(dbPath, destinationPath, true);
            }
        }
    }
}