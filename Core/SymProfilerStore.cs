using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Symformance.Logging;

namespace Symformance.Core
{
    internal class SymProfilerStore
    {
        private static List<LogEntry> _entries = new List<LogEntry>();

        public static void Add(LogEntry logEntry)
        {
            lock (_entries)
                _entries.Add(logEntry);
        }

        public static List<LogEntry> GetAll()
        {
            lock (_entries)
                return new List<LogEntry>(_entries);
        }

        public static void Clear()
        {
            lock (_entries)
                _entries.Clear();
        }
    }
}
