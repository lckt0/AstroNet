using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using KoiVM;
using MethodAttributes = dnlib.DotNet.MethodAttributes;
using System.IO;
using KoiVM.RT.Mutation;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KVM
{
    public class InitializePhase
    {
        // fish.cs
        public static object VirtualizerKey = new object();
        public static object MergeKey = new object();
        public static HashSet<MethodDef> methods = new HashSet<MethodDef>();
        public static AssemblyDef moduleConvert;

        public void InitializeP(ModuleDef module, List<string> methodName)
        {
            foreach (var typeDef in module.Types)
            {
                foreach (MethodDef methodDef in typeDef.Methods)
                {
                    if (methodName.Contains(methodDef.Name))
                    {
                        methods.Add(methodDef);
                    }
                }
            }

            var seed = new Random().Next(1, int.MaxValue);
            
            
            ModuleDef merge = module;

            string rtName = "Runtime";

            ModuleDefMD rtModule = ModuleDefMD.Load("Runtime.dll");
         
            rtModule.Assembly.Name = rtName;
            rtModule.Name = rtName + ".dll";
            var vr = new Virtualizer(seed, false);
            vr.ExportDbgInfo = false;
            vr.DoStackWalk = false;
            vr.Initialize(rtModule);
            
            VirtualizerKey = vr;
            MergeKey = merge;
            
            vr.CommitRuntime(merge);
            var ctor = typeof(InternalsVisibleToAttribute).GetConstructor(new[] { typeof(string) });
            
            if (methods.Count > 0)
            {
                var ca = new CustomAttribute((ICustomAttributeType)module.Import(ctor));
                ca.ConstructorArguments.Add(new CAArgument(module.CorLibTypes.String, vr.RuntimeModule.Assembly.Name.String));
                module.Assembly.CustomAttributes.Add(ca);
            }
            
            MarkPhase(module);
        }

        public void MarkPhase(ModuleDef module)
        {
            //string eddyType = "AstroNet";
            var vr = (Virtualizer)VirtualizerKey;

            var refRepl = new Dictionary<IMemberRef, IMemberRef>();

            var oldType = module.GlobalType;
            var newType = new TypeDefUser(oldType.Name);
            oldType.Name = "AstroNet";
            oldType.BaseType = module.CorLibTypes.GetTypeRef("System", "Object");
            module.Types.Insert(0, newType);

            var old_cctor = oldType.FindOrCreateStaticConstructor();
            var cctor = newType.FindOrCreateStaticConstructor();
            old_cctor.Name = "AstroNet";
            old_cctor.IsRuntimeSpecialName = false;
            old_cctor.IsSpecialName = false;
            old_cctor.Access = MethodAttributes.PrivateScope;
            cctor.Body = new CilBody(true, new List<Instruction> {
                Instruction.Create(OpCodes.Call, old_cctor),
                Instruction.Create(OpCodes.Ret)
            }, new List<ExceptionHandler>(), new List<Local>());

            for (int i = 0; i < oldType.Methods.Count; i++)
            {
                var nativeMethod = oldType.Methods[i];
                if (nativeMethod.IsNative)
                {
                    var methodStub = new MethodDefUser(nativeMethod.Name, nativeMethod.MethodSig.Clone());
                    methodStub.Attributes = MethodAttributes.Assembly | MethodAttributes.Static;
                    methodStub.Body = new CilBody();
                    methodStub.Body.Instructions.Add(new Instruction(OpCodes.Jmp, nativeMethod));
                    methodStub.Body.Instructions.Add(new Instruction(OpCodes.Ret));

                    oldType.Methods[i] = methodStub;
                    newType.Methods.Add(nativeMethod);
                    refRepl[nativeMethod] = methodStub;
                }
            }

            methods.Add(old_cctor);

            var toProcess = new Dictionary<ModuleDef, List<MethodDef>>();
            foreach (var entry in new Scanner(module, methods).Scan())
            {

                vr.AddMethod(entry.Item1, entry.Item2);
                toProcess.AddListEntry(entry.Item1.Module, entry.Item1);
            }

            Utils.ModuleWriterListener.OnWriterEvent += new Listener
            {
                vr = vr,
                methods = toProcess,
                refRepl = refRepl,
                module = module
            }.OnWriterEvent;
        }

        class Listener
        {
            public Virtualizer vr;
            public Dictionary<ModuleDef, List<MethodDef>> methods;
            public Dictionary<IMemberRef, IMemberRef> refRepl;
            IModuleWriterListener commitListener = null;
            public ModuleDef module;
            public void OnWriterEvent(object sender, ModuleWriterListenerEventArgs e)
            {
                var writer = (ModuleWriter)sender;
                if (commitListener != null)
                    commitListener.OnWriterEvent(writer, e.WriterEvent);

                if (e.WriterEvent == ModuleWriterEvent.MDBeginWriteMethodBodies && methods.ContainsKey(writer.Module))
                {

                    vr.ProcessMethods(writer.Module);

                    foreach (var repl in refRepl)
                        vr.Runtime.Descriptor.Data.ReplaceReference(repl.Key, repl.Value);

                    commitListener = vr.CommitModule((ModuleDefMD)module);
                    Astro_Renewed.Program.afterVM((ModuleDefMD)writer.Module);
                }
            }
        }

    }

}