using Astro_Renewed.Protections.Arithmetic.Utils;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;

namespace Astro_Renewed.Protections.Arithmetic.Functions.Maths
{
    public class Abs : Function
    {
        public virtual ArithmeticTypes ArithmeticTypes => ArithmeticTypes.Abs;

        public override ArithmeticVt Arithmetic(Instruction instruction, ModuleDef module)
        {
            if (!ArithmeticUtils.CheckArithmetic(instruction)) return null;
            var arithmeticTypes = new List<ArithmeticTypes> { ArithmeticTypes.Add, ArithmeticTypes.Sub };
            var arithmeticEmulator = new ArithmeticEmulator(instruction.GetLdcI4Value(), ArithmeticUtils.GetY(instruction.GetLdcI4Value()), ArithmeticTypes);
            return new ArithmeticVt(new Value(arithmeticEmulator.GetValue(arithmeticTypes), arithmeticEmulator.GetY()), new Token(ArithmeticUtils.GetOpCode(arithmeticEmulator.GetType), module.Import(ArithmeticUtils.GetMethod(ArithmeticTypes))), ArithmeticTypes);
        }
    }
}