using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Astro_Renewed.Protections
{
    class Attribute
    {
        private static string ConvertStringtoMD5(string strword)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(strword);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private static string randomMD5()
        {
            var bytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            return "{" + ConvertStringtoMD5(BitConverter.ToString(bytes)).ToLower() + "}";
        }

        public static void Execute(ModuleDefMD module)
        {
            var attrLs = new List<string>()
            {
                "Borland_Delphi",
                "AstroObfuscator",
                "ProtectedByAstro",
                "AstroByLockT",
                "...",
                "..."
            };

            for (int i = 0; i < 20; i++)
            {
                attrLs.Add(randomMD5());
            }

            foreach (string attr in attrLs)
            {
                TypeRef attrRef = module.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", attr);
                var ctorRef = new MemberRefUser(module, ".ctor", MethodSig.CreateInstance(module.CorLibTypes.Int32), attrRef);
                var attrx = new CustomAttribute(ctorRef);
                module.CustomAttributes.Add(attrx);
            }

            Resource res1 = null;

            foreach (Resource res in module.Resources)
            {
                if (res.Name != "AstroNet.resources" && res.Name.EndsWith(".resources"))
                {
                    res1 = res;
                    //res.Name = randomMD5();
                }
            }

            foreach (Resource res in module.Resources)
            {
                if (res.Name == "AstroNet.resources")
                {
                    res.Attributes = res1.Attributes;
                    res.Visibility = res1.Visibility;
                }
            }


            module.Assembly.Name = $"[>AstroNet<] {randomMD5()}";
            module.Name = $"PE {randomMD5()}";

            Services.ConsoleLogger.Log("Processing \"Metadata Attribute\" protection.");
        }
    }
}
