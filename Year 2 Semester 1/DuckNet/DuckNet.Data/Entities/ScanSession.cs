using System.ComponentModel.DataAnnotations;

namespace DuckNet.Data.Entities
{
    public class ScanSession
    {
        [Key]
        public int Id { get; set; }

        public DateTime ScanTime { get; set; }
        public int DevicesFound { get; set; }
        public int AveragePingMs { get; set; }
    }
}