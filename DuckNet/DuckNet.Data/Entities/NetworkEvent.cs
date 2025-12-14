using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuckNet.Data.Entities
{
    public enum EventType { DeviceConnected, DeviceDisconnected, Error, AdapterChanged }

    public class NetworkEvent
    {
        [Key]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }
        public EventType Type { get; set; }
        public string Message { get; set; } = string.Empty;


        public int? DeviceId { get; set; }
        [ForeignKey("DeviceId")]
        public virtual Device? Device { get; set; }
    }
}