using System.Windows.Media;
using System.Text.Json;
using System.Collections.Immutable;
using System.Windows;
using System.Runtime.InteropServices;
using System.IO.Hashing;

namespace Jump_Bruteforcer
{
    [Flags]
    public enum Bools: byte
    {
        None = 0,
        CanDJump = 1,
        OnPlatform = 2,
        FacingRight = 4,
        InvertedGravity = 8,
        ParentInvertedGravity = 16
    }
    [StructLayout(LayoutKind.Auto)]
    public readonly record struct State 
    {

        public int X { get; init; }
        public double Y { get; init; }
        public double VSpeed { get; init; }
        public Bools Flags { get; init; }
        public int RoundedY { get { return (int)Math.Round(Y); } }




    }

    public readonly record struct NeighborCandidate(State State, Input Input, ulong Hash);

    public class PlayerNode : IEquatable<PlayerNode>
    {
        static XxHash64 hasher = new();
        const int epsilon = 10;
        public const int MaxNeighborCount = 12;
        public State State { get; set; }
        public int NodeIndex { get; set; }
        public uint PathCost { get; set; }

        public static readonly ImmutableArray<Input> inputs = ImmutableArray.Create(Input.Neutral, Input.Left, Input.Right);
        public static readonly ImmutableArray<Input> inputsJump = ImmutableArray.Create(Input.Jump, Input.Left | Input.Jump, Input.Right | Input.Jump, Input.Jump | Input.Release, Input.Left | Input.Jump | Input.Release, Input.Right | Input.Jump | Input.Release);
        public static readonly ImmutableArray<Input> inputsRelease = ImmutableArray.Create(Input.Release, Input.Left | Input.Release, Input.Right | Input.Release);
        private static readonly CollisionType jumpables = CollisionType.Solid | CollisionType.Platform | CollisionType.Water1 | CollisionType.Water2 | CollisionType.Water3;
        public PlayerNode(int x, double y, double vSpeed, Bools flags = Bools.CanDJump | Bools.FacingRight, Input? action = null, int nodeIndex = 0) =>
            (State, NodeIndex, PathCost) = (new State() { X = x, Y = y, VSpeed = vSpeed, Flags = flags }, nodeIndex, uint.MaxValue);

        public PlayerNode(State state)
        {
            State = state;
            PathCost = uint.MaxValue;
        }

        public bool IsGoal((int x, int y) goal) => Math.Abs(State.X - goal.x) <= 1 & State.RoundedY == goal.y;








        /// <summary>
        /// creates the set of all unique states that can be reached in one frame from the current state with arbitrary inputs.
        /// states with fewer inputs are favored if two states are the same. States inside playerkillers are excluded.
        /// </summary>
        /// <returns>a Hashset of playerNodes</returns>
        public int GetNeighborCandidates(CollisionMap CollisionMap, NeighborCandidate[] neighbors)
        {
            int neighborCount = 0;
            fillNeighbors(CollisionMap, neighbors, inputs, ref neighborCount);
            //corresponds to global.grav = 1
            bool globalGravInverted = (State.Flags & Bools.InvertedGravity) == Bools.InvertedGravity;
            //corresponds to the player being replaced with the player2 object, which is the upsidedown kid
            bool kidUpsidedown = (this.State.Flags & Bools.ParentInvertedGravity) == Bools.ParentInvertedGravity; ; //todo replace with correct calculation

            double checkOffset = globalGravInverted ? -1 : 1;
            if (Math.Sign(State.VSpeed) == -checkOffset)
            {
                fillNeighbors(CollisionMap, neighbors, inputsRelease, ref neighborCount);
            }
            
            if ((State.Flags & (Bools.OnPlatform | Bools.CanDJump)) != Bools.None || (CollisionMap.GetCollisionTypes(State.X, (int)Math.Round(State.Y + checkOffset), kidUpsidedown) | jumpables) != 0)
            {
                fillNeighbors(CollisionMap, neighbors, inputsJump, ref neighborCount);
            }

            return neighborCount;

            void fillNeighbors(CollisionMap collisionMap, NeighborCandidate[] candidates, ImmutableArray<Input> candidateInputs, ref int count)
            {
                foreach (Input input in candidateInputs)
                {
                    State? nextState = Player.Update(State, input, collisionMap);
                    if (nextState is not State state || !Player.IsAlive(state))
                    {
                        continue;
                    }

                    ulong hash = Hash(state);
                    bool duplicate = false;
                    for (int i = 0; i < count; i++)
                    {
                        if (candidates[i].Hash == hash)
                        {
                            duplicate = true;
                            break;
                        }
                    }

                    if (!duplicate)
                    {
                        candidates[count++] = new NeighborCandidate(state, input, hash);
                    }
                }
            }
        }

        public IEnumerable<(PlayerNode Node, Input Input, ulong Hash)> GetNeighbors(CollisionMap CollisionMap)
        {
            var candidates = new NeighborCandidate[MaxNeighborCount];
            int count = GetNeighborCandidates(CollisionMap, candidates);
            for (int i = 0; i < count; i++)
            {
                NeighborCandidate candidate = candidates[i];
                yield return (new PlayerNode(candidate.State), candidate.Input, candidate.Hash);
            }
        }

        /// <summary>
        /// Creates the next state in the tree after applying inputs to the game starting from the current state
        /// </summary>
        /// <param name="input"></param> the inputs for the next frame
        /// <param name="CollisionMap"></param> the game field
        /// <returns>A new PlayerNode that results from running inputs on the collision map</returns>
        public PlayerNode? NewState(Input input, CollisionMap CollisionMap)
        {

            State? newState = Player.Update(State, input, CollisionMap);
            if (newState != null)
            {
                return new PlayerNode(newState.Value);
            }
            return null;
            
        }

        public bool Equals(PlayerNode? other)
        {
            if (other is null)
            {
                return false;
            }

            return State.X == other.State.X & ApproximatelyEquals(State.Y, other.State.Y) &
            ApproximatelyEquals(State.VSpeed, other.State.VSpeed) & State.Flags == other.State.Flags;
        }

        private static double Quantize(double a)
        {
            return Math.Round(a * epsilon);
        }
        private static bool ApproximatelyEquals(double a, double b)
        {
            return Quantize(a) == Quantize(b);
        }

        public override int GetHashCode() => Hash().GetHashCode();
        public ulong Hash() => Hash(State);
        public static ulong Hash(State state)
        {
            int x = state.X;
            double y = Quantize(state.Y);
            double vspeed = Quantize(state.VSpeed);
            byte flags = (byte)state.Flags;

            hasher.Append(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref x, 1)));
            hasher.Append(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref y, 1)));
            hasher.Append(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref vspeed, 1)));
            hasher.Append(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref flags, 1)));
            ulong hash = hasher.GetCurrentHashAsUInt64();
            hasher.Reset();
            return hash;
        }
        
        public override string ToString() => $"{{State: {JsonSerializer.Serialize(State)}}}";
    }
}
