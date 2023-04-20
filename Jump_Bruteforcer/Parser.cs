using System.Globalization;

namespace Jump_Bruteforcer
{
    public static class Parser
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
            ("objwater", ObjectType.Water3),
            ("objwater2", ObjectType.Water2),
            ("spikeDown", ObjectType.SpikeDown),
            ("spikeLeft", ObjectType.SpikeLeft),
            ("spikeRight", ObjectType.SpikeRight),
            ("spikeUp", ObjectType.SpikeUp),
            ("minispikeDown", ObjectType.MiniSpikeDown),
            ("minispikeLeft", ObjectType.MiniSpikeLeft),
            ("minispikeRight", ObjectType.MiniSpikeRight),
            ("minispikeUp", ObjectType.MiniSpikeUp),
            ("playerKiller", ObjectType.SpikeDown),
            ("platform", ObjectType.Platform), // yuuutu platform hitbox is a lot smaller than the regular one, good luck distinguishing here
            ("movingPlatform", ObjectType.Platform), // like this one should have the regular hitbox but not the other one since its just supposed to be an object parent
        };

        static ObjectType GetObjectType(string ObjectName)
        {
            return ObjectNames.FirstOrDefault<(string Name, ObjectType Type)>(
                (x) => x.Name == ObjectName,
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
                    throw new Exception($"Expected integer image_xscale, found {XScale}");
                }
                if (YScale % 1 != 0)
                {
                    throw new Exception($"Expected integer image_yscale, found {XScale}");
                }
                if (Angle != 0)
                {
                    throw new Exception($"Expected angle 0, found {Angle}");
                }

                ObjectType Type = GetObjectType(ObjectName);

                if (Type != ObjectType.Unknown)
                {
                    BoundingBox? bbox = null;
                    if (Type == ObjectType.Platform)
                    {
                        bbox = new BoundingBox(X - 5, Y - 10, 42, 36);
                    }

                    Objects.Add(new Object(X, Y, Type, bbox, i));
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
