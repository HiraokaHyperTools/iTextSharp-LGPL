using iTextSharp.text.pdf;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace itextsharp.test
{
    public class ProducerTest
    {
        [Test]
        public void ProducerModification()
        {
            var newPdf = TestFileUtil.NewTempPdfFilePath();

            using (var outSt = File.Create(newPdf))
            {
                using (var inSt = File.OpenRead(TestFileUtil.Locate(@"Word.pdf")))
                {
                    var reader = new PdfReader(inSt);
                    var stamper = new PdfStamper(reader, outSt);

                    stamper.Close();
                }
            }

            using (var inSt = File.OpenRead(newPdf))
            {
                var reader = new PdfReader(inSt);

                StringAssert.StartsWith(
                    "Microsoft® Word 2013; modified using iTextSharp",
                    (string)reader.Info["Producer"]
                );
            }
        }
    }
}
