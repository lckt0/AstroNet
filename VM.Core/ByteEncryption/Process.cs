using Core.Protection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
//using ConversionBack;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace Core.ByteEncryption
{
    class Process
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr LoadLibrary(string dllToLoad);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress", ExactSpelling = true)]
        private static extern IntPtr e(IntPtr intptr, string str);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "GetModuleHandle")]
        private static extern IntPtr ab(string str);
        public delegate void abc(byte[] bytes, int len, byte[] key, int keylen);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(string dllToLoad, IntPtr hFile, uint flags);

        public static byte[] tester(MethodDef methodDef, ModuleDefMD updated)
        {
            dnlib.IO.IImageStream streamFull = updated.MetaData.PEImage.CreateFullStream();
            var upated = (updated.ResolveToken(methodDef.MDToken.ToInt32()) as MethodDef);
            var offset = updated.MetaData.PEImage.ToFileOffset(upated.RVA);
            streamFull.Position = (long)offset;
            byte b = streamFull.ReadByte();

            ushort flags;
            byte headerSize;
            ushort maxStack;
            uint codeSize = 0;

            switch (b & 7)
            {
                case 2:
                case 6:
                    flags = 2;
                    maxStack = 8;
                    codeSize = (uint)(b >> 2);
                    headerSize = 1;
                    break;

                case 3:
                    flags = (ushort)((streamFull.ReadByte() << 8) | b);
                    headerSize = (byte)(flags >> 12);
                    maxStack = streamFull.ReadUInt16();
                    codeSize = streamFull.ReadUInt32();
                    break;
            }
            if (codeSize != 0)
            {
                byte[] il_byte = new byte[codeSize];
                streamFull.Position = (long)offset + upated.Body.HeaderSize;
                streamFull.Read(il_byte, 0, il_byte.Length);
                return il_byte;
            }
            return null;
        }
        [DllImport("NativePRo.dll")]
        public static extern void a(byte[] bytes, int len, byte[] key, int keylen);

        public unsafe static void processConvertedMethods(ModuleDefMD moduleDefMD, List<MethodData> allMethodDatas)
        {
            int pos = 0;
            Stream tester = new MemoryStream();
            ModuleWriterOptions modopts = new ModuleWriterOptions(moduleDefMD);
            modopts.MetaDataOptions.Flags = MetaDataFlags.PreserveAll;
            modopts.Logger = DummyLogger.NoThrowInstance;
            moduleDefMD.Write(tester, modopts);
            ModuleDefMD updated = ModuleDefMD.Load(tester);

            foreach (MethodData methodData in allMethodDatas)
            {
                var decryptedBytes = methodData.DecryptedBytes;
                var method = methodData.Method;
                var md5 = MD5.Create();
                byte[] methodBytes = Process.tester(method, updated);

                var nameHash = md5.ComputeHash(Encoding.ASCII.GetBytes(method.Name));
                var enc = ByteEncryption.Encrypt(nameHash, decryptedBytes);
                //   ConvertBack.a(enc, enc.Length, methodBytes, methodBytes.Length);

                //	Console.WriteLine("got to native");

                enc = aMethod2(enc, enc.Length, methodBytes, methodBytes.Length);
                //    conversion(enc, enc.Length, methodBytes, methodBytes.Length);
                methodData.EncryptedBytes = enc;

                methodData.Encrypted = true;
                methodData.size = methodData.EncryptedBytes.Length;
                methodData.position = pos;
                pos += methodData.EncryptedBytes.Length;
                //////////////////////////////////////////////////////////




            }
        }
        /*EXTERN_DLL_EXPORT   void __stdcall a(unsigned char * data, int datalen, unsigned char key[], int keylen) {
              int N1 = 12, N2 = 14, NS = 258, I = 0;
              for (I; I < keylen; I++) NS += NS % (key[I] + 1);

              for (I = 0; I < datalen; I++)
              {
                  NS = key[I % keylen] + NS;
                  N1 = (NS + 5) * (N1 & 255) + (N1 >> 8);
                  N2 = (NS + 7) * (N2 & 255) + (N2 >> 8);
                  NS = ((N1 << 8) + N2) & 255;

                  data[I] = ((data[I]) ^ NS);
              }
              b(data, datalen);

          }*/
        /*EXTERN_DLL_EXPORT void b(unsigned char * toEncrypt, int len) {
                  char key[3] = { 'H', 'C', 'P' }; //Any chars will work, in an array of any size
                  unsigned char * output = toEncrypt;

                  for (int i = 0; i < len; i++)
                      output[i] = toEncrypt[i] ^ key[i % (sizeof(key) / sizeof(char))];

                  //return output;
              }*/
        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private static byte[] b(byte[] toEncrypt, int len)
        {
            string key = "HCP"; //Any chars will work, in an array of any size
            byte[] output = toEncrypt;

            for (int i = 0; i < len; i++)
            {
                //C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
                //ORIGINAL LINE: output[i] = toEncrypt[i] ^ key[i % (sizeof(key) / sizeof(sbyte))];
                output[i] = (byte)(toEncrypt[i] ^ key[i % (key.Length)]);
            }

            return output;
        }
        [Obfuscation(Feature = "virtualization", Exclude = false)]
        //C++ TO C# CONVERTER NOTE: __stdcall is not available in C#:
        //ORIGINAL LINE: EXTERN_DLL_EXPORT void __stdcall a(byte * data, int datalen, byte key[], int keylen)
        private static byte[] aMethod2(byte[] data, int datalen, byte[] key, int keylen)
        {
            int N1 = 12;
            int N2 = 14;
            int NS = 258;
            int I = 0;
            for (I = 0; I < keylen; I++)
            {
                NS += NS % (key[I] + 1);
            }

            for (I = 0; I < datalen; I++)
            {
                NS = key[I % keylen] + NS;
                N1 = (NS + 5) * (N1 & 255) + (N1 >> 8);
                N2 = (NS + 7) * (N2 & 255) + (N2 >> 8);
                NS = ((N1 << 8) + N2) & 255;

                data[I] = (byte)((data[I]) ^ NS);
            }
            return b(data, datalen);

        }

        public static byte[] aMethod(byte[] data, int datalen, byte[] key, int keylen)
        {
            int N1 = 12, N2 = 14, NS = 258, I = 0;
            for (I = 0; I < keylen; I++) NS += NS % (key[I] + 1);
            for (I = 0; I < datalen; I++)
            {
                NS = key[I % keylen] + NS;
                N1 = (NS + 5) * (N1 & 255) + (N1 >> 8);
                N2 = (NS + 7) * (N2 & 255) + (N2 >> 8);
                NS = ((N1 << 8) + N2) & 255;

                data[I] = (byte)((byte)(data[I]) ^ NS);
            }
            return Bmethod(data, data.Length);
        }

        public static byte[] Bmethod(byte[] toEncrypt, int len)
        {
            char[] key = { 'H', 'C', 'P' };
            byte[] output = toEncrypt;
            for (int i = 0; i < len; i++)
                output[i] = (byte)(toEncrypt[i] ^ key[i % (3 / sizeof(char))]);
            return output;

        }
    }
}
