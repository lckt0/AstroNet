using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Astro_Renewed.Services;
using System.Collections.Generic;
using System.Linq;

namespace Astro_Renewed.Protections
{
    internal class LocalToField
    {
        private static Dictionary<Local, FieldDef> _convertedLocals = new();

        public static void Execute(ModuleDef module)
        {
            foreach (var type in module.Types.Where(x => x != module.GlobalType))
            {
                foreach (var method2 in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions && !x.IsConstructor))
                {
                    _convertedLocals = new Dictionary<Local, FieldDef>();
                    Process(module, method2);
                }
            }
            Services.ConsoleLogger.Log("Processing \"Local To Field\" protection.");
        }

        private static void Process(ModuleDef module, MethodDef method)
        {
            var instructions = method.Body.Instructions;
            foreach (var t in instructions)
            {
                if (t.Operand is not Local local) continue;
                FieldDef def;
                if (!_convertedLocals.ContainsKey(local))
                {
                    def = new FieldDefUser(RandomGenerator.randomMD5(), new FieldSig(local.Type), FieldAttributes.Public | FieldAttributes.Static);
                    module.GlobalType.Fields.Add(def);
                    _convertedLocals.Add(local, def);
                }
                else
                    def = _convertedLocals[local];

                var eq = t.OpCode.Code switch
                {
                    Code.Ldloc => OpCodes.Ldsfld,
                    Code.Ldloc_S => OpCodes.Ldsfld,
                    Code.Ldloc_0 => OpCodes.Ldsfld,
                    Code.Ldloc_1 => OpCodes.Ldsfld,
                    Code.Ldloc_2 => OpCodes.Ldsfld,
                    Code.Ldloc_3 => OpCodes.Ldsfld,
                    Code.Ldloca => OpCodes.Ldsflda,
                    Code.Ldloca_S => OpCodes.Ldsflda,
                    Code.Stloc => OpCodes.Stsfld,
                    Code.Stloc_0 => OpCodes.Stsfld,
                    Code.Stloc_1 => OpCodes.Stsfld,
                    Code.Stloc_2 => OpCodes.Stsfld,
                    Code.Stloc_3 => OpCodes.Stsfld,
                    Code.Stloc_S => OpCodes.Stsfld,
                    _ => null
                };
                t.OpCode = eq;
                t.Operand = def;
            }
        }
    }
}