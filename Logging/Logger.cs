using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Symformance.Configuration;

namespace Symformance.Logging
{
    internal static class Logger
    {
        private static readonly string logFilePath = ProfilerConfig.OutputPath;

        static Logger()
        {
            string filename = $"Perf_Log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            var filePath = Path.Combine(logFilePath, filename);
            File.AppendAllText(
                logFilePath,
                $"==== Symformance Log - {DateTime.Now} ===={Environment.NewLine}"
            );
        }

        public static void LogMethod(string methodName, string filePath, long duration)
        {
            var logMessage =
                $"Method: {methodName}, File: {filePath}, Duration: {duration} ms{Environment.NewLine}";
            File.AppendAllText(logFilePath, logMessage);
        }

        public static void LogCallStack(List<LogEntry> entries)
        {
            File.AppendAllText(
                logFilePath,
                Environment.NewLine + "--- CALL STACK ---" + Environment.NewLine
            );
            foreach (var entry in entries)
            {
                File.AppendAllText(
                    logFilePath,
                    $"- {entry.MethodName} | {entry.FilePath} | {entry.ElapsedMilliseconds} ms"
                        + Environment.NewLine
                );
            }
        }

        public static void LogBottlenecks(List<LogEntry> entries)
        {
            File.AppendAllText(
                logFilePath,
                Environment.NewLine + "--- BOTTLENECKS (Descending Time) ---" + Environment.NewLine
            );
            foreach (var entry in entries.OrderByDescending(e => e.ElapsedMilliseconds))
            {
                File.AppendAllText(
                    logFilePath,
                    $"- {entry.MethodName} | {entry.FilePath} | {entry.ElapsedMilliseconds} ms" + Environment.NewLine
                );
            }
            ;
        }
    }
}
