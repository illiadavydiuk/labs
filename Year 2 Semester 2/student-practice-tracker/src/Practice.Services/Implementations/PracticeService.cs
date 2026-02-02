using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq; // Додано для LINQ
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
            return await _topicRepo.GetAllAsync();
        }

        public async Task<bool> AddTopicAsync(InternshipTopic topic)
        {
            topic.IsAvailable = true;
            _topicRepo.Add(topic);
            await _topicRepo.SaveAsync();
            await _auditService.LogActionAsync(null, "Create", $"Topic {topic.Title}", "Topic", topic.TopicId);
            return true;
        }

        public async Task UpdateTopicAsync(InternshipTopic topic)
        {
            var existing = await _topicRepo.GetByIdAsync(topic.TopicId);
            if (existing != null)
            {
                existing.Title = topic.Title;
                existing.Description = topic.Description;
                existing.OrganizationId = topic.OrganizationId;
                existing.DisciplineId = topic.DisciplineId;
                existing.IsAvailable = topic.IsAvailable;

                _topicRepo.Update(existing);
                await _topicRepo.SaveAsync();
            }
        }

        public async Task DeleteTopicAsync(int topicId)
        {
            var item = await _topicRepo.GetByIdAsync(topicId);
            if (item != null)
            {
                _topicRepo.Delete(item);
                await _topicRepo.SaveAsync();
            }
        }

        // --- Assignments ---
        public async Task<bool> AssignTopicAsync(int studentId, int topicId, int courseId, int supervisorId, string individualTask)
        {
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

            await _auditService.LogActionAsync(studentId, "Assign", $"Topic assigned", "Assignment", assignment.AssignmentId);
            return true;
        }

        public async Task<InternshipAssignment?> GetStudentAssignmentAsync(int studentId)
        {
            return await _assignmentRepo.GetActiveAssignmentAsync(studentId);
        }

        // --- Organizations ---
        public async Task<IEnumerable<Organization>> GetAllOrganizationsAsync() => await _orgRepo.GetAllAsync();

        public async Task<Organization> CreateOrganizationAsync(string name, string address, string type, string email)
        {
            var org = new Organization
            {
                Name = name,
                Address = address ?? "",
                Type = type ?? "Company",
                ContactEmail = email ?? "" 
            };
            _orgRepo.Add(org);
            await _orgRepo.SaveAsync();
            await _auditService.LogActionAsync(null, "Create", $"Створено орг.: {name}", "Organization", org.OrganizationId);
            return org;
        }

        public async Task DeleteOrganizationAsync(int id)
        {
            var item = await _orgRepo.GetByIdAsync(id);
            if (item != null) { _orgRepo.Delete(item); await _orgRepo.SaveAsync(); }
        }
    }
}