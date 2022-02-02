using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace itextsharp.test
{
    internal static class TestFileUtil
    {
        internal static string Locate(string path)
        {
            return Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                "Files",
                path
            );
        }

        internal static string NewTempPdfFilePath()
        {
            return Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                Guid.NewGuid().ToString("N") + ".pdf"
            );
        }
    }
}
