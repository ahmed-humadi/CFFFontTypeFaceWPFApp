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
            //this._cFFReader.Seek((long)this._topDictCFF["CharStrings"]);
            //Card16 count3 = this._cFFReader.ReadCard16();
            //OffSize offSize3 = this._cFFReader.ReadOffSize();
            //Offset16[] offSetsArray3 = new Offset16[count3 + 1];
            //for (int i = 0; i < offSetsArray3.Length; i++)
            //{
            //    Offset16 offset = this._cFFReader.ReadOffset16();
            //    offSetsArray3[i] = offset;
            //}
            //for (ushort i = 0; i < offSetsArray3.Length - 1; i++)
            //{
            //    ushort length = offSetsArray3[i + 1] - offSetsArray3[i];
            //    byte[] dataArray = new byte[length];
            //    for (int j = 0; j < length; j++)
            //    {
            //        Card8 data = this._cFFReader.ReadCard8();
            //        dataArray[j] = (byte)data;
            //    }
            //    this._charStringCFF.Add(i, dataArray);
            //}
            //GetGlyphOutLineCFF2(2);
        }
        private Dictionary<string, double> _topDicCFF = new Dictionary<string, double>();
        private Dictionary<string, double> _privateDicCFF = new Dictionary<string, double>();
        private Dictionary<ushort, byte[]> _glyphsOutlineDic = new Dictionary<ushort, byte[]>();
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
                    this._charStringStackCFF.Pop();
                }
                // operator : encoding, operand : number
                else if (b0 == 16)
                {
                    this._charStringStackCFF.Pop();
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
            long charStringsOffset = (long)this._topDicCFF["CharStrings"];
            this._cFFReader.Seek(charStringsOffset);
            Card16 count3 = this._cFFReader.ReadCard16();
            OffSize offSize3 = this._cFFReader.ReadOffSize();
            long glypfOffset = (gID * offSize3) + charStringsOffset;
            long nextGlypfOffset = ((gID + 1) * offSize3) + charStringsOffset;
            this._cFFReader.Seek(glypfOffset);

            byte[] dataArray = null;
            ushort length = (ushort)(nextGlypfOffset - glypfOffset);
            dataArray = new byte[length];
            for (int j = 0; j < length; j++)
            {
                Card8 data = this._cFFReader.ReadCard8();
                dataArray[j] = (byte)data;
            }
            return dataArray;
        }
        public Geometry GetGlyphOutLine(ushort gID)
        {
            byte[] data = this.GetGlyphDescription(gID);
            _charStringStackCFF.Clear();
            _pathGeometry = new PathGeometry();
            double integer = 0;
            // decode glyfdescription
            int i = 0;
            for (; i < data.Length; i++)
            {
                #region Decode numbers
                byte charStringBtye = data[i];
                // number decoded
                if (charStringBtye >= 32 && charStringBtye <= 246) // [-107,107]
                {
                    integer = charStringBtye - 139;
                    this._charStringStackCFF.Push(integer);
                }
                if (charStringBtye >= 247 && charStringBtye <= 250) // [108,1131]
                {
                    byte w = data[++i];
                    integer = ((charStringBtye - 247) * 256) + w + 108;
                    this._charStringStackCFF.Push(integer);
                }
                if (charStringBtye >= 251 && charStringBtye <= 254) // [-1131,-108]
                {
                    byte w = data[++i];
                    integer = -((charStringBtye - 251) * 256) - w - 108;
                    this._charStringStackCFF.Push(integer);
                }
                if (charStringBtye == 255) // any 32-bit signed integer
                {
                    byte b1 = data[++i];
                    byte b2 = data[++i];
                    byte b3 = data[++i];
                    byte b4 = data[++i];
                    integer = ((((((b1 << 8) + b2) << 8) + b3) << 8) + b4) / (1 << 16);
                    this._charStringStackCFF.Push(integer);
                }
                #endregion
                #region path construction operators
                if (charStringBtye == 4)
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
                    for (int n = 0; n < this._charStringStackCFF.Count;)
                    {
                        double dx = this._charStringStackCFF.ElementAt(n);
                        double dy = this._charStringStackCFF.ElementAt(++n);
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
                    for (int n = 0; n < count; n++)
                    {
                        if (n % 2 == 0) // even number
                        {
                            double dx = this._charStringStackCFF.ElementAt(n);
                            // Line
                            LineSegment line = this.rlineto(dx, 0, _currentPoint);
                            this._currentPoint = line.Point;
                            this._pathFigure.Segments.Add(line);
                        }
                        else // odd number
                        {
                            double dy = this._charStringStackCFF.ElementAt(n);
                            // Line
                            LineSegment line = this.rlineto(0, dy, _currentPoint);
                            this._currentPoint = line.Point;
                            this._pathFigure.Segments.Add(line);
                        }
                    }
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 7)
                {
                    // vlineto command
                    int count = this._charStringStackCFF.Count;
                    for (int n = 0; n < count; n++)
                    {
                        if (n % 2 == 0) // even number
                        {
                            double dy = this._charStringStackCFF.ElementAt(n);
                            // Line
                            LineSegment line = this.rlineto(0, dy, _currentPoint);
                            _currentPoint = line.Point;
                            _pathFigure.Segments.Add(line);
                        }
                        else // odd number
                        {
                            double dx = this._charStringStackCFF.ElementAt(n);
                            // Line
                            LineSegment line = this.rlineto(dx, 0, _currentPoint);
                            _currentPoint = line.Point;
                            _pathFigure.Segments.Add(line);
                        }
                    }
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 8)
                {
                    // rrcurveto command
                    int count = this._charStringStackCFF.Count;
                    for (int n = 0; n < count; n += 6)
                    {
                        double dy3 = this._charStringStackCFF.Pop();
                        double dx3 = this._charStringStackCFF.Pop();
                        double dy2 = this._charStringStackCFF.Pop();
                        double dx2 = this._charStringStackCFF.Pop();
                        double dy1 = this._charStringStackCFF.Pop();
                        double dx1 = this._charStringStackCFF.Pop();
                        // Bezier
                        BezierSegment bezier = this.rrcurveto(dx1, dy1, dx2, dy2,
                            dx3, dy3, _currentPoint);
                        _currentPoint = bezier.Point3;
                        _pathFigure.Segments.Add(bezier);
                    }

                }
                else if (charStringBtye == 21)
                {
                    // rmoveto command
                    double dy = this._charStringStackCFF.Pop();
                    double dx = this._charStringStackCFF.Pop();
                    // startpoint
                    _pathFigure = new PathFigure();
                    _pathGeometry.Figures.Add(_pathFigure);
                    _pathFigure.StartPoint = this.rmoveto(dx, dy, _currentPoint);
                    _currentPoint = _pathFigure.StartPoint;
                }
                else if (charStringBtye == 22)
                {
                    // hmoveto command
                    double dx = this._charStringStackCFF.Pop();
                    // startpoint
                    _pathFigure = new PathFigure();
                    _pathGeometry.Figures.Add(_pathFigure);
                    _pathFigure.StartPoint = this.rmoveto(dx, 0, _currentPoint);
                    _currentPoint = _pathFigure.StartPoint;
                }
                else if (charStringBtye == 24)
                {
                    // rcurveline command
                    int count = this._charStringStackCFF.Count;
                    for (int n = 0; n < count - 2;)
                    {
                        double dxa = this._charStringStackCFF.ElementAt(n++);
                        double dya = this._charStringStackCFF.ElementAt(n++);
                        double dxb = this._charStringStackCFF.ElementAt(n++);
                        double dyb = this._charStringStackCFF.ElementAt(n++);
                        double dxc = this._charStringStackCFF.ElementAt(n++);
                        double dyc = this._charStringStackCFF.ElementAt(n++);
                        // Bezier
                        BezierSegment bezier = this.rrcurveto(dxa, dya, dxb, dyb,
                            dxc, dyc, _currentPoint);
                        _currentPoint = bezier.Point3;
                        _pathFigure.Segments.Add(bezier);
                    }
                    double dx = this._charStringStackCFF.ElementAt(count - 2);
                    double dy = this._charStringStackCFF.ElementAt(count - 1);
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
                    for (int n = 0; n < count - 6;)
                    {
                        double dxa = this._charStringStackCFF.ElementAt(n++);
                        double dya = this._charStringStackCFF.ElementAt(n++);
                        LineSegment line = this.rlineto(dxa, dya, _currentPoint);
                        _currentPoint = line.Point;
                        _pathFigure.Segments.Add(line);
                    }
                    double dxb = this._charStringStackCFF.ElementAt(count - 6);
                    double dyb = this._charStringStackCFF.ElementAt(count - 5);
                    double dxc = this._charStringStackCFF.ElementAt(count - 4);
                    double dyc = this._charStringStackCFF.ElementAt(count - 3);
                    double dxd = this._charStringStackCFF.ElementAt(count - 2);
                    double dyd = this._charStringStackCFF.ElementAt(count - 1);
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
                        double dx1 = this._charStringStackCFF.ElementAt(0);
                        for (int n = 1; n < count;)
                        {
                            double dya = this._charStringStackCFF.ElementAt(n++);
                            double dxb = this._charStringStackCFF.ElementAt(n++);
                            double dyb = this._charStringStackCFF.ElementAt(n++);
                            double dyc = this._charStringStackCFF.ElementAt(n++);
                            // Bezier
                            if (n == 5)
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
                        this._charStringStackCFF.Clear();
                    }
                }
                else if (charStringBtye == 27)
                {
                    // hhvcurveto command
                    int count = this._charStringStackCFF.Count;
                    if (count % 2 != 0) // odd
                    {
                        double dy1 = this._charStringStackCFF.ElementAt(0);
                        for (int n = 1; n < count;)
                        {
                            double dxa = this._charStringStackCFF.ElementAt(n++);
                            double dxb = this._charStringStackCFF.ElementAt(n++);
                            double dyb = this._charStringStackCFF.ElementAt(n++);
                            double dxc = this._charStringStackCFF.ElementAt(n++);
                            // Bezier
                            if (n == 5)
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
                        this._charStringStackCFF.Clear();
                    }
                }
                else if (charStringBtye == 30)
                {
                    // vhcurveto command
                    double dx3 = this._charStringStackCFF.Pop();
                    double dy2 = this._charStringStackCFF.Pop();
                    double dx2 = this._charStringStackCFF.Pop();
                    double dy1 = this._charStringStackCFF.Pop();
                    // Bezier
                    BezierSegment bezier = this.rrcurveto(0, dy1, dx2, dy2,
                        dx3, 0, _currentPoint);
                    _currentPoint = bezier.Point3;
                    _pathFigure.Segments.Add(bezier);
                }
                else if (charStringBtye == 31)
                {
                    // hvcurveto command
                    double dy3 = this._charStringStackCFF.Pop();
                    double dy2 = this._charStringStackCFF.Pop();
                    double dx2 = this._charStringStackCFF.Pop();
                    double dx1 = this._charStringStackCFF.Pop();
                    // Bezier
                    BezierSegment bezier = this.rrcurveto(dx1, 0, dx2, dy2,
                        0, dy3, _currentPoint);
                    _currentPoint = bezier.Point3;
                    _pathFigure.Segments.Add(bezier);
                }
                #endregion
                #region hint operators
                if (charStringBtye == 1)
                {
                    // hstem command
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 3)
                {
                    // vstem command 
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 18)
                {
                    // hstemhm command 
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 23)
                {
                    // vstemhm command 
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 19)
                {
                    // hintmask command 
                    this._charStringStackCFF.Clear();
                }
                else if (charStringBtye == 20)
                {
                    // cntrmask command 
                    this._charStringStackCFF.Clear();
                }
                #endregion
                if (charStringBtye == 9)
                {
                    // closepath command
                    _pathFigure.IsClosed = true;
                }
                #region callsubr
                if (charStringBtye == 10)
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
                if (charStringBtye == 14)
                {
                    // endchar command
                    this._charStringStackCFF.Clear();
                    return _pathGeometry;
                }
                #endregion
                if (charStringBtye == 12)
                {
                    // read next byte
                    byte next = data[++i];
                    if (next == 0)
                    {
                        // dotsection command
                    }
                    if (next == 1)
                    {
                        // vstem3 command
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                    }
                    if (next == 2)
                    {
                        // hstem3 command
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                    }
                    if (next == 6)
                    {
                        // seac command
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                    }
                    if (next == 7)
                    {
                        // sbw command
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                        this._charStringStackCFF.Pop();
                    }
                    if (next == 12)
                    {
                        // div command
                        double y = this._charStringStackCFF.Pop();
                        double x = this._charStringStackCFF.Pop();
                        // push x / y
                        this._charStringStackCFF.Push((x / y));
                    }
                    if (next == 16)
                    {
                        // callothersubr command
                    }
                    if (next == 17)
                    {
                        // pop command
                    }
                    if (next == 33)
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
            if (this._pathGeometry is not null)
            { this._pathGeometry.Clear(); this._pathGeometry = null; }
            if (this._pathFigure is not null)
            {this._pathFigure = null; }
        }
    }
}
