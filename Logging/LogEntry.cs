using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Symformance.Logging
{
    internal class LogEntry
    {
        public string MethodName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long ElapsedMilliseconds { get; set; } = default;
    }
}