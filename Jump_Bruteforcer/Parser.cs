﻿using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Jump_Bruteforcer
{
    public class Parser
    {
        public static Map Parse(string Text) => Parse(".jmap", Text, "");

        static readonly ObjectType[] scalableTypes =
        [
            ObjectType.Block,
            ObjectType.MiniBlock,
            ObjectType.Water1,
            ObjectType.WaterMini,
            ObjectType.Water2,
            ObjectType.Water2Mini,
            ObjectType.Water3,
            ObjectType.Water3Mini,
            ObjectType.CatharsisWater,
            ObjectType.CatharsisWaterMini,
            ObjectType.KillerBlock,
            ObjectType.MiniKillerBlock,

            // dont think these would come into play??
            ObjectType.WaterDisappear,
            ObjectType.WaterDisappearMini,
            ObjectType.GravityBlockDown,
            ObjectType.GravityBlockDownMini,
            ObjectType.GravityBlockUp,
            ObjectType.GravityBlockUpMini,

            // could maybe add platforms?
        ];

        public static Map Parse(string Extension, string Text, string Path)
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
                    objects.Add(new(x, y, 1, 1, o, i / 3));
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
                    string uid = Parameters[3];
                    //0
                    double xScale = ParseDouble(Parameters[5]);
                    double yScale = ParseDouble(Parameters[6]);
                    //alpha
                    //angle
                    bool creation_code = Parameters[9] == "1";

                    if (creation_code)
                    {
                        string FilePath = Path + $"/{uid}.gml";
                        string CreationCode = File.ReadAllText(FilePath);
                        string[] CodeLines = CreationCode.Split([ '\n', ';' ]);

                        foreach (string Code in CodeLines)
                        {
                            // matches image_x/yscale, captures x/y and numeric argument
                            // for example captures "3", "7/5", "6.25", does not capture "1 + 1 / 2" (correctly)
                            // cannot handle assignment to variables either, hopefully wont matter :3
                            Match match = Regex.Match(Code, "image_([xy])scale\\s*=\\s*(\\d+(?:\\.\\d+)?(?:\\s*\\/\\s*\\d+(?:\\.\\d+)?)?)");
                            if (match.Success)
                            {
                                string Direction = match.Groups[1].Value;
                                string Scale = match.Groups[2].Value;

                                string[] Operands = Scale.Split('/', StringSplitOptions.TrimEntries);

                                double scale = 1;
                                if (Operands.Length == 1)
                                {
                                    scale = double.Parse(Operands[0]);
                                }
                                else if (Operands.Length == 2)
                                {
                                    double a = double.Parse(Operands[0]);
                                    double b = double.Parse(Operands[1]);
                                    scale = a / b;
                                }
                                else
                                {
                                    throw new Exception($"expected one or two operands, got {Operands.Length} (in expression: \"{Scale}\")");
                                }

                                if (Direction == "x")
                                {
                                    xScale = scale;
                                }
                                else if (Direction == "y")
                                {
                                    yScale = scale;
                                }
                                else
                                {
                                    throw new Exception($"expected either \"x\" or \"y\", got {Direction} (in expression {Code})");
                                }
                            }
                        }
                    }

                    if (!scalableTypes.Contains(o))
                    {
                        xScale = 1;
                        yScale = 1;
                    }

                    objects.Add(new(x, y, xScale, yScale, o, i));
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

