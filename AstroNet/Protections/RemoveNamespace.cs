using Astro_Renewed.Services;
using dnlib.DotNet;

namespace Astro_Renewed.Protections
{
    class RemoveNamespace
    {
        public static void Execute(ModuleDefMD module)
        {
            foreach (TypeDef type in module.Types)
            {
                type.Namespace = "";
            }
            Services.ConsoleLogger.Log("Processing \"Remove Namespace\" protection.");
        }
    }
}
