using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Services.Interfaces
{
    public interface IIdentityService
    {
        Task<User>? LoginAsync(string email, string password);
        Task<bool> RegisterStudentAsync(User user, string password, int groupId, string recordBook);
        Task<bool> RegisterSupervisorAsync(User user, string password, int? departmentId, int? positionId, string? phone);
    }
}
