namespace CFFFont.CFFDataType
{
    public readonly struct Offset8
    {

        private readonly byte _value;
        public readonly byte value => _value;
        private Offset8(byte value) => _value = value;
        public static explicit operator Offset8(Card8 i)
        {
            return new Offset8(i.value);
        }
        public static explicit operator int(Offset8 i)
        {
            return i.value;
        }
        public static ushort operator -(Offset8 value1, Offset8 value2)
        {
            return (ushort)(value1.value - value2.value);
        }
       
        public override string ToString()
        {
            return value.ToString();
        }
    }
}
