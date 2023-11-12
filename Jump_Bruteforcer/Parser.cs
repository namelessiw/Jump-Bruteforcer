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
 
                    ObjectType o = ObjectNames.GetValueOrDefault(Parameters[0].ToLower());
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
            {"object787", ObjectType.Block },
            {"object789", ObjectType.Block },
            {"object793", ObjectType.Block },
            {"object818", ObjectType.Block },
            {"object821", ObjectType.Block },
            {"object823", ObjectType.Block },
            {"object827", ObjectType.Block },
            {"object832", ObjectType.Block },
            {"miniblock", ObjectType.MiniBlock},
            {"damageblock", ObjectType.KillerBlock},
            {"object798", ObjectType.KillerBlock},
            {"apple", ObjectType.Apple},
            {"cherry", ObjectType.Apple},
            {"deliciousfruit", ObjectType.Apple},
            {"object797", ObjectType.Apple},
            {"warp", ObjectType.Warp},
            {"warpstart", ObjectType.Warp},
            {"object784", ObjectType.Warp},
            {"object785", ObjectType.Warp},
            {"object786", ObjectType.Warp},
            {"object788", ObjectType.Warp},
            {"object790", ObjectType.Warp},
            {"object791", ObjectType.Warp},
            {"object792", ObjectType.Warp},
            {"object796", ObjectType.Warp},
            {"object816", ObjectType.Warp},
            {"object817", ObjectType.Warp},
            {"object819", ObjectType.Warp},
            {"object820", ObjectType.Warp},
            {"object822", ObjectType.Warp},
            {"object824", ObjectType.Warp},
            {"object825", ObjectType.Warp},
            {"object826", ObjectType.Warp},
            {"object830", ObjectType.Warp},
            {"object831", ObjectType.Warp},
            {"object833", ObjectType.Warp},
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
            {"object828", ObjectType.Platform},
            {"object795", ObjectType.Platform},
            {"catharsiswater", ObjectType.CatharsisWater},
            {"object802", ObjectType.BlackHole},
            {"object803", ObjectType.UpArrow},
            {"object804", ObjectType.DownArrow},
            {"object805", ObjectType.Cross},
            {"object806", ObjectType.BigApple},
            {"object807", ObjectType.YellowBullet},

        };
    }
}

