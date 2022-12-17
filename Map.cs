using System.Text;

namespace Jump_Bruteforcer
{
    internal class Map
    {
        private List<Object> Objects;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
        private int SkippedObjects = 0;

        public Map(List<Object> Objects)
        {
            this.Objects = Objects;
        }

        public Map(List<Object> Objects, Dictionary<string, string> Properties) : this(Objects)
        {
            this.Properties = Properties;
        }

        public new string ToString()
        {
            StringBuilder sb = new StringBuilder("Properties:\n");

            foreach (KeyValuePair<string, string> p in Properties)
            {
                sb.AppendLine($"\t{p.Key}: {p.Value}");
            }

            sb.AppendLine($"\nObjects ({Objects.Count}):");

            foreach (Object o in Objects)
            {
                sb.AppendLine($"{o.Type}:\n\tx: {o.X}\n\ty: {o.Y}");
                foreach (KeyValuePair<string, string> p in o.Properties)
                {
                    sb.AppendLine($"\t{p.Key}: {p.Value}");
                }
                sb.AppendLine();
            }

            sb.AppendLine($"Skipped objects: {SkippedObjects}\n");

            return sb.ToString();
        }

        public void AddObject(double X, double Y, ObjectType Type)
        {
            if (Type == ObjectType.Unknown)
            {
                SkippedObjects++;
                return;
            }
            Objects.Add(new Object(X, Y, Type));
        }

        public void AddObject(Object Object)
        {
            if (Object.Type == ObjectType.Unknown)
            {
                SkippedObjects++;
                return;
            }
            Objects.Add(Object);
        }

        private static readonly (ObjectType, string)[] Filenames = new (ObjectType, string)[]
        {
            (ObjectType.Block, "block"),
            (ObjectType.KillerBlock, "killerblock"),
            (ObjectType.MiniBlock, "miniblock"),
            (ObjectType.MiniSpikeDown, "minidown"),
            (ObjectType.MiniSpikeLeft, "minileft"),
            (ObjectType.MiniSpikeRight, "miniright"),
            (ObjectType.MiniSpikeUp, "miniup"),
            (ObjectType.SpikeDown, "spikedown"),
            (ObjectType.SpikeRight, "spikeright"),
            (ObjectType.SpikeLeft, "spikeleft"),
            (ObjectType.SpikeUp, "spikeup"),
            (ObjectType.Warp, "warp"),
            (ObjectType.Water1, "water1"),
            (ObjectType.Water2, "water2"),
            (ObjectType.Water3, "water3"),
            (ObjectType.Apple, "apple"),
        };

        private static Dictionary<ObjectType, bool[,]> Hitboxes = new Dictionary<ObjectType, bool[,]>();

        private void InitHitboxes()
        {
            // generate dictionary of hitboxes using images
            if (Hitboxes.Count != 0)
            {
                return;
            }

            foreach ((ObjectType Type, string Filename) in Filenames)
            {
                Hitboxes.Add(Type, GetHitbox(Filename));
            }


        }

        private bool[,] GetHitbox(string Filename)
        {
            string Path = @"images\";
            Bitmap b = new Bitmap($"{Path}{Filename}.png");
            bool[,] Hitbox = new bool[b.Width, b.Height];

            for (int x = 0; x < b.Width; x++)
            {
                for (int y = 0; y < b.Height; y++)
                {
                    Hitbox[x, y] = b.GetPixel(x, y).A != 0;
                }
            }

            return Hitbox;
        }

