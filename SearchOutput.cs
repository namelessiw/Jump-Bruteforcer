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
            throw new NotImplementedException();
        }
    }
}