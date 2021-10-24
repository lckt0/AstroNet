using Astro_Renewed.Services;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Astro_Renewed.Services;

namespace Astro_Renewed.Protections
{
    public static class StringEnc
    {
        internal class stub
        {
            public static string Decoder(string plainText)
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames()
                    .Single(str => str.EndsWith("AstroNet.resources"));

                string result = "";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
                return DecodeComplete(plainText, result);
            }

            public static string DecodeComplete(string plainText, string result)
            {
                string line = plainText.Replace("AstroNet_", "");
                line = line.Replace(line.Substring(line.Length - 6, 6), "");
                int lineNum = Convert.ToInt32(line) / 1337;

                string[] readLines = result.Split('\n');

                return Encoding.UTF8.GetString(Convert.FromBase64String(EncryptStr(readLines[lineNum - 1])));
            }

            private static string EncryptStr(string plainText)
            {
                return Decrypt(plainText, true);
            }

            public static string Decrypt(string cipherString, bool useHashing)
            {
                byte[] keyArray;
                byte[] toEncryptArray = Convert.FromBase64String(cipherString);
                byte[] resultArray;
                string key = string.Empty;

                //key = ConfigurationManager.AppSettings.Get("SecurityKey"); 
                key = "MAKV2SPBNI99212";


                if (useHashing)
                {
                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    hashmd5.Clear();
                }
                else
                {
                    keyArray = UTF8Encoding.UTF8.GetBytes(key);
                }

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;


                ICryptoTransform cTransform = tdes.CreateDecryptor();
                resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);


                tdes.Clear();


                return UTF8Encoding.UTF8.GetString(resultArray);

            }
        }

        public static string Encrypt(string secureUserData, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(secureUserData);
            string key = string.Empty;
            byte[] resultArray;

            // Get the key from Web.Config file
            //key = ConfigurationManager.AppSettings.Get("EncKey");
            key = "MAKV2SPBNI99212";


            if (useHashing)
            {

                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();

            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();

            resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            tdes.Clear();

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        private static string EncryptStr(string plainText)
        {
            return Encrypt(plainText, true);
        }

        private static int countEnc = 0;
        public static List<string> lsLines = new List<string>();

        public static void Execute(ModuleDef module)
        {
            InjectClass1(module);
            foreach (var type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (var methodDef2 in type.Methods)
                {
                    if (!methodDef2.HasBody || !methodDef2.Body.HasInstructions) continue;
                    if (methodDef2.Name.Contains("Decoder")) continue;
                    for (var i = 0; i < methodDef2.Body.Instructions.Count; i++)
                    {
                        if (methodDef2.Body.Instructions[i].OpCode != OpCodes.Ldstr) continue;
                        var plainText = methodDef2.Body.Instructions[i].Operand.ToString();
                        countEnc++;
                        var operand = string.Concat("AstroNet_", countEnc * 1337) + new Random().Next(100000, 999999); //EncryptStr(plainText);
                        lsLines.Add(EncryptStr(Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText))));
                        methodDef2.Body.Instructions[i].Operand = operand;
                        methodDef2.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Call, InitMemory.Init));
                    }
                    methodDef2.Body.SimplifyBranches();
                }
            }
            if (File.Exists($"{Path.GetTempPath()}AstroNet.resources"))
            { File.Delete($"{Path.GetTempPath()}AstroNet.resources"); }

            File.WriteAllLines($"{Path.GetTempPath()}AstroNet.resources", lsLines);
            var bytes = File.ReadAllBytes($"{Path.GetTempPath()}AstroNet.resources");
            //bytes = bytes;

            module.Resources.Add(new EmbeddedResource("AstroNet.resources", bytes, ManifestResourceAttributes.Public));
            File.Delete($"{Path.GetTempPath()}AstroNet.resources");
            Services.ConsoleLogger.Log("Processing \"String Encryption\" protection.");
        }

        private static void InjectClass1(ModuleDef module)
        {
            var typeModule = ModuleDefMD.Load(typeof(stub).Module);
            var typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(stub).MetadataToken));
            var members = Inject_Helper.InjectHelper.Inject(typeDef, module.GlobalType, module);
            InitMemory.Init = (MethodDef)members.Single(method => method.Name == "Decoder");
            InitMemory.Init2 = (MethodDef)members.Single(method => method.Name == "EncryptStr");
            InitMemory.Init3 = (MethodDef)members.Single(method => method.Name == "Decrypt");
            InitMemory.Init4 = (MethodDef)members.Single(method => method.Name == "DecodeComplete");

            InitMemory.Init.Name = RandomGenerator.randomMD5();
            InitMemory.Init2.Name = RandomGenerator.randomMD5();
            InitMemory.Init3.Name = RandomGenerator.randomMD5();
            InitMemory.Init4.Name = RandomGenerator.randomMD5();

            //Program.VirtualizeMethods.Add(InitMemory.Init.Name);
            //Program.VirtualizeMethods.Add(InitMemory.Init2.Name);
            Program.VirtualizeMethods.Add(InitMemory.Init3.Name);
            Program.VirtualizeMethods.Add(InitMemory.Init4.Name);
            foreach (var md in module.GlobalType.Methods)
            {
                if (md.Name != ".ctor") continue;
                module.GlobalType.Remove(md);
                break;
            }
        }
    }
}