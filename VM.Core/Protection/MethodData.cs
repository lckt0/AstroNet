using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Protection
{
    public class MethodData
    {
        public MethodDef Method;//the methoddef 
        public byte[] DecryptedBytes;//the decrypted bytes
        public byte[] EncryptedBytes;//the encrypted bytes
        public bool Converted = false;//check to see if ive done conversion
        public bool Encrypted = false;//check to see if ive encrypted
        public int ID;//method ID
        public int size;//byte array size
        public int cipherSize;
        public int position;
        public MethodData(MethodDef methods)
        {
            Method = methods;
        }
    }
}
