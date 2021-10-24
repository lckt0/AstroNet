using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace KVM
{
    public static class Utils
    {
        public static ModuleWriterOptions ModuleWriterOptions;

        public static ModuleWriterListener ModuleWriterListener;

      /*  public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
        {
            MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
            return expressionBody.Member.Name;
        }
        public static string GetMethodInfo<T>(Expression<Action<T>> expression)
        {
            var member = expression.Body as MethodCallExpression;

            if (member != null)
                return member.Method.Name;

            throw new ArgumentException("Expression is not a method", "expression");
        }*/
        public static void AddListEntry<TKey, TValue>(this IDictionary<TKey, List<TValue>> self, TKey key, TValue value)
        {
            bool flag = key == null;
            bool flag3 = flag;
            if (flag3)
            {
                throw new ArgumentNullException("key");
            }
            List<TValue> list;
            bool flag2 = !self.TryGetValue(key, out list);
            bool flag4 = flag2;
            if (flag4)
            {
                list = (self[key] = new List<TValue>());
            }
            list.Add(value);
        }

        public static StrongNameKey LoadSNKey(string path, string pass)
        {
            if (pass != null) //pfx
            {
                var cert = new X509Certificate2();
                cert.Import(path, pass, X509KeyStorageFlags.Exportable);

                var rsa = cert.PrivateKey as RSACryptoServiceProvider;
                if (rsa == null)
                    throw new ArgumentException("RSA key does not present in the certificate.", "path");

                return new StrongNameKey(rsa.ExportCspBlob(true));
            }
            return new StrongNameKey(path);
        }
    }
}
