using iTextSharp.text.pdf;
using NUnit.Framework;
using System;
using System.Text;

namespace itextsharp.test
{
    public class PRTokeniserTest
    {
        [Test]
        public void Test1InputToken()
        {
            var tokeniser = new PRTokeniser(Encoding.ASCII.GetBytes("123.456"));
            tokeniser.NextValidToken();
            Assert.AreEqual("123.456", tokeniser.StringValue);
        }

        [Test]
        public void Test2InputTokens()
        {
            var tokeniser = new PRTokeniser(Encoding.ASCII.GetBytes("123.456 567.890"));
            tokeniser.NextValidToken();
            Assert.AreEqual("123.456", tokeniser.StringValue);
        }

        [Test]
        public void Test3InputTokens()
        {
            var tokeniser = new PRTokeniser(Encoding.ASCII.GetBytes("123.456 567.890 ABC"));
            tokeniser.NextValidToken();
            Assert.AreEqual("123.456", tokeniser.StringValue);
        }
    }
}
