using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CFFFont.IO;
using CFFFont.CFFDataType;
using System.Windows.Media;
using System.Windows;

namespace CFFFont
{
    public class CFFFontTypeFace : IDisposable
    {
        private CFFReader _cFFReader;
        private ushort _numberOfGlyphs;

        public ushort NumberOfGlyphs
        {
            get { return _numberOfGlyphs; }
            set { _numberOfGlyphs = value; }
        }

        public CFFFontTypeFace(byte[] fontData) 
        { 
            this._cFFReader = new CFFReader(fontData);
            InitializeCFFFontConmponents();
        }
        private void InitializeCFFFontConmponents()
        {
            // Header
            Card8 major = this._cFFReader.ReadCard8();
            if (major < 1)
                throw new Exception();
            Card8 minor = this._cFFReader.ReadCard8();
            Card8 hdrSize = this._cFFReader.ReadCard8();
            OffSize offSize = this._cFFReader.ReadOffSize();
            // Name Index
            Card16 count1 = this._cFFReader.ReadCard16();
            OffSize offSize1 = this._cFFReader.ReadOffSize();
            Offset8[] offSetsArray1 = new Offset8[count1 + 1];
            for (int i = 0; i < offSetsArray1.Length; i++)
            {
                Offset8 offset = this._cFFReader.ReadOffset8();
                offSetsArray1[i] = offset;
            }
            for (int i = 0; i < offSetsArray1.Length - 1; i++)
            {
                // read data
                ushort length = offSetsArray1[i + 1] - offSetsArray1[i];
                for (int j = 0; j < length; j++)
                {
                    Card8 data = this._cFFReader.ReadCard8();
                }
            }
            // Top Dict Index
            Card16 count2 = this._cFFReader.ReadCard16();
            OffSize offSize2 = this._cFFReader.ReadOffSize();
            Offset8[] offSetsArray2 = new Offset8[count2 + 1];
            for (int i = 0; i < offSetsArray2.Length; i++)
            {
                Offset8 offset = this._cFFReader.ReadOffset8();
                offSetsArray2[i] = offset;
            }
            for (int i = 0; i < offSetsArray2.Length - 1; i++)
            {
                // read data
                ushort length = offSetsArray2[i + 1] - offSetsArray2[i];
                for (int j = 0; j < length; j++)
                {
                    Card8 data = this._cFFReader.ReadCard8();
                    this.DecodeTopDic((byte)data, ref j);
                }
            }
            // CharStrings
            this._cFFReader.Seek((long)this._topDicCFF["CharStrings"]);
            Card16 count3 = this._cFFReader.ReadCard16();
            this._numberOfGlyphs = (ushort)count3;
            OffSize offSize3 = this._cFFReader.ReadOffSize();
            Offset16[] offSetsArray3 = new Offset16[count3 + 1];
            for (int i = 0; i < offSetsArray3.Length; i++)
            {
                Offset16 offset = this._cFFReader.ReadOffset16();
                offSetsArray3[i] = offset;
            }
            for (ushort i = 0; i < offSetsArray3.Length - 1; i++)
            {
                ushort length = offSetsArray3[i + 1] - offSetsArray3[i];
                byte[] dataArray = new byte[length];
                for (int j = 0; j < length; j++)
                {
                    Card8 data = this._cFFReader.ReadCard8();
                    dataArray[j] = (byte)data;
                }
                this._glyphsOutlineDic.Add(i, dataArray);
            }
            // Char Encoding 
            if (this._topDicCFF.ContainsKey("Encoding"))
            {
                this._cFFReader.Seek((long)this._topDicCFF["Encoding"]);
                Card8 encodingFormat = this._cFFReader.ReadCard8();
                if (encodingFormat == 0)
                {
                    ushort gID = 0;
                    Card8 nCodes = this._cFFReader.ReadCard8();
                    Card8[] data = new Card8[(int)nCodes];
                    for (Card8 i = (Card8)0; i < nCodes; i++)
                    {
                        data[(int)i] = this._cFFReader.ReadCard8();
                    }
                    foreach(Card8 code in data)
                    {
                        this._encoding.Add(++gID, (ushort)code);
                    }
                }
                else if (encodingFormat == 1)
                {
                    ushort gID = 0;
                    Card8 nRanges = this._cFFReader.ReadCard8();
                    {
                        for (Card8 i = (Card8)0; i < nRanges; i++)
                        {
                            Card8 first = this._cFFReader.ReadCard8();
                            Card8 nLeft = this._cFFReader.ReadCard8();

                            // range
                            for(int j = (int)first; j <= (first + nLeft); j++)
                            {
                                // In encoding and charset always gID starts at 1
                                // since gID = 0 is always .notdef
                                this._encoding.Add(++gID, (ushort)j);
                            }
                        }
                    }
                }
                if (this._topDicCFF.ContainsKey("Charset"))
                {
                    this._cFFReader.Seek((long)this._topDicCFF["Charset"]);
                    Card8 format = this._cFFReader.ReadCard8();
                    if (format == 0)
                    {

                    }
                    else if (format == 1)
                    {
                        for (int i = 0; i < NumberOfGlyphs - 1; i++)
                        {
                            Card16 sID = this._cFFReader.ReadCard16();
                            Card8 nLeft = this._cFFReader.ReadCard8();
                        }
                    }
                }
            }
        }
        private Dictionary<string, double> _topDicCFF = new Dictionary<string, double>();
        private Dictionary<string, double> _privateDicCFF = new Dictionary<string, double>();
        private Dictionary<ushort, byte[]> _glyphsOutlineDic = new Dictionary<ushort, byte[]>();
        // this maps char code to glyph's name
        private Dictionary<ushort, ushort> _encoding = new Dictionary<ushort, ushort>();
        private Stack<double> _charStringStackCFF = new Stack<double>();
        private void DecodeTopDic(byte b0, ref int j)
        {
            #region operands
            double integer = 0;
            if (b0 >= 32 && b0 <= 246) // [-107,107]
            {
                integer = b0 - 139;
                this._charStringStackCFF.Push(integer);
            }
            else if (b0 >= 247 && b0 <= 250) // [108,1131]
            {
                byte byte1 = (byte)this._cFFReader.ReadCard8();
                ++j;
                integer = ((b0 - 247) * 256) + byte1 + 108;
                this._charStringStackCFF.Push(integer);
            }
            else if (b0 >= 251 && b0 <= 254) // [-1131,-108]
            {
                byte byte1 = (byte)this._cFFReader.ReadCard8();
                ++j;
                integer = -((b0 - 251) * 256) - byte1 - 108;
                this._charStringStackCFF.Push(integer);
            }
            else if (b0 == 28) // [-32768, +32767]
            {
                byte byte1 = (byte)this._cFFReader.ReadCard8();
                byte byte2 = (byte)this._cFFReader.ReadCard8();
                j += 2;
                integer = byte1 << 8 | byte2;
                this._charStringStackCFF.Push(integer);
            }
            else if (b0 == 29) // any 32-bit signed integer
            {
                byte b1 = (byte)this._cFFReader.ReadCard8();
                byte b2 = (byte)this._cFFReader.ReadCard8();
                byte b3 = (byte)this._cFFReader.ReadCard8();
                byte b4 = (byte)this._cFFReader.ReadCard8();
                j += 4;
                integer = b1 << 24 | b2 << 16 | b3 << 8 | b4;
                this._charStringStackCFF.Push(integer);
            }
            else if (b0 == 30) // nibbles
            {
                StringBuilder str = new StringBuilder();
                while (true)
                {
                    byte b1 = (byte)this._cFFReader.ReadCard8();
                    ++j;
                    byte niblle0 = (byte)(b1 >> 4);
                    byte niblle1 = (byte)(b1 & (0b00001111));
                    string nD0 = NibbleDefinitions(niblle0);
                    string nD1 = NibbleDefinitions(niblle1);
                    if (nD0.Equals("eon") || nD1.Equals("eon"))
                        break;
                    else
                    {
                        str.Append(nD0);
                        str.Append(nD1);
                    }
                }
                #region Nibbles Definitions
                string NibbleDefinitions(byte nibble)
                {
                    if (nibble >= 0 && nibble <= 9)
                    {
                        return Convert.ToString(nibble);
                    }
                    else if (nibble == 0xa)
                    {
                        return ".";
                    }
                    else if (nibble == 0xb)
                    {
                        return "E";
                    }
                    else if (nibble == 0xc)
                    {
                        return "E-";
                    }
                    else if (nibble == 0xd)
                    {
                        return "reserved";
                    }
                    else if (nibble == 0xe)
                    {
                        return "-";
                    }
                    else if (nibble == 0xf)
                    {
                        return "eon";
                    }
                    else
                        throw new ArgumentOutOfRangeException();
                }
                #endregion
                this._charStringStackCFF.Push(double.Parse(str.ToString()));
            }
            #endregion
            #region operators
            else
            {
                // operator : version, operand : SID
                if (b0 == 0)
                {
                    this._charStringStackCFF.Pop();
                }
                // operator : notice, operand : SID
                else if (b0 == 1)
                {
                    this._charStringStackCFF.Pop();
                }
                // operator : fullname, operand : SID
                else if (b0 == 2)
                {
                    this._charStringStackCFF.Pop();
                }
                // operator : familyname, operand : SID
                else if (b0 == 3)
                {
                    this._charStringStackCFF.Pop();
                }
                // operator : weight, operand : SID
                else if (b0 == 4)
                {
                    this._charStringStackCFF.Pop();
                }
                // operator : fontBBox, operand : array
                else if (b0 == 5)
                {
                    while (this._charStringStackCFF.Count != 0)
                        this._charStringStackCFF.Pop();
                }
                // operator : uniqueID, operand : number
                else if (b0 == 13)
                {
                    this._charStringStackCFF.Pop();
                }
                // operator : xUID, operand : array
                else if (b0 == 14)
                {
                    while (this._charStringStackCFF.Count != 0)
                        this._charStringStackCFF.Pop();
                }
                // operator : charset, operand : number
                else if (b0 == 15)
                {
                    var offset = this._charStringStackCFF.Pop();
                    this._topDicCFF.Add("Charset", offset);
                }
                // operator : encoding, operand : number
                else if (b0 == 16)
                {
                   var offset = this._charStringStackCFF.Pop();
                    this._topDicCFF.Add("Encoding", offset);
                }
                // operator : charStrings, operand : number
                else if (b0 == 17)
                {
                    var offset = this._charStringStackCFF.Pop();
                    this._topDicCFF.Add("CharStrings", offset);
                }
                // operator : private, operand : number, number
                else if (b0 == 18)
                {
                    var offset = this._charStringStackCFF.Pop();
                    var privateDicSize = this._charStringStackCFF.Pop();
                    this._topDicCFF.Add("PrivateDic", offset);
                }
                #region multibytes
                else if (b0 == 12)
                {
                    // read next byte
                    byte byte1 = (byte)this._cFFReader.ReadCard8();
                    ++j;
                    // operator : copyRights, operand : SID
                    if (byte1 == 0)
                    {
                        this._charStringStackCFF.Pop();
                    }
                    // operator : isfixedPitch, operand : boolean
                    else if (byte1 == 1)
                    {
                        this._charStringStackCFF.Pop();
                    }
                    // operator : italicAngle, operand : number
                    else if (byte1 == 2)
                    {
                        this._charStringStackCFF.Pop();
                    }
                    // operator : underlinePosition, operand : number
                    else if (byte1 == 3)
                    {
                        this._charStringStackCFF.Pop();
                    }
                    // operator : underlineThickness, operand : number
                    else if (byte1 == 4)
                    {
                        this._charStringStackCFF.Pop();
                    }
                    // operator : paintType, operand : number
                    else if (byte1 == 5)
                    {
                        this._charStringStackCFF.Pop();
                    }
                    // operator : charstringType, operand : number
                    else if (byte1 == 6)
                    {
                        this._charStringStackCFF.Pop();
                    }
                    // operator : fontMatrix, operand : array
                    else if (byte1 == 7)
                    {
                        while (this._charStringStackCFF.Count != 0)
                            this._charStringStackCFF.Pop();
                    }
                    // operator : strokeWidth, operand : number
                    else if (byte1 == 8)
                    {
                        this._charStringStackCFF.Pop();
                    }
                    // operator : syntheticBase, operand : number
                    else if (byte1 == 20)
                    {
                        this._charStringStackCFF.Pop();
                    }
                    // operator : postScript, operand : SID
                    else if (byte1 == 21)
                    {
                        this._charStringStackCFF.Pop();
                    }
                    // operator : baseFontName, operand : SID
                    else if (byte1 == 22)
                    {
                        this._charStringStackCFF.Pop();
                    }
                    // operator : baseFontBlend, operand : delt
                    else if (byte1 == 23)
                    {
                        while (this._charStringStackCFF.Count != 0)
                            this._charStringStackCFF.Pop();
                    }
                }
                #endregion
            }
            #endregion
        }
        private byte[] GetGlyphDescription(ushort gID)
        {
            return _glyphsOutlineDic[gID];
        }
        public Geometry GetGlyphOutLine(ushort gID)
        {
            byte[] data = this.GetGlyphDescription(gID);
            _charStringStackCFF.Clear();
            _pathGeometry = new PathGeometry();
            double integer = 0;
            int hstemhmCount = 0; bool hintmask = true; int hintmaskCount = 0;
            int vstemhmCount = 0;
            bool ctrmask = true; int ctrmaskCount = 0;
            int hsemCount = 0;
            int vsemCount = 0;
            // decode glyfdescription
            int i = 0;
            for (; i < data.Length; i++)
            {
                #region Decode numbers between 32 and 255
                byte charStringBtye = data[i];
                // number decoded
                //return _pathGeometry;
                if (charStringBtye >= 32 && charStringBtye <= 246) // [-107,107]
                {
                    integer = charStringBtye - 139;
                    this._charStringStackCFF.Push(integer);
                }
                else if (charStringBtye >= 247 && charStringBtye <= 250) // [108,1131]
                {
                    byte w = data[++i];
                    integer = ((charStringBtye - 247) * 256) + w + 108;
                    this._charStringStackCFF.Push(integer);
                }
                else if (charStringBtye >= 251 && charStringBtye <= 254) // [-1131,-108]
                {
                    byte w = data[++i];
                    integer = -((charStringBtye - 251) * 256) - w - 108;
                    this._charStringStackCFF.Push(integer);
                }
                else if (charStringBtye == 255) // any 32-bit signed integer
                {
                    byte b1 = data[++i];
                    byte b2 = data[++i];
                    byte b3 = data[++i];
                    byte b4 = data[++i];
                    integer = ((((((b1 << 8) + b2) << 8) + b3) << 8) + b4) / (1 << 16);
                    this._charStringStackCFF.Push(integer);
                }
                #endregion
                #region shortInt 28
                // Note 2: in addition to range 32 to 255 there is 28 followed by two bytes
                // represent numbers [-32768, 32767]
                else if (charStringBtye == 28)
                {
                    byte b1 = data[++i];
                    byte b2 = data[++i];
                    integer = BitConverter.ToInt16(new byte[] { b2, b1 });
                    this._charStringStackCFF.Push(integer);
                }
                #endregion
                #region path construction operators
                else if (charStringBtye == 4)
                {
                    // vmoveto command
                    double dy = this._charStringStackCFF.Pop();
                    // startpoint
                    _pathFigure = new PathFigure();
                    _pathGeometry.Figures.Add(_pathFigure);
                    _pathFigure.StartPoint = this.rmoveto(0, dy, _currentPoint);
                    _currentPoint = _pathFigure.StartPoint;
                }
                else if (charStringBtye == 5)
                {
                    // rlineto command
                    int count = this._charStringStackCFF.Count;
                    while (count > 0)
                    {
                        double dx = this._charStringStackCFF.ElementAt(--count);
                        double dy = this._charStringStackCFF.ElementAt(--count);
                        // Line
                        LineSegment line = this.rlineto(dx, dy, _currentPoint);
                        this._currentPoint = line.Point;
                        this._pathFigure.Segments.Add(line);
                    }
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 6)
                {
                    // hlineto command
                    int count = this._charStringStackCFF.Count;
                    if (count % 2 != 0) // odd 
                    {
                        while (count > 0)
                        {
                            if (count % 2 == 0) // even number
                            {
                                double dya = this._charStringStackCFF.ElementAt(--count);
                                //double dxb = this._charStringStackCFF.ElementAt(--count);
                                // Line
                                LineSegment line = this.rlineto(0, dya, _currentPoint);
                                this._currentPoint = line.Point;
                                this._pathFigure.Segments.Add(line);
                            }
                            else // odd number
                            {
                                double dx1 = this._charStringStackCFF.ElementAt(--count);
                                // Line
                                LineSegment line = this.rlineto(dx1, 0, _currentPoint);
                                this._currentPoint = line.Point;
                                this._pathFigure.Segments.Add(line);
                            }
                        }
                    }
                    else
                    {
                        while (count > 0)
                        {
                            if (count % 2 == 0)
                            {
                                double dxa = this._charStringStackCFF.ElementAt(--count);
                                // Line
                                LineSegment line = this.rlineto(dxa, 0, _currentPoint);
                                this._currentPoint = line.Point;
                                this._pathFigure.Segments.Add(line);
                            }
                            else
                            {
                                double dyb = this._charStringStackCFF.ElementAt(--count);
                                // Line
                                LineSegment line = this.rlineto(0, dyb, _currentPoint);
                                this._currentPoint = line.Point;
                                this._pathFigure.Segments.Add(line);
                            }
                        }
                    }
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 7)
                {
                    // vlineto command
                    int count = this._charStringStackCFF.Count;
                    if (count % 2 != 0) // odd
                    {
                        while (count > 0)
                        {
                            if (count % 2 == 0) // even number
                            {
                                double dxa = this._charStringStackCFF.ElementAt(--count);
                                //double dyb = this._charStringStackCFF.ElementAt(--count);
                                // Line
                                LineSegment line = this.rlineto(dxa, 0, _currentPoint);
                                _currentPoint = line.Point;
                                _pathFigure.Segments.Add(line);
                            }
                            else // odd number
                            {
                                double dy1 = this._charStringStackCFF.ElementAt(--count);
                                // Line
                                LineSegment line = this.rlineto(0, dy1, _currentPoint);
                                _currentPoint = line.Point;
                                _pathFigure.Segments.Add(line);
                            }
                        }
                    }
                    else
                    {
                        while (count > 0)
                        {
                            if (count % 2 == 0)
                            {
                                double dya = this._charStringStackCFF.ElementAt(--count);
                                // Line
                                LineSegment line = this.rlineto(0, dya, _currentPoint);
                                _currentPoint = line.Point;
                                _pathFigure.Segments.Add(line);
                            }
                            else
                            {
                                double dxb = this._charStringStackCFF.ElementAt(--count);
                                // Line
                                LineSegment line = this.rlineto(dxb, 0, _currentPoint);
                                _currentPoint = line.Point;
                                _pathFigure.Segments.Add(line);
                            }
                        }
                    }
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 8)
                {
                    // rrcurveto command
                    int count = this._charStringStackCFF.Count;
                    int n = count - 1;
                    while (n > 0)
                    {
                        double dxa = this._charStringStackCFF.ElementAt(n--);
                        double dya = this._charStringStackCFF.ElementAt(n--);
                        double dxb = this._charStringStackCFF.ElementAt(n--);
                        double dyb = this._charStringStackCFF.ElementAt(n--);
                        double dxc = this._charStringStackCFF.ElementAt(n--);
                        double dyc = this._charStringStackCFF.ElementAt(n--);

                        // Bezier
                        BezierSegment bezier = this.rrcurveto(dxa, dya, dxb, dyb,
                            dxc, dyc, _currentPoint);
                        _currentPoint = bezier.Point3;
                        _pathFigure.Segments.Add(bezier);

                    }
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 0 || charStringBtye == 2 ||
                    charStringBtye == 9 || charStringBtye == 13 ||
                    charStringBtye == 15 ||
                    charStringBtye == 16 || charStringBtye == 17)
                {
                    // reserved

                }
                else if (charStringBtye == 21)
                {
                    // rmoveto command
                    int count = this._charStringStackCFF.Count;
                    if (count > 2)
                    {
                        double dy = this._charStringStackCFF.Pop();
                        double dx = this._charStringStackCFF.Pop();
                        double glyfWidth = this._charStringStackCFF.Pop();
                        // startpoint
                        _pathFigure = new PathFigure();
                        _pathGeometry.Figures.Add(_pathFigure);
                        _pathFigure.StartPoint = this.rmoveto(dx, dy, _currentPoint);
                        _currentPoint = _pathFigure.StartPoint;
                    }
                    else
                    {
                        double dy = this._charStringStackCFF.Pop();
                        double dx = this._charStringStackCFF.Pop();
                        // startpoint
                        _pathFigure = new PathFigure();
                        _pathGeometry.Figures.Add(_pathFigure);
                        _pathFigure.StartPoint = this.rmoveto(dx, dy, _currentPoint);
                        _currentPoint = _pathFigure.StartPoint;
                    }
                }
                else if (charStringBtye == 22)
                {
                    // hmoveto command
                    int count = this._charStringStackCFF.Count;
                    if (count > 1)
                    {
                        double dx = this._charStringStackCFF.Pop();
                        double glyfWidth = this._charStringStackCFF.Pop();
                        // startpoint
                        _pathFigure = new PathFigure();
                        _pathGeometry.Figures.Add(_pathFigure);
                        _pathFigure.StartPoint = this.rmoveto(dx, 0, _currentPoint);
                        _currentPoint = _pathFigure.StartPoint;
                    }
                    else
                    {
                        double dx = this._charStringStackCFF.Pop();
                        // startpoint
                        _pathFigure = new PathFigure();
                        _pathGeometry.Figures.Add(_pathFigure);
                        _pathFigure.StartPoint = this.rmoveto(dx, 0, _currentPoint);
                        _currentPoint = _pathFigure.StartPoint;
                    }
                }
                else if (charStringBtye == 24)
                {
                    // rcurveline command
                    int count = this._charStringStackCFF.Count;
                    while (count > 0)
                    {
                        double dxa = this._charStringStackCFF.ElementAt(--count);
                        double dya = this._charStringStackCFF.ElementAt(--count);
                        double dxb = this._charStringStackCFF.ElementAt(--count);
                        double dyb = this._charStringStackCFF.ElementAt(--count);
                        double dxc = this._charStringStackCFF.ElementAt(--count);
                        double dyc = this._charStringStackCFF.ElementAt(--count);
                        // Bezier
                        BezierSegment bezier = this.rrcurveto(dxa, dya, dxb, dyb,
                            dxc, dyc, _currentPoint);
                        _currentPoint = bezier.Point3;
                        _pathFigure.Segments.Add(bezier);
                        if (count == 2)
                            break;
                    }
                    double dx = this._charStringStackCFF.ElementAt(--count);
                    double dy = this._charStringStackCFF.ElementAt(--count);
                    // Line
                    LineSegment line = this.rlineto(dx, dy, _currentPoint);
                    this._currentPoint = line.Point;
                    this._pathFigure.Segments.Add(line);
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 25)
                {
                    // rlinecurve command
                    int count = this._charStringStackCFF.Count;
                    while (count > 0)
                    {
                        double dxa = this._charStringStackCFF.ElementAt(--count);
                        double dya = this._charStringStackCFF.ElementAt(--count);
                        LineSegment line = this.rlineto(dxa, dya, _currentPoint);
                        _currentPoint = line.Point;
                        _pathFigure.Segments.Add(line);
                        if (count == 6)
                            break;
                    }
                    double dxb = this._charStringStackCFF.ElementAt(--count);
                    double dyb = this._charStringStackCFF.ElementAt(--count);
                    double dxc = this._charStringStackCFF.ElementAt(--count);
                    double dyc = this._charStringStackCFF.ElementAt(--count);
                    double dxd = this._charStringStackCFF.ElementAt(--count);
                    double dyd = this._charStringStackCFF.ElementAt(--count);
                    // Bezier
                    BezierSegment bezier = this.rrcurveto(dxb, dyb, dxc, dyc,
                        dxd, dyd, _currentPoint);
                    _currentPoint = bezier.Point3;
                    _pathFigure.Segments.Add(bezier);
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 26)
                {
                    // vvcurveto command
                    int count = this._charStringStackCFF.Count;
                    if (count % 2 != 0) // odd
                    {
                        double dx1 = this._charStringStackCFF.ElementAt(--count);
                        while (count > 0)
                        {
                            double dya = this._charStringStackCFF.ElementAt(--count);
                            double dxb = this._charStringStackCFF.ElementAt(--count);
                            double dyb = this._charStringStackCFF.ElementAt(--count);
                            double dyc = this._charStringStackCFF.ElementAt(--count);
                            // Bezier
                            if (count % 2 == 0)
                            {
                                BezierSegment bezier = this.rrcurveto(dx1, dya, dxb, dyb,
                                    0, dyc, _currentPoint);
                                _currentPoint = bezier.Point3;
                                _pathFigure.Segments.Add(bezier);
                            }
                            else
                            {
                                BezierSegment bezier = this.rrcurveto(0, dya, dxb, dyb,
                                    0, dyc, _currentPoint);
                                _currentPoint = bezier.Point3;
                                _pathFigure.Segments.Add(bezier);
                            }
                        }
                    }
                    else
                    {
                        while(count > 0)
                        {
                            double dya = this._charStringStackCFF.ElementAt(--count);
                            double dxb = this._charStringStackCFF.ElementAt(--count);
                            double dyb = this._charStringStackCFF.ElementAt(--count);
                            double dyc = this._charStringStackCFF.ElementAt(--count);
                            // Bezier
                            BezierSegment bezier = this.rrcurveto(0, dya, dxb, dyb,
                                0, dyc, _currentPoint);
                            _currentPoint = bezier.Point3;
                            _pathFigure.Segments.Add(bezier);

                        }
                    }
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 27)
                {
                    // hhvcurveto command
                    int count = this._charStringStackCFF.Count;
                    if (count % 2 != 0) // odd
                    {
                        double dy1 = this._charStringStackCFF.ElementAt(--count);
                        while (count > 0)
                        {
                            double dxa = this._charStringStackCFF.ElementAt(--count);
                            double dxb = this._charStringStackCFF.ElementAt(--count);
                            double dyb = this._charStringStackCFF.ElementAt(--count);
                            double dxc = this._charStringStackCFF.ElementAt(--count);
                            // Bezier
                            if (count % 2 == 0)
                            {
                                BezierSegment bezier = this.rrcurveto(dxa, dy1, dxb, dyb,
                                    dxc, 0, _currentPoint);
                                _currentPoint = bezier.Point3;
                                _pathFigure.Segments.Add(bezier);
                            }
                            else
                            {
                                BezierSegment bezier = this.rrcurveto(dxa, 0, dxb, dyb,
                                    dxc, 0, _currentPoint);
                                _currentPoint = bezier.Point3;
                                _pathFigure.Segments.Add(bezier);
                            }
                        }
                    }
                    else
                    {
                        while (count > 0)
                        {
                            double dxa = this._charStringStackCFF.ElementAt(--count);
                            double dxb = this._charStringStackCFF.ElementAt(--count);
                            double dyb = this._charStringStackCFF.ElementAt(--count);
                            double dxc = this._charStringStackCFF.ElementAt(--count);
                            // Bezier
                            BezierSegment bezier = this.rrcurveto(dxa, 0, dxb, dyb,
                                dxc, 0, _currentPoint);
                            _currentPoint = bezier.Point3;
                            _pathFigure.Segments.Add(bezier);

                        }
                    }
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 30)
                {
                    // vhcurveto command
                    int count = this._charStringStackCFF.Count;
                    if (count % 2 != 0) // odd
                    {
                        while (count > 0)
                        {
                            if (count % 2 != 0)
                            {
                                double dya = this._charStringStackCFF.ElementAt(--count);
                                double dxb = this._charStringStackCFF.ElementAt(--count);
                                double dyb = this._charStringStackCFF.ElementAt(--count);
                                double dxc = this._charStringStackCFF.ElementAt(--count);
                                double dyf = 0.0;
                                --count;
                                if (count == 0)
                                    dyf = this._charStringStackCFF.ElementAt(count);
                                // Bezier
                                BezierSegment bezier = this.rrcurveto(0, dya, dxb, dyb,
                                    dxc, dyf, _currentPoint);
                                _currentPoint = bezier.Point3;
                                _pathFigure.Segments.Add(bezier);
                            }
                            else
                            {
                                double dxd = this._charStringStackCFF.ElementAt(count);
                                double dxe = this._charStringStackCFF.ElementAt(--count);
                                double dye = this._charStringStackCFF.ElementAt(--count);
                                double dyf = this._charStringStackCFF.ElementAt(--count);
                                double dxf = 0.0;
                                if (count == 1)
                                    dxf = this._charStringStackCFF.ElementAt(--count);
                                // Bezier
                                BezierSegment bezier = this.rrcurveto(dxd, 0, dxe, dye,
                                    dxf, dyf, _currentPoint);
                                _currentPoint = bezier.Point3;
                                _pathFigure.Segments.Add(bezier);
                            }
                        }
                    }
                    else // even
                    {
                        while (count > 0)
                        {
                            if (count % 2 == 0)
                            {
                                double dya = this._charStringStackCFF.ElementAt(--count);
                                double dxb = this._charStringStackCFF.ElementAt(--count);
                                double dyb = this._charStringStackCFF.ElementAt(--count);
                                double dxc = this._charStringStackCFF.ElementAt(--count);
                                --count;
                                // Bezier
                                BezierSegment bezier = this.rrcurveto(0, dya, dxb, dyb,
                                    dxc, 0, _currentPoint);
                                _currentPoint = bezier.Point3;
                                _pathFigure.Segments.Add(bezier);
                            }
                            else
                            {
                                double dxd = this._charStringStackCFF.ElementAt(count);
                                double dxe = this._charStringStackCFF.ElementAt(--count);
                                double dye = this._charStringStackCFF.ElementAt(--count);
                                double dyf = this._charStringStackCFF.ElementAt(--count);
                                // Bezier
                                BezierSegment bezier = this.rrcurveto(dxd, 0, dxe, dye,
                                    0, dyf, _currentPoint);
                                _currentPoint = bezier.Point3;
                                _pathFigure.Segments.Add(bezier);
                            }
                        }
                    }
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 31)
                {
                    // hvcurveto command
                    int count = this._charStringStackCFF.Count;
                    if (count % 2 != 0) // odd
                    {
                        while (count > 0)
                        {
                            if (count % 2 != 0)
                            {
                                double dxa = this._charStringStackCFF.ElementAt(--count);
                                double dxb = this._charStringStackCFF.ElementAt(--count);
                                double dyb = this._charStringStackCFF.ElementAt(--count);
                                double dyc = this._charStringStackCFF.ElementAt(--count);
                                double dxf = 0.0;
                                --count;
                                if (count == 0)
                                    dxf = this._charStringStackCFF.ElementAt(count);
                                // Bezier
                                BezierSegment bezier = this.rrcurveto(dxa, 0, dxb, dyb,
                                    dxf, dyc, _currentPoint);
                                _currentPoint = bezier.Point3;
                                _pathFigure.Segments.Add(bezier);
                            }
                            else
                            {
                                double dyd = this._charStringStackCFF.ElementAt(count);
                                double dxe = this._charStringStackCFF.ElementAt(--count);
                                double dye = this._charStringStackCFF.ElementAt(--count);
                                double dxf = this._charStringStackCFF.ElementAt(--count);
                                double dyf = 0.0;
                                if (count == 1)
                                    dyf = this._charStringStackCFF.ElementAt(--count);
                                // Bezier
                                BezierSegment bezier = this.rrcurveto(0, dyd, dxe, dye,
                                    dxf, dyf, _currentPoint);
                                _currentPoint = bezier.Point3;
                                _pathFigure.Segments.Add(bezier);
                            }
                        }
                    }
                    else // even
                    {
                        while (count > 0)
                        {
                            if (count % 2 == 0)
                            {
                                double dxa = this._charStringStackCFF.ElementAt(--count);
                                double dxb = this._charStringStackCFF.ElementAt(--count);
                                double dyb = this._charStringStackCFF.ElementAt(--count);
                                double dyc = this._charStringStackCFF.ElementAt(--count);
                                --count;
                                // Bezier
                                BezierSegment bezier = this.rrcurveto(dxa, 0, dxb, dyb,
                                    0, dyc, _currentPoint);
                                _currentPoint = bezier.Point3;
                                _pathFigure.Segments.Add(bezier);
                            }
                            else
                            {
                                double dyd = this._charStringStackCFF.ElementAt(count);
                                double dxe = this._charStringStackCFF.ElementAt(--count);
                                double dye = this._charStringStackCFF.ElementAt(--count);
                                double dxf = this._charStringStackCFF.ElementAt(--count);
                                // Bezier
                                BezierSegment bezier = this.rrcurveto(0, dyd, dxe, dye,
                                    dxf, 0, _currentPoint);
                                _currentPoint = bezier.Point3;
                                _pathFigure.Segments.Add(bezier);
                            }
                        }
                    }
                    this._charStringStackCFF.Clear();
                }
                #endregion
                #region hint operators
                else if (charStringBtye == 1)
                {
                    // hstem command
                    int count = this._charStringStackCFF.Count;
                    if(count % 2 == 0) // even
                    {
                        hsemCount = this._charStringStackCFF.Count;
                    }
                    else // odd
                    {
                        // this first number is width
                        hsemCount = this._charStringStackCFF.Count - 1;
                    }
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 3)
                {
                    // vstem command 
                    int count = this._charStringStackCFF.Count;
                    if (count % 2 == 0) // even
                    {
                        vsemCount = this._charStringStackCFF.Count;
                    }
                    else // odd
                    {
                        // this first number is width
                        vsemCount = this._charStringStackCFF.Count - 1;
                    }
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 18)
                {
                    // hstemhm command
                    int count = this._charStringStackCFF.Count;
                    if (count % 2 == 0) // even
                    {
                        hstemhmCount = this._charStringStackCFF.Count;
                    }
                    else // odd
                    {
                        // this first number is width
                        hstemhmCount = this._charStringStackCFF.Count - 1;
                    }
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 23)
                {
                    // vstemhm command
                    int count = this._charStringStackCFF.Count;
                    if (count % 2 == 0) // even
                    {
                        vstemhmCount = this._charStringStackCFF.Count;
                    }
                    else // odd
                    {
                        // this first number is width
                        vstemhmCount = this._charStringStackCFF.Count - 1;
                    }
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 19)
                {
                    // hintmask operator 
                    if(hintmask)
                    {
                        hintmaskCount = this._charStringStackCFF.Count;
                        hintmask = false;
                    }
                    if (((hstemhmCount / 2) + hintmaskCount / 2) > 8 || (hsemCount / 2) + hintmaskCount / 2 > 8 || (vsemCount / 2) + hintmaskCount / 2 > 8 
                        || (vstemhmCount / 2) + hintmaskCount / 2 > 8)
                    {
                        byte mask1 = data[++i];
                        byte mask2 = data[++i];
                    }
                    else
                    {
                        byte mask1 = data[++i];
                    }
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 20)
                {
                    // cntrmask operator 
                    if (ctrmask)
                    {
                        ctrmaskCount = this._charStringStackCFF.Count;
                        ctrmask = false;
                    }
                    if (((hstemhmCount / 2) + hintmaskCount / 2) > 8 || (hsemCount / 2) + hintmaskCount / 2 > 8 || (vsemCount / 2) + hintmaskCount / 2 > 8
                       || (vstemhmCount / 2) + hintmaskCount / 2 > 8)
                    {
                        byte mask1 = data[++i];
                        byte mask2 = data[++i];
                    }
                    else
                    {
                        byte mask1 = data[++i];
                    }
                    this._charStringStackCFF.Clear();
                }
                #endregion
                #region callsubr
                else if (charStringBtye == 10)
                {
                    // callsubr command
                    double no, index = 0;
                    if (this._charStringStackCFF.Count == 2)
                    {
                        no = this._charStringStackCFF.Pop();
                        index = this._charStringStackCFF.Pop();
                    }
                    if (this._charStringStackCFF.Count == 1)
                    {
                        index = this._charStringStackCFF.Pop();
                        no = 0;
                    }
                    //if (index > this._segment2.Subrs.Count)
                    //    index = this._segment2.Subrs.Count - 2;
                    //byte[] subr = this._segment2.Subrs[(int)index];
                    //_pathGeometry = (PathGeometry)DecodeCharString(subr);
                }
                else if (charStringBtye == 29)
                {
                    // callgsubr command
                    return _pathGeometry;
                }
                else if (charStringBtye == 11)
                {
                    // return command for callsubr
                    return _pathGeometry;
                }
                #endregion
                #region close path
                else if (charStringBtye == 14)
                {
                    // endchar command
                    this._charStringStackCFF.Clear();
                    return _pathGeometry;
                }
                #endregion
                else if (charStringBtye == 12)
                {
                    // read next byte
                    byte next = data[++i];
                    if (next == 0)
                    {
                        // dotsection is deprecated in CFF2
                    }
                    else if (next == 1)
                    {
                        // vstem3 command
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                    }
                    else if (next == 2)
                    {
                        // hstem3 command
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                    }
                    else if (next == 6)
                    {
                        // seac command
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                    }
                    else if (next == 7)
                    {
                        // sbw command
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                    }
                    else if (next == 12)
                    {
                        // div command
                        double y = this._charStringStackCFF.Pop();
                        double x = this._charStringStackCFF.Pop();
                        // push x / y
                        this._charStringStackCFF.Push((x / y));
                    }
                    else if (next == 16)
                    {
                        // callothersubr command
                    }
                    else if (next == 17)
                    {
                        // pop command
                    }
                    else if (next == 33)
                    {
                        // setcurrentpoint command
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                    }
                }
            }
            // some glyphse as i and j the closepath and endchar commands are placed inside callsubr so
            // the control will reach this return instead the endchar return which will return to the callsubr call
            return _pathGeometry;
        }
        private PathGeometry _pathGeometry = null;
        private PathFigure _pathFigure = null;
        private Point _lsb = new Point(0.0, 0.0);
        private Point _charWidth = new Point(0.0, 0.0);
        private Point _currentPoint = new Point(0.0, 0.0);
        private LineSegment rlineto(double dx, double dy, Point currentPoint)
        {
            Point point;
            point.X = currentPoint.X + dx;
            point.Y = currentPoint.Y + dy;
            LineSegment segment = new LineSegment() { Point = point };
            return segment;
        }
        private Point rmoveto(double dx, double dy, Point currentPoint)
        {
            Point startPoint;
            startPoint.X = currentPoint.X + dx;
            startPoint.Y = currentPoint.Y + dy;
            return startPoint;
        }
        private BezierSegment rrcurveto
            (double dx1, double dy1, double dx2, double dy2, double dx3, double dy3, Point currentPoint)
        {
            // Bezier curve
            Point controlPoint1, controlPoint2, endPoint;
            controlPoint1.X = currentPoint.X + dx1;
            controlPoint1.Y = currentPoint.Y + dy1;
            controlPoint2.X = currentPoint.X + (dx1 + dx2);
            controlPoint2.Y = currentPoint.Y + (dy1 + dy2);
            endPoint.X = currentPoint.X + (dx1 + dx2 + dx3);
            endPoint.Y = currentPoint.Y + (dy1 + dy2 + dy3);
            BezierSegment segment = new BezierSegment()
            { Point1 = controlPoint1, Point2 = controlPoint2, Point3 = endPoint };
            return segment;
        }
        public void Dispose()
        {
            if (this._cFFReader is not null)
                this._cFFReader.Dispose();
            if (this._topDicCFF is not null)
            { this._topDicCFF.Clear(); this._topDicCFF = null; }
            if (this._privateDicCFF is not null)
            { this._privateDicCFF.Clear(); this._privateDicCFF = null; }
            if (this._glyphsOutlineDic is not null)
            { this._glyphsOutlineDic.Clear(); this._glyphsOutlineDic = null; }
            if (this._charStringStackCFF is not null)
            { this._charStringStackCFF.Clear(); this._charStringStackCFF = null; }
            if (this._pathFigure is not null)
            {this._pathFigure = null; }
        }
    }
}
