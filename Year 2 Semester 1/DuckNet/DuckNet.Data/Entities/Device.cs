using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace DuckNet.Data.Entities
{
    public class Device
    {
        [Key]
        public int Id { get; set; }

        public string IpAddress { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime LastSeen { get; set; }
        public string? CustomName { get; set; }
        public bool IsTrusted { get; set; }

        public virtual ICollection<NetworkEvent> Events { get; set; } = new List<NetworkEvent>();

  
        [NotMapped]
        public long LastPingMs { get; set; }
    }
}