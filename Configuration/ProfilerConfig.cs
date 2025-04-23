using Microsoft.Extensions.Configuration;

namespace Symformance.Configuration
{
    internal static class ProfilerConfig
    {
        internal static string OutputPath { get; private set; }

        static ProfilerConfig()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var jsonPath = configuration["Symformance:OutputPath"];

            OutputPath = !string.IsNullOrWhiteSpace(jsonPath)
                ? jsonPath
                : Path.Combine(AppContext.BaseDirectory, "PerformanceLogs");

            Directory.CreateDirectory(OutputPath);
        }
    }
}
