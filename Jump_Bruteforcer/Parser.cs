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
            {"object791", ObjectType.Block},
            {"object792", ObjectType.Block},
            {"object800", ObjectType.Block},
            {"object802", ObjectType.Block},
            {"object804", ObjectType.Block},
            {"object806", ObjectType.Block},
            {"object812", ObjectType.Block},
            {"object815", ObjectType.Block},
            {"object817", ObjectType.Block},
            {"object818", ObjectType.Block},
            {"object824", ObjectType.Block},
            {"object827", ObjectType.Block},
            {"object829", ObjectType.Block},
            {"object830", ObjectType.Block},
            {"object836", ObjectType.Block},
            {"object839", ObjectType.Block},
            {"object841", ObjectType.Block},
            {"object842", ObjectType.Block},
            {"object848", ObjectType.Block},
            {"object851", ObjectType.Block},
            {"object853", ObjectType.Block},
            {"object855", ObjectType.Block},
            {"object858", ObjectType.Block},
            {"object860", ObjectType.Block},
            {"object861", ObjectType.Block},
            {"object894", ObjectType.Block},
            {"object915", ObjectType.Block},
            {"object922", ObjectType.Block},
            {"object943", ObjectType.Block},
            {"object1015", ObjectType.Block},
            {"object1026", ObjectType.Block},
            {"object1040", ObjectType.Block},
            {"object1057", ObjectType.Block},
            {"miniblock", ObjectType.MiniBlock},
            {"damageblock", ObjectType.KillerBlock},
            {"object903", ObjectType.KillerBlock},
            {"apple", ObjectType.Apple},
            {"cherry", ObjectType.Apple},
            {"deliciousfruit", ObjectType.Apple},
            {"warp", ObjectType.Warp},
            {"warpstart", ObjectType.Warp},
            {"object789", ObjectType.Warp},
            {"object790", ObjectType.Warp},
            {"object797", ObjectType.Warp},
            {"object798", ObjectType.Warp},
            {"object799", ObjectType.Warp},
            {"object801", ObjectType.Warp},
            {"object803", ObjectType.Warp},
            {"object811", ObjectType.Warp},
            {"object813", ObjectType.Warp},
            {"object814", ObjectType.Warp},
            {"object816", ObjectType.Warp},
            {"object823", ObjectType.Warp},
            {"object825", ObjectType.Warp},
            {"object826", ObjectType.Warp},
            {"object828", ObjectType.Warp},
            {"object835", ObjectType.Warp},
            {"object837", ObjectType.Warp},
            {"object838", ObjectType.Warp},
            {"object840", ObjectType.Warp},
            {"object847", ObjectType.Warp},
            {"object849", ObjectType.Warp},
            {"object850", ObjectType.Warp},
            {"object852", ObjectType.Warp},
            {"object854", ObjectType.Warp},
            {"object856", ObjectType.Warp},
            {"object857", ObjectType.Warp},
            {"object859", ObjectType.Warp},
            {"object866", ObjectType.Warp},
            {"object867", ObjectType.Warp},
            {"object888", ObjectType.Warp},
            {"object893", ObjectType.Warp},
            {"object899", ObjectType.Warp},
            {"object900", ObjectType.Warp},
            {"object901", ObjectType.Warp},
            {"object911", ObjectType.Warp},
            {"object912", ObjectType.Warp},
            {"object920", ObjectType.Warp},
            {"object921", ObjectType.Warp},
            {"object923", ObjectType.Warp},
            {"object925", ObjectType.Warp},
            {"object927", ObjectType.Warp},
            {"object928", ObjectType.Warp},
            {"object929", ObjectType.Warp},
            {"object930", ObjectType.Warp},
            {"object931", ObjectType.Warp},
            {"object932", ObjectType.Warp},
            {"object933", ObjectType.Warp},
            {"object934", ObjectType.Warp},
            {"object935", ObjectType.Warp},
            {"object936", ObjectType.Warp},
            {"object937", ObjectType.Warp},
            {"object942", ObjectType.Warp},
            {"object945", ObjectType.Warp},
            {"object947", ObjectType.Warp},
            {"object953", ObjectType.Warp},
            {"object954", ObjectType.Warp},
            {"object955", ObjectType.Warp},
            {"object956", ObjectType.Warp},
            {"object957", ObjectType.Warp},
            {"object958", ObjectType.Warp},
            {"object959", ObjectType.Warp},
            {"object960", ObjectType.Warp},
            {"object961", ObjectType.Warp},
            {"object962", ObjectType.Warp},
            {"object963", ObjectType.Warp},
            {"object964", ObjectType.Warp},
            {"object965", ObjectType.Warp},
            {"object966", ObjectType.Warp},
            {"object967", ObjectType.Warp},
            {"object968", ObjectType.Warp},
            {"object969", ObjectType.Warp},
            {"object970", ObjectType.Warp},
            {"object971", ObjectType.Warp},
            {"object972", ObjectType.Warp},
            {"object973", ObjectType.Warp},
            {"object974", ObjectType.Warp},
            {"object975", ObjectType.Warp},
            {"object976", ObjectType.Warp},
            {"object977", ObjectType.Warp},
            {"object978", ObjectType.Warp},
            {"object979", ObjectType.Warp},
            {"object980", ObjectType.Warp},
            {"object981", ObjectType.Warp},
            {"object982", ObjectType.Warp},
            {"object983", ObjectType.Warp},
            {"object984", ObjectType.Warp},
            {"object985", ObjectType.Warp},
            {"object986", ObjectType.Warp},
            {"object987", ObjectType.Warp},
            {"object988", ObjectType.Warp},
            {"object989", ObjectType.Warp},
            {"object990", ObjectType.Warp},
            {"object991", ObjectType.Warp},
            {"object992", ObjectType.Warp},
            {"object993", ObjectType.Warp},
            {"object994", ObjectType.Warp},
            {"object995", ObjectType.Warp},
            {"object996", ObjectType.Warp},
            {"object1010", ObjectType.Warp},
            {"object1013", ObjectType.Warp},
            {"object1014", ObjectType.Warp},
            {"object1016", ObjectType.Warp},
            {"object1017", ObjectType.Warp},
            {"object1019", ObjectType.Warp},
            {"object1020", ObjectType.Warp},
            {"object1021", ObjectType.Warp},
            {"object1024", ObjectType.Warp},
            {"object1031", ObjectType.Warp},
            {"playerstart", ObjectType.PlayerStart},
            {"water", ObjectType.Water1}, // yuuutu
            {"water1", ObjectType.Water1}, // renex
            {"water2", ObjectType.Water2}, // renex
            {"water3", ObjectType.Water3}, // renex
            {"spikedown", ObjectType.SpikeDown},
            {"object795", ObjectType.SpikeDown},
            {"object809", ObjectType.SpikeDown},
            {"object820", ObjectType.SpikeDown},
            {"object832", ObjectType.SpikeDown},
            {"object844", ObjectType.SpikeDown},
            {"object864", ObjectType.SpikeDown},
            {"object870", ObjectType.SpikeDown},
            {"object874", ObjectType.SpikeDown},
            {"object878", ObjectType.SpikeDown},
            {"object882", ObjectType.SpikeDown},
            {"object886", ObjectType.SpikeDown},
            {"object891", ObjectType.SpikeDown},
            {"object897", ObjectType.SpikeDown},
            {"object918", ObjectType.SpikeDown},
            {"object940", ObjectType.SpikeDown},
            {"object951", ObjectType.SpikeDown},
            {"object999", ObjectType.SpikeDown},
            {"object1004", ObjectType.SpikeDown},
            {"object1008", ObjectType.SpikeDown},
            {"object1029", ObjectType.SpikeDown},
            {"object1034", ObjectType.SpikeDown},
            {"object1038", ObjectType.SpikeDown},
            {"object1044", ObjectType.SpikeDown},
            {"object1051", ObjectType.SpikeDown},
            {"object1055", ObjectType.SpikeDown},
            {"object1060", ObjectType.SpikeDown},
            {"spikeleft", ObjectType.SpikeLeft},
            {"object796", ObjectType.SpikeDown},
            {"object810", ObjectType.SpikeDown},
            {"object822", ObjectType.SpikeDown},
            {"object834", ObjectType.SpikeDown},
            {"object846", ObjectType.SpikeDown},
            {"object865", ObjectType.SpikeDown},
            {"object871", ObjectType.SpikeDown},
            {"object875", ObjectType.SpikeDown},
            {"object879", ObjectType.SpikeDown},
            {"object883", ObjectType.SpikeDown},
            {"object887", ObjectType.SpikeDown},
            {"object892", ObjectType.SpikeDown},
            {"object898", ObjectType.SpikeDown},
            {"object909", ObjectType.SpikeDown},
            {"object919", ObjectType.SpikeDown},
            {"object941", ObjectType.SpikeDown},
            {"object952", ObjectType.SpikeDown},
            {"object1000", ObjectType.SpikeDown},
            {"object1005", ObjectType.SpikeDown},
            {"object1009", ObjectType.SpikeDown},
            {"object1030", ObjectType.SpikeDown},
            {"object1035", ObjectType.SpikeDown},
            {"object1039", ObjectType.SpikeDown},
            {"object1042", ObjectType.SpikeDown},
            {"object1052", ObjectType.SpikeDown},
            {"object1056", ObjectType.SpikeDown},
            {"object1061", ObjectType.SpikeDown},
            {"spikeright", ObjectType.SpikeRight},
            {"object794", ObjectType.SpikeRight},
            {"object808", ObjectType.SpikeRight},
            {"object821", ObjectType.SpikeRight},
            {"object833", ObjectType.SpikeRight},
            {"object845", ObjectType.SpikeRight},
            {"object863", ObjectType.SpikeRight},
            {"object869", ObjectType.SpikeRight},
            {"object873", ObjectType.SpikeRight},
            {"object877", ObjectType.SpikeRight},
            {"object881", ObjectType.SpikeRight},
            {"object885", ObjectType.SpikeRight},
            {"object890", ObjectType.SpikeRight},
            {"object896", ObjectType.SpikeRight},
            {"object908", ObjectType.SpikeRight},
            {"object917", ObjectType.SpikeRight},
            {"object939", ObjectType.SpikeRight},
            {"object950", ObjectType.SpikeRight},
            {"object998", ObjectType.SpikeRight},
            {"object1003", ObjectType.SpikeRight},
            {"object1007", ObjectType.SpikeRight},
            {"object1028", ObjectType.SpikeRight},
            {"object1033", ObjectType.SpikeRight},
            {"object1037", ObjectType.SpikeRight},
            {"object1043", ObjectType.SpikeRight},
            {"object1050", ObjectType.SpikeRight},
            {"object1054", ObjectType.SpikeRight},
            {"object1059", ObjectType.SpikeRight},
            {"spikeup", ObjectType.SpikeUp},
            {"object793", ObjectType.SpikeUp},
            {"object807", ObjectType.SpikeUp},
            {"object819", ObjectType.SpikeUp},
            {"object831", ObjectType.SpikeUp},
            {"object843", ObjectType.SpikeUp},
            {"object862", ObjectType.SpikeUp},
            {"object868", ObjectType.SpikeUp},
            {"object872", ObjectType.SpikeUp},
            {"object876", ObjectType.SpikeUp},
            {"object880", ObjectType.SpikeUp},
            {"object884", ObjectType.SpikeUp},
            {"object889", ObjectType.SpikeUp},
            {"object895", ObjectType.SpikeUp},
            {"object916", ObjectType.SpikeUp},
            {"object938", ObjectType.SpikeUp},
            {"object948", ObjectType.SpikeUp},
            {"object997", ObjectType.SpikeUp},
            {"object1002", ObjectType.SpikeUp},
            {"object1006", ObjectType.SpikeUp},
            {"object1027", ObjectType.SpikeUp},
            {"object1032", ObjectType.SpikeUp},
            {"object1036", ObjectType.SpikeUp},
            {"object1041", ObjectType.SpikeUp},
            {"object1049", ObjectType.SpikeUp},
            {"object1053", ObjectType.SpikeUp},
            {"object1058", ObjectType.SpikeUp},
            {"spiked", ObjectType.SpikeDown},
            {"spikel", ObjectType.SpikeLeft},
            {"spiker", ObjectType.SpikeRight},
            {"spikeu", ObjectType.SpikeUp},
            {"minispikedown", ObjectType.MiniSpikeDown},
            {"object1048", ObjectType.MiniSpikeDown},
            {"minispikeleft", ObjectType.MiniSpikeLeft},
            {"object1046", ObjectType.MiniSpikeLeft},
            {"minispikeright", ObjectType.MiniSpikeRight},
            {"object1047", ObjectType.MiniSpikeRight},
            {"minispikeup", ObjectType.MiniSpikeUp},
            {"object1045", ObjectType.MiniSpikeUp},
            {"playerkiller", ObjectType.SpikeDown},
            {"walljumpl", ObjectType.VineLeft},
            {"walljumpr", ObjectType.VineRight},
            {"platform", ObjectType.Platform}, // yuuutu platform hitbox is a lot smaller than the regular one, good luck distinguishing here
            {"movingplatform", ObjectType.Platform}, // like this one should have the regular hitbox but not the other one since its just supposed to be an object parent
            {"catharsiswater", ObjectType.CatharsisWater},
        };
    }
}

