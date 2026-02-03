using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class InternshipAssignmentRepository : Repository<InternshipAssignment>, IInternshipAssignmentRepository
    {
        public InternshipAssignmentRepository(AppDbContext context) : base(context) { }

        public async Task<InternshipAssignment> GetFullAssignmentDetailsAsync(int assignmentId)
        {
            return await _dbSet
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.InternshipTopic).ThenInclude(t => t.Organization) 
                .Include(a => a.Supervisor).ThenInclude(sup => sup.User)
                .Include(a => a.Course)
                .Include(a => a.AssignmentStatus)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);
        }

        public async Task<IEnumerable<InternshipAssignment>> GetAssignmentsByCourseAsync(int courseId)
        {
            return await _dbSet
                .Where(a => a.CourseId == courseId)
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.InternshipTopic)
                .Include(a => a.AssignmentStatus)
                .ToListAsync();
        }
        public async Task<InternshipAssignment?> GetActiveAssignmentAsync(int studentId)
        {
            return await _dbSet
                .Where(a => a.StudentId == studentId) 
                .Include(a => a.InternshipTopic)     
                .Include(a => a.AssignmentStatus) 
                .Include(a => a.Supervisor).ThenInclude(s => s.User)
                .OrderByDescending(a => a.StartDate)
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<InternshipAssignment>> GetBySupervisorWithDetailsAsync(int supervisorId)
        {
            return await _dbSet
                .Include(a => a.Student).ThenInclude(s => s.User)       
                .Include(a => a.Student).ThenInclude(s => s.StudentGroup)
                .Include(a => a.InternshipTopic).ThenInclude(t => t.Organization) 
                .Include(a => a.Course)
                .Include(a => a.Reports)
                .Where(a => a.SupervisorId == supervisorId)
                .ToListAsync();
        }

        public async Task<InternshipAssignment?> GetByStudentAndCourseWithDetailsAsync(int studentId, int courseId)
        {
            return await GetByStudentAndCourseAsync(studentId, courseId);
        }

        public async Task<InternshipAssignment?> GetByStudentAndCourseAsync(int studentId, int courseId)
        {
            return await _context.InternshipAssignments
                .AsNoTracking() 
                .AsSplitQuery() 
                .Include(a => a.Course)
                .Include(a => a.InternshipTopic).ThenInclude(t => t.Organization)
                .Include(a => a.Supervisor).ThenInclude(s => s.User)

                .Include(a => a.Reports).ThenInclude(r => r.Attachments)
                .Include(a => a.Reports).ThenInclude(r => r.ReportStatus)

                .Include(a => a.AssignmentStatus)
                .OrderByDescending(a => a.StartDate)
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.CourseId == courseId);
        }

        public async Task AddAssignmentWithTopicUpdateAsync(InternshipAssignment assignment, int topicId)
        {
            var topic = await _context.InternshipTopics.FindAsync(topicId);
            if (topic != null) { topic.IsAvailable = false; _context.InternshipTopics.Update(topic); }
            _context.InternshipAssignments.Add(assignment);
        }

        public async Task UpdateAssignmentTopicAsync(InternshipAssignment assignment, int newTopicId)
        {
            var oldTopic = await _context.InternshipTopics.FindAsync(assignment.TopicId);
            if (oldTopic != null) { oldTopic.IsAvailable = true; _context.InternshipTopics.Update(oldTopic); }

            var newTopic = await _context.InternshipTopics.FindAsync(newTopicId);
            if (newTopic != null) { newTopic.IsAvailable = false; _context.InternshipTopics.Update(newTopic); }

            assignment.TopicId = newTopicId;
            _context.InternshipAssignments.Update(assignment);
        }
        public async Task<List<InternshipAssignment>> GetByStudentIdAsync(int studentId)
        {
            return await _context.InternshipAssignments
                .Include(a => a.Course).ThenInclude(c => c.Discipline)
                .Include(a => a.InternshipTopic)
                .Include(a => a.Supervisor).ThenInclude(s => s.User)
                .Include(a => a.AssignmentStatus)
                .Where(a => a.StudentId == studentId)
                .ToListAsync();
        }
        public async Task<List<InternshipAssignment>> GetBySupervisorIdAsync(int supervisorId)
        {
            return await _context.InternshipAssignments
                .AsNoTracking()
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.InternshipTopic)
                .Include(a => a.Reports) 
                    .ThenInclude(r => r.ReportStatus) 
                .Where(a => a.SupervisorId == supervisorId)
                .ToListAsync();
        }
    }
}
