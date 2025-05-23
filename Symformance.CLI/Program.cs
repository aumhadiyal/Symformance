using System.IO;
using Mono.Cecil;
using Symformance.CLI.Injector;
using Symformance.Model;

namespace Program
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string assemblyName = args[0];

            var resolver = new DefaultAssemblyResolver();

            // Add output directory to search path (this includes all NuGet DLLs)
            string outputDir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            resolver.AddSearchDirectory(outputDir);

            var readerParameters = new ReaderParameters { AssemblyResolver = resolver };

            var assembly = AssemblyDefinition.ReadAssembly(assemblyName, readerParameters);

            SymInjector.Inject(assembly);
        }
    }
}
