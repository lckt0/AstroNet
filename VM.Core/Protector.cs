using System;
using System.Linq;
using System.Reflection;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using Core.Properties;

namespace Core
{
    public class Protector
    {
        public static string path2;

        public static string name { get; private set; }

        public static void Protect(ModuleDefMD moduleDefMD)
        {
            Console.WriteLine("Hi");

            name = "AstroNet"; //Key

            asmRefAdder(moduleDefMD); //this will resolve references (dlls) such as mscorlib and any dlls the unprotected binary may use. this will be to make sure resolving methods/types/fields in another assembly can be correctly identified
            Console.WriteLine("processing");
            Protection.MethodProccesor.ModuleProcessor(moduleDefMD); //this will process the module
            Console.WriteLine("Passed processing");
            var nativePath = Resources.NativeEncoderx86;
            EmbeddedResource emv = new EmbeddedResource("ASTRO", (nativePath));
            moduleDefMD.Resources.Add(emv);
            EmbeddedResource emv64 = new EmbeddedResource("ASTR0", (Resources.NativeEncoderx64));
            moduleDefMD.Resources.Add(emv64);

            byte[] cleanConversion = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Runtime.dll"));

            EmbeddedResource embc = new EmbeddedResource("ASTRO_COOL", cleanConversion); //Full
            moduleDefMD.Resources.Add(embc);

            EmbeddedResource emb = new EmbeddedResource("AstroNet", Resources.XorMethod); //XorMethod
            moduleDefMD.Resources.Add(emb);
        }


        private static void asmRefAdder(ModuleDefMD moduleDefMD)
        {
            var asmResolver = new AssemblyResolver { EnableTypeDefCache = true };
            var modCtx = new ModuleContext(asmResolver);
            asmResolver.DefaultModuleContext = modCtx;
            var asmRefs = moduleDefMD.GetAssemblyRefs().ToList();
            moduleDefMD.Context = modCtx;
            foreach (var asmRef in asmRefs)
            {
                try
                {
                    if (asmRef == null)
                        continue;
                    var asm = asmResolver.Resolve(asmRef.FullName, moduleDefMD);
                    if (asm == null)
                        continue;
                    moduleDefMD.Context.AssemblyResolver.AddToCache(asm);

                }
                catch { }
            }
        }
    }
}