        public Dictionary<(int X, int Y), CollisionType> GetCollisionMap()
        {
            InitHitboxes();

            Dictionary<(int X, int Y), CollisionType> CollisionMap = new Dictionary<(int X, int Y), CollisionType>();

            bool[,] PlayerHitbox = GetHitbox("player");

            int max_x = PlayerHitbox.GetLength(0),
                min_x = -max_x,
                max_y = PlayerHitbox.GetLength(1),
                min_y = -max_y;

            foreach (Object o in Objects)
            {
                // skip if not in dict
                if (!Hitboxes.ContainsKey(o.Type))
                {
                    Console.WriteLine($"No hitbox found for object type {o.Type}");
                    continue;
                }

                // base coordinates are relative to the player
                int base_x = (int)Math.Round(o.X) + 5, base_y = (int)Math.Round(o.Y) + 12;
                bool[,] ObjectHitbox = Hitboxes[o.Type];

                CollisionType collision_type = CollisionType.Killer;

                switch (o.Type)
                {
                    case ObjectType.Apple:
                        base_x -= 10;
                        base_y -= 12;
                        break;
                    case ObjectType.Block:
                    case ObjectType.MiniBlock:
                        collision_type = CollisionType.Solid;
                        break;
                    case ObjectType.Warp:
                        collision_type = CollisionType.Warp;
                        break;
                    case ObjectType.Water1:
                        collision_type = CollisionType.Water1;
                        break;
                    case ObjectType.Water2:
                        collision_type = CollisionType.Water2;
                        break;
                    case ObjectType.Water3:
                        collision_type = CollisionType.Water3;
                        break;
                }

                for (int p_x = min_x; p_x < ObjectHitbox.GetLength(0) + max_x; p_x++)
                {
                    for (int p_y = min_y; p_y < ObjectHitbox.GetLength(0) + max_y; p_y++)
                    {
                        (int x, int y) Position = (p_x + base_x, p_y + base_y);
                        if (Collision(p_x, p_y, PlayerHitbox, ObjectHitbox))
                        {
                            if (!CollisionMap.ContainsKey(Position))
                            {
                                CollisionMap.Add(Position, collision_type);
                            }
                            else
                            {
                                CollisionType ct = CollisionMap[Position];

                                CollisionMap[Position] = Object.GetHigherCollisionPriority(collision_type, ct);
                            }
                        }
                    }
                }
            }

            return CollisionMap;
        }

        public static Bitmap GenerateImage(Dictionary<(int X, int Y), CollisionType> CollisionMap)
        {
            int width = 800, height = 608;
            Bitmap b = new Bitmap(width, height);

            for (int y = 0; y < 608; y++)
            {
                for (int x = 0; x < 800; x++)
                {
                    Color c = Color.Gray;
                    if (CollisionMap.ContainsKey((x, y)))
                    {
                        switch (CollisionMap[(x, y)])
                        {
                            case CollisionType.Killer:
                                c = Color.FromArgb(128, 24, 24);
                                break;
                            case CollisionType.Solid:
                                c = Color.Black;
                                break;
                            case CollisionType.Warp:
                                c = Color.LawnGreen;
                                break;
                            case CollisionType.Water1:
                                c = Color.DarkBlue;
                                break;
                            case CollisionType.Water2:
                                c = Color.LightBlue;
                                break;
                            case CollisionType.Water3:
                                c = Color.Blue;
                                break;
                        }
                    }
                    b.SetPixel(x, y, c);
                }
            }

            return b;
        }

        private bool Collision(int Relative_X, int Relative_Y, bool[,] PlayerHitbox, bool[,] ObjectHitbox)
        {
            for (int hitbox_x = 0; hitbox_x < PlayerHitbox.GetLength(0); hitbox_x++)
            {
                for (int hitbox_y = 0; hitbox_y < PlayerHitbox.GetLength(1); hitbox_y++)
                {
                    if (!PlayerHitbox[hitbox_x, hitbox_y])
                    {
                        continue;
                    }

                    int x = Relative_X + hitbox_x;
                    int y = Relative_Y + hitbox_y;

                    // bounds check
                    if (x < 0 || y < 0 || x >= ObjectHitbox.GetLength(0) || y >= ObjectHitbox.GetLength(1))
                    {
                        continue;
                    }

                    if (ObjectHitbox[x, y])
                        return true;
                }
            }
            return false;
        }
    }
}
