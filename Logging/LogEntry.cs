using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Symformance.Logging
{
    internal class LogEntry
    {
        public string? MethodName { get; set; } = string.Empty;
        public string? FilePath { get; set; } = string.Empty;
        public long? ElapsedMilliseconds { get; set; } = default;
        public string? ThreadId { get; set; } = string.Empty;
        public string? Parameters { get; set; } = string.Empty;
        public string? ExceptionMessage { get; set; } = string.Empty;
    }
}
