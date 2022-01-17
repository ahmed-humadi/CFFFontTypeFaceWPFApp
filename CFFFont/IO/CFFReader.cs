using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CFFFont.CFFDataType;
namespace CFFFont.IO
{
    public class CFFReader : IDisposable
    {
        private BinaryReader _binaryReader;     
        public CFFReader(byte[] data)
        {
            this._binaryReader = new BinaryReader(new MemoryStream(data));
        }
        private byte[] Reverse(byte[] buffer)
        {
            Array.Reverse<byte>(buffer);
            return buffer;
        }
        public void Seek(long position)
        {
            if (!this._binaryReader.BaseStream.CanSeek)
                return;
            this._binaryReader.BaseStream.Position = position;
        }
        public void GoBack() => --this._binaryReader.BaseStream.Position;
        public void GoForword() => ++this._binaryReader.BaseStream.Position;
        public Card8 ReadCard8()
        {
            return (Card8)this._binaryReader.ReadByte();
        }
        public Card16 ReadCard16()
        {
            byte[] buffer = this.Reverse(this._binaryReader.ReadBytes(2));

            Card16 number = (Card16)0;

            return buffer.Length < 2 ? number : (Card16)BitConverter.ToUInt16(buffer);
        }
        public OffSize ReadOffSize()
        {
            return (OffSize)this._binaryReader.ReadByte();
        }
        public Offset8 ReadOffset8()
        {
            return (Offset8)this.ReadCard8();
        }
        public Offset16 ReadOffset16()
        {
            return (Offset16)this.ReadCard16();
        }
        public Offset24 ReadOffset24()
        {
            byte[] buffer = this.Reverse(this._binaryReader.ReadBytes(3));

            Offset24 number = (Offset24)0;

            return buffer.Length < 3 ? number : (Offset24)(BitConverter.ToUInt32(buffer) >> 8);
        }
        public Offset32 ReadOffset32()
        {
            byte[] buffer = this.Reverse(this._binaryReader.ReadBytes(4));

            Offset32 number = (Offset32)0;

            return buffer.Length < 4 ? number : (Offset32)BitConverter.ToUInt32(buffer);
        }

        public void Dispose()
        {
            if (this._binaryReader is not null)
                this._binaryReader.Dispose();
        }
    }
}
