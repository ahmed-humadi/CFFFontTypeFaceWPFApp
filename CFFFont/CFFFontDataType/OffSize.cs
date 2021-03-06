namespace CFFFont.CFFDataType
{
    public readonly struct OffSize
    {
        private readonly byte _value;
        public readonly byte value => _value;
        private OffSize(byte value) => _value = value;
        public static explicit operator OffSize(byte i)
        {
            return new OffSize((byte)i);
        }
        public static int operator *(ushort value, OffSize offSize)
        {
            return (value * offSize.value);
        }
        public static int operator *(int value, OffSize offSize)
        {
            return (value * offSize.value);
        }
        public override string ToString()
        {
            return value.ToString();
        }
    }
}
