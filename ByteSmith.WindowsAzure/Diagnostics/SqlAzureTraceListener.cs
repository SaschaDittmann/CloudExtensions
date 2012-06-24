using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using ByteSmith.WindowsAzure.Configuration;

namespace ByteSmith.WindowsAzure.Diagnostics
{
    public class SqlAzureTraceListener : TraceListener
    {
        private const string DefaultDiagnosticsConnectionStringSettingName =
            "ByteSmith.WindowsAzure.Diagnostics.SqlAzureTraceListener.ConnectionString";

        [ThreadStatic]
        private static StringBuilder _messageBuffer;

        private readonly object _initializationSection = new object();
        private bool _isInitialized;

        private readonly object _traceLogAccess = new object();
        private readonly List<SqlAzureLogEntry> _traceLog = new List<SqlAzureLogEntry>();

        private readonly string _connectionStringSettingName;
        private SqlAzureLogContext _context;

        public SqlAzureTraceListener()
            : this(DefaultDiagnosticsConnectionStringSettingName)
        {
        }

        public SqlAzureTraceListener(string connectionStringSettingName)
        {
            _connectionStringSettingName = connectionStringSettingName;
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

            lock (_traceLogAccess)
            {
                _traceLog.ForEach(entry => _context.LogEntries.Add(entry));
                _traceLog.Clear();
            }

            _context.SaveChanges();
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
            _context = new SqlAzureLogContext(
                HybridConfigurationManager.GetConnectionString(_connectionStringSettingName)
                .ConnectionString);
            _isInitialized = true;
        }

        private void AppendEntry(int id, TraceEventType eventType, TraceEventCache eventCache)
        {
            if (_messageBuffer == null)
            {
                _messageBuffer = new StringBuilder();
            }

            var message = _messageBuffer.ToString();
            _messageBuffer.Length = 0;

            if (message.EndsWith(Environment.NewLine))
            {
                message = message.Substring(0, message.Length - Environment.NewLine.Length);
            }

            if (message.Length == 0)
            {
                return;
            }

            var entry = new SqlAzureLogEntry
            {
                EventTickCount = eventCache.Timestamp,
                Level = (int)eventType,
                EventId = id,
                Pid = eventCache.ProcessId,
                Tid = eventCache.ThreadId,
                RoleName = RoleEnvironment.CurrentRoleInstance.Role.Name,
                RoleId = RoleEnvironment.CurrentRoleInstance.Id,
                Message = message,
                Timestamp = eventCache.DateTime
            };

            lock (_traceLogAccess)
            {
                _traceLog.Add(entry);
            }
        }
    }
}