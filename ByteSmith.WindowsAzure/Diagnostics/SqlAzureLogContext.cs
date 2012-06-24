using System.Data.SqlClient;
using System.Data.Entity;

namespace ByteSmith.WindowsAzure.Diagnostics
{
    public class SqlAzureLogContext : DbContext
    {
        public SqlAzureLogContext(string connectionString) 
            : base(new SqlConnection(connectionString), true) 
        {
            // Erzeuge die Tabelle, wenn sie nicht existiert.
            Database.ExecuteSqlCommand(
                "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DevLogsTable]') AND type in (N'U')) "
                + "CREATE TABLE [dbo].[DevLogsTable]("
                + "[LogEntryId] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,"
                + "[EventTickCount] [bigint] NOT NULL,"
                + "[Level] [int] NOT NULL,"
                + "[EventId] [int] NOT NULL,"
                + "[Pid] [int] NOT NULL,"
                + "[Tid] [nvarchar](50) NOT NULL,"
                + "[RoleName] [nvarchar](255) NOT NULL,"
                + "[RoleId] [nvarchar](255) NOT NULL,"
                + "[Message] [nvarchar](max) NOT NULL,"
                + "[Timestamp] [datetime] NOT NULL);");
        }

        public DbSet<SqlAzureLogEntry> LogEntries { get; set; }
    }
}