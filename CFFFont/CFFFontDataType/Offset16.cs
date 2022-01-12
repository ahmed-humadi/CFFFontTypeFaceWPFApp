namespace CFFFont.CFFDataType
{
    public readonly struct Offset16
    {

        private readonly ushort _value;
        public readonly ushort value => _value;
        private Offset16(ushort value) => _value = value;
        public static explicit operator Offset16(Card16 i)
        {
            return new Offset16(i.value);
        }
        public static explicit operator int(Offset16 i)
        {
            return i.value;
        }
        public static ushort operator -(Offset16 value1, Offset16 value2)
        {
            return (ushort)(value1.value - value2.value);
        }
    }
}
