using System.Text;

namespace Jump_Bruteforcer
{
    [Flags]
    public enum Input
    {
        Neutral = 1,
        Left = 2,
        Right = 4,
        Jump = 8,
        Release = 16
    }

    public class Player
    {
        public int X_position { get; set; }
        Input LastDirection;
        readonly SortedDictionary<int, Input> InputHistory;

        public Player(int x)
        {
            X_position = x;
            LastDirection = Input.Neutral;

            InputHistory = new();
        }

        private Player(int x, Input LastDirection, SortedDictionary<int, Input> Inputs)
        {
            X_position = x;
            this.LastDirection = LastDirection;

            this.InputHistory = new(Inputs);
        }

        public Player MoveLeft(int Frame)
        {
            Player p = new(X_position, LastDirection, InputHistory);

            p.X_position -= 3;

            if (p.LastDirection != Input.Left)
            {
                p.InputHistory.Add(Frame, Input.Left);
                p.LastDirection = Input.Left;
            }

            return p;
        }

        public Player MoveRight(int Frame)
        {
            Player p = new(X_position, LastDirection, InputHistory);

            p.X_position += 3;

            if (p.LastDirection != Input.Right)
            {
                p.InputHistory.Add(Frame, Input.Right);
                p.LastDirection = Input.Right;
            }

            return p;
        }

        public void MoveNeutral(int Frame)
        {
            if (LastDirection != Input.Neutral)
            {
                InputHistory.Add(Frame, Input.Neutral);
                LastDirection = Input.Neutral;
            }
        }

        public void MergeVStringInputs(SortedDictionary<int, Input> VStringInputs, int Length)
        {
            /*foreach ((int Frame, Input Input) Action in VStringInputs)
            {
                if (Action.Frame >= Length)
                {
                    break;
                }

                

                if (InputHistory.ContainsKey(Action.Frame))
                {
                    InputHistory.Add(Action.Frame, Action.Input);
                }
                else
                {
                    InputHistory[i] = (Action.Frame, InputHistory[i].Input | Action.Input);
                }
            }
            */
            var query = from kvp in VStringInputs
                        where kvp.Key < Length
                        select kvp;
            foreach(KeyValuePair<int, Input> kvp in query)
            {
                if (InputHistory.TryGetValue(kvp.Key, out Input input))
                {
                    InputHistory[kvp.Key] = input | kvp.Value;
                }
                else
                {
                    InputHistory[kvp.Key] = kvp.Value;
                }
            }

        }



        public string GetInputString()
        {
            StringBuilder sb = new();
            foreach ((int Frame, Input Input) in InputHistory)
            {
                sb.AppendLine($"({Frame}) {Input}");
            }

            return sb.ToString();
        }

        public static (CollisionType Type, int NewX, double NewY, bool VSpeedReset) CollisionCheck(Dictionary<(int X, int Y), CollisionType> CollisionMap, int CurrentX, int NewX, double CurrentY, double NewY)
        {
            int RoundedNewY = (int)Math.Round(NewY);

            if (CollisionMap.TryGetValue((NewX, RoundedNewY), out CollisionType Type))
            {
                switch (Type)
                {
                    case CollisionType.Killer:
                    case CollisionType.Warp:
                        return (Type, NewX, NewY, false);
                    case CollisionType.Solid:
                        bool VSpeedReset;
                        (NewX, NewY, VSpeedReset) = SolidCollision(CollisionMap, CurrentX, NewX, CurrentY, NewY);

                        if (CollisionMap.TryGetValue((NewX, (int)Math.Round(NewY)), out Type))
                        {
                            return (Type, NewX, NewY, VSpeedReset);
                        }
                        return (CollisionType.None, NewX, NewY, VSpeedReset);

                    default:
                        throw new NotImplementedException($"Collision with type {Type} not implemented");
                }
            }
            return (CollisionType.None, NewX, NewY, false);
        }

        public static (int NewX, double NewY, bool VSpeedReset) SolidCollision(Dictionary<(int X, int Y), CollisionType> CollisionMap, int CurrentX, int NewX, double CurrentY, double NewY)
        {
            int CurrentYRounded = (int)Math.Round(CurrentY);
            int NewYRounded = (int)Math.Round(NewY);
            bool VSpeedReset = false;
            CollisionType Type;

            if (CollisionMap.TryGetValue((NewX, CurrentYRounded), out Type) && Type == CollisionType.Solid)
            {
                if (CurrentX < NewX) // moving right
                {
                    NewX = CurrentX;

                    while (!CollisionMap.TryGetValue((CurrentX + 1, CurrentYRounded), out Type) || Type != CollisionType.Solid)
                    {
                        CurrentX++;
                        NewX++;
                    }
                }
                else if(CurrentX > NewX) // moving left
                {
                    NewX = CurrentX;

                    while (!CollisionMap.TryGetValue((CurrentX - 1, CurrentYRounded), out Type) || Type != CollisionType.Solid)
                    {
                        CurrentX--;
                        NewX--;
                    }
                }
            }
            if (CollisionMap.TryGetValue((CurrentX, NewYRounded), out Type) && Type == CollisionType.Solid)
            {
                // (re)rounding everytime because otherwise vfpi would lose its parity
                double VSpeed = NewY - CurrentY;
                if (VSpeed < 0) // moving up
                {
                    while (VSpeed <= -1 && (!CollisionMap.TryGetValue((CurrentX, (int)Math.Round(CurrentY) - 1), out Type) || Type != CollisionType.Solid))
                    {
                        CurrentY--;
                        VSpeed++;
                    }
                }
                else if (VSpeed > 0) // moving down
                {
                    while (VSpeed >= 1 && (!CollisionMap.TryGetValue((CurrentX, (int)Math.Round(CurrentY + 1)), out Type) || Type != CollisionType.Solid))
                    {
                        CurrentY++;
                        VSpeed--;
                    }

                    // djump = true
                }

                NewYRounded = (int)Math.Round(CurrentY);
                NewY = CurrentY;
                VSpeedReset = true;
            }

            if (CollisionMap.TryGetValue((NewX, NewYRounded), out Type) && Type == CollisionType.Solid)
            {
                NewX = CurrentX;
            }

            return (NewX, NewY, VSpeedReset);
        }
    }
}
