namespace Jump_Bruteforcer
{
    static class JMap
    {
        // https://github.com/patrickgh3/jtool/blob/master/source.gmx/scripts/saveMapName.gml

        private const string BASE32STRING = "0123456789abcdefghijklmnopqrstuv";

        static JMap()
        {
            foreach (KeyValuePair<long, ObjectType> Entry in IDToType)
            {
                TypeToID[Entry.Value] = Entry.Key;
            }
        }

        public static bool CheckFormat(string Text)
        {
            try
            {
                return Text.Split('\n')[0].Split('|')[0] == "jtool";
            }
            catch
            {
                return false;
            }
        }

        public static Map Parse(string Text)
        {
            List<Object> Objects = new();
            Map Map = new(Objects);

            string[] args = Text.Split('\n')[0].Split('|');
            if (args[0] != "jtool")
                throw new Exception("Map is not a valid JMap");

            for (int i = 2; i < args.Length; i++)
            {
                string[] temp = args[i].Split(':');
                string property = temp[0];
                string value = temp[1];
                switch (property)
                {
                    case "px":
                    case "py":

                        long result = Base32StringToLong(value);

                        Map.Properties.Add(property, BitConverter.Int64BitsToDouble(result).ToString());

                        break;

                    case "objects":

                        int length = value.Length;
                        long y = 0;
                        for (int j = 0; j < length; j += 3)
                        {
                            char c = value[j];
                            if (c == '-')
                            {
                                y = Base32StringToLong(value.Substring(j + 1, 2));
                            }
                            else
                            {
                                long objectid = Base32StringToLong(value.Substring(j, 1));
                                if (objectid != -1)
                                {
                                    long x = Base32StringToLong(value.Substring(j + 1, 2));
                                    Object o = new(x - 128, y - 128, IDToType.ContainsKey(objectid) ? IDToType[objectid] : ObjectType.Unknown);
                                    Map.AddObject(o);
                                }
                            }
                        }

                        break;

                    default:
                        if (Map.Properties.ContainsKey(property))
                        {
                            Console.WriteLine($"Duplicate property {property} (values: {Map.Properties[property]}, {value}); Keeping first value");
                        }
                        else
                        {
                            Map.Properties.Add(property, value);
                        }
                        break;
                }
            }

            return Map;
        }

        private static long Base32StringToLong(string value)
        {
            long result = 0;
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                char c = value[i];
                long charvalue = BASE32STRING.IndexOf(c);
                long placevalue = (long)Math.Pow(32, length - 1 - i);
                result += charvalue * placevalue;
            }

            return result;
        }

        private static Dictionary<long, ObjectType> IDToType = new()
        {
            {1, ObjectType.Block},
            {2, ObjectType.MiniBlock},
            {3, ObjectType.SpikeUp},
            {4, ObjectType.SpikeRight},
            {5, ObjectType.SpikeLeft},
            {6, ObjectType.SpikeDown},
            {7, ObjectType.MiniSpikeUp},
            {8, ObjectType.MiniSpikeRight},
            {9, ObjectType.MiniSpikeLeft},
            {10, ObjectType.MiniSpikeDown},
            {11, ObjectType.Apple},
            {12, ObjectType.Save},
            {13, ObjectType.Platform},
            {14, ObjectType.Water1},
            {15, ObjectType.Water2},
            {16, ObjectType.VineRight},
            {17, ObjectType.VineLeft},
            {18, ObjectType.KillerBlock},
            {19, ObjectType.BulletBlocker},
            {20, ObjectType.PlayerStart},
            {21, ObjectType.Warp},
            {22, ObjectType.JumpRefresher},
            {23, ObjectType.Water3},
            {24, ObjectType.GravityArrowUp},
            {25, ObjectType.GravityArrowDown},
            {26, ObjectType.SaveUpsideDown},
        };

        private static Dictionary<ObjectType, long> TypeToID = new();
    }
}
