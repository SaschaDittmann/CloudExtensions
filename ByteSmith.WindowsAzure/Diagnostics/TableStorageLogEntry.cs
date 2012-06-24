using Microsoft.WindowsAzure.StorageClient;

namespace ByteSmith.WindowsAzure.Diagnostics
{
    public class TableStorageLogEntry : TableServiceEntity
    {
        public long EventTickCount { get; set; }
        public int Level { get; set; }
        public int EventId { get; set; }
        public int Pid { get; set; }
        public string Tid { get; set; }
        public string RoleName { get; set; }
        public string RoleId { get; set; }
        public string Message { get; set; }
    }
}
