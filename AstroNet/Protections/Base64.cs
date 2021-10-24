using Astro_Renewed.Services;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Text;

namespace Astro_Renewed.Protections
{
    class Base64
    {
        public static void Execute(ModuleDefMD md)
        {
            foreach (TypeDef type in md.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (method.Body == null) continue;
                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                        {
                            //Encoding.UTF8.GetString(Convert.FromBase64String(""));
                            String oldString = method.Body.Instructions[i].Operand.ToString(); //Original String.
                            String newString = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(oldString)); //Encrypted String by Base64
                            method.Body.Instructions[i].OpCode = OpCodes.Nop; //Change the Opcode for the Original Instruction
                            method.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Call, md.Import(typeof(System.Text.Encoding).GetMethod("get_UTF8", new Type[] { })))); //get Method (get_UTF8) from Type (System.Text.Encoding).
                            method.Body.Instructions.Insert(i + 2, new Instruction(OpCodes.Ldstr, newString)); //add the Encrypted String
                            method.Body.Instructions.Insert(i + 3, new Instruction(OpCodes.Call, md.Import(typeof(System.Convert).GetMethod("FromBase64String", new Type[] { typeof(string) })))); //get Method (FromBase64String) from Type (System.Convert), and arguments for method we will get it using "new Type[] { typeof(string) }"
                            method.Body.Instructions.Insert(i + 4, new Instruction(OpCodes.Callvirt, md.Import(typeof(System.Text.Encoding).GetMethod("GetString", new Type[] { typeof(byte[]) }))));
                            i += 4; //skip the Instructions that we have just added.
                        }
                    }
                }
            }
            Services.ConsoleLogger.Log("Processing \"Base64 String\" protection.");
        }
    }
}
