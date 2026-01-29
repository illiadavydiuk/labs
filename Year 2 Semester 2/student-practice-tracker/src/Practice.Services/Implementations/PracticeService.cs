using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class PracticeService : IPracticeService
    {
        private readonly IInternshipTopicRepository _topicRepo;
        private readonly IInternshipAssignmentRepository _assignmentRepo;
        private readonly IAssignmentStatusRepository _statusRepo;
        private readonly IAuditService _auditService;

        public PracticeService(
            IInternshipTopicRepository topicRepo,
            IInternshipAssignmentRepository assignmentRepo,
            IAssignmentStatusRepository statusRepo,
            IAuditService auditService)
        {
            _topicRepo = topicRepo;
            _assignmentRepo = assignmentRepo;
            _statusRepo = statusRepo;
            _auditService = auditService;
        }

        public async Task<IEnumerable<InternshipTopic>> GetAvailableTopicsAsync()
        {
            return await _topicRepo.GetAvailableTopicsAsync();
        }

        public async Task<bool> AddTopicAsync(InternshipTopic topic)
        {
            topic.IsAvailable = true;
            _topicRepo.Add(topic);
            await _topicRepo.SaveAsync();
            return true;
        }

        public async Task<bool> AssignTopicAsync(int studentId, int topicId, int courseId, int supervisorId, string individualTask)
        {
            var status = await _statusRepo.GetByNameAsync("Assigned")
                         ?? await _statusRepo.GetByIdAsync(1);

            var assignment = new InternshipAssignment
            {
                StudentId = studentId,
                TopicId = topicId,
                CourseId = courseId,
                StatusId = 1,
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
    }
}