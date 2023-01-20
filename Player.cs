using System.Collections.Immutable;
using System.Text;
using System.Windows;
using System.Windows.Media;

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
        public List<int> XPositionHistory { get; }
        Input LastDirection;
        readonly SortedDictionary<int, Input> InputHistory;

        public Player(int x)
        {
            X_position = x;
            LastDirection = Input.Neutral;
            InputHistory = new();
            XPositionHistory= new() {x};
        }

        private Player(int x, Input LastDirection, SortedDictionary<int, Input> Inputs, List<int> XPositionHistory)
        {
            X_position = x;
            this.LastDirection = LastDirection;
            this.InputHistory = new(Inputs);
            this.XPositionHistory= new(XPositionHistory);
            this.XPositionHistory.Add(x);
        }
        public PointCollection GetTrajectory(List<double> vString)
        {
            var query = from Point p in XPositionHistory.AsQueryable().
                        Zip(vString, (x, y) => new Point(x, (int)Math.Round(y)))
                        select p;
            
            return new PointCollection(query);
        }

        public Player MoveLeft(int Frame)
        {
            Player p = new(X_position - 3, LastDirection, InputHistory, XPositionHistory);


            if (p.LastDirection != Input.Left)
            {
                p.InputHistory.Add(Frame, Input.Left);
                p.LastDirection = Input.Left;
            }

            return p;
        }

        public Player MoveRight(int Frame)
        {
            Player p = new(X_position + 3, LastDirection, InputHistory, XPositionHistory);

            if (p.LastDirection != Input.Right)
            {
                p.InputHistory.Add(Frame, Input.Right);
                p.LastDirection = Input.Right;
            }

            return p;
        }

        public void MoveNeutral(int Frame)
        {
            XPositionHistory.Add(X_position);
            if (LastDirection != Input.Neutral)
            {
                InputHistory.Add(Frame, Input.Neutral);
                LastDirection = Input.Neutral;
            }
        }

        public void MergeVStringInputs(SortedDictionary<int, Input> VStringInputs, int Length)
        {
            var query = from kvp in VStringInputs
                        where kvp.Key < Length
                        select kvp;
            foreach (KeyValuePair<int, Input> kvp in query)
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
                int sign = Math.Sign(NewX - CurrentX);
                if (sign != 0)
                {
                    while (!CollisionMap.TryGetValue((CurrentX + sign, CurrentYRounded), out Type) || Type != CollisionType.Solid)
                    {
                        CurrentX += sign;
                    }
                }

            }
            if (CollisionMap.TryGetValue((CurrentX, NewYRounded), out Type) && Type == CollisionType.Solid)
            {
                // (re)rounding everytime because otherwise vfpi would lose its parity
                double VSpeed = NewY - CurrentY;
                int sign = Math.Sign(VSpeed);
                if (sign != 0)
                {
                    int yRounded = sign < 0 ? (int)Math.Round(CurrentY) + sign : (int)Math.Round(CurrentY + sign);
                    while (Math.Abs(VSpeed) >= 1 && (!CollisionMap.TryGetValue((CurrentX, yRounded), out Type) || Type != CollisionType.Solid))
                    {
                        CurrentY += sign;
                        VSpeed -= sign;
                        yRounded = sign < 0 ? (int)Math.Round(CurrentY) + sign : (int)Math.Round(CurrentY + sign);
                    }
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
