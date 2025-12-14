using DuckNet.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuckNet.Services.Interfaces
{
    public interface INetworkScanner
    {
        Task<List<Device>> ScanNetworkAsync(string baseIp); // Наприклад "192.168.1."
    }
}