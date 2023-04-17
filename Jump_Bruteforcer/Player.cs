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
        private static bool PlaceMeeting(int x, double y, CollisionType type, CollisionMap CollisionMap)
        {
            return CollisionMap.GetCollisionTypes(x, (int)Math.Round(y)).Contains(type);
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
            (int x, double y, double vSpeed, double hSpeed, bool canDJump, bool onPlatform) = (state.X, state.Y, state.VSpeed, 0, state.CanDJump, state.OnPlatform);
            // mutate state variables here:
            //step event:
            if ((input & Input.Left) == Input.Left)
            {
                hSpeed = -PhysicsParams.WALKING_SPEED;
            }
            if ((input & Input.Right) == Input.Right)
            {
                hSpeed = PhysicsParams.WALKING_SPEED;
            }
            onPlatform &= PlaceMeeting(x, y + 4, CollisionType.Platform, collisionMap);
            vSpeed = Math.Clamp(vSpeed, -PhysicsParams.MAX_VSPEED, PhysicsParams.MAX_VSPEED);
            //  playerJump
            if ((input & Input.Jump) == Input.Jump)
            {
                if (PlaceMeeting(x, y + 1, CollisionType.Solid, collisionMap) || onPlatform || PlaceMeeting(x, y + 1, CollisionType.Water1, collisionMap))
                {
                    vSpeed = PhysicsParams.SJUMP_VSPEED;
                    canDJump = true;
                }
                else if (canDJump || PlaceMeeting(x, y + 1, CollisionType.Water2, collisionMap))
                {
                    vSpeed = PhysicsParams.DJUMP_VSPEED;
                }

            }
            //  playerVJump
            if ((input & Input.Release) == Input.Release & vSpeed < 0)
            {
                vSpeed *= PhysicsParams.RELEASE_MULTIPLIER;
            }
            //apply friction, gravity, hspeed/vspeed:
            vSpeed += PhysicsParams.GRAVITY;


            return new State() { X = x, Y = y, VSpeed = vSpeed, CanDJump = canDJump, OnPlatform = onPlatform };
        }
    }
}