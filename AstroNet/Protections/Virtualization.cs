using dnlib.DotNet;
using System.Collections.Generic;
using KVM;

namespace Astro_Renewed.Protections
{
    class Virtualization
    {
        public static void Execute(ModuleDefMD module, string outPath, List<string> mList)
        {
            Services.ConsoleLogger.Log("Processing \"Virtualization\" protection.");
            new KVMTask().Exceute(module, outPath, "", null, mList);
            //Core.Protector.Protect(module);
        }
    }
}
