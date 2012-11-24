namespace testparser
{
    partial class frmtest
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtinput = new System.Windows.Forms.TextBox();
            this.btnexec = new System.Windows.Forms.Button();
            this.txtresults = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtinput
            // 
            this.txtinput.Location = new System.Drawing.Point(12, 8);
            this.txtinput.Name = "txtinput";
            this.txtinput.Size = new System.Drawing.Size(287, 20);
            this.txtinput.TabIndex = 0;
            this.txtinput.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // btnexec
            // 
            this.btnexec.Location = new System.Drawing.Point(307, 4);
            this.btnexec.Name = "btnexec";
            this.btnexec.Size = new System.Drawing.Size(72, 23);
            this.btnexec.TabIndex = 2;
            this.btnexec.Text = "Exec";
            this.btnexec.UseVisualStyleBackColor = true;
            this.btnexec.Click += new System.EventHandler(this.btnexec_Click);
            // 
            // txtresults
            // 
            this.txtresults.Location = new System.Drawing.Point(11, 41);
            this.txtresults.Name = "txtresults";
            this.txtresults.Size = new System.Drawing.Size(367, 236);
            this.txtresults.TabIndex = 3;
            this.txtresults.Text = "";
            this.txtresults.WordWrap = false;
            this.txtresults.TextChanged += new System.EventHandler(this.txtresults_TextChanged);
            // 
            // frmtest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 281);
            this.Controls.Add(this.txtresults);
            this.Controls.Add(this.btnexec);
            this.Controls.Add(this.txtinput);
            this.Name = "frmtest";
            this.Text = "BASeParser.NET tester";
            this.Load += new System.EventHandler(this.frmtest_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtinput;
        private System.Windows.Forms.Button btnexec;
        private System.Windows.Forms.RichTextBox txtresults;
    }
}

