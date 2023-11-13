using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace System.util
{
    public class DigestWrapped : HashAlgorithm
    {
        private readonly IDigest _digest;

        public DigestWrapped(IDigest digest)
        {
            _digest = digest;
        }

        public override void Initialize()
        {
            _digest.Reset();
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _digest.BlockUpdate(array, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            var hash = new byte[_digest.GetDigestSize()];
            _digest.DoFinal(hash, 0);
            return hash;
        }
    }
}
