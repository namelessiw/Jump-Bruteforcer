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

        public static string GetMacro(List<Input> inputs, bool scraperOn)
        {
            if (inputs.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();

            Input Direction = Input.Neutral, NextDirection;

            foreach (Input input in inputs)
            {
                if ((input & Input.Facescraper) == Input.Facescraper)
                {

                    string MenuDirection = scraperOn ? "LeftArrow": "RightArrow";

                    // menu movement left/right dependant on current hitbox, go left to regular, go right to facescraper, full left macro:
                    // Escape(PR)>LeftShift(R)>>>>>>>>>>>>>>>>>>>>>>LeftShift(PR)>LeftArrow(P)>LeftArrow(R),Escape(PR)>>>>>>>>>>>>>>>>>>>>>>>
                    sb.Append("Escape(PR)>>>>>>>>>>>>>>>>>>>>>>>LeftShift(RP)>" + MenuDirection + "(RP)>.,Escape(PR)>>>>>>>>>>>>>>>>>>>>>>>");

                    scraperOn = !scraperOn;
                    continue;
                }

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
                        sb.Append((InputChanged ? "," : "") + (NextDirection == Input.Right ? "RightArrow" : "LeftArrow") + "(PRP)");
                    }

                    InputChanged = true;
                }
                else if (Direction != Input.Neutral)
                {
                    sb.Append((Direction == Input.Right ? "RightArrow" : "LeftArrow") + "(RP)");
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