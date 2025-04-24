using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Symformance.Configuration;
using Symformance.Core;

namespace Symformance.Logging
{
    internal static class Logger
    {
        private static readonly string logFilePath = ProfilerConfig.OutputPath;
        private static readonly string logFileName =
            $"Perf_Log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        private static readonly string filePath = Path.Combine(logFilePath, logFileName);
        private static StreamWriter? streamWriter;
        private static readonly string HeaderSeparator = new('=', 80);
        private static readonly string SectionSeparator = new('-', 80);

        static Logger()
        {
            try
            {
                Directory.CreateDirectory(logFilePath);
                streamWriter = new StreamWriter(filePath, append: true) { AutoFlush = true };
                streamWriter.WriteLine(HeaderSeparator);
                streamWriter.WriteLine(
                    $"SYMFORMANCE PROFILER LOG - {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                );
                streamWriter.WriteLine(HeaderSeparator);
                streamWriter.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing log file: {ex.Message}");
            }
        }

        public static string FormatLogEntry(
            string? methodName,
            string? _filePath,
            long? duration,
            string? threadId,
            string? parameters,
            string? exceptionMessage
        )
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Method: {methodName ?? "Unknown"}");
            sb.AppendLine($"  File: {_filePath ?? "Unknown"}");
            sb.AppendLine($"  Duration: {duration ?? 0} ms");
            sb.AppendLine($"  Thread ID: {threadId ?? "Unknown"}");

            if (!string.IsNullOrEmpty(parameters))
                sb.AppendLine($"  Parameters: {parameters}");

            if (!string.IsNullOrEmpty(exceptionMessage))
                sb.AppendLine($"  Exception: {exceptionMessage}");

            return sb.ToString();
        }

        public static void LogMethod(
            string? methodName,
            string? _filePath,
            long? duration,
            string? threadId,
            string? parameters,
            string? exceptionMessage
        )
        {
            try
            {
                var logMessage = FormatLogEntry(
                    methodName,
                    _filePath,
                    duration,
                    threadId,
                    parameters,
                    exceptionMessage
                );
                streamWriter?.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging method: {ex.Message}");
            }
        }

        public static void LogCallStack(List<LogEntry> entries)
        {
            try
            {
                streamWriter?.WriteLine(SectionSeparator);
                streamWriter?.WriteLine("CALL STACK (CHRONOLOGICAL ORDER)");
                streamWriter?.WriteLine(SectionSeparator);
                streamWriter?.WriteLine();

                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];

                    streamWriter?.WriteLine($"[{i + 1}] {entry.MethodName ?? "Unknown"}");
                    streamWriter?.WriteLine($"    File: {entry.FilePath ?? "Unknown"}");
                    streamWriter?.WriteLine($"    Duration: {entry.ElapsedMilliseconds ?? 0} ms");
                    streamWriter?.WriteLine($"    Thread ID: {entry.ThreadId ?? "Unknown"}");

                    if (!string.IsNullOrEmpty(entry.Parameters))
                        streamWriter?.WriteLine($"    Parameters: {entry.Parameters}");

                    if (!string.IsNullOrEmpty(entry.ExceptionMessage))
                        streamWriter?.WriteLine($"    Exception: {entry.ExceptionMessage}");

                    streamWriter?.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging call stack: {ex.Message}");
            }
        }

        public static void LogBottlenecks(List<LogEntry> entries)
        {
            try
            {
                streamWriter?.WriteLine(SectionSeparator);
                streamWriter?.WriteLine("BOTTLENECKS (DESCENDING TIME)");
                streamWriter?.WriteLine(SectionSeparator);
                streamWriter?.WriteLine();

                long totalTime = entries.Sum(e => e.ElapsedMilliseconds ?? 0);

                var sortedEntries = entries.OrderByDescending(e => e.ElapsedMilliseconds).ToList();

                foreach (var entry in sortedEntries.Take(10))
                {
                    double percentage =
                        totalTime > 0
                            ? ((double)(entry.ElapsedMilliseconds ?? 0) / totalTime) * 100
                            : 0;

                    streamWriter?.WriteLine($"Method: {entry.MethodName ?? "Unknown"}");
                    streamWriter?.WriteLine($"  File: {entry.FilePath ?? "Unknown"}");
                    streamWriter?.WriteLine(
                        $"  Duration: {entry.ElapsedMilliseconds ?? 0} ms ({percentage:F2}% of total)"
                    );

                    if (!string.IsNullOrEmpty(entry.ExceptionMessage))
                        streamWriter?.WriteLine($"  Exception: {entry.ExceptionMessage}");

                    streamWriter?.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging bottlenecks: {ex.Message}");
            }
        }

        public static void LogSummaryStatistics(List<LogEntry> entries)
        {
            try
            {
                streamWriter?.WriteLine(SectionSeparator);
                streamWriter?.WriteLine("SUMMARY STATISTICS");
                streamWriter?.WriteLine(SectionSeparator);
                streamWriter?.WriteLine();

                long totalTime = entries.Sum(e => e.ElapsedMilliseconds ?? 0);
                long maxTime = entries.Max(e => e.ElapsedMilliseconds ?? 0);
                double avgTime = entries.Count > 0 ? totalTime / (double)entries.Count : 0;

                int methodCount = entries.Select(e => e.MethodName).Distinct().Count();
                int exceptionsCount = entries.Count(e => !string.IsNullOrEmpty(e.ExceptionMessage));

                streamWriter?.WriteLine($"Total Profiled Methods: {methodCount}");
                streamWriter?.WriteLine($"Total Execution Time: {totalTime} ms");
                streamWriter?.WriteLine($"Average Method Time: {avgTime:F2} ms");
                streamWriter?.WriteLine($"Longest Method Time: {maxTime} ms");
                streamWriter?.WriteLine($"Methods With Exceptions: {exceptionsCount}");

                if (exceptionsCount > 0)
                {
                    streamWriter?.WriteLine();
                    streamWriter?.WriteLine("Methods with exceptions:");
                    foreach (
                        var entry in entries.Where(e => !string.IsNullOrEmpty(e.ExceptionMessage))
                    )
                    {
                        streamWriter?.WriteLine(
                            $"  - {entry.MethodName}: {entry.ExceptionMessage}"
                        );
                    }
                }

                streamWriter?.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging summary statistics: {ex.Message}");
            }
        }

        public static void Close()
        {
            try
            {
                streamWriter?.WriteLine(HeaderSeparator);
                streamWriter?.WriteLine(
                    $"PROFILING SESSION ENDED - {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                );
                streamWriter?.WriteLine(HeaderSeparator);
                streamWriter?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing StreamWriter: {ex.Message}");
            }
        }

        public static void SymSummarize()
        {
            var entries = SymProfilerStore.GetAll();

            if (entries.Count == 0)
            {
                Console.WriteLine("[Symformance] No profiling data to summarize.");
                return;
            }

            LogCallStack(entries);
            LogBottlenecks(entries);
            LogSummaryStatistics(entries);
            SymProfilerStore.Clear();
            Close();
        }
    }
}
