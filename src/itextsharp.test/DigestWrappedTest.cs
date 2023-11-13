using NUnit.Framework;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.util;

namespace itextsharp.test
{
    public class DigestWrappedTest
    {
        [Test]
        [TestCase(new byte[0])]
        [TestCase(new byte[] { 0, })]
        [TestCase(new byte[] { 0, 0, })]
        [TestCase(new byte[] { 0, 0, 0, })]
        [TestCase(new byte[] { 0, 0, 0, 0, })]
        [TestCase(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })]
        public void TestHashAlgorithms(byte[] data)
        {
            CompareHashedValue(data, MD5.Create(), new DigestWrapped(new MD5Digest()));
            CompareHashedValue(data, SHA1.Create(), new DigestWrapped(new Sha1Digest()));
        }

        private void CompareHashedValue(byte[] data, HashAlgorithm expected, HashAlgorithm actual)
        {
            var outExpected = expected.ComputeHash(data);
            var outActual = actual.ComputeHash(data);
            CollectionAssert.AreEqual(outExpected, outActual);
        }
    }
}
