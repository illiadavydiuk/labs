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
        public InternshipAssignmentRepository(AppDbContext context) : base(context)
        {
        }
    }
}
