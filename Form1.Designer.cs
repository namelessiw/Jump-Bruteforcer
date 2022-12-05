namespace Jump_Bruteforcer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnNewPlayer = new System.Windows.Forms.Button();
            this.btnAdvance = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtY = new System.Windows.Forms.TextBox();
            this.chkPress = new System.Windows.Forms.CheckBox();
            this.chkRelease = new System.Windows.Forms.CheckBox();
            this.lblY = new System.Windows.Forms.Label();
            this.lblVspeed = new System.Windows.Forms.Label();
            this.lblFrame = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnNewPlayer
            // 
            this.btnNewPlayer.Location = new System.Drawing.Point(118, 27);
            this.btnNewPlayer.Name = "btnNewPlayer";
            this.btnNewPlayer.Size = new System.Drawing.Size(75, 23);
            this.btnNewPlayer.TabIndex = 0;
            this.btnNewPlayer.Text = "New Player";
            this.btnNewPlayer.UseVisualStyleBackColor = true;
            this.btnNewPlayer.Click += new System.EventHandler(this.btnNewPlayer_Click);
            // 
            // btnAdvance
            // 
            this.btnAdvance.Enabled = false;
            this.btnAdvance.Location = new System.Drawing.Point(12, 81);
            this.btnAdvance.Name = "btnAdvance";
            this.btnAdvance.Size = new System.Drawing.Size(75, 23);
            this.btnAdvance.TabIndex = 1;
            this.btnAdvance.Text = "Advance";
            this.btnAdvance.UseVisualStyleBackColor = true;
            this.btnAdvance.Click += new System.EventHandler(this.btnAdvance_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Y";
            // 
            // txtY
            // 
            this.txtY.Location = new System.Drawing.Point(12, 27);
            this.txtY.Name = "txtY";
            this.txtY.Size = new System.Drawing.Size(100, 23);
            this.txtY.TabIndex = 3;
            // 
            // chkPress
            // 
            this.chkPress.AutoSize = true;
            this.chkPress.Location = new System.Drawing.Point(12, 56);
            this.chkPress.Name = "chkPress";
            this.chkPress.Size = new System.Drawing.Size(53, 19);
            this.chkPress.TabIndex = 4;
            this.chkPress.Text = "Press";
            this.chkPress.UseVisualStyleBackColor = true;
            // 
            // chkRelease
            // 
            this.chkRelease.AutoSize = true;
            this.chkRelease.Location = new System.Drawing.Point(71, 56);
            this.chkRelease.Name = "chkRelease";
            this.chkRelease.Size = new System.Drawing.Size(65, 19);
            this.chkRelease.TabIndex = 5;
            this.chkRelease.Text = "Release";
            this.chkRelease.UseVisualStyleBackColor = true;
            // 
            // lblY
            // 
            this.lblY.AutoSize = true;
            this.lblY.Location = new System.Drawing.Point(12, 107);
            this.lblY.Name = "lblY";
            this.lblY.Size = new System.Drawing.Size(17, 15);
            this.lblY.TabIndex = 6;
            this.lblY.Text = "Y:";
            // 
            // lblVspeed
            // 
            this.lblVspeed.AutoSize = true;
            this.lblVspeed.Location = new System.Drawing.Point(12, 122);
            this.lblVspeed.Name = "lblVspeed";
            this.lblVspeed.Size = new System.Drawing.Size(49, 15);
            this.lblVspeed.TabIndex = 7;
            this.lblVspeed.Text = "VSpeed:";
            // 
            // lblFrame
            // 
            this.lblFrame.AutoSize = true;
            this.lblFrame.Location = new System.Drawing.Point(12, 137);
            this.lblFrame.Name = "lblFrame";
            this.lblFrame.Size = new System.Drawing.Size(43, 15);
            this.lblFrame.TabIndex = 8;
            this.lblFrame.Text = "Frame:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lblFrame);
            this.Controls.Add(this.lblVspeed);
            this.Controls.Add(this.lblY);
            this.Controls.Add(this.chkRelease);
            this.Controls.Add(this.chkPress);
            this.Controls.Add(this.txtY);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnAdvance);
            this.Controls.Add(this.btnNewPlayer);
            this.Name = "Form1";
            this.Text = "Jump Bruteforcer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btnNewPlayer;
        private Button btnAdvance;
        private Label label1;
        private TextBox txtY;
        private CheckBox chkPress;
        private CheckBox chkRelease;
        private Label lblY;
        private Label lblVspeed;
        private Label lblFrame;
    }
}