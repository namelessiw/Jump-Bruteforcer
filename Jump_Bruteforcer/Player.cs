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
            return CollisionMap.GetHighestPriorityCollisionType(x, y) != CollisionType.Solid;
        }
        private static bool PlaceFree(int x, double y, CollisionMap CollisionMap)
        {
            return CollisionMap.GetHighestPriorityCollisionType(x, (int)Math.Round(y)) != CollisionType.Solid;
        }



        public static bool IsAlive(CollisionMap CollisionMap, PlayerNode node)
        {
            int yRounded = node.State.RoundedY;
            bool notOnKiller = !CollisionMap.GetCollisionTypes(node.State.X, yRounded).Contains(CollisionType.Killer);
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
            bool globalGravInverted = false; //corresponds to global.grav = 1
            bool kidUpsidedown = false; //corresponds to the player being replaced with the player2 object, which is the upsidedown kid
            if (node.Parent != null)
            {
                globalGravInverted = (node.Parent.State.Flags & Bools.InvertedGravity) == Bools.InvertedGravity;
                if (node.Parent.Parent != null )
                {
                    kidUpsidedown = (node.Parent.Parent.State.Flags & Bools.InvertedGravity) == Bools.InvertedGravity;
                }

            }
            // mutate state variables here:
            //step event:
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
            flags = PlaceMeeting(x, y + onPlatformOffset, CollisionType.Platform, collisionMap) ? flags | (flags & Bools.OnPlatform) : flags & ~Bools.OnPlatform;
            vSpeed = Math.Clamp(vSpeed, -PhysicsParams.MAX_VSPEED, PhysicsParams.MAX_VSPEED);
            //  playerJump
            int vspeedDirection = globalGravInverted ? -1 : 1;
            if ((input & Input.Jump) == Input.Jump)
            {
                double checkOffset = globalGravInverted ? -1 : 1;
                
                if (PlaceMeeting(x, y + checkOffset, CollisionType.Solid, collisionMap) || (flags & Bools.OnPlatform) == Bools.OnPlatform || PlaceMeeting(x, y + 1, CollisionType.Water1, collisionMap) || PlaceMeeting(x, y + 1, CollisionType.Platform, collisionMap))
                {
                    vSpeed = vspeedDirection * PhysicsParams.SJUMP_VSPEED;
                    flags |= Bools.CanDJump;
                }
                else if ((flags & Bools.CanDJump) == Bools.CanDJump || PlaceMeeting(x, y + checkOffset, CollisionType.Water2, collisionMap))
                {
                    vSpeed = vspeedDirection * PhysicsParams.DJUMP_VSPEED;
                    flags &= ~Bools.CanDJump;
                }
                else if ((flags & Bools.CanDJump) == Bools.CanDJump || PlaceMeeting(x, y + checkOffset, CollisionType.Water3, collisionMap))
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
            int vineOffset = kidUpsidedown ? -1 : 1;
            int upsidedownKidVSpeedDirection = kidUpsidedown ? -1 : 1;
            if (vineLDistanace != VineDistance.FAR && PlaceFree(x, y + vineOffset, collisionMap))
            {
                vSpeed = 2 * upsidedownKidVSpeedDirection;
                flags |= Bools.FacingRight;
                //simplified physics where you always jump off a vinebecause keyboard_check is unimplemented
                if (h == 1)
                {
                    vSpeed = -9 * upsidedownKidVSpeedDirection;
                    hSpeed = 15 * upsidedownKidVSpeedDirection;
                }
            }
            if (vineRDistance == VineDistance.EDGE && PlaceFree(x, y + vineOffset, collisionMap))
            {
                vSpeed = 2 * upsidedownKidVSpeedDirection;
                flags &= ~Bools.FacingRight;
                //simplified physics where you always jump off a vinebecause keyboard_check is unimplemented
                if (h == -1)
                {
                    vSpeed = -9 * upsidedownKidVSpeedDirection;
                    hSpeed = -15 * upsidedownKidVSpeedDirection;
                }
            }
            //global.grav
            if (globalGravInverted & !kidUpsidedown)
            {
                y -= 4;
            }
            if (!globalGravInverted & kidUpsidedown)
            {
                y += 4;
            }

            //apply friction, gravity, hspeed/vspeed:
            vSpeed += vspeedDirection * PhysicsParams.GRAVITY;
            x += (int)hSpeed;
            y += vSpeed;
            //collision event
            var collisionTypes = collisionMap.GetCollisionTypes(x, y);
            (var currentX, var currentY) = (x,  y);
            int minInstanceNum = 0;
            int collisionIdx = 0;
            while (collisionIdx < collisionTypes.Count)
            {
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
                                flags |= sign == vspeedDirection ? Bools.CanDJump : Bools.None;
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
                        
                        Object? platform = collisionMap.GetCollidingPlatform(x, y, minInstanceNum);
                        if (platform != null)
                        {
                            int invertedkidOffset = globalGravInverted ? 15 : 0; 
                            if (y - vSpeed / 2 <= platform.Y + invertedkidOffset)
                            {
                                y = globalGravInverted ? platform.Y + 15 : platform.Y - 9;
                                vSpeed = 0;
                                flags |= Bools.CanDJump;
                                flags |= Bools.OnPlatform;

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
                        vSpeed = globalGravInverted ? Math.Max(-2, vSpeed) : Math.Min(2, vSpeed);
                        break;
                    case CollisionType.Water2:
                    case CollisionType.CatharsisWater:
                        vSpeed = globalGravInverted ? Math.Max(-2, vSpeed) : Math.Min(2, vSpeed);
                        break;

                }
                if ((x,y) != (currentX, currentY))
                {
                    //update the collision types we'll check for on this frame
                    var currentCollisionType = collisionTypes[collisionIdx];
                    collisionTypes = collisionMap.GetCollisionTypes(x, y);
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