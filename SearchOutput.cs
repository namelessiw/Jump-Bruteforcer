using System.Text;

namespace Jump_Bruteforcer
{
    internal static class SearchOutput
    {

        public static string GetInputString(List<Input> inputs)
        {
            if (inputs.Count == 0)
                return "Frames: 0";

            StringBuilder sb = new();

            sb.AppendLine($"Frames: {inputs.Count}");

            Input PreviousInput = inputs[0];
            int Count = 1;

            for (int i = 1; i < inputs.Count; i++)
            {
                if (inputs[i] == PreviousInput)
                {
                    Count++;
                }
                else
                {
                    sb.AppendLine($"{PreviousInput}{(Count > 1 ? $" x{Count}" : "")}");
                    PreviousInput = inputs[i];
                    Count = 1;
                }
            }

            sb.AppendLine($"{PreviousInput}{(Count > 1 ? $" x{Count}" : "")}");

            return sb.ToString();
        }

        public static string GetMacro(List<Input> inputs)
        {
            if (inputs.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();

            int Direction = 0, NextDirection;

            foreach (Input input in inputs)
            {
                bool InputChanged = false;

                NextDirection = 0;
                if ((input & Input.Left) == Input.Left)
                {
                    NextDirection = -1;
                }
                else if ((input & Input.Right) == Input.Right)
                {
                    NextDirection = 1;
                }

                if (Direction != NextDirection)
                {
                    if (Direction != 0)
                    {
                        sb.Append((Direction == 1 ? "RightArrow" : "LeftArrow") + "(R)");
                        InputChanged = true;
                    }

                    if (NextDirection != 0)
                    {
                        sb.Append((InputChanged ? "," : "") + (NextDirection == 1 ? "RightArrow" : "LeftArrow") + "(P)");
                    }

                    InputChanged = true;
                }

                if ((input & Input.Jump) == Input.Jump)
                {
                    sb.Append((InputChanged ? "," : "") + "J(PR)");

                    InputChanged = true;
                }
                if ((input & Input.Release) == Input.Release)
                {
                    sb.Append((InputChanged ? "," : "") + "K(PR)");
                }

                Direction = NextDirection;

                sb.Append('>');
            }

            return sb.ToString();
        }
    }
}