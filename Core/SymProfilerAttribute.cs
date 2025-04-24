using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MethodDecorator.Fody.Interfaces;
using Symformance.Logging;

namespace Symformance.Core
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class SymProfilerAttribute : Attribute, IMethodDecorator
    {
        private Stopwatch? _stopwatch;
        private string? _methodName;
        private string? _declaringType;
        private string? _threadId;
        private string? _parameters;
        private string? _exceptionMessage;

        public void Init(object instance, MethodBase method, object[] args)
        {
            if (method.IsConstructor)
                _methodName = method.DeclaringType?.FullName + ".ctor";
            else
                _methodName = method.DeclaringType?.FullName + "." + method.Name;

            _declaringType = method.DeclaringType?.FullName ?? "UnknownType";
            _parameters = string.Join(", ", args.Select(a => a?.ToString() ?? "null"));
        }

        public void OnEntry()
        {
            _stopwatch = Stopwatch.StartNew();
            _threadId = Thread.CurrentThread.ManagedThreadId.ToString();
        }

        public void OnExit()
        {
            _stopwatch?.Stop();
            SymProfilerStore.Add(
                new LogEntry
                {
                    MethodName = _methodName,
                    FilePath = _declaringType,
                    ElapsedMilliseconds = _stopwatch?.ElapsedMilliseconds,
                    ThreadId = _threadId,
                    Parameters = _parameters,
                    ExceptionMessage = _exceptionMessage,
                }
            );
            Logger.LogMethod(
                _methodName,
                _declaringType,
                _stopwatch?.ElapsedMilliseconds,
                _threadId,
                _parameters,
                _exceptionMessage
            );
        }

        public void OnException(Exception exception)
        {
            _stopwatch?.Stop();
            _exceptionMessage = exception.Message;
            Logger.LogMethod(
                _methodName + " (Exception)",
                _declaringType,
                _stopwatch?.ElapsedMilliseconds ?? 0,
                _threadId,
                _parameters,
                _exceptionMessage
            );
            SymProfilerStore.Add(
                new LogEntry
                {
                    MethodName = _methodName,
                    FilePath = _declaringType,
                    ElapsedMilliseconds = _stopwatch?.ElapsedMilliseconds,
                }
            );
            Console.WriteLine($"[SymProfiler] Exception: {exception.Message}");
        }
    }
}
