using System.Globalization;
using System.Text.RegularExpressions;

namespace Jump_Bruteforcer
{
    public class Parser
    {
        public static Map Parse(string Text) => Parse(".jmap", Text);

        public static Map Parse(string Extension, string Text)
        {
            Extension = Extension.ToLower();
            List<Object> objects = new List<Object>();
            if (Extension == ".cmap" || Extension == ".jmap")
            {
                int datalinenum = 5;
                string[] args = Text.Split('\n')[datalinenum - 1].Trim().Split(' ');

                for (int i = 0; i < args.Length; i += 3)
                {
                    (int x, int y, int objectid) = (int.Parse(args[i]), int.Parse(args[i + 1]), int.Parse(args[i + 2]));
                    ObjectType o = Enum.IsDefined(typeof(ObjectType), objectid) ? (ObjectType)objectid : ObjectType.Unknown;
                    objects.Add(new(x, y, o, i / 3));
                }
            }
            else //Extension == ".txt"
            {
                const int MinParams = 10;
                const NumberStyles Style = NumberStyles.Float;

                static double ParseDouble(string s) => double.Parse(s, Style, CultureInfo.InvariantCulture);

                string[] Lines = Text.Split('\n');

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
            }
            return new Map(objects);

        }

        static readonly Dictionary<string, ObjectType> ObjectNames = new()
        {
            {"block", ObjectType.Block },
            {"objRainbow2", ObjectType.Block },
            {"miniblock", ObjectType.MiniBlock},
            {"damageblock", ObjectType.KillerBlock},
            {"apple", ObjectType.Apple},
            {"cherry", ObjectType.Apple},
            {"deliciousfruit", ObjectType.Apple},
            {"objChe34", ObjectType.Apple },
            {"objHidChe90", ObjectType.Apple },
            {"warp", ObjectType.Warp},
            {"warpstart", ObjectType.Warp},
            {"warpNR", ObjectType.Warp},
            {"warpNREnd", ObjectType.Warp},
            {"warp_fade", ObjectType.Warp},
            {"warpEXNREnd", ObjectType.Warp },
            {"warpEXTBEnd", ObjectType.Warp },
            {"playerstart", ObjectType.PlayerStart},
            {"water", ObjectType.Water1}, // yuuutu
            {"water1", ObjectType.Water1}, // renex
            {"water2", ObjectType.Water2}, // renex
            {"water3", ObjectType.CatharsisWater}, // catharsis water
            {"spikedown", ObjectType.SpikeDown},
            {"spikeleft", ObjectType.SpikeLeft},
            {"spikeright", ObjectType.SpikeRight},
            {"spikeup", ObjectType.SpikeUp},
            {"spiked", ObjectType.SpikeDown},
            {"objHidD24", ObjectType.SpikeDown },
            {"obRainbowD", ObjectType.SpikeDown },
            {"objHidD49", ObjectType.SpikeDown },
            {"objHidD74", ObjectType.SpikeDown },
            {"spiD92", ObjectType.SpikeDown },
            {"spikel", ObjectType.SpikeLeft},
            {"objHidL24_b", ObjectType.SpikeLeft },
            {"obRainbowL", ObjectType.SpikeLeft },
            {"objHidL74", ObjectType.SpikeLeft },
            {"objHidL90", ObjectType.SpikeLeft },
            {"spiL92", ObjectType.SpikeLeft },
            {"spiker", ObjectType.SpikeRight},
            {"objHidR24_b", ObjectType.SpikeRight },
            {"objHidR24", ObjectType.SpikeRight },
            {"obRainbowR", ObjectType.SpikeRight },
            {"objHidR49", ObjectType.SpikeRight },
            {"spiR92", ObjectType.SpikeRight },
            {"spikeu", ObjectType.SpikeUp},
            {"objHidU24", ObjectType.SpikeUp },
            {"obRainbowU", ObjectType.SpikeUp },
            {"objHidU49", ObjectType.SpikeUp },
            {"objHidU74", ObjectType.SpikeUp },
            {"objHidU90", ObjectType.SpikeUp },
            {"minispikedown", ObjectType.MiniSpikeDown},
            {"obRainbowMiD", ObjectType.MiniSpikeDown },
            {"objHidMiD74", ObjectType.MiniSpikeDown },
            {"minispikeleft", ObjectType.MiniSpikeLeft},
            {"obRainbowMiL", ObjectType.MiniSpikeLeft },
            {"minispikeright", ObjectType.MiniSpikeRight},
            {"obRainbowMiR", ObjectType.MiniSpikeRight },
            {"minispikeup", ObjectType.MiniSpikeUp},
            {"obRainbowMiU", ObjectType.MiniSpikeUp },
            {"objHidMiU49", ObjectType.MiniSpikeUp },
            {"playerkiller", ObjectType.SpikeDown},
            {"walljumpl", ObjectType.VineLeft},
            {"walljumpr", ObjectType.VineRight},
            {"platform", ObjectType.Platform}, // yuuutu platform hitbox is a lot smaller than the regular one, good luck distinguishing here
            {"movingplatform", ObjectType.Platform}, // like this one should have the regular hitbox but not the other one since its just supposed to be an object parent
            {"objRainbow", ObjectType.Platform },
            {"catharsiswater", ObjectType.CatharsisWater},
            {"grav_up", ObjectType.GravityArrowUp },
            {"grav_down", ObjectType.GravityArrowDown },
            {"_ue", ObjectType.GravityArrowUp },
            {"_sita", ObjectType.GravityArrowDown },
        };
    }
}

