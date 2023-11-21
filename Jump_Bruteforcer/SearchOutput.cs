using System.IO;
using System.Text;

namespace Jump_Bruteforcer
{
    internal static class SearchOutput
    {
        /// <summary>
        /// For a given PlayerNode, writes to a file the states of all nodes on the path through the game space ending at the current node
        /// </summary>
        public static void DumpPath(PlayerNode node)
        {
            List<PlayerNode> path = new List<PlayerNode>();
            PlayerNode? currentNode = node;

            while (currentNode != null)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            path.Reverse();
            string states = string.Join<PlayerNode>("\n", path.ToArray());
            string outputPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jump Bruteforcer macros");
            Directory.CreateDirectory(outputPath);
            File.WriteAllText(Path.Join(outputPath, $"states.txt"), states);

        }
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