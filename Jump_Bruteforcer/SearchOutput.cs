﻿using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Jump_Bruteforcer
{
    internal static class SearchOutput
    {
        /// <summary>
        /// For a given PlayerNode, returns the inputs to get there and the path taken through the game space
        /// For a given PlayerNode, writes to a file the states of all nodes on the path through the game space ending at the current node
        /// </summary>
        /// <returns>a tuple containing the list of inputs and a PointCollection representing the path</returns>
        public static (List<Input> Inputs, PointCollection Points) GetPath(PlayerNode root, int endNode, List<int> nodeParentIndices, List<Input> nodeInputs, CollisionMap collisionMap)
        {
            List<Input> inputs = new List<Input>();
            List<Point> points = new List<Point>();
            List<PlayerNode> path = new List<PlayerNode>();
            int currentNodeIndex = endNode;
            Input currentInput;


            while (currentNodeIndex != 0)
            {
                currentInput = nodeInputs[currentNodeIndex];
                inputs.Add(currentInput);
                currentNodeIndex = nodeParentIndices[currentNodeIndex];
            }
            inputs.Reverse();
            PlayerNode curr = root;
            points.Add(new Point(curr.State.X, curr.State.RoundedY));
            path.Add(curr);
            foreach (Input input in inputs)
            {
                curr = curr.NewState(input, collisionMap);
                points.Add(new Point(curr.State.X, curr.State.RoundedY));
                path.Add(curr);
            }
            string states = string.Join<PlayerNode>("\n", path.ToArray());
            string outputPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jump Bruteforcer macros");
            Directory.CreateDirectory(outputPath);
            File.WriteAllText(Path.Join(outputPath, $"states.txt"), states);
            return (inputs, new PointCollection(points));
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