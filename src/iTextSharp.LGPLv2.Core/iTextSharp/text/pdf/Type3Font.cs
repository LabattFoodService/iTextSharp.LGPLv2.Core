using System;
using System.Collections;

namespace iTextSharp.text.pdf
{

    /// <summary>
    /// A class to support Type3 fonts.
    /// </summary>
    public class Type3Font : BaseFont
    {

        private readonly Hashtable _char2Glyph = new Hashtable();
        private readonly bool _colorized;
        private readonly PageResources _pageResources = new PageResources();
        private readonly bool[] _usedSlot;
        private readonly IntHashtable _widths3 = new IntHashtable();
        private readonly PdfWriter _writer;
        private float _llx = float.NaN, _lly, _urx, _ury;

        /// <summary>
        /// Creates a Type3 font.
        /// and only images as masks can be used
        /// </summary>
        /// <param name="writer">the writer</param>
        /// <param name="chars">an array of chars corresponding to the glyphs used (not used, prisent for compability only)</param>
        /// <param name="colorized">if  true  the font may specify color, if  false  no color commands are allowed</param>
        public Type3Font(PdfWriter writer, char[] chars, bool colorized) : this(writer, colorized)
        {
        }

        /// <summary>
        /// Creates a Type3 font. This implementation assumes that the /FontMatrix is
        /// [0.001 0 0 0.001 0 0] or a 1000-unit glyph coordinate system.
        ///
        /// An example:
        ///
        ///
        /// Document document = new Document(PageSize.A4);
        /// PdfWriter writer = PdfWriter.getInstance(document, new FileOutputStream("type3.pdf"));
        /// document.open();
        /// Type3Font t3 = new Type3Font(writer, false);
        /// PdfContentByte g = t3.defineGlyph('a', 1000, 0, 0, 750, 750);
        /// g.rectangle(0, 0, 750, 750);
        /// g.fill();
        /// g = t3.defineGlyph('b', 1000, 0, 0, 750, 750);
        /// g.moveTo(0, 0);
        /// g.lineTo(375, 750);
        /// g.lineTo(750, 0);
        /// g.fill();
        /// Font f = new Font(t3, 12);
        /// document.add(new Paragraph("ababab", f));
        /// document.close();
        ///
        /// and only images as masks can be used
        /// </summary>
        /// <param name="writer">the writer</param>
        /// <param name="colorized">if  true  the font may specify color, if  false  no color commands are allowed</param>
        public Type3Font(PdfWriter writer, bool colorized)
        {
            _writer = writer;
            _colorized = colorized;
            fontType = FONT_TYPE_T3;
            _usedSlot = new bool[256];
        }

        public override string[][] AllNameEntries
        {
            get
            {
                return new[] { new[] { "4", "", "", "", "" } };
            }
        }

        public override string[][] FamilyFontName
        {
            get
            {
                return FullFontName;
            }
        }

        public override string[][] FullFontName
        {
            get
            {
                return new[] { new[] { "", "", "", "" } };
            }
        }

        public override string PostscriptFontName
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public override bool CharExists(int c)
        {
            if (c > 0 && c < 256)
            {
                return _usedSlot[c];
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Defines a glyph. If the character was already defined it will return the same content
        ///  true  the value is ignored
        ///  true  the value is ignored
        ///  true  the value is ignored
        ///  true  the value is ignored
        /// </summary>
        /// <param name="c">the character to match this glyph.</param>
        /// <param name="wx">the advance this character will have</param>
        /// <param name="llx">the X lower left corner of the glyph bounding box. If the  colorize  option is</param>
        /// <param name="lly">the Y lower left corner of the glyph bounding box. If the  colorize  option is</param>
        /// <param name="urx">the X upper right corner of the glyph bounding box. If the  colorize  option is</param>
        /// <param name="ury">the Y upper right corner of the glyph bounding box. If the  colorize  option is</param>
        /// <returns>a content where the glyph can be defined</returns>
        public PdfContentByte DefineGlyph(char c, float wx, float llx, float lly, float urx, float ury)
        {
            if (c == 0 || c > 255)
                throw new ArgumentException("The char " + (int)c + " doesn't belong in this Type3 font");
            _usedSlot[c] = true;
            Type3Glyph glyph = (Type3Glyph)_char2Glyph[c];
            if (glyph != null)
                return glyph;
            _widths3[c] = (int)wx;
            if (!_colorized)
            {
                if (float.IsNaN(_llx))
                {
                    _llx = llx;
                    _lly = lly;
                    _urx = urx;
                    _ury = ury;
                }
                else
                {
                    _llx = Math.Min(_llx, llx);
                    _lly = Math.Min(_lly, lly);
                    _urx = Math.Max(_urx, urx);
                    _ury = Math.Max(_ury, ury);
                }
            }
            glyph = new Type3Glyph(_writer, _pageResources, wx, llx, lly, urx, ury, _colorized);
            _char2Glyph[c] = glyph;
            return glyph;
        }
        public override int[] GetCharBBox(int c)
        {
            return null;
        }

        public override float GetFontDescriptor(int key, float fontSize)
        {
            return 0;
        }
        /// <summary>
        /// Always returns null, because you can't get the FontStream of a Type3 font.
        /// @since   2.1.3
        /// </summary>
        /// <returns>null</returns>
        public override PdfStream GetFullFontStream()
        {
            return null;
        }

        public override int GetKerning(int char1, int char2)
        {
            return 0;
        }
        public override int GetWidth(int char1)
        {
            if (!_widths3.ContainsKey(char1))
                throw new ArgumentException("The char " + char1 + " is not defined in a Type3 font");
            return _widths3[char1];
        }

        public override int GetWidth(string text)
        {
            char[] c = text.ToCharArray();
            int total = 0;
            for (int k = 0; k < c.Length; ++k)
                total += GetWidth(c[k]);
            return total;
        }

        public override bool HasKernPairs()
        {
            return false;
        }

        public override bool SetCharAdvance(int c, int advance)
        {
            return false;
        }

        public override bool SetKerning(int char1, int char2, int kern)
        {
            return false;
        }

        internal override byte[] ConvertToBytes(string text)
        {
            char[] cc = text.ToCharArray();
            byte[] b = new byte[cc.Length];
            int p = 0;
            for (int k = 0; k < cc.Length; ++k)
            {
                char c = cc[k];
                if (CharExists(c))
                    b[p++] = (byte)c;
            }
            if (b.Length == p)
                return b;
            byte[] b2 = new byte[p];
            Array.Copy(b, 0, b2, 0, p);
            return b2;
        }

        internal override byte[] ConvertToBytes(int char1)
        {
            if (CharExists(char1))
                return new[] { (byte)char1 };
            else return new byte[0];
        }

        internal override int GetRawWidth(int c, string name)
        {
            return 0;
        }

        internal override void WriteFont(PdfWriter writer, PdfIndirectReference piRef, object[] oParams)
        {
            if (_writer != writer)
                throw new ArgumentException("Type3 font used with the wrong PdfWriter");
            // Get first & lastchar ...
            int firstChar = 0;
            while (firstChar < _usedSlot.Length && !_usedSlot[firstChar]) firstChar++;

            if (firstChar == _usedSlot.Length)
            {
                throw new DocumentException("No glyphs defined for Type3 font");
            }
            int lastChar = _usedSlot.Length - 1;
            while (lastChar >= firstChar && !_usedSlot[lastChar]) lastChar--;

            int[] widths = new int[lastChar - firstChar + 1];
            int[] invOrd = new int[lastChar - firstChar + 1];

            int invOrdIndx = 0, w = 0;
            for (int u = firstChar; u <= lastChar; u++, w++)
            {
                if (_usedSlot[u])
                {
                    invOrd[invOrdIndx++] = u;
                    widths[w] = _widths3[u];
                }
            }
            PdfArray diffs = new PdfArray();
            PdfDictionary charprocs = new PdfDictionary();
            int last = -1;
            for (int k = 0; k < invOrdIndx; ++k)
            {
                int c = invOrd[k];
                if (c > last)
                {
                    last = c;
                    diffs.Add(new PdfNumber(last));
                }
                ++last;
                int c2 = invOrd[k];
                string s = GlyphList.UnicodeToName(c2);
                if (s == null)
                    s = "a" + c2;
                PdfName n = new PdfName(s);
                diffs.Add(n);
                Type3Glyph glyph = (Type3Glyph)_char2Glyph[(char)c2];
                PdfStream stream = new PdfStream(glyph.ToPdf(null));
                stream.FlateCompress(compressionLevel);
                PdfIndirectReference refp = writer.AddToBody(stream).IndirectReference;
                charprocs.Put(n, refp);
            }
            PdfDictionary font = new PdfDictionary(PdfName.Font);
            font.Put(PdfName.Subtype, PdfName.Type3);
            if (_colorized)
                font.Put(PdfName.Fontbbox, new PdfRectangle(0, 0, 0, 0));
            else
                font.Put(PdfName.Fontbbox, new PdfRectangle(_llx, _lly, _urx, _ury));
            font.Put(PdfName.Fontmatrix, new PdfArray(new[] { 0.001f, 0, 0, 0.001f, 0, 0 }));
            font.Put(PdfName.Charprocs, writer.AddToBody(charprocs).IndirectReference);
            PdfDictionary encoding = new PdfDictionary();
            encoding.Put(PdfName.Differences, diffs);
            font.Put(PdfName.Encoding, writer.AddToBody(encoding).IndirectReference);
            font.Put(PdfName.Firstchar, new PdfNumber(firstChar));
            font.Put(PdfName.Lastchar, new PdfNumber(lastChar));
            font.Put(PdfName.Widths, writer.AddToBody(new PdfArray(widths)).IndirectReference);
            if (_pageResources.HasResources())
                font.Put(PdfName.Resources, writer.AddToBody(_pageResources.Resources).IndirectReference);
            writer.AddToBody(font, piRef);
        }

        protected override int[] GetRawCharBBox(int c, string name)
        {
            return null;
        }
    }
}