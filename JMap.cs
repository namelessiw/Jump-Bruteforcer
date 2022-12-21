namespace Jump_Bruteforcer
{
    static class JMap
    {
        // https://github.com/patrickgh3/jtool/blob/master/source.gmx/scripts/saveMapName.gml

        public static Map Parse(string Text)
        {

            List<Object> Objects = new List<Object>();
            Map Map = new Map(Objects);

            int datalinenum = 5;
            string[] args = Text.Split('\n')[datalinenum - 1].Trim().Split(' ');

            for (int i = 0; i < args.Length; i+=3)
            {
                
                (double x, double y, int objectid) = (double.Parse(args[i]), double.Parse(args[i + 1]), int.Parse(args[i+2]));

                ObjectType o = Enum.IsDefined(typeof(ObjectType), objectid) ? (ObjectType)objectid : ObjectType.Unknown;
                Map.AddObject(new(x, y, o));
            }

            return Map;
        }

        

        

    
    }
}
