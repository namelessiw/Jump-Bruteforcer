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
            this.btnSelectJmap = new System.Windows.Forms.Button();
            this.lblFileName = new System.Windows.Forms.Label();
            this.picJmap = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picJmap)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSelectJmap
            // 
            this.btnSelectJmap.Location = new System.Drawing.Point(12, 12);
            this.btnSelectJmap.Name = "btnSelectJmap";
            this.btnSelectJmap.Size = new System.Drawing.Size(79, 23);
            this.btnSelectJmap.TabIndex = 0;
            this.btnSelectJmap.Text = "Select jmap";
            this.btnSelectJmap.UseVisualStyleBackColor = true;
            this.btnSelectJmap.Click += new System.EventHandler(this.btnSelectJmap_Click);
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Location = new System.Drawing.Point(97, 16);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(0, 15);
            this.lblFileName.TabIndex = 1;
            // 
            // picJmap
            // 
            this.picJmap.Location = new System.Drawing.Point(12, 41);
            this.picJmap.Name = "picJmap";
            this.picJmap.Size = new System.Drawing.Size(800, 608);
            this.picJmap.TabIndex = 2;
            this.picJmap.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(823, 660);
            this.Controls.Add(this.picJmap);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.btnSelectJmap);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.picJmap)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btnSelectJmap;
        private Label lblFileName;
        private PictureBox picJmap;
    }
}