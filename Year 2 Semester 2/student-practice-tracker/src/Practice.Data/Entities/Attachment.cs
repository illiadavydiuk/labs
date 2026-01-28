using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Practice.Data.Entities
{
    public class Attachment
    {
        [Key]
        public int AttachmentId { get; set; }
        [Required]
        public int ReportId { get; set; }
        [ForeignKey("ReportId")]
        public virtual Report Report { get; set; }
        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }
        [Required]
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
