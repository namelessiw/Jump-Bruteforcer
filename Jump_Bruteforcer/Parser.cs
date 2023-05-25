using System.Globalization;

namespace Jump_Bruteforcer
{
    public  class Parser
    {
        public static Map Parse(string Text) => Parse(".jmap", Text);

        public static Map Parse(string Extension, string Text)
        {
            return Extension.ToLower() switch
            {
                ".cmap" or ".jmap" => ParseJMap(Text),
                ".txt" => ParseGM82Room(Text),
                _ => throw new UnknownExtensionException ("Unknown Extension \"" + Extension + "\""),
            };
        }

        // yuuutu object names to object type for auto-matching
        // should these get compared against each other as lowercase?
        static readonly (string Name, ObjectType Type)[] ObjectNames = new (string, ObjectType)[]
        {
            ("block", ObjectType.Block),
            ("miniblock", ObjectType.MiniBlock),
            ("damageblock", ObjectType.KillerBlock),
            ("apple", ObjectType.Apple),
            ("cherry", ObjectType.Apple),
            ("objwater", ObjectType.Water1), // yuuutu
            ("objwater2", ObjectType.Water2), // yuuutu
            ("water1", ObjectType.Water1), // renex
            ("water2", ObjectType.Water2), // renex
            ("water3", ObjectType.Water3), // renex
            ("spikeDown", ObjectType.SpikeDown),
            ("spikeLeft", ObjectType.SpikeLeft),
            ("spikeRight", ObjectType.SpikeRight),
            ("spikeUp", ObjectType.SpikeUp),
            ("spikeD", ObjectType.SpikeDown),
            ("spikeL", ObjectType.SpikeLeft),
            ("spikeR", ObjectType.SpikeRight),
            ("spikeU", ObjectType.SpikeUp),
            ("minispikeDown", ObjectType.MiniSpikeDown),
            ("minispikeLeft", ObjectType.MiniSpikeLeft),
            ("minispikeRight", ObjectType.MiniSpikeRight),
            ("minispikeUp", ObjectType.MiniSpikeUp),
            ("playerKiller", ObjectType.SpikeDown),
            ("platform", ObjectType.Platform), // yuuutu platform hitbox is a lot smaller than the regular one, good luck distinguishing here
            ("movingPlatform", ObjectType.Platform), // like this one should have the regular hitbox but not the other one since its just supposed to be an object parent
            ("catharsiswater", ObjectType.CatharsisWater),
        };

        static ObjectType GetObjectType(string ObjectName)
        {
            return ObjectNames.FirstOrDefault<(string Name, ObjectType Type)>(
                (x) => 
                    x.Name.ToLower() == ObjectName.ToLower() || 
                    x.Name.ToLower() == ("obj" + ObjectName).ToLower(),
                ("", ObjectType.Unknown)
            ).Type;
        }

        static Map ParseGM82Room(string Text)
        {
            const int MinParams = 10;
            const NumberStyles Style = NumberStyles.Float;
            List<Object> Objects = new List<Object>();

            static double ParseDouble(string s) => double.Parse(s, Style, CultureInfo.InvariantCulture);

            string[] Lines = Text.Split('\n');
            int ObjectCount = Lines.Length - 1;

            for (int i = 0; i < ObjectCount; i++)
            {
                string[] Parameters = Lines[i].Split(',');
                if (Parameters.Length < MinParams)
                {
                    throw new Exception($"Expected {MinParams} parameters, found {Parameters.Length} (Line {i + 1})");
                }

                string ObjectName = Parameters[0];
                ObjectType Type = GetObjectType(ObjectName);

                if (Type == ObjectType.Unknown)
                {
                    continue;
                }

                int X = (int)Math.Round(ParseDouble(Parameters[1]));
                int Y = (int)Math.Round(ParseDouble(Parameters[2]));
                //uid
                //0
                double XScale = ParseDouble(Parameters[5]);
                double YScale = ParseDouble(Parameters[6]);
                //alpha
                double Angle = ParseDouble(Parameters[8]);
                //savecode



                // ignore for non-representable objects?
                if (XScale % 1 != 0)
                {
                    throw new Exception($"Expected integer image_xscale, found {XScale} for object {ObjectName} (line {i})");
                }
                if (YScale % 1 != 0)
                {
                    throw new Exception($"Expected integer image_yscale, found {YScale} for object {ObjectName} (line {i})");
                }
                if (Angle != 0)
                {
                    throw new Exception($"Expected angle 0, found {Angle}");
                }

                if ((Type == ObjectType.Platform || 
                    Type == ObjectType.SpikeDown ||
                    Type == ObjectType.SpikeLeft ||
                    Type == ObjectType.SpikeRight ||
                    Type == ObjectType.SpikeUp ||
                    Type == ObjectType.MiniSpikeDown ||
                    Type == ObjectType.MiniSpikeLeft ||
                    Type == ObjectType.MiniSpikeRight ||
                    Type == ObjectType.MiniSpikeUp ||
                    Type == ObjectType.Warp ||
                    Type == ObjectType.Apple) && 
                    (YScale != 1 || XScale != 1))
                {
                    throw new Exception($"Scaling not implemented for object type {Type}");
                }



                for (double xs = 0; xs < XScale; xs++)
                {
                    for (double ys = 0; ys < YScale; ys++)
                    {
                        BoundingBox? bbox = null;
                        if (Type == ObjectType.Platform)
                        {
                            bbox = new BoundingBox(X - 5, Y - 10, 42, 36);
                        }

                        Objects.Add(new Object(X + (int)(xs * 32), Y + (int)(ys * 32), Type, bbox, i));


                    }


                }
            }

            return new Map(Objects);
        }

        // https://github.com/patrickgh3/jtool/blob/master/source.gmx/scripts/saveMapName.gml
        static Map ParseJMap(string Text)
        {
            List<Object> objects = new List<Object>();

            int datalinenum = 5;
            string[] args = Text.Split('\n')[datalinenum - 1].Trim().Split(' ');
            int instanceNum = 0;

            for (int i = 0; i < args.Length; i += 3)
            {

                (int x, int y, int objectid) = (int.Parse(args[i]), int.Parse(args[i + 1]), int.Parse(args[i + 2]));

                ObjectType o = Enum.IsDefined(typeof(ObjectType), objectid) ? (ObjectType)objectid : ObjectType.Unknown;
                if (o == ObjectType.Apple)
                {
                    x -= 10;
                    y -= 12;
                }
                BoundingBox? bbox = null;
                if (o == ObjectType.Platform)
                {
                    bbox = new BoundingBox(x - 5, y - 10, 42, 36);
                }
                objects.Add(new(x, y, o, bbox, instanceNum));
                instanceNum++;
            }


            return new Map(objects);
        }
    }

    public class UnknownExtensionException : Exception
    {
        public UnknownExtensionException(string Message) : base(Message)
        {
        }
    }
}
