namespace System.Security.Cryptography
{
    class FNV1a32 : HashAlgorithm
    {
        private const UInt32 _prime = unchecked(16777619);
        private const UInt32 _offset = unchecked(2166136261);
        private UInt32 _hash = _offset;
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            for (var i = ibStart; i < cbSize; i++)
            {
                unchecked { 
                    _hash ^= array[i];
                    _hash *= _prime;
                }
            }
        }

        protected override byte[] HashFinal()
        {
            return BitConverter.GetBytes(_hash);
        }

        public override void Initialize()
        {
            _hash = _offset;
        }
    }
}
