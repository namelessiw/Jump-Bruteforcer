using System.Security.Cryptography.X509Certificates;

namespace Jump_Bruteforcer
{
    public static class JMap
    {
        // https://github.com/patrickgh3/jtool/blob/master/source.gmx/scripts/saveMapName.gml

        public static Map Parse(string Text)
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
}
