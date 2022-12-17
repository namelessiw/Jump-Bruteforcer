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

    class Player
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
    }
}
