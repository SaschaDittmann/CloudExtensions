using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Diagnostics;
using System.Text;
using ByteSmith.WindowsAzure.Configuration;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;

namespace ByteSmith.WindowsAzure.Diagnostics
{
    public class TableStorageTraceListener : TraceListener
    {
        private const string DefaultDiagnosticsConnectionStringSettingName = 
            "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString";

        private const string DefaultDiagnosticsTableName = "DevLogsTable";

        [ThreadStatic]
        private static StringBuilder _messageBuffer;

        private readonly object _initializationSection = new object();
        private bool _isInitialized;

        private readonly object _traceLogAccess = new object();
        private readonly List<TableStorageLogEntry> _traceLog = new List<TableStorageLogEntry>();

        private CloudTableClient _tableStorage;
        private readonly string _diagnosticsTableName;
        private readonly string _configurationStringSettingName;

        public TableStorageTraceListener()
            : this(DefaultDiagnosticsConnectionStringSettingName, DefaultDiagnosticsTableName)
        {
        }

        public TableStorageTraceListener(string configurationStringSettingName)
            : this(configurationStringSettingName, DefaultDiagnosticsTableName)
        {
        }
        
        public TableStorageTraceListener(string configurationStringSettingName, string diagnosticsTableName)
        {
            _configurationStringSettingName = configurationStringSettingName;
            _diagnosticsTableName = diagnosticsTableName;
        }

        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        public override void Write(string message)
        {
            if (_messageBuffer == null)
            {
                _messageBuffer = new StringBuilder();
            }

            _messageBuffer.Append(message);
        }

        public override void WriteLine(string message)
        {
            if (_messageBuffer == null)
            {
                _messageBuffer = new StringBuilder();
            }

            _messageBuffer.AppendLine(message);
        }

        public override void Flush()
        {
            if (!_isInitialized)
            {
                lock (_initializationSection)
                {
                    if (!_isInitialized)
                    {
                        Initialize();
                    }
                }
            }

            var context = _tableStorage.GetDataServiceContext();
            context.MergeOption = MergeOption.AppendOnly;
            lock (_traceLogAccess)
            {
                _traceLog.ForEach(entry => context.AddObject(_diagnosticsTableName, entry));
                _traceLog.Clear();
            }

            if (context.Entities.Count > 0)
            {
                context.BeginSaveChangesWithRetries(
                    SaveChangesOptions.None, 
                    ar => context.EndSaveChangesWithRetries(ar), 
                    null);
            }
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            base.TraceData(eventCache, source, eventType, id, data);
            AppendEntry(id, eventType, eventCache);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            base.TraceData(eventCache, source, eventType, id, data);
            AppendEntry(id, eventType, eventCache);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            base.TraceEvent(eventCache, source, eventType, id);
            AppendEntry(id, eventType, eventCache);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            base.TraceEvent(eventCache, source, eventType, id, format, args);
            AppendEntry(id, eventType, eventCache);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            base.TraceEvent(eventCache, source, eventType, id, message);
            AppendEntry(id, eventType, eventCache);
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            base.TraceTransfer(eventCache, source, id, message, relatedActivityId);
            AppendEntry(id, TraceEventType.Transfer, eventCache);
        }

        private void Initialize()
        {
            var connectionString = HybridConfigurationManager.GetAppSetting(_configurationStringSettingName);
            var account = CloudStorageAccount.Parse(connectionString);
            _tableStorage = account.CreateCloudTableClient();
            _tableStorage.CreateTableIfNotExist(_diagnosticsTableName);
            _isInitialized = true;
        }

        private void AppendEntry(int id, TraceEventType eventType, TraceEventCache eventCache)
        {
            if (_messageBuffer == null)
                _messageBuffer = new StringBuilder();

            string message = _messageBuffer.ToString();
            _messageBuffer.Length = 0;

            if (message.EndsWith(Environment.NewLine))
                message = message.Substring(0, message.Length - Environment.NewLine.Length);

            if (message.Length == 0)
                return;

            var entry = new TableStorageLogEntry
            {
                PartitionKey = string.Format("{0:D10}", eventCache.Timestamp >> 30),
                RowKey = string.Format("{0:D19}", eventCache.Timestamp),
                EventTickCount = eventCache.Timestamp,
                Level = (int)eventType,
                EventId = id,
                Pid = eventCache.ProcessId,
                Tid = eventCache.ThreadId,
                RoleName = RoleEnvironment.CurrentRoleInstance.Role.Name,
                RoleId = RoleEnvironment.CurrentRoleInstance.Id,
                Message = message
            };

            lock (_traceLogAccess)
                _traceLog.Add(entry);
        }
    }
}
