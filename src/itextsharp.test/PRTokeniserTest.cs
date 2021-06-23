using iTextSharp.text.pdf;
using NUnit.Framework;
using System;
using System.Text;

namespace itextsharp.test
{
    public class PRTokeniserTest
    {
        [Test]
        public void TestEmpty()
        {
            {
                var tokeniser = new PRTokeniser(Encoding.ASCII.GetBytes(""));
                tokeniser.NextValidToken();
                Assert.AreEqual(0, tokeniser.TokenType);
                Assert.IsNull(tokeniser.StringValue);
            }

            {
                var tokeniser = new PRTokeniser(Encoding.ASCII.GetBytes(" "));
                tokeniser.NextValidToken();
                Assert.AreEqual(0, tokeniser.TokenType);
                Assert.IsNull(tokeniser.StringValue);
            }
        }

        [Test]
        public void TestNumber()
        {
            {
                var tokeniser = new PRTokeniser(Encoding.ASCII.GetBytes("123.456"));
                tokeniser.NextValidToken();
                Assert.AreEqual(PRTokeniser.TK_NUMBER, tokeniser.TokenType);
                Assert.AreEqual("123.456", tokeniser.StringValue);
            }

            {
                var tokeniser = new PRTokeniser(Encoding.ASCII.GetBytes("123.456 567.890"));
                tokeniser.NextValidToken();
                Assert.AreEqual(PRTokeniser.TK_NUMBER, tokeniser.TokenType);
                Assert.AreEqual("123.456", tokeniser.StringValue);
            }

            {
                var tokeniser = new PRTokeniser(Encoding.ASCII.GetBytes("123.456 567.890 ABC"));
                tokeniser.NextValidToken();
                Assert.AreEqual(PRTokeniser.TK_NUMBER, tokeniser.TokenType);
                Assert.AreEqual("123.456", tokeniser.StringValue);
            }
        }

        [Test]
        public void TestRef()
        {
            var tokeniser = new PRTokeniser(Encoding.ASCII.GetBytes("123 456 R"));
            tokeniser.NextValidToken();
            Assert.AreEqual(PRTokeniser.TK_REF, tokeniser.TokenType);
            Assert.AreEqual(123, tokeniser.Reference);
            Assert.AreEqual(456, tokeniser.Generation);
            Assert.AreEqual("R", tokeniser.StringValue);
        }
    }
}
