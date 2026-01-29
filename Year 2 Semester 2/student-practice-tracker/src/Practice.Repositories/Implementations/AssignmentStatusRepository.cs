using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class AssignmentStatusRepository : Repository<AssignmentStatus>, IAssignmentStatusRepository
    {
        public AssignmentStatusRepository(AppDbContext context) : base(context)
        {
        }
    }
}
