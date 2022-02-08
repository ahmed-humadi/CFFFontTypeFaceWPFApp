using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFFont.CFFDataType
{
    public readonly struct Card8
    {
        private readonly byte _value;
        public readonly byte value => _value;
        private Card8(byte value) => _value = value;

        public static explicit operator Card8(int i)
        {
            return new Card8((byte)i);
        }
        public static explicit operator Card8(ushort i)
        {
            return new Card8((byte)i);
        }
        public static explicit operator ushort(Card8 i)
        {
            return i.value;
        }
        public static explicit operator int(Card8 i)
        {
            return i.value;
        }
        public static explicit operator Card8(byte i)
        {
            return new Card8(i);
        }
        public static explicit operator char(Card8 i)
        {
            return (char)i.value;
        }
        public static explicit operator byte(Card8 i)
        {
            return i.value;
        }
        public static  bool operator <(Card8 card8, int value)
        {
            if (card8.value < value)
                return true;
            else
                return false;
        }
        public static bool operator <(Card8 v1, Card8 v2)
        {
            if (v1.value < v2.value)
                return true;
            else
                return false;
        }
        public static bool operator >(Card8 v1, Card8 v2)
        {
            if (v1.value < v2.value)
                return false;
            else
                return true;
        }
        public static bool operator >(Card8 card8, int value)
        {
            if (card8.value > value)
                return true;
            else
                return false;
        }
        public static bool operator ==(Card8 card8, int value)
        {
            if (card8._value == (byte)value)
                return true;
            else
              return  false;
        }
        public static bool operator !=(Card8 card8, int value)
        {
            if (card8._value == (byte)value)
                return false;
            else
                return true;
        }
        public static int operator +(Card8 v1, Card8 v2)
        {
                return v1.value + v2.value;
        }
        public static Card8 operator ++(Card8 v1)
        {
            return new Card8((byte)(v1.value + 1));
        }
        public static Card8 operator --(Card8 v1)
        {
            return new Card8((byte)(v1.value - 1));
        }
        public static int operator -(Card8 v1, Card8 v2)
        {
            return v1.value - v2.value;
        }
        public override string ToString()
        {
            return value.ToString();
        }
    }
}
