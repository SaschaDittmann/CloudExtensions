using System;
using System.ComponentModel.DataAnnotations;

namespace ByteSmith.WindowsAzure.Diagnostics
{
    [Table("DevLogsTable")]
    public class SqlAzureLogEntry
    {
        [Key]
        public long LogEntryId { get; set; }
        [Required]
        public long EventTickCount { get; set; }
        [Required]
        public int Level { get; set; }
        [Required]
        public int EventId { get; set; }
        [Required]
        public int Pid { get; set; }
        [Required]
        [StringLength(50)]
        public string Tid { get; set; }
        [Required]
        [StringLength(255)]
        public string RoleName { get; set; }
        [Required]
        [StringLength(255)]
        public string RoleId { get; set; }
        [Required]
        [MaxLength()]
        public string Message { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
    }
}
