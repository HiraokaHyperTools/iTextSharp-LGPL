using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace System.util
{
    public class UseCryptography
    {
        public static HashAlgorithm MD5()
        {
#if NET5_0_OR_GREATER || NETSTANDARD2_0
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Create("browser")))
            {
                return new DigestWrapped(new MD5Digest());
            }
#endif

            return System.Security.Cryptography.MD5.Create();
        }

        public static HashAlgorithm SHA1()
        {
#if NET5_0_OR_GREATER || NETSTANDARD2_0
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Create("browser")))
            {
                return new DigestWrapped(new Sha1Digest());
            }
#endif

            return System.Security.Cryptography.SHA1.Create();
        }
    }
}
