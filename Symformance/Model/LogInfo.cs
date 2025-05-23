using System.Collections.Generic;

namespace Symformance.Model
{
    public class LogInfo
    {
        public string NamespaceName;
        public string ClassName;
        public string MethodName;
        public List<string> Parameters;
        public long ElapsedTime;
    }
}