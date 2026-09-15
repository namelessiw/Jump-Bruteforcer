namespace Jump_Bruteforcer
{
    internal readonly record struct PathLink(int ParentIndex, Input Input);

    /// <summary>
    /// Append-only, segmented storage for the parent and input of each discovered
    /// search node. Common links use four bytes instead of separate int and byte
    /// lists, and fixed chunks avoid copying very large arrays while they grow.
    /// </summary>
    internal sealed class PathLinkStore
    {
        private const int ChunkShift = 18;
        private const int ChunkSize = 1 << ChunkShift;
        private const int ChunkMask = ChunkSize - 1;
        private const int InputShift = 28;
        private const int MaxPackedNodeCount = 1 << InputShift;
        private const uint ParentMask = MaxPackedNodeCount - 1;

        private readonly int packedNodeLimit;
        private readonly List<uint[]> packedChunks = new();
        private readonly List<WideChunk> wideChunks = new();
        private uint[] currentPackedChunk = Array.Empty<uint>();
        private WideChunk? currentWideChunk;

        public int Count { get; private set; }
        public int WideCount => Math.Max(0, Count - packedNodeLimit);
        public long PackedBytes => (long)packedChunks.Count * ChunkSize * sizeof(uint);
        public long WideBytes => (long)wideChunks.Count * ChunkSize * (sizeof(int) + sizeof(byte));
        public long TotalBytes => PackedBytes + WideBytes;

        public PathLinkStore() : this(MaxPackedNodeCount)
        {
        }

        internal PathLinkStore(int packedNodeLimit)
        {
            if (packedNodeLimit <= 0 || packedNodeLimit > MaxPackedNodeCount)
            {
                throw new ArgumentOutOfRangeException(nameof(packedNodeLimit));
            }

            this.packedNodeLimit = packedNodeLimit;
        }

        public int Add(int parentIndex, Input input)
        {
            if (parentIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(parentIndex));
            }

            uint inputValue = (byte)input;
            if (inputValue > 0xf)
            {
                throw new ArgumentOutOfRangeException(nameof(input));
            }
            if (Count == int.MaxValue)
            {
                throw new InvalidOperationException("The path link store reached the Int32 node-index limit.");
            }

            int nodeIndex = Count;
            if (nodeIndex < packedNodeLimit)
            {
                if ((uint)parentIndex > ParentMask)
                {
                    throw new ArgumentOutOfRangeException(nameof(parentIndex));
                }

                int offset = nodeIndex & ChunkMask;
                if (offset == 0)
                {
                    currentPackedChunk = new uint[ChunkSize];
                    packedChunks.Add(currentPackedChunk);
                }

                currentPackedChunk[offset] = inputValue << InputShift | (uint)parentIndex;
            }
            else
            {
                int wideIndex = nodeIndex - packedNodeLimit;
                int offset = wideIndex & ChunkMask;
                if (offset == 0)
                {
                    currentWideChunk = new WideChunk();
                    wideChunks.Add(currentWideChunk);
                }

                currentWideChunk!.Parents[offset] = parentIndex;
                currentWideChunk.Inputs[offset] = (byte)inputValue;
            }

            Count++;
            return nodeIndex;
        }

        public PathLink this[int nodeIndex]
        {
            get
            {
                if ((uint)nodeIndex >= (uint)Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(nodeIndex));
                }

                if (nodeIndex < packedNodeLimit)
                {
                    uint packed = packedChunks[nodeIndex >> ChunkShift][nodeIndex & ChunkMask];
                    return new PathLink(
                        (int)(packed & ParentMask),
                        (Input)(packed >> InputShift));
                }

                int wideIndex = nodeIndex - packedNodeLimit;
                WideChunk chunk = wideChunks[wideIndex >> ChunkShift];
                int offset = wideIndex & ChunkMask;
                return new PathLink(chunk.Parents[offset], (Input)chunk.Inputs[offset]);
            }
        }

        private sealed class WideChunk
        {
            public readonly int[] Parents = new int[ChunkSize];
            public readonly byte[] Inputs = new byte[ChunkSize];
        }
    }
}
