namespace CFFFont.CFFDataType
{
    public readonly struct Offset32
    {

        private readonly uint _value;
        public readonly uint value => _value;
        private Offset32(uint value) => _value = value;
        public static explicit operator Offset32(int i)
        {
            return new Offset32((uint)i);
        }
        public static explicit operator Offset32(uint i)
        {
            return new Offset32(i);
        }
    }
}
