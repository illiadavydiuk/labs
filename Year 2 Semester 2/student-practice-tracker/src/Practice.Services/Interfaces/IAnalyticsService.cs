using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<string> GenerateSimpleReportAsync();
        void ExportDatabase(string destinationPath);
    }
}
