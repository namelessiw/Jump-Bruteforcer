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
        private static bool PlaceMeeting(int x, int y, bool kidUpsidedown, CollisionType type, CollisionMap CollisionMap)
        {
            return CollisionMap.GetCollisionTypes(x, y, kidUpsidedown).Contains(type);
        }
        private static bool PlaceMeeting(int x, double y, bool kidUpsidedown, CollisionType type, CollisionMap CollisionMap)
        {
            return CollisionMap.GetCollisionTypes(x, y, kidUpsidedown).Contains(type);
        }
        private static bool PlaceFree(int x, int y, bool kidUpsidedown, CollisionMap CollisionMap)
        {
            return CollisionMap.GetHighestPriorityCollisionType(x, y, kidUpsidedown) != CollisionType.Solid;
        }
        private static bool PlaceFree(int x, double y, bool kidUpsidedown, CollisionMap CollisionMap)
        {
            return CollisionMap.GetHighestPriorityCollisionType(x, y, kidUpsidedown) != CollisionType.Solid;
        }



        public static bool IsAlive(CollisionMap CollisionMap, PlayerNode node)
        {
            int yRounded = node.State.RoundedY;
            //corresponds to global.grav = 1
            bool globalGravInverted = (node.State.Flags & Bools.InvertedGravity) == Bools.InvertedGravity;
            //corresponds to the player being replaced with the player2 object, which is the upsidedown kid
            bool kidUpsidedown = node.Parent != null ? (node.Parent.State.Flags & Bools.InvertedGravity) == Bools.InvertedGravity : globalGravInverted;

            bool notOnKiller = !CollisionMap.GetCollisionTypes(node.State.X, yRounded, kidUpsidedown).Contains(CollisionType.Killer);
            bool inbounds = node.State.X is >= 0 and <= Map.WIDTH - 1 & node.State.Y is >= 0 and <= Map.HEIGHT - 1;
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
        public static State Update(PlayerNode node, Input input, CollisionMap collisionMap)
        {
            State state = node.State;
            (int x, double y, double vSpeed, double hSpeed, Bools flags) = (state.X, state.Y, state.VSpeed, 0, state.Flags);
            (int xPrevious, double yPrevious) = (state.X, state.Y);

            //corresponds to global.grav = 1
            bool globalGravInverted = (flags & Bools.InvertedGravity) == Bools.InvertedGravity;
            //corresponds to the player being replaced with the player2 object, which is the upsidedown kid
            bool kidUpsidedown = node.Parent != null ? (node.Parent.State.Flags & Bools.InvertedGravity) == Bools.InvertedGravity : globalGravInverted;

            // mutate state variables here:
            //step event:
            beginningOfStepEvent:
            int h = (input & Input.Left) == Input.Left ? -1 : 0;
            h = (input & Input.Right) == Input.Right ? 1 : h;
            //vines
            VineDistance vineLDistanace = collisionMap.GetVineDistance(x, y, ObjectType.VineLeft, (flags & Bools.FacingRight) == Bools.FacingRight);
            VineDistance vineRDistance = collisionMap.GetVineDistance(x, y, ObjectType.VineRight, (flags & Bools.FacingRight) == Bools.FacingRight);
            if (h != 0)
            {
                if (vineRDistance != VineDistance.EDGE && (vineLDistanace == VineDistance.CORNER || vineLDistanace == VineDistance.FAR))
                {
                    flags = h == 1 ? Bools.FacingRight | flags : ~Bools.FacingRight & flags;
                }
            }
                
            vineLDistanace = collisionMap.GetVineDistance(x, y, ObjectType.VineLeft, (flags & Bools.FacingRight) == Bools.FacingRight);
            vineRDistance = collisionMap.GetVineDistance(x, y, ObjectType.VineRight, (flags & Bools.FacingRight) == Bools.FacingRight);
            if (h == -1 && vineRDistance != VineDistance.EDGE || h == 1 && (vineLDistanace == VineDistance.CORNER || vineLDistanace == VineDistance.FAR))
            {
                hSpeed = h * PhysicsParams.WALKING_SPEED;
            }
            int onPlatformOffset = kidUpsidedown ? -4 : 4;
            flags = PlaceMeeting(x, y + onPlatformOffset, kidUpsidedown, CollisionType.Platform, collisionMap) ? flags | (flags & Bools.OnPlatform) : flags & ~Bools.OnPlatform;
            vSpeed = Math.Clamp(vSpeed, -PhysicsParams.MAX_VSPEED, PhysicsParams.MAX_VSPEED);
            //  playerJump
            int vspeedDirection = globalGravInverted ? -1 : 1;
            if ((input & Input.Jump) == Input.Jump)
            {
                double checkOffset = globalGravInverted ? -1 : 1;
                if ((flags & Bools.CanDJump) != Bools.CanDJump)
                {
                    vSpeed = PhysicsParams.DJUMP_VSPEED;
                }

                if (PlaceMeeting(x, y + checkOffset, kidUpsidedown, CollisionType.Solid, collisionMap) || (flags & Bools.OnPlatform) == Bools.OnPlatform || PlaceMeeting(x, y + checkOffset, kidUpsidedown, CollisionType.Water1, collisionMap) || PlaceMeeting(x, y + checkOffset, kidUpsidedown, CollisionType.Platform, collisionMap))
                {
                    vSpeed = vspeedDirection * PhysicsParams.SJUMP_VSPEED;
                    flags |= Bools.CanDJump;
                }
                else if ((flags & Bools.CanDJump) == Bools.CanDJump || PlaceMeeting(x, y + checkOffset, kidUpsidedown, CollisionType.Water2, collisionMap))
                {
                    vSpeed = vspeedDirection * PhysicsParams.DJUMP_VSPEED;
                    flags &= ~Bools.CanDJump;
                }
                else if ((flags & Bools.CanDJump) == Bools.CanDJump || PlaceMeeting(x, y + checkOffset, kidUpsidedown, CollisionType.Water3, collisionMap))
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
            if (vineLDistanace != VineDistance.FAR && PlaceFree(x, y + vineOffset, globalGravInverted, collisionMap))
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
            if (vineRDistance == VineDistance.EDGE && PlaceFree(x, y + vineOffset, globalGravInverted, collisionMap))
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

            //  playerJump
            if ((input & Input.Jump) == Input.Jump)
            {
                if ((flags & Bools.CanDJump) != Bools.CanDJump)
                {
                    vSpeed = PhysicsParams.DJUMP_VSPEED;
                }


            }

            //global.grav
            if (globalGravInverted& !kidUpsidedown)
            {
                y -= 4;
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
            var collisionTypes = collisionMap.GetCollisionTypes(x, y, kidUpsidedown);
            (var currentX, var currentY) = (x,  y);
            int minInstanceNum = 0;
            int collisionIdx = 0;
            while (collisionIdx < collisionTypes.Count)
            {
                switch (collisionTypes[collisionIdx])
                {
                    case CollisionType.Solid:
                        (x, y) = (xPrevious, yPrevious);
                        if (!PlaceFree(x + (int)hSpeed, y, kidUpsidedown, collisionMap))
                        {
                            int sign = Math.Sign(hSpeed);
                            if (sign != 0)
                            {
                                while (PlaceFree(x + sign, y, kidUpsidedown, collisionMap))
                                {
                                    x += sign;
                                }
                                hSpeed = 0;
                            }
                        }
                        if (!PlaceFree(x, y + vSpeed, kidUpsidedown, collisionMap))
                        {
                            int sign = Math.Sign(vSpeed);
                            if (sign != 0)
                            {
                                flags |= sign == vspeedDirection ? Bools.CanDJump : Bools.None;
                                while (Math.Abs(vSpeed) >= 1 && PlaceFree(x, y + sign, kidUpsidedown, collisionMap))
                                {
                                    y += sign;
                                    vSpeed -= sign;
                                }
                                vSpeed = 0;
                            }

                        }
                        if (!PlaceFree(x + (int)hSpeed, y + vSpeed, kidUpsidedown, collisionMap))
                        {
                            hSpeed = 0;
                        }
                        x += (int)hSpeed;
                        y += vSpeed;
                        if (!PlaceFree(x, y, kidUpsidedown, collisionMap))
                        {
                            (x, y) = (xPrevious, yPrevious);
                        }

                        break;
                    case CollisionType.Platform:
                        
                        Object? platform = collisionMap.GetCollidingPlatform(x, y, minInstanceNum);
                        if (platform != null)
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
                    var currentCollisionType = collisionTypes[collisionIdx];
                    collisionTypes = collisionMap.GetCollisionTypes(x, y, kidUpsidedown);
                    CollisionType nextCollisionType = collisionTypes.FirstOrDefault(c => c < currentCollisionType);
                    if (nextCollisionType == CollisionType.None)
                        goto collisionDone;
                    collisionIdx = collisionTypes.IndexOf((CollisionType)nextCollisionType) - 1;
                    (currentX, currentY) = (x, y);
                }
                collisionIdx++;
            }
        collisionDone:

            return new State() { X = x, Y = y, VSpeed = vSpeed, Flags = flags};
        }
    }
}