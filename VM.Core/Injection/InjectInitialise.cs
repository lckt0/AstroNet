using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using dnlib.PE;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Core.Injection
{
    class InjectInitialise
    {
        public static MemberRef conversionInit;
        public static MemberRef convertBack;

        public static void initaliseMethod()
        {

            byte[] conversionPlain = System.IO.File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Runtime.dll"));
            conversionAssembly = Assembly.Load(conversionPlain).ManifestModule;

            conversionDef = ModuleDefMD.Load(conversionPlain);

        }
        public static void injectIntoCctor(ModuleDefMD moduleDefMD, string ResName)
        {

            //var conversionTypes = conversionDef.Types;

            foreach (TypeDef t in conversionDef.Types)
            {
                foreach (MethodDef m in t.Methods)
                {
                    if (m.Name == "Initalize")
                    {
                        conversionInit = (MemberRef)moduleDefMD.Import(m);
                    }
                    if (m.Name == "Runner")
                    {
                        convertBack = (MemberRef)moduleDefMD.Import(m);
                    }
                }
            }

            //conversionInit = Protector.moduleDefMD.Import(conversionTypes[33].Methods[2]);
            var a = typeof(Resource);
            var asm = ModuleDefMD.Load(typeof(Resource).Assembly.Location);
            var tester2 = asm.GetTypes();
            var abc = InjectHelper.Inject(tester2.ToArray()[13], moduleDefMD.GlobalType, moduleDefMD);
            foreach (MethodDef md in moduleDefMD.GlobalType.Methods)
            {
                if (md.Name == ".ctor")
                {
                    moduleDefMD.GlobalType.Remove(md);
                    //Now we go out of this mess
                    break;
                }
            }
            if (moduleDefMD.GlobalType.FindOrCreateStaticConstructor().Body == null)
            {
                var cil = new CilBody();


                cil.Instructions.Add(new Instruction(OpCodes.Call, moduleDefMD.Types[0].Methods[0]));

                cil.Instructions.Add(new Instruction(OpCodes.Ret));
                moduleDefMD.GlobalType.FindOrCreateStaticConstructor().Body = cil;
            }
            else
            {
                var vody = moduleDefMD.GlobalType.FindOrCreateStaticConstructor().Body;
                //vody.Instructions.Insert(0, new Instruction(OpCodes.Call, Protector.moduleDefMD.Types[0].Methods[3]));
                vody.Instructions.Insert(0, new Instruction(OpCodes.Call, moduleDefMD.Types[0].Methods.Where(i => i.Name == "setup").First()));

                if ((moduleDefMD.Characteristics & Characteristics.Dll) != 0)
                {

                    vody.Instructions.Insert(1, new Instruction(OpCodes.Ldstr, "TestResc"));

                    vody.Instructions.Insert(2, new Instruction(OpCodes.Call, InjectInitialise.conversionInit));


                }


            }
        }
        public static void InjectMethod(ModuleDefMD moduleDefMD ,MethodDef meth, int pos, int id, int size)
        {
            Console.WriteLine("started");
            var containsOut = false;
            meth.Body.Instructions.Clear();
            var rrr = meth.Parameters.Where(i => i.Type.FullName.EndsWith("&"));
            if (rrr.Count() != 0)
                containsOut = true;

            var rrg = moduleDefMD.CorLibTypes.Object.ToSZArraySig();
            var loc = new Local(moduleDefMD.CorLibTypes.Object);
            var loc2 = new Local(new SZArraySig(moduleDefMD.CorLibTypes.Object));
            var cli = new CilBody();
            foreach (var bodyVariable in meth.Body.Variables)
                cli.Variables.Add(bodyVariable);
            cli.Variables.Add(loc);
            cli.Variables.Add(loc2);
            var outParams = new List<Local>();
            var testerDictionary = new Dictionary<Parameter, Local>();
            if (containsOut)
                foreach (var parameter in rrr)
                {
                    var locf = new Local(parameter.Type.Next);
                    testerDictionary.Add(parameter, locf);
                    cli.Variables.Add(locf);
                }



            var outp = 0;
            cli.Instructions.Add(new Instruction(OpCodes.Ldc_I4, meth.Parameters.Count));
            cli.Instructions.Add(new Instruction(OpCodes.Newarr, moduleDefMD.CorLibTypes.Object.ToTypeDefOrRef()));
            for (var i = 0; i < meth.Parameters.Count; i++)
            {
                var par = meth.Parameters[i];
                cli.Instructions.Add(new Instruction(OpCodes.Dup));
                cli.Instructions.Add(new Instruction(OpCodes.Ldc_I4, i));
                if (containsOut)
                    if (rrr.Contains(meth.Parameters[i]))
                    {
                        cli.Instructions.Add(new Instruction(OpCodes.Ldloc, testerDictionary[meth.Parameters[i]]));
                        outp++;
                    }
                    else
                    {
                        cli.Instructions.Add(new Instruction(OpCodes.Ldarg, meth.Parameters[i]));
                    }
                else
                    cli.Instructions.Add(new Instruction(OpCodes.Ldarg, meth.Parameters[i]));

                if (true)
                {
                    cli.Instructions.Add(par.Type.FullName.EndsWith("&")
                        ? new Instruction(OpCodes.Box, par.Type.Next.ToTypeDefOrRef())
                        : new Instruction(OpCodes.Box, par.Type.ToTypeDefOrRef()));
                    cli.Instructions.Add(new Instruction(OpCodes.Stelem_Ref));
                }
            }


            cli.Instructions.Add(new Instruction(OpCodes.Stloc, loc2));

            cli.Instructions.Add(new Instruction(OpCodes.Ldc_I4, pos));


            cli.Instructions.Add(new Instruction(OpCodes.Ldc_I4, size));
            cli.Instructions.Add(new Instruction(OpCodes.Ldc_I4, id));

            cli.Instructions.Add(new Instruction(OpCodes.Ldloc, loc2));


            cli.Instructions.Add(new Instruction(OpCodes.Call, convertBack));
            if (meth.HasReturnType)
                cli.Instructions.Add(new Instruction(OpCodes.Unbox_Any, meth.ReturnType.ToTypeDefOrRef()));
            else
                cli.Instructions.Add(new Instruction(OpCodes.Stloc, loc));
            if (containsOut)
            {
                foreach (var parameter in rrr)
                {
                    /*10	0012	ldloc.1
               11	0013	ldc.i4.0
               12	0014	ldelem.ref
               13	0015	castclass	[mscorlib]System.String
               14	001A	stloc.0
               */
                    cli.Instructions.Add(new Instruction(OpCodes.Ldarg, parameter));
                    cli.Instructions.Add(new Instruction(OpCodes.Ldloc, loc2));
                    cli.Instructions.Add(new Instruction(OpCodes.Ldc_I4, meth.Parameters.IndexOf(parameter)));
                    cli.Instructions.Add(new Instruction(OpCodes.Ldelem_Ref));
                    cli.Instructions.Add(new Instruction(OpCodes.Unbox_Any, parameter.Type.Next.ToTypeDefOrRef()));
                    cli.Instructions.Add(new Instruction(OpCodes.Stind_Ref));


                }
                cli.Instructions.Add(new Instruction(OpCodes.Ret));
            }
            else
                cli.Instructions.Add(new Instruction(OpCodes.Ret));
            //     module.EntryPoint.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldstr, "Tester"));
            //    module.EntryPoint.Body.Instructions.Insert(1, new Instruction(OpCodes.Call, init));
            //    module.EntryPoint.Body.Instructions.Insert(1, new Instruction(OpCodes.Stloc, loc));
            meth.Body = cli;
            meth.Body.UpdateInstructionOffsets();
            meth.Body.MaxStack += 10;
        }

        public static System.Reflection.Module conversionAssembly { get; set; }

        public static ModuleDefMD conversionDef { get; set; }
    }
}
