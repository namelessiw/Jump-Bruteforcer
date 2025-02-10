using System.Globalization;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace Jump_Bruteforcer
{
    public class Parser
    {
        public static Map Parse(string text) => Parse(".jmap", text);

        public static Map Parse(string extension, string text)
        {

            return extension.ToLower() switch
            {
                ".cmap" or ".jmap" => new Map(parseJmap(text)),
                ".txt" => new Map(parseTxt(text)),
                _ => throw new Exception($"Unrecognized map file extension: {extension}")
            };

        }

        private static List<Object> parseTxt(string Text)
        {
            const int MinParams = 10;
            const NumberStyles Style = NumberStyles.Float;

            static double ParseDouble(string s) => double.Parse(s, Style, CultureInfo.InvariantCulture);

            string[] Lines = Text.Split('\n');
            List<Object> objects = new();

            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].Trim() == string.Empty)
                {
                    continue;
                }
                string[] Parameters = Lines[i].Split(',');
                if (Parameters.Length < MinParams)
                {
                    throw new Exception($"Expected {MinParams} parameters, found {Parameters.Length} (Line {i + 1})");
                }
                string name = Regex.Replace(Parameters[0].ToLower(), "^obj", "");
                ObjectType o = ObjectNames.GetValueOrDefault(name);
                int x = (int)Math.Round(ParseDouble(Parameters[1]));
                int y = (int)Math.Round(ParseDouble(Parameters[2]));

                objects.Add(new(x, y, o, i));
            }
            return objects;
        }

        private static List<Object> parseJmap(string Text)
        {
            List <Object> objects = new();
            int datalinenum = 5;
            string[] args = Text.Split('\n')[datalinenum - 1].Trim().Split(' ');

            for (int i = 0; i < args.Length; i += 3)
            {
                (int x, int y, int objectid) = (int.Parse(args[i]), int.Parse(args[i + 1]), int.Parse(args[i + 2]));
                ObjectType o = Enum.IsDefined(typeof(ObjectType), objectid) ? (ObjectType)objectid : ObjectType.Unknown;
                objects.Add(new(x, y, o, i / 3));
            }
            return objects;
        }

        static readonly Dictionary<string, ObjectType> ObjectNames = new()
        {
            {"block", ObjectType.Block },
            {"miniblock", ObjectType.MiniBlock},
            {"damageblock", ObjectType.KillerBlock},
            {"apple", ObjectType.Apple},
            {"cherry", ObjectType.Apple},
            {"deliciousfruit", ObjectType.Apple},
            {"warp", ObjectType.Warp},
            {"warpstart", ObjectType.Warp},
            {"playerstart", ObjectType.PlayerStart},
            {"water", ObjectType.Water1}, // yuuutu
            {"water1", ObjectType.Water1}, // renex
            {"water2", ObjectType.Water2}, // renex
            {"water3", ObjectType.Water3}, // renex
            {"spikedown", ObjectType.SpikeDown},
            {"spikeleft", ObjectType.SpikeLeft},
            {"spikeright", ObjectType.SpikeRight},
            {"spikeup", ObjectType.SpikeUp},
            {"spiked", ObjectType.SpikeDown},
            {"spikel", ObjectType.SpikeLeft},
            {"spiker", ObjectType.SpikeRight},
            {"spikeu", ObjectType.SpikeUp},
            {"minispikedown", ObjectType.MiniSpikeDown},
            {"minispikeleft", ObjectType.MiniSpikeLeft},
            {"minispikeright", ObjectType.MiniSpikeRight},
            {"minispikeup", ObjectType.MiniSpikeUp},
            {"playerkiller", ObjectType.SpikeDown},
            {"walljumpl", ObjectType.VineLeft},
            {"walljumpr", ObjectType.VineRight},
            {"platform", ObjectType.Platform}, // yuuutu platform hitbox is a lot smaller than the regular one, good luck distinguishing here
            {"movingplatform", ObjectType.Platform}, // like this one should have the regular hitbox but not the other one since its just supposed to be an object parent
            {"catharsiswater", ObjectType.CatharsisWater},
            {"grav_up", ObjectType.GravityArrowUp },
            {"grav_down", ObjectType.GravityArrowDown },
            {"_ue", ObjectType.GravityArrowUp },
            {"_sita", ObjectType.GravityArrowDown },
        };
    }
}

