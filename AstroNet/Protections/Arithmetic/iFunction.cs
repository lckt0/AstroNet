using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Astro_Renewed.Protections.Arithmetic
{
    public abstract class Function
    {
        public abstract ArithmeticVt Arithmetic(Instruction instruction, ModuleDef module);
    }
}