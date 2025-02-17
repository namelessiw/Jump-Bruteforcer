using System.Numerics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Jump_Bruteforcer
{
    [Flags]
    public enum Input:byte
    {
        Neutral = 0,
        Left = 1,
        Right = 2,
        Jump = 4,
        Release = 8,
        Facescraper = 16,
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
        private static bool PlaceMeeting(int x, double y, bool kidUpsidedown, CollisionType type, CollisionMap CollisionMap, Bools flags)
        {
            if ((flags & Bools.FaceScraper) == Bools.FaceScraper)
            {
                return CollisionMap.GetCollisionTypes(x, (int)Math.Round(y), kidUpsidedown, (flags & Bools.FacingRight) == Bools.FacingRight).HasFlag(type);
            }
            return CollisionMap.GetCollisionTypes(x, y, kidUpsidedown).HasFlag(type);
        }
        private static bool PlaceFree(int x, double y, bool kidUpsidedown, CollisionMap CollisionMap, Bools flags)
        {
            return !PlaceMeeting(x, y, kidUpsidedown, CollisionType.Solid, CollisionMap, flags);
        }



        public static bool IsAlive(PlayerNode? node)
        {
            if (node == null) return false;
            bool inbounds = node.State.X is >= 0 and <= Map.WIDTH - 1 & node.State.Y is >= 0 and <= Map.HEIGHT - 1;
            return inbounds;
        }


        /// <summary>
        /// Runs the GameMaker game loop on a state with the given inputs and CollisionMap and returns a new State. 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="input"></param>
        /// <param name="collisionMap"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static State? Update(PlayerNode node, Input input, CollisionMap collisionMap)
        {
            State state = node.State;

            (int x, double y, double vSpeed, double hSpeed, Bools flags) = (state.X, state.Y, state.VSpeed, 0, state.Flags);
            (int xPrevious, double yPrevious) = (state.X, state.Y);
            var facingRightAtBeginning = (flags & Bools.FacingRight) == Bools.FacingRight;

            //corresponds to global.grav = 1
            bool globalGravInverted = (flags & Bools.InvertedGravity) == Bools.InvertedGravity;
            //corresponds to the player being replaced with the player2 object, which is the upsidedown kid
            bool kidUpsidedown = (node.State.Flags & Bools.ParentInvertedGravity) == Bools.ParentInvertedGravity; //TODO replace with the correct calculation
            int vspeedDirection = 1;
        beginningOfStepEvent:
            // mutate state variables here:
            if ((input & Input.Facescraper) != Input.Facescraper)
            {
            //step event:

                int h = (input & Input.Left) == Input.Left ? -1 : 0;
                h = (input & Input.Right) == Input.Right ? 1 : h;
                //vines
                VineDistance vineLDistanace = collisionMap.GetVineDistance(x, y, ObjectType.VineLeft, (flags & Bools.FacingRight) == Bools.FacingRight);
                VineDistance vineRDistance = collisionMap.GetVineDistance(x, y, ObjectType.VineRight, (flags & Bools.FacingRight) == Bools.FacingRight);

                bool turnAround = false;

                if (h != 0)
                {
                    if (vineRDistance != VineDistance.EDGE && (vineLDistanace == VineDistance.CORNER || vineLDistanace == VineDistance.FAR))
                    {
                        turnAround = true;
                    }
                }

                vineLDistanace = collisionMap.GetVineDistance(x, y, ObjectType.VineLeft, (flags & Bools.FacingRight) == Bools.FacingRight);
                vineRDistance = collisionMap.GetVineDistance(x, y, ObjectType.VineRight, (flags & Bools.FacingRight) == Bools.FacingRight);
                if (h == -1 && vineRDistance != VineDistance.EDGE || h == 1 && (vineLDistanace == VineDistance.CORNER || vineLDistanace == VineDistance.FAR))
                {
                    hSpeed = h * PhysicsParams.WALKING_SPEED;
                }
                int onPlatformOffset = kidUpsidedown ? -4 : 4;
                flags = PlaceMeeting(x, y + onPlatformOffset, kidUpsidedown, CollisionType.Platform, collisionMap, flags) ? flags | (flags & Bools.OnPlatform) : flags & ~Bools.OnPlatform;
                vSpeed = Math.Clamp(vSpeed, -PhysicsParams.MAX_VSPEED, PhysicsParams.MAX_VSPEED);
                //  playerJump
                vspeedDirection = globalGravInverted ? -1 : 1;
                if ((input & Input.Jump) == Input.Jump)
                {
                    double checkOffset = globalGravInverted ? -1 : 1;
                    var facingRightAtBeginningFlags = flags | (facingRightAtBeginning ? Bools.FacingRight : Bools.None);
                    if (PlaceMeeting(x, y + checkOffset, kidUpsidedown, CollisionType.Solid, collisionMap, facingRightAtBeginningFlags) || (flags & Bools.OnPlatform) == Bools.OnPlatform || PlaceMeeting(x, y + checkOffset, kidUpsidedown, CollisionType.Water1, collisionMap, flags) || PlaceMeeting(x, y + checkOffset, kidUpsidedown, CollisionType.Platform, collisionMap, flags))
                    {
                        vSpeed = vspeedDirection * PhysicsParams.SJUMP_VSPEED;
                        flags |= Bools.CanDJump;
                    }
                    else if ((flags & Bools.CanDJump) == Bools.CanDJump || PlaceMeeting(x, y + checkOffset, kidUpsidedown, CollisionType.Water2, collisionMap, flags))
                    {
                        vSpeed = vspeedDirection * PhysicsParams.DJUMP_VSPEED;
                        flags &= ~Bools.CanDJump;
                    }
                    else if ((flags & Bools.CanDJump) == Bools.CanDJump || PlaceMeeting(x, y + checkOffset, kidUpsidedown, CollisionType.Water3, collisionMap, flags))
                    {
                        vSpeed = vspeedDirection * PhysicsParams.DJUMP_VSPEED;
                        flags |= Bools.CanDJump;
                    }

                }
                //  playerVJump
                if ((input & Input.Release) == Input.Release & (vSpeed < 0 & !globalGravInverted | vSpeed > 0 & globalGravInverted))
                {
                    vSpeed *= PhysicsParams.RELEASE_MULTIPLIER;
                }
                //more vines
                int vineOffset = globalGravInverted ? -1 : 1;
                int upsidedownKidVSpeedDirection = globalGravInverted ? -1 : 1;
                if (vineLDistanace != VineDistance.FAR && PlaceFree(x, y + vineOffset, globalGravInverted, collisionMap, flags))
                {
                    vSpeed = 2 * upsidedownKidVSpeedDirection;
                    flags |= Bools.FacingRight;
                    //simplified physics where you always jump off a vinebecause keyboard_check is unimplemented
                    if (h == 1)
                    {
                        vSpeed = -9 * upsidedownKidVSpeedDirection;
                        hSpeed = 15;
                    }
                }
                if (vineRDistance == VineDistance.EDGE && PlaceFree(x, y + vineOffset, globalGravInverted, collisionMap, flags))
                {
                    vSpeed = 2 * upsidedownKidVSpeedDirection;
                    flags &= ~Bools.FacingRight;
                    //simplified physics where you always jump off a vinebecause keyboard_check is unimplemented
                    if (h == -1)
                    {
                        vSpeed = -9 * upsidedownKidVSpeedDirection;
                        hSpeed = -15;
                    }
                }
                if (turnAround)
                {
                    flags = h == 1 ? Bools.FacingRight | flags : ~Bools.FacingRight & flags;
                }
            }
            //facescraper
            else
            {
                int h = (node.Action & Input.Left) == Input.Left ? -1 : 0;
                h = (node.Action & Input.Right) == Input.Right ? 1 : h;
                hSpeed = h * PhysicsParams.WALKING_SPEED;

            }
            if ((node.Action & Input.Facescraper) == Input.Facescraper)
            {
                if ((flags & Bools.FaceScraper) != Bools.FaceScraper & !PlaceMeeting(x, y, kidUpsidedown, CollisionType.Solid, collisionMap, flags))
                {
                    flags |= Bools.FaceScraper;
                }
                else if ((flags & Bools.FaceScraper) == Bools.FaceScraper & PlaceFree(x, Math.Floor(y - 3), kidUpsidedown, collisionMap, flags & ~Bools.FaceScraper))
                {
                    flags &= ~Bools.FaceScraper;
                }
            }

            //global.grav
            if ((node.State.Flags & Bools.InvertedGravity) == Bools.InvertedGravity)
            {
                flags |= Bools.ParentInvertedGravity;
            }
            if (globalGravInverted & !kidUpsidedown)
            {
                y -= 4;
                (xPrevious, yPrevious) = (x, y);
                vSpeed = 0;
                hSpeed = 0;
                Bools facingDirection = Bools.FacingRight & flags;
                Bools invertedGravity = Bools.InvertedGravity & flags;
                flags = facingDirection | invertedGravity | Bools.CanDJump;
                kidUpsidedown = true;
                goto beginningOfStepEvent;
            }
            if (!globalGravInverted & kidUpsidedown)
            {
                y += 4;
                (xPrevious, yPrevious) = (x, y);
                vSpeed = 0;
                hSpeed = 0;
                Bools facingDirection = Bools.FacingRight & flags;
                Bools invertedGravity = Bools.InvertedGravity & flags;
                flags = facingDirection | invertedGravity | Bools.CanDJump;
                kidUpsidedown = false;
            }
              


            //apply friction, gravity, hspeed/vspeed:
            vSpeed += kidUpsidedown ? -PhysicsParams.GRAVITY : PhysicsParams.GRAVITY;
            x += (int)hSpeed;
            y += vSpeed;
            //collision event
            var collisionTypes = ((flags & Bools.FaceScraper) == Bools.FaceScraper) ? collisionMap.GetCollisionTypes(x, y, kidUpsidedown, (flags & Bools.FacingRight) == Bools.FacingRight) : collisionMap.GetCollisionTypes(x, y, kidUpsidedown);
            (var currentX, var currentY) = (x,  y);
            int minInstanceNum = 0;
            CollisionType currentCollision = (CollisionType)CollisionMap.UnsetAllBitsExceptMSB((int)collisionTypes);
            while (currentCollision > CollisionType.None)
            {
                switch (currentCollision)
                {
                    case CollisionType.Solid:
                        (x, y) = (xPrevious, yPrevious);
                        if (!PlaceFree(x + (int)hSpeed, y, kidUpsidedown, collisionMap, flags))
                        {
                            int sign = Math.Sign(hSpeed);
                            if (sign != 0)
                            {
                                while (PlaceFree(x + sign, y, kidUpsidedown, collisionMap, flags))
                                {
                                    x += sign;
                                }
                                hSpeed = 0;
                            }
                        }
                        if (!PlaceFree(x, y + vSpeed, kidUpsidedown, collisionMap, flags))
                        {
                            int sign = Math.Sign(vSpeed);
                            if (sign != 0)
                            {
                                flags |= sign == vspeedDirection ? Bools.CanDJump : Bools.None;
                                while (Math.Abs(vSpeed) >= 1 && PlaceFree(x, y + sign, kidUpsidedown, collisionMap, flags))
                                {
                                    y += sign;
                                    vSpeed -= sign;
                                }
                                vSpeed = 0;
                            }

                        }
                        if (!PlaceFree(x + (int)hSpeed, y + vSpeed, kidUpsidedown, collisionMap, flags))
                        {
                            hSpeed = 0;
                        }
                        x += (int)hSpeed;
                        y += vSpeed;
                        if (!PlaceFree(x, y, kidUpsidedown, collisionMap, flags))
                        {
                            (x, y) = (xPrevious, yPrevious);
                        }

                        break;

                    case CollisionType.Killer:
                        return null;
                    case CollisionType.Platform:
                        
                        Object? platform = collisionMap.GetCollidingPlatform(x, y, minInstanceNum);
                        while (platform != null)
                        {
                            if (kidUpsidedown)
                            {
                                if (y - vSpeed / 2 >= platform.Y + 15)
                                {
                                    y = platform.Y + 23;
                                    vSpeed = 0;
                                    flags |= Bools.CanDJump;
                                    flags |= Bools.OnPlatform;

                                }
                            }
                            else
                            {
                                if (y - vSpeed / 2 <= platform.Y)
                                {
                                    y = platform.Y - 9;
                                    vSpeed = 0;
                                    flags |= Bools.CanDJump;
                                    flags |= Bools.OnPlatform;

                                }
                            }

                            minInstanceNum = platform.instanceNum + 1;
                            platform = collisionMap.GetCollidingPlatform(x, (int)Math.Round(y), minInstanceNum);
                        }
                        break;
                    case CollisionType.GravityArrowUp:
                        flags |= Bools.InvertedGravity;
                        break;
                    case CollisionType.GravityArrowDown:
                        flags &= ~Bools.InvertedGravity;
                        break;
                    case CollisionType.Water1:
                    case CollisionType.Water3:
                        flags |= Bools.CanDJump;
                        vSpeed = kidUpsidedown ? Math.Max(-2, vSpeed) : Math.Min(2, vSpeed);
                        break;
                    case CollisionType.Water2:
                    case CollisionType.CatharsisWater:
                        vSpeed = kidUpsidedown ? Math.Max(-2, vSpeed) : Math.Min(2, vSpeed);
                        break;

                }
                if ((x,y) != (currentX, currentY))
                {
                    //update the collision types we'll check for on this frame
                    CollisionType nextCollisionTypes = ((flags & Bools.FaceScraper) == Bools.FaceScraper) ? collisionMap.GetCollisionTypes(x, y, kidUpsidedown, (flags & Bools.FacingRight) == Bools.FacingRight) : collisionMap.GetCollisionTypes(x, y, kidUpsidedown);
                    currentCollision = (CollisionType)CollisionMap.UnsetAllBitsExceptMSB((int)nextCollisionTypes % (int)currentCollision);
                    if (currentCollision == CollisionType.None)
                        goto collisionDone;
                    (currentX, currentY) = (x, y);
                }
                else
                {
                    currentCollision = (CollisionType)CollisionMap.UnsetAllBitsExceptMSB((int)collisionTypes % (int)currentCollision);
                }
                
            }
        collisionDone:
            //more facescraper
            if (((input & Input.Facescraper) == Input.Facescraper))
            {
                flags &= ~Bools.CanDJump;
            }
            return new State() { X = x, Y = y, VSpeed = vSpeed, Flags = flags};
        }
    }
}