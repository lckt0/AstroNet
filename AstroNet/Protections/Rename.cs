using Astro_Renewed.Services;
using dnlib.DotNet;
using System.Collections.Generic;

namespace Astro_Renewed.Protections
{
    class Rename
    {
        public static void Execute(ModuleDefMD module)
        {
            string namespaceCustom = RandomGenerator.randomMD5();

            foreach (TypeDef type in module.Types)
            {
                if (!type.Name.Contains("<Module>") && !type.Name.Contains("AstroNet") && !type.IsGlobalModuleType && !type.IsRuntimeSpecialName && !type.IsSpecialName && !type.IsWindowsRuntime && !type.IsInterface)
                {
                    type.Namespace = RandomGenerator.randomMD5();
                    type.Name = RandomGenerator.randomMD5();

                    foreach (PropertyDef property in type.Properties)
                    {
                        property.Name = RandomGenerator.randomMD5();
                    }
                    foreach (FieldDef fields in type.Fields)
                    {
                        fields.Name = RandomGenerator.randomMD5();
                    }
                    foreach (EventDef eventdef in type.Events)
                    {
                        eventdef.Name = RandomGenerator.randomMD5();
                    }
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.Name == "InitializeComponent")
                        {
                            method.Name = RandomGenerator.randomMD5();
                            Program.VirtualizeMethods.Add(method.Name);
                        }
                        else
                        {
                            List<string> a = new List<string>();
                            a.Add("Main");
                            a.Add("Run");
                            //a.Add("..ctor");
                            //a.Add(".ctor");
                            //a.Add("ctor");
                            //a.Add("Main");
                            //a.Add("<Module>");
                            //a.Add("AstroNet");
                            if (a.Contains(method.Name))
                            {
                                if (true)
                                {

                                }
                                method.Name = RandomGenerator.randomMD5();
                            }
                        }
                    }
                }
            }
            Services.ConsoleLogger.Log("Processing \"Methods Renamer\" protection.");
        }
    }
}
