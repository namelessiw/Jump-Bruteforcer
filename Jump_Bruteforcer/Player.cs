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
        private static bool PlaceFree(int x, int y, CollisionMap CollisionMap)
        {
            return !CollisionMap.GetCollisionTypes(x, y).Contains(CollisionType.Solid);
        }
        private static bool PlaceFree(int x, double y, CollisionMap CollisionMap)
        {
            return !CollisionMap.GetCollisionTypes(x, (int)Math.Round(y)).Contains(CollisionType.Solid);
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
            (int xPrevious, double yPrevious) = (state.X, state.Y);
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
                if (PlaceMeeting(x, y + 1, CollisionType.Solid, collisionMap) || onPlatform || PlaceMeeting(x, y + 1, CollisionType.Water3, collisionMap) || PlaceMeeting(x, y + 1, CollisionType.Platform, collisionMap))
                {
                    vSpeed = PhysicsParams.SJUMP_VSPEED;
                    canDJump = true;
                }
                else if (canDJump || PlaceMeeting(x, y + 1, CollisionType.Water2, collisionMap))
                {
                    vSpeed = PhysicsParams.DJUMP_VSPEED;
                    canDJump = false;
                }

            }
            //  playerVJump
            if ((input & Input.Release) == Input.Release & vSpeed < 0)
            {
                vSpeed *= PhysicsParams.RELEASE_MULTIPLIER;
            }
            //apply friction, gravity, hspeed/vspeed:
            vSpeed += PhysicsParams.GRAVITY;
            x += (int)hSpeed;
            y += vSpeed;
            //collision event
            var collisionTypes = collisionMap.GetCollisionTypes(x, y);
            int collisionIdx = 0;
            while (collisionIdx < collisionTypes.Count){ 
                switch (collisionTypes[collisionIdx])
                    {
                        case CollisionType.Solid:
                            (x, y) = (xPrevious, yPrevious);
                            if (!PlaceFree(x + (int)hSpeed, y, collisionMap))
                            {
                                int sign = Math.Sign(hSpeed);
                                if (sign != 0)
                                {
                                    while (PlaceFree(x + sign, y, collisionMap))
                                    {
                                        x += sign;
                                    }
                                    hSpeed = 0;
                                }
                            }
                            if (!PlaceFree(x, y + vSpeed, collisionMap))
                            {
                                int sign = Math.Sign(vSpeed);
                                if (sign != 0)
                                {
                                    canDJump |= sign > 0;
                                    while (Math.Abs(vSpeed) >= 1 && PlaceFree(x, y + sign, collisionMap))
                                    {
                                        y += sign;
                                        vSpeed -= sign;
                                    }
                                    vSpeed = 0;
                                }

                            }
                            if (!PlaceFree(x + (int)hSpeed, y + vSpeed, collisionMap))
                            {
                                hSpeed = 0;
                            }
                            x += (int)hSpeed;
                            y += vSpeed;
                            if (!PlaceFree(x, y, collisionMap))
                            {
                                (x, y) = (xPrevious, yPrevious);
                            }
                            break;
                        case CollisionType.Platform:
                            int minInstanceNum = 0;
                            Object? platform = collisionMap.GetCollidingPlatform(x, y, minInstanceNum);
                            while (platform is not null)
                            {
                                if (y - vSpeed / 2 <= platform.Y)
                                {
                                    y = platform.Y - 9;
                                    vSpeed = 0;
                                    canDJump = true;
                                    onPlatform = true;
                                    var currentCollisionType = collisionTypes[collisionIdx];
                                    collisionTypes = collisionMap.GetCollisionTypes(x, y);
                                    collisionIdx = collisionTypes.IndexOf(collisionTypes.SkipWhile(x => x > currentCollisionType).FirstOrDefault()) - 1;
                                    if (collisionIdx < 0)
                                        goto collisionDone;
                                }
                                minInstanceNum = platform.instanceNum + 1;
                                platform = collisionMap.GetCollidingPlatform(x, y, minInstanceNum);
                            }
                            break;
                    case CollisionType.Water3:
                        canDJump = true;
                        vSpeed = Math.Min(2, vSpeed);
                        break;
                    case CollisionType.Water2:
                        vSpeed = Math.Min(2, vSpeed);
                        break;

                }
                collisionIdx++;
            }
            collisionDone:

            return new State() { X = x, Y = y, VSpeed = vSpeed, CanDJump = canDJump, OnPlatform = onPlatform };
        }
    }
}