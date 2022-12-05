namespace Jump_Bruteforcer
{
    public partial class Form1 : Form
    {
        Player p;
        bool sjump;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnNewPlayer_Click(object sender, EventArgs e)
        {
            if (double.TryParse(txtY.Text, out double Y))
            {
                p = new Player(Y);
                sjump = true;
                btnAdvance.Enabled = true;

                UpdateLabels();
            }
            else
            {
                MessageBox.Show($"Could not parse Y value: {txtY.Text}");
            }
        }

        private void btnAdvance_Click(object sender, EventArgs e)
        {
            if (chkPress.Checked)
            {
                p.Jump(sjump);
                sjump = false;
            }
            p.Advance(chkRelease.Checked);

            UpdateLabels();
        }

        void UpdateLabels()
        {
            lblY.Text = $"Y: {p.Y}";
            lblVspeed.Text = $"VSpeed: {p.VSpeed}";
            lblFrame.Text = $"Frame: {p.Frame}";
        }
    }
}