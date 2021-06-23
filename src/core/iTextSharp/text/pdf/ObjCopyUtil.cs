using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static iTextSharp.text.pdf.PdfWriter;

namespace iTextSharp.text.pdf
{
    public class ObjCopyUtil
    {
        private readonly PdfWriter writer;
        private readonly Dictionary<RefKey, IndirectReferences> indirects = new Dictionary<RefKey, IndirectReferences>();
        private PdfBody body => writer.body;

        public ObjCopyUtil(PdfWriter writer)
        {
            this.writer = writer;
        }

        public PdfIndirectReference CopyIndirect(PRIndirectReference inp)
        {
            PdfIndirectReference theRef;
            RefKey key = new RefKey(inp);
            IndirectReferences iRef;
            indirects.TryGetValue(key, out iRef);
            if (iRef != null)
            {
                theRef = iRef.Ref;
                if (iRef.Copied)
                {
                    return theRef;
                }
            }
            else
            {
                theRef = body.PdfIndirectReference;
                iRef = new IndirectReferences(theRef);
                indirects[key] = iRef;
            }
            PdfObject obj = PdfReader.GetPdfObjectRelease(inp);
            if (obj != null && obj.IsDictionary())
            {
                PdfObject type = PdfReader.GetPdfObjectRelease(((PdfDictionary)obj).Get(PdfName.TYPE));
                if (type != null && PdfName.PAGE.Equals(type))
                {
                    return theRef;
                }
            }
            iRef.SetCopied();
            obj = CopyObject(obj);
            writer.AddToBody(obj, theRef);
            return theRef;
        }

        private class RefKey
        {
            internal int num;
            internal int gen;
            internal RefKey(int num, int gen)
            {
                this.num = num;
                this.gen = gen;
            }
            internal RefKey(PdfIndirectReference refi)
            {
                num = refi.Number;
                gen = refi.Generation;
            }
            internal RefKey(PRIndirectReference refi)
            {
                num = refi.Number;
                gen = refi.Generation;
            }
            public override int GetHashCode()
            {
                return (gen << 16) + num;
            }
            public override bool Equals(Object o)
            {
                if (!(o is RefKey)) return false;
                RefKey other = (RefKey)o;
                return this.gen == other.gen && this.num == other.num;
            }
            public override String ToString()
            {
                return "" + num + " " + gen;
            }
        }

        private class IndirectReferences
        {
            PdfIndirectReference theRef;
            bool hasCopied;
            internal IndirectReferences(PdfIndirectReference refi)
            {
                theRef = refi;
                hasCopied = false;
            }
            internal void SetCopied() { hasCopied = true; }
            internal bool Copied
            {
                get
                {
                    return hasCopied;
                }
            }
            internal PdfIndirectReference Ref
            {
                get
                {
                    return theRef;
                }
            }
        }

        public PdfObject CopyObject(PdfObject inp)
        {
            if (inp == null)
                return PdfNull.PDFNULL;
            switch (inp.Type)
            {
                case PdfObject.DICTIONARY:
                    return CopyDictionary((PdfDictionary)inp);
                case PdfObject.INDIRECT:
                    return CopyIndirect((PRIndirectReference)inp);
                case PdfObject.ARRAY:
                    return CopyArray((PdfArray)inp);
                case PdfObject.NUMBER:
                case PdfObject.NAME:
                case PdfObject.STRING:
                case PdfObject.NULL:
                case PdfObject.BOOLEAN:
                case 0:
                    return inp;
                case PdfObject.STREAM:
                    return CopyStream((PRStream)inp);
                //                return in;
                default:
                    if (inp.Type < 0)
                    {
                        String lit = ((PdfLiteral)inp).ToString();
                        if (lit.Equals("true") || lit.Equals("false"))
                        {
                            return new PdfBoolean(lit);
                        }
                        return new PdfLiteral(lit);
                    }
                    return null;
            }
        }

        public PdfDictionary CopyDictionary(PdfDictionary inp)
        {
            PdfDictionary outp = new PdfDictionary();
            PdfObject type = PdfReader.GetPdfObjectRelease(inp.Get(PdfName.TYPE));

            foreach (PdfName key in inp.Keys)
            {
                PdfObject value = inp.Get(key);
                if (type != null && PdfName.PAGE.Equals(type))
                {
                    if (!key.Equals(PdfName.B) && !key.Equals(PdfName.PARENT))
                        outp.Put(key, CopyObject(value));
                }
                else
                    outp.Put(key, CopyObject(value));
            }
            return outp;
        }

        public PdfArray CopyArray(PdfArray inp)
        {
            PdfArray outp = new PdfArray();

            foreach (PdfObject value in inp.ArrayList)
            {
                outp.Add(CopyObject(value));
            }
            return outp;
        }

        public PdfStream CopyStream(PRStream inp)
        {
            PRStream outp = new PRStream(inp, null);

            foreach (PdfName key in inp.Keys)
            {
                PdfObject value = inp.Get(key);
                outp.Put(key, CopyObject(value));
            }

            return outp;
        }
    }
}
