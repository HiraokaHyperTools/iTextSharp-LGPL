using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace System.util
{
    public class UrlDownload
    {
        public static Stream DownloadFrom(Uri url)
        {
#pragma warning disable SYSLIB0014
            WebRequest w = WebRequest.Create(url);
            return w.GetResponse().GetResponseStream();
#pragma warning restore SYSLIB0014
        }
    }
}
