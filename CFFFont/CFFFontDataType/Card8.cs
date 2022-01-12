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
        public static bool operator >(Card8 card8, int value)
        {
            if (card8.value > value)
                return true;
            else
                return false;
        }
        public override string ToString()
        {
            return value.ToString();
        }
    }
}
