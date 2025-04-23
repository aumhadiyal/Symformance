using System.Diagnostics;
using Symformance.Logging;

namespace Symformance.Core
{
    internal class Profiler
    {
        private static List<LogEntry> _entries = new();

        public static void Profile(
            Action action,
            [System.Runtime.CompilerServices.CallerMemberName] string methodName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = ""
        )
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            action.Invoke();
            stopwatch.Stop();

            _entries.Add(
                new LogEntry
                {
                    MethodName = methodName,
                    FilePath = filePath,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                }
            );

            Logger.LogMethod(methodName, filePath, stopwatch.ElapsedMilliseconds);
        }

        public static void Summary()
        {
            Logger.LogCallStack(_entries);
            Logger.LogBottlenecks(_entries);
        }
    }
}
