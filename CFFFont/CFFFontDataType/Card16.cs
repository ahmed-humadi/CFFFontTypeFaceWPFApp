using System;

namespace CFFFont.CFFDataType
{
    public readonly struct Card16
    {
        private readonly ushort _value;
        public readonly ushort value => _value;
        private Card16(ushort value) => _value = value;

        public static explicit operator Card16(int i)
        {
            return new Card16((ushort)i);
        }
        public static explicit operator Card16(ushort i)
        {
            return new Card16(i);
        }
        public static explicit operator Card16(Card8 i)
        {
            return new Card16((ushort)i.value);
        }
        public static explicit operator ushort(Card16 card16)
        {
            return card16._value;
        }
        public static int operator +(Card16 value1 , int value2)
        {
            return (value1.value + value2);
        }
        public static Card16 operator -(Card16 value1, Card16 value2)
        {
            return new Card16((ushort)(value1.value - value2.value));
        }
        //public static bool operator <(ushort value2, Card16 value1)
        //{
        //    if (value2 < value1.value)
        //        return true;
        //    else
        //        return false;
        //}
        //public static bool operator >(ushort value2, Card16 value1)
        //{
        //    if (value2 > value1.value)
        //        return true;
        //    else
        //        return false;
        //}
        public override string ToString()
        {
            return value.ToString();
        }
    }
}
