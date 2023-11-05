namespace Jump_Bruteforcer
{
    [Flags]
    public enum Input
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
        private static bool PlaceMeeting(int x, int y, CollisionType type, CollisionMap CollisionMap)
        {
            return CollisionMap.GetCollisionTypes(x, y).Contains(type);
        }
        private static bool PlaceMeeting(int x, double y, CollisionType type, CollisionMap CollisionMap)
        {
            return CollisionMap.GetCollisionTypes(x, (int)Math.Round(y)).Contains(type);
        }
        public static bool PlaceMeeting(int x, int y, CollisionType type, CollisionMap CollisionMap, bool facingRight, bool facescraper)
        {
            if (facescraper)
            {
                return CollisionMap.GetCollisionTypes(x, y, facingRight).Contains(type);
            }
            return PlaceMeeting(x, y, type, CollisionMap);
        }
        public static bool PlaceMeeting(int x, double y, CollisionType type, CollisionMap CollisionMap, bool facingRight, bool facescraper)
        {
            if (facescraper)
            {
                return CollisionMap.GetCollisionTypes(x, (int)Math.Round(y), facingRight).Contains(type);
            }
            return PlaceMeeting(x, y, type, CollisionMap);
        }
        private static bool PlaceFree(int x, int y, CollisionMap CollisionMap, bool facingRight, bool facescraper)
        {
            if (facescraper)
            {
                return !CollisionMap.GetCollisionTypes(x, y, facingRight).Contains(CollisionType.Solid);
            }
            return PlaceFree(x, y, CollisionMap);
        }
        private static bool PlaceFree(int x, double y, CollisionMap CollisionMap, bool facingRight, bool facescraper)
        {
            if (facescraper)
            {
                return !CollisionMap.GetCollisionTypes(x, y, facingRight).Contains(CollisionType.Solid);
            }
            return PlaceFree(x, y, CollisionMap);
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
            bool notOnKiller;
            if ((node.State.Flags & Bools.FaceScraper) == Bools.FaceScraper)
            {
                notOnKiller = !CollisionMap.GetCollisionTypes(node.State.X, yRounded, (node.State.Flags & Bools.FacingRight) == Bools.FacingRight).Contains(CollisionType.Killer);
            }
            else { 
                notOnKiller = !CollisionMap.GetCollisionTypes(node.State.X, yRounded).Contains(CollisionType.Killer);
            }
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
        public static State Update(PlayerNode parent, Input input, CollisionMap collisionMap)
        {
            var state = parent.State;
            (int x, double y, double vSpeed, double hSpeed, Bools flags) = (state.X, state.Y, state.VSpeed, 0, state.Flags);
            (int xPrevious, double yPrevious) = (state.X, state.Y);
            var facingRightAtBeginning = (flags & Bools.FacingRight) == Bools.FacingRight;
            // mutate state variables here:
            if ((input & Input.Facescraper) != Input.Facescraper)
            {
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

                flags = PlaceMeeting(x, y + 4, CollisionType.Platform, collisionMap) ? flags | (flags & Bools.OnPlatform) : flags & ~Bools.OnPlatform;
                vSpeed = Math.Clamp(vSpeed, -PhysicsParams.MAX_VSPEED, PhysicsParams.MAX_VSPEED);
                //  playerJump
                if ((input & Input.Jump) == Input.Jump)
                {
                    if (PlaceMeeting(x, y + 1, CollisionType.Solid, collisionMap, (flags & Bools.FacingRight) == Bools.FacingRight || facingRightAtBeginning, (flags & Bools.FaceScraper) == Bools.FaceScraper) || (flags & Bools.OnPlatform) == Bools.OnPlatform || PlaceMeeting(x, y + 1, CollisionType.Water1, collisionMap, (flags & Bools.FacingRight) == Bools.FacingRight, (flags & Bools.FaceScraper) == Bools.FaceScraper) || PlaceMeeting(x, y + 1, CollisionType.Platform, collisionMap, (flags & Bools.FacingRight) == Bools.FacingRight, (flags & Bools.FaceScraper) == Bools.FaceScraper))
                    {
                        vSpeed = PhysicsParams.SJUMP_VSPEED;
                        flags |= Bools.CanDJump;
                    }
                    else if ((flags & Bools.CanDJump) == Bools.CanDJump || PlaceMeeting(x, y + 1, CollisionType.Water2, collisionMap, (flags & Bools.FacingRight) == Bools.FacingRight, (flags & Bools.FaceScraper) == Bools.FaceScraper))
                    {
                        vSpeed = PhysicsParams.DJUMP_VSPEED;
                        flags &= ~Bools.CanDJump;
                    }
                    else if ((flags & Bools.CanDJump) == Bools.CanDJump || PlaceMeeting(x, y + 1, CollisionType.Water3, collisionMap, (flags & Bools.FacingRight) == Bools.FacingRight, (flags & Bools.FaceScraper) == Bools.FaceScraper))
                    {
                        vSpeed = PhysicsParams.DJUMP_VSPEED;
                        flags |= Bools.CanDJump;
                    }

                }
                //  playerVJump
                if ((input & Input.Release) == Input.Release & vSpeed < 0)
                {
                    vSpeed *= PhysicsParams.RELEASE_MULTIPLIER;
                }
                //more vines
                if (vineLDistanace != VineDistance.FAR && PlaceFree(x, y + 1, collisionMap))
                {
                    vSpeed = 2;
                    flags |= Bools.FacingRight;
                    //simplified physics where you always jump off a vinebecause keyboard_check is unimplemented
                    if (h == 1)
                    {
                        vSpeed = -9;
                        hSpeed = 15;
                    }
                }
                if (vineRDistance == VineDistance.EDGE && PlaceFree(x, y + 1, collisionMap))
                {
                    vSpeed = 2;
                    flags &= ~Bools.FacingRight;
                    //simplified physics where you always jump off a vinebecause keyboard_check is unimplemented
                    if (h == -1)
                    {
                        vSpeed = -9;
                        hSpeed = -15;
                    }
                }
            }
            //facescraper
            else {
                int h = (parent.Action & Input.Left) == Input.Left ? -1 : 0;
                h = (parent.Action & Input.Right) == Input.Right ? 1 : h;
                hSpeed = h * PhysicsParams.WALKING_SPEED;

            }

            if ((parent.Action & Input.Facescraper) == Input.Facescraper)
            {
                if ((flags & Bools.FaceScraper) != Bools.FaceScraper & !collisionMap.GetCollisionTypes(x, y, (flags & Bools.FacingRight) == Bools.FacingRight, true).Contains(CollisionType.Solid))
                {
                    flags |= Bools.FaceScraper;
                }
                else if ((flags & Bools.FaceScraper) == Bools.FaceScraper & PlaceFree(x, Math.Floor(y - 3), collisionMap))
                {
                    flags &= ~Bools.FaceScraper;
                }
            }


            //apply friction, gravity, hspeed/vspeed:
            vSpeed += PhysicsParams.GRAVITY;
            x += (int)hSpeed;
            y += vSpeed;
            //collision event
            var collisionTypes = ((flags & Bools.FaceScraper) == Bools.FaceScraper)? collisionMap.GetCollisionTypes(x, y, (flags & Bools.FacingRight) == Bools.FacingRight) : collisionMap.GetCollisionTypes(x, y);
            (var currentX, var currentY) = (x,  y);
            int minInstanceNum = 0;
            int collisionIdx = 0;
            while (collisionIdx < collisionTypes.Count)
            {
                switch (collisionTypes[collisionIdx])
                {
                    case CollisionType.Solid:
                        (x, y) = (xPrevious, yPrevious);
                        if (!PlaceFree(x + (int)hSpeed, y, collisionMap, (flags & Bools.FacingRight) == Bools.FacingRight, (flags & Bools.FaceScraper) == Bools.FaceScraper))
                        {
                            int sign = Math.Sign(hSpeed);
                            if (sign != 0)
                            {
                                while (PlaceFree(x + sign, y, collisionMap, (flags & Bools.FacingRight) == Bools.FacingRight, (flags & Bools.FaceScraper) == Bools.FaceScraper))
                                {
                                    x += sign;
                                }
                                hSpeed = 0;
                            }
                        }
                        if (!PlaceFree(x, y + vSpeed, collisionMap, (flags & Bools.FacingRight) == Bools.FacingRight, (flags & Bools.FaceScraper) == Bools.FaceScraper))
                        {
                            int sign = Math.Sign(vSpeed);
                            if (sign != 0)
                            {
                                flags |= sign > 0 ? Bools.CanDJump : Bools.None;
                                while (Math.Abs(vSpeed) >= 1 && PlaceFree(x, y + sign, collisionMap, (flags & Bools.FacingRight) == Bools.FacingRight, (flags & Bools.FaceScraper) == Bools.FaceScraper))
                                {
                                    y += sign;
                                    vSpeed -= sign;
                                }
                                vSpeed = 0;
                            }

                        }
                        if (!PlaceFree(x + (int)hSpeed, y + vSpeed, collisionMap, (flags & Bools.FacingRight) == Bools.FacingRight, (flags & Bools.FaceScraper) == Bools.FaceScraper))
                        {
                            hSpeed = 0;
                        }
                        x += (int)hSpeed;
                        y += vSpeed;
                        if (!PlaceFree(x, y, collisionMap, (flags & Bools.FacingRight) == Bools.FacingRight, (flags & Bools.FaceScraper) == Bools.FaceScraper))
                        {
                            (x, y) = (xPrevious, yPrevious);
                        }

                        break;
                    case CollisionType.Platform:
                        
                        Object? platform = collisionMap.GetCollidingPlatform(x, y, minInstanceNum);
                        if (platform != null)
                        {
                            if (y - vSpeed / 2 <= platform.Y)
                            {
                                y = platform.Y - 9;
                                vSpeed = 0;
                                flags |= Bools.CanDJump;
                                flags |= Bools.OnPlatform;

                            }
                            minInstanceNum = platform.instanceNum + 1;
                        }

                        
                        break;
                    case CollisionType.Water1:
                    case CollisionType.Water3:
                        flags |= Bools.CanDJump;
                        vSpeed = Math.Min(2, vSpeed);
                        break;
                    case CollisionType.Water2:
                    case CollisionType.CatharsisWater:
                        vSpeed = Math.Min(2, vSpeed);
                        break;

                }
                if ((x,y) != (currentX, currentY))
                {
                    //update the collision types we'll check for on this frame
                    var currentCollisionType = collisionTypes[collisionIdx];
                    collisionTypes = ((flags & Bools.FaceScraper) == Bools.FaceScraper) ? collisionMap.GetCollisionTypes(x, y, (flags & Bools.FacingRight) == Bools.FacingRight) : collisionMap.GetCollisionTypes(x, y);
                    CollisionType nextCollisionType = collisionTypes.FirstOrDefault(c => c < currentCollisionType);
                    if (nextCollisionType == CollisionType.None)
                        goto collisionDone;
                    collisionIdx = collisionTypes.IndexOf((CollisionType)nextCollisionType) - 1;
                    (currentX, currentY) = (x, y);
                }
                collisionIdx++;
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