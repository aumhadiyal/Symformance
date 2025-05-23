using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Symformance.Model;
using Symformance.Store;

namespace Symformance.CLI.Injector
{
    public class SymInjector
    {
        public static void Inject(AssemblyDefinition assembly)
        {
            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    foreach (var method in type.Methods)
                    {
                        if (!method.HasBody)
                            continue;

                        bool hasSymAttribute = method.CustomAttributes.Any(attr =>
                            attr.AttributeType.Name == "SymAttribute"
                        );

                        if (!hasSymAttribute)
                            continue;

                        var processor = method.Body.GetILProcessor();
                        var instructions = method.Body.Instructions;

                        // Declare Stopwatch variable
                        var stopwatchType = module.ImportReference(
                            typeof(System.Diagnostics.Stopwatch)
                        );
                        var stopwatchVar = new VariableDefinition(stopwatchType);
                        method.Body.Variables.Add(stopwatchVar);

                        // Declare elapsed time variable
                        var longType = module.ImportReference(typeof(long));
                        var elapsedVar = new VariableDefinition(longType);
                        method.Body.Variables.Add(elapsedVar);

                        // Declare LogInfo variable
                        var logInfoType = module.ImportReference(typeof(LogInfo));
                        var logInfoVar = new VariableDefinition(logInfoType);
                        method.Body.Variables.Add(logInfoVar);

                        method.Body.InitLocals = true;

                        // Build prologue: Stopwatch stopwatch = Stopwatch.StartNew();
                        var firstInstruction = instructions.First();
                        var startNewMethod = module.ImportReference(
                            typeof(System.Diagnostics.Stopwatch).GetMethod("StartNew")
                        );
                        processor.InsertBefore(
                            firstInstruction,
                            processor.Create(OpCodes.Call, startNewMethod)
                        );
                        processor.InsertBefore(
                            firstInstruction,
                            processor.Create(OpCodes.Stloc, stopwatchVar)
                        );

                        // Find all return instructions
                        var returnInstructions = instructions
                            .Where(i => i.OpCode == OpCodes.Ret)
                            .ToList();

                        foreach (var ret in returnInstructions)
                        {
                            // stopwatch.Stop()
                            var stopMethod = module.ImportReference(
                                typeof(System.Diagnostics.Stopwatch).GetMethod("Stop")
                            );
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Ldloc, stopwatchVar)
                            );
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Callvirt, stopMethod)
                            );

                            // long elapsed = stopwatch.ElapsedMilliseconds;
                            var elapsedGetter = module.ImportReference(
                                typeof(System.Diagnostics.Stopwatch)
                                    .GetProperty("ElapsedMilliseconds")
                                    .GetGetMethod()
                            );
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Ldloc, stopwatchVar)
                            );
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Callvirt, elapsedGetter)
                            );
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Stloc, elapsedVar)
                            );

                            // LogInfo log = new LogInfo();
                            var logCtor = module.ImportReference(
                                typeof(LogInfo).GetConstructor(System.Type.EmptyTypes)
                            );
                            processor.InsertBefore(ret, processor.Create(OpCodes.Newobj, logCtor));
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Stloc, logInfoVar)
                            );

                            // log.NamespaceName = ...
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Ldloc, logInfoVar)
                            );
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Ldstr, type.Namespace)
                            );
                            processor.InsertBefore(
                                ret,
                                processor.Create(
                                    OpCodes.Stfld,
                                    module.ImportReference(
                                        typeof(LogInfo).GetField("NamespaceName")
                                    )
                                )
                            );

                            // log.ClassName = ...
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Ldloc, logInfoVar)
                            );
                            processor.InsertBefore(ret, processor.Create(OpCodes.Ldstr, type.Name));
                            processor.InsertBefore(
                                ret,
                                processor.Create(
                                    OpCodes.Stfld,
                                    module.ImportReference(typeof(LogInfo).GetField("ClassName"))
                                )
                            );

                            // log.MethodName = ...
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Ldloc, logInfoVar)
                            );
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Ldstr, method.Name)
                            );
                            processor.InsertBefore(
                                ret,
                                processor.Create(
                                    OpCodes.Stfld,
                                    module.ImportReference(typeof(LogInfo).GetField("MethodName"))
                                )
                            );

                            // log.ElapsedTime = ...
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Ldloc, logInfoVar)
                            );
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Ldloc, elapsedVar)
                            );
                            processor.InsertBefore(
                                ret,
                                processor.Create(
                                    OpCodes.Stfld,
                                    module.ImportReference(typeof(LogInfo).GetField("ElapsedTime"))
                                )
                            );

                            // LogStore.logInfos.Add(log)
                            var logList = module.ImportReference(
                                typeof(LogStore).GetField("logInfos")
                            );
                            var addMethod = module.ImportReference(
                                typeof(List<LogInfo>).GetMethod("Add")
                            );
                            processor.InsertBefore(ret, processor.Create(OpCodes.Ldsfld, logList));
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Ldloc, logInfoVar)
                            );
                            processor.InsertBefore(
                                ret,
                                processor.Create(OpCodes.Callvirt, addMethod)
                            );
                        }
                    }
                }
            }

            assembly.Write("ModifiedAssembly.dll");
        }
    }
}
