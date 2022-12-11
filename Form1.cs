namespace Jump_Bruteforcer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSelectJmap_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new()
            {
                DefaultExt = ".jmap"
            };

            if (o.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string FileName = o.FileName;
            lblFileName.Text = FileName;

            string Text = File.ReadAllText(FileName);

            Map Map = JMap.Parse(Text);

            picJmap.Image = Map.GenerateImage(Map.GetCollisionMap());
        }
    }
}