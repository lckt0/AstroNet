using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiVM
{
    public class SafeAnalyzer
    {
        public static bool CanObfuscate(MethodDef item)
        {
            if (item.IsRuntimeSpecialName)
                return false;
            if (item.IsConstructor || item.IsStaticConstructor)
                return false;
            if (item.DeclaringType.IsForwarder)
                return false;
            if (!item.HasBody)
                return false;
            if (!item.Body.HasInstructions)
                return false;
            if (item.FullName.Contains(".My")) //VB.NET
                return false;
            if (item.DeclaringType.IsGlobalModuleType)
                return false;
            if (item.FullName.Contains("<Module>"))
                return false;
            return true;
        }
    }
}
