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
        List<(int Frame, Input Input)> Inputs;

        public Player(int x)
        {
            X_position = x;
            LastDirection = Input.Neutral;

            Inputs = new List<(int Frame, Input Input)>();
        }

        private Player(int x, Input LastDirection, List<(int Frame, Input Input)> Inputs)
        {
            X_position = x;
            this.LastDirection = LastDirection;

            this.Inputs = new List<(int Frame, Input Input)>(Inputs);
        }

        public Player MoveLeft(int Frame)
        {
            Player p = new(X_position, LastDirection, Inputs);

            p.X_position -= 3;

            if (p.LastDirection != Input.Left)
            {
                p.Inputs.Add((Frame, Input.Left));
                p.LastDirection = Input.Left;
            }

            return p;
        }

        public Player MoveRight(int Frame)
        {
            Player p = new(X_position, LastDirection, Inputs);

            p.X_position += 3;

            if (p.LastDirection != Input.Right)
            {
                p.Inputs.Add((Frame, Input.Right));
                p.LastDirection = Input.Right;
            }

            return p;
        }

        public void MoveNeutral(int Frame)
        {
            if (LastDirection != Input.Neutral)
            {
                Inputs.Add((Frame, Input.Neutral));
                LastDirection = Input.Neutral;
            }
        }

        public void MergeVStringInputs(List<(int, Input)> VStringInputs, int Length)
        {
            foreach ((int Frame, Input Input) Action in VStringInputs)
            {
                if (Action.Frame >= Length)
                {
                    break;
                }

                int i = FindInputIndex(Action.Frame);

                if (i == -1)
                {
                    Inputs.Add(Action);
                }
                else
                {
                    Inputs[i] = (Action.Frame, Inputs[i].Input | Action.Input);
                }
            }

            Inputs.Sort(new Comparison<(int Frame, Input Input)>((a, b) => a.Frame - b.Frame));
        }

        private int FindInputIndex(int Frame)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (Inputs[i].Frame == Frame)
                {
                    return i;
                }
            }

            return -1;
        }

        public string GetInputString()
        {
            StringBuilder sb = new();
            foreach ((int Frame, Input Input) in Inputs)
            {
                sb.AppendLine($"({Frame}) {Input}");
            }

            return sb.ToString();
        }

        public (CollisionType Type, int NewX, double NewY, bool VSpeedReset) CollisionCheck(Dictionary<(int X, int Y), CollisionType> CollisionMap, int CurrentX, int NewX, double CurrentY, double NewY)
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
            }else if (CollisionMap.TryGetValue((CurrentX, NewYRounded), out Type) && Type == CollisionType.Solid)
            {
                // (re)rounding everytime because otherwise vfpi would lose its parity
                if (CurrentY > NewY) // moving up
                {
                    NewY = CurrentY;
                    while (!CollisionMap.TryGetValue((CurrentX, (int)Math.Round(CurrentY) - 1), out Type) || Type != CollisionType.Solid)
                    {
                        NewY--;
                        CurrentY--;
                    }
                }
                else if (CurrentY < NewY) // moving down
                {
                    NewY = CurrentY;
                    while (!CollisionMap.TryGetValue((CurrentX, (int)Math.Round(CurrentY) + 1), out Type) || Type != CollisionType.Solid)
                    {
                        NewY++;
                        CurrentY++;
                    }

                    // djump = true
                }

                NewYRounded = (int)Math.Round(NewY);
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
