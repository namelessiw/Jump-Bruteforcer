using System.Windows.Media;
using System.Text.Json;
using System.Collections.Immutable;
using System.Windows;
using System.Runtime.InteropServices;
using System.Diagnostics;

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

    public readonly record struct NeighborCandidate(State State, Input Input, ulong Key);

    public class PlayerNode : IEquatable<PlayerNode>
    {
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
        public static int GetNeighborCandidates(State currentState, CollisionMap CollisionMap, NeighborCandidate[] neighbors)
        {
            int neighborCount = 0;
            fillNeighbors(CollisionMap, neighbors, inputs, ref neighborCount);
            //corresponds to global.grav = 1
            bool globalGravInverted = (currentState.Flags & Bools.InvertedGravity) == Bools.InvertedGravity;
            //corresponds to the player being replaced with the player2 object, which is the upsidedown kid
            bool kidUpsidedown = (currentState.Flags & Bools.ParentInvertedGravity) == Bools.ParentInvertedGravity; ; //todo replace with correct calculation

            double checkOffset = globalGravInverted ? -1 : 1;
            if (Math.Sign(currentState.VSpeed) == -checkOffset)
            {
                fillNeighbors(CollisionMap, neighbors, inputsRelease, ref neighborCount);
            }
            
            if ((currentState.Flags & (Bools.OnPlatform | Bools.CanDJump)) != Bools.None || (CollisionMap.GetCollisionTypes(currentState.X, (int)Math.Round(currentState.Y + checkOffset), kidUpsidedown) | jumpables) != 0)
            {
                fillNeighbors(CollisionMap, neighbors, inputsJump, ref neighborCount);
            }

            return neighborCount;

            void fillNeighbors(CollisionMap collisionMap, NeighborCandidate[] candidates, ImmutableArray<Input> candidateInputs, ref int count)
            {
                foreach (Input input in candidateInputs)
                {
                    State? nextState = Player.Update(currentState, input, collisionMap);
                    if (nextState is not State state || !Player.IsAlive(state))
                    {
                        continue;
                    }

                    ulong key = StateKey(state);
                    bool duplicate = false;
                    for (int i = 0; i < count; i++)
                    {
                        if (candidates[i].Key == key)
                        {
                            duplicate = true;
                            break;
                        }
                    }

                    if (!duplicate)
                    {
                        candidates[count++] = new NeighborCandidate(state, input, key);
                    }
                }
            }
        }

        public IEnumerable<(PlayerNode Node, Input Input, ulong Hash)> GetNeighbors(CollisionMap CollisionMap)
        {
            var candidates = new NeighborCandidate[MaxNeighborCount];
            int count = GetNeighborCandidates(State, CollisionMap, candidates);
            for (int i = 0; i < count; i++)
            {
                NeighborCandidate candidate = candidates[i];
                yield return (new PlayerNode(candidate.State), candidate.Input, candidate.Key);
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

        public override int GetHashCode() => StateKey(State).GetHashCode();
        public ulong Hash() => StateKey(State);
        public static ulong StateKey(State state)
        {
            int quantizedY = (int)Quantize(state.Y);
            int quantizedVSpeed = (int)Quantize(state.VSpeed);

            // Search states are in bounds: X needs 10 bits and Y*10 needs
            // 13 bits. VSpeed keeps its full signed 32-bit representation.
            Debug.Assert((uint)state.X <= 0x3ff);
            Debug.Assert((uint)quantizedY <= 0x1fff);
            return ((ulong)(uint)state.X & 0x3ffUL)
                | (((ulong)(uint)quantizedY & 0x1fffUL) << 10)
                | ((ulong)(uint)quantizedVSpeed << 23)
                | ((ulong)(byte)state.Flags << 55);
        }
        
        public override string ToString() => $"{{State: {JsonSerializer.Serialize(State)}}}";
    }
}
