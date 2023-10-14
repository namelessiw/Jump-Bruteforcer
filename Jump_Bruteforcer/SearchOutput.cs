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

            Input Direction = Input.Neutral, NextDirection;

            foreach (Input input in inputs)
            {
                bool InputChanged = false;

                NextDirection = input & Input.Right | input & Input.Left;

                if (Direction != NextDirection)
                {
                    if (Direction != Input.Neutral)
                    {
                        sb.Append((Direction == Input.Right ? "RightArrow" : "LeftArrow") + "(R)");
                        InputChanged = true;
                    }

                    if (NextDirection != Input.Neutral)
                    {
                        sb.Append((InputChanged ? "," : "") + (NextDirection == Input.Right ? "RightArrow" : "LeftArrow") + "(P)");
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