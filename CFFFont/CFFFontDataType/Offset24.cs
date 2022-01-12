namespace CFFFont.CFFDataType
{ 
    public readonly struct Offset24
    {

        private readonly uint _value;
        public readonly uint value => _value;
        private Offset24(uint value) => _value = value;
        public static explicit operator Offset24(uint i)
        {
            return new Offset24((uint)i);
        }
        public static explicit operator Offset24(int i)
        {
            return new Offset24((uint)i);
        }
    }
}
