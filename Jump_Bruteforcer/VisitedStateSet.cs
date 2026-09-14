namespace Jump_Bruteforcer
{
    /// <summary>
    /// Sparse direct-address set for quantized search states. A plane is allocated
    /// only when a Flags/VSpeed combination is first encountered.
    /// </summary>
    internal sealed class VisitedStateSet
    {
        private const int QuantizedHeight = (Map.HEIGHT - 1) * 10 + 1;
        private const int PositionCount = Map.WIDTH * QuantizedHeight;
        private const int WordsPerPlane = (PositionCount + 63) / 64;
        private const int MinQuantizedVSpeed = -94;
        private const int MaxQuantizedVSpeed = 94;
        private const int VSpeedCount = MaxQuantizedVSpeed - MinQuantizedVSpeed + 1;
        private const int FlagCount = 32;

        private readonly ulong[]?[] planes = new ulong[FlagCount * VSpeedCount][];
        private HashSet<ulong>? overflow;

        public int Count { get; private set; }
        public int AllocatedPlaneCount { get; private set; }
        public long BitmapBytes => (long)AllocatedPlaneCount * WordsPerPlane * sizeof(ulong);
        public int OverflowCount => overflow?.Count ?? 0;

        public bool Add(ulong key)
        {
            int x = (int)(key & 0x3ffUL);
            int quantizedY = (int)((key >> 10) & 0x1fffUL);
            int quantizedVSpeed = unchecked((int)(uint)(key >> 23));
            int flags = (int)((key >> 55) & 0xffUL);

            if ((uint)x >= Map.WIDTH || (uint)quantizedY >= QuantizedHeight ||
                quantizedVSpeed < MinQuantizedVSpeed || quantizedVSpeed > MaxQuantizedVSpeed ||
                (uint)flags >= FlagCount)
            {
                overflow ??= new HashSet<ulong>();
                if (!overflow.Add(key))
                {
                    return false;
                }

                Count++;
                return true;
            }

            int planeIndex = flags * VSpeedCount + quantizedVSpeed - MinQuantizedVSpeed;
            ulong[]? plane = planes[planeIndex];
            if (plane == null)
            {
                plane = new ulong[WordsPerPlane];
                planes[planeIndex] = plane;
                AllocatedPlaneCount++;
            }

            int bitIndex = quantizedY * Map.WIDTH + x;
            int wordIndex = bitIndex >> 6;
            ulong mask = 1UL << (bitIndex & 63);
            if ((plane[wordIndex] & mask) != 0)
            {
                return false;
            }

            plane[wordIndex] |= mask;
            Count++;
            return true;
        }
    }
}
