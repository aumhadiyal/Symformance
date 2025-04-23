using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Symformance.Core
{
    internal static class ProfileScanner
    {
        public static void ScanMethods()
        {
            var methods = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .SelectMany(m => m.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(SymProfilerAttribute), false).Any())
                .ToList();
        }
    }
}
