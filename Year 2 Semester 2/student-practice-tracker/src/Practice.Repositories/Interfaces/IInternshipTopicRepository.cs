using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Interfaces
{
    public interface IInternshipTopicRepository : IRepository<InternshipTopic>
    {
        // Отримати всі доступні теми
        Task<IEnumerable<InternshipTopic>> GetAvailableTopicsAsync();

        // Теми конкретної організації
        Task<IEnumerable<InternshipTopic>> GetByOrganizationAsync(int organizationId);
    }
}
