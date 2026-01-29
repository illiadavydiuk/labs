using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Interfaces
{
    public interface IStudentGroupRepository : IRepository<StudentGroup>
    {
        Task<IEnumerable<StudentGroup>> GetGroupsBySpecialtyAsync(int specialtyId);
    }
}
