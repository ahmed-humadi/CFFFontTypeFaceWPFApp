using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CFFFont.IO;
using CFFFont.CFFDataType;
namespace CFFFont
{
    public class CFFFontTypeFace
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
            this._cFFReader.Seek((long)this._topDictCFF["CharStrings"]);
            Card16 count3 = this._cFFReader.ReadCard16();
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
                this._charStringCFF.Add(i, dataArray);
            }
            //GetGlyphOutLineCFF2(2);
        }
        private Dictionary<string, double> _topDictCFF = new Dictionary<string, double>();
        private Dictionary<ushort, byte[]> _charStringCFF = new Dictionary<ushort, byte[]>();
        private Stack<double> _charstringStackCFF = new Stack<double>();
        private void DecodeTopDic(byte b0, ref int j)
        {
            #region operands
            double integer = 0;
            if (b0 >= 32 && b0 <= 246) // [-107,107]
            {
                integer = b0 - 139;
                this._charstringStackCFF.Push(integer);
            }
            else if (b0 >= 247 && b0 <= 250) // [108,1131]
            {
                byte byte1 = (byte)this._cFFReader.ReadCard8();
                ++j;
                integer = ((b0 - 247) * 256) + byte1 + 108;
                this._charstringStackCFF.Push(integer);
            }
            else if (b0 >= 251 && b0 <= 254) // [-1131,-108]
            {
                byte byte1 = (byte)this._cFFReader.ReadCard8();
                ++j;
                integer = -((b0 - 251) * 256) - byte1 - 108;
                this._charstringStackCFF.Push(integer);
            }
            else if (b0 == 28) // [-32768, +32767]
            {
                byte byte1 = (byte)this._cFFReader.ReadCard8();
                byte byte2 = (byte)this._cFFReader.ReadCard8();
                j += 2;
                integer = byte1 << 8 | byte2;
                this._charstringStackCFF.Push(integer);
            }
            else if (b0 == 29) // any 32-bit signed integer
            {
                byte b1 = (byte)this._cFFReader.ReadCard8();
                byte b2 = (byte)this._cFFReader.ReadCard8();
                byte b3 = (byte)this._cFFReader.ReadCard8();
                byte b4 = (byte)this._cFFReader.ReadCard8();
                j += 4;
                integer = b1 << 24 | b2 << 16 | b3 << 8 | b4;
                this._charstringStackCFF.Push(integer);
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
                this._charstringStackCFF.Push(double.Parse(str.ToString()));
            }
            #endregion
            #region operators
            else
            {
                if (b0 == 0)
                {
                    this._charstringStackCFF.Pop();
                }
                else if (b0 == 1)
                {
                    this._charstringStackCFF.Pop();
                }
                else if (b0 == 2)
                {
                    this._charstringStackCFF.Pop();
                }
                else if (b0 == 3)
                {
                    this._charstringStackCFF.Pop();
                }
                else if (b0 == 4)
                {
                    this._charstringStackCFF.Pop();
                }
                else if (b0 == 5)
                {
                    while (this._charstringStackCFF.Count != 0)
                        this._charstringStackCFF.Pop();
                }
                else if (b0 == 13)
                {
                    this._charstringStackCFF.Pop();
                }
                else if (b0 == 14)
                {
                    while (this._charstringStackCFF.Count != 0)
                        this._charstringStackCFF.Pop();
                }
                else if (b0 == 15)
                {
                    this._charstringStackCFF.Pop();
                }
                else if (b0 == 16)
                {
                    this._charstringStackCFF.Pop();
                }
                else if (b0 == 17)
                {
                    this._topDictCFF.Add("CharStrings", this._charstringStackCFF.Pop());
                }
                else if (b0 == 18)
                {
                    this._charstringStackCFF.Pop();
                    this._charstringStackCFF.Pop();
                }
                if (b0 == 12)
                {
                    // read next byte
                    byte byte1 = (byte)this._cFFReader.ReadCard8();
                    ++j;
                    if (byte1 == 1)
                    {
                        this._charstringStackCFF.Pop();
                    }
                    if (byte1 == 2)
                    {
                        this._charstringStackCFF.Pop();
                    }
                    if (byte1 == 3)
                    {
                        this._charstringStackCFF.Pop();
                    }
                    if (byte1 == 4)
                    {
                        this._charstringStackCFF.Pop();
                    }
                    if (byte1 == 5)
                    {
                        this._charstringStackCFF.Pop();
                    }
                    if (byte1 == 6)
                    {
                        this._charstringStackCFF.Pop();
                    }
                    if (byte1 == 7)
                    {
                        while (this._charstringStackCFF.Count != 0)
                            this._charstringStackCFF.Pop();
                    }
                    if (byte1 == 8)
                    {
                        this._charstringStackCFF.Pop();
                    }
                    if (byte1 == 20)
                    {
                        this._charstringStackCFF.Pop();
                    }
                    if (byte1 == 21)
                    {
                        this._charstringStackCFF.Pop();
                    }
                    if (byte1 == 22)
                    {
                        this._charstringStackCFF.Pop();
                    }
                    if (byte1 == 23)
                    {
                        while (this._charstringStackCFF.Count != 0)
                            this._charstringStackCFF.Pop();
                    }
                }
            }
            #endregion
        }
        private string NibbleDefinitions(byte nibble)
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
    }
}
