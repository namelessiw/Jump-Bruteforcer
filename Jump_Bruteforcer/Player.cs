using System.Collections.Immutable;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Jump_Bruteforcer
{
    [Flags]
    public enum Input
    {
        Neutral = 0,
        Left = 1,
        Right = 2,
        Jump = 4,
        Release = 8
    }

    public static class Player
    {

        /// <summary>
        /// Checks the CollisionMap for collision of "type" at (x, y). If there is, return true, else false.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="type"></param>
        /// <param name="CollisionMap"></param>
        /// <returns></returns>
        private static bool PlaceMeeting(int x, int y, CollisionType type, CollisionMap CollisionMap)
        {
            return CollisionMap.GetCollisionTypes(x, y).Contains(type);
        }



        public static bool IsAlive(CollisionMap CollisionMap, PlayerNode node)
        {
            int yRounded = node.State.RoundedY;
            bool notOnKiller = !CollisionMap.GetCollisionTypes(node.State.X, yRounded).Contains(CollisionType.Killer);
            bool inbounds = node.State.X is >= 0 and <= 799 & yRounded is >= 0 and <= 607;
            return notOnKiller & inbounds;
        }

        /// <summary>
        /// Runs the GameMaker game loop on a state with the given inputs and CollisionMap and returns a new State. 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="input"></param>
        /// <param name="collisionMap"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static State Update(State state, Input input, CollisionMap collisionMap)
        {
            (int x, double y, double vSpeed, bool canDJump, bool onPlatform) = (state.X, state.Y, state.VSpeed, state.CanDJump, state.OnPlatform);
            // mutate state variables here:
            throw new NotImplementedException();

            return new State() { X = x, Y = y, VSpeed = vSpeed, CanDJump = canDJump, OnPlatform = onPlatform };
        }
    }
}