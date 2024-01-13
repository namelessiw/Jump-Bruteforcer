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
            {"rainbow2", ObjectType.Block },
            {"miniblock", ObjectType.MiniBlock},
            {"damageblock", ObjectType.KillerBlock},
            {"apple", ObjectType.Apple},
            {"cherry", ObjectType.Apple},
            {"deliciousfruit", ObjectType.Apple},
            {"jche34", ObjectType.Apple },
            {"hidche90", ObjectType.Apple },
            {"warp", ObjectType.Warp},
            {"warpstart", ObjectType.Warp},
            {"warpnr", ObjectType.Warp},
            {"warpnrend", ObjectType.Warp},
            {"warp_fade", ObjectType.Warp},
            {"warpexnrend", ObjectType.Warp },
            {"warpextbend", ObjectType.Warp },
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
            {"hidd24", ObjectType.SpikeDown },
            {"obrainbowd", ObjectType.SpikeDown },
            {"hidd49", ObjectType.SpikeDown },
            {"hidd74", ObjectType.SpikeDown },
            {"spid92", ObjectType.SpikeDown },
            {"spikel", ObjectType.SpikeLeft},
            {"hidl24_b", ObjectType.SpikeLeft },
            {"obrainbowl", ObjectType.SpikeLeft },
            {"hidl74", ObjectType.SpikeLeft },
            {"hidl90", ObjectType.SpikeLeft },
            {"spil92", ObjectType.SpikeLeft },
            {"spiker", ObjectType.SpikeRight},
            {"hidr24_b", ObjectType.SpikeRight },
            {"hidr24", ObjectType.SpikeRight },
            {"obrainbowr", ObjectType.SpikeRight },
            {"hidr49", ObjectType.SpikeRight },
            {"spir92", ObjectType.SpikeRight },
            {"spikeu", ObjectType.SpikeUp},
            {"hidu24", ObjectType.SpikeUp },
            {"obrainbowu", ObjectType.SpikeUp },
            {"hidu49", ObjectType.SpikeUp },
            {"hidu74", ObjectType.SpikeUp },
            {"hidu90", ObjectType.SpikeUp },
            {"minispikedown", ObjectType.MiniSpikeDown},
            {"obrainbowmid", ObjectType.MiniSpikeDown },
            {"hidmid74", ObjectType.MiniSpikeDown },
            {"minispikeleft", ObjectType.MiniSpikeLeft},
            {"obrainbowmil", ObjectType.MiniSpikeLeft },
            {"minispikeright", ObjectType.MiniSpikeRight},
            {"obrainbowmir", ObjectType.MiniSpikeRight },
            {"minispikeup", ObjectType.MiniSpikeUp},
            {"obrainbowmiu", ObjectType.MiniSpikeUp },
            {"hidmiu49", ObjectType.MiniSpikeUp },
            {"playerkiller", ObjectType.SpikeDown},
            {"walljumpl", ObjectType.VineLeft},
            {"walljumpr", ObjectType.VineRight},
            {"platform", ObjectType.Platform}, // yuuutu platform hitbox is a lot smaller than the regular one, good luck distinguishing here
            {"movingplatform", ObjectType.Platform}, // like this one should have the regular hitbox but not the other one since its just supposed to be an object parent
            {"rainbow", ObjectType.Platform },
            {"catharsiswater", ObjectType.CatharsisWater},
            {"grav_up", ObjectType.GravityArrowUp },
            {"grav_down", ObjectType.GravityArrowDown },
            {"_ue", ObjectType.GravityArrowUp },
            {"_sita", ObjectType.GravityArrowDown },
        };
    }
}

