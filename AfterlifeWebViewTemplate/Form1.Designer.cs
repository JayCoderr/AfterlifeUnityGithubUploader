namespace AfterlifeUnityGitTool
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
            panel4 = new Panel();
            panel6 = new Panel();
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            webView22 = new Microsoft.Web.WebView2.WinForms.WebView2();
            panel8 = new Panel();
            panel10 = new Panel();
            panel11 = new Panel();
            panel12 = new Panel();
            panel13 = new Panel();
            panel14 = new Panel();
            panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            ((System.ComponentModel.ISupportInitialize)webView22).BeginInit();
            panel10.SuspendLayout();
            panel12.SuspendLayout();
            SuspendLayout();
            // 
            // panel4
            // 
            panel4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel4.BackColor = Color.Firebrick;
            panel4.Location = new Point(29, 14);
            panel4.Name = "panel4";
            panel4.Size = new Size(483, 10);
            panel4.TabIndex = 3;
            // 
            // panel6
            // 
            panel6.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel6.BackColor = Color.FromArgb(0, 100, 162);
            panel6.Controls.Add(panel4);
            panel6.Location = new Point(-2, 7);
            panel6.Name = "panel6";
            panel6.Size = new Size(552, 13);
            panel6.TabIndex = 4;
            panel6.MouseDown += panel6_MouseDown;
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            webView21.BackColor = Color.White;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.ForeColor = Color.Black;
            webView21.Location = new Point(2, 18);
            webView21.Name = "webView21";
            webView21.Size = new Size(546, 40);
            webView21.Source = new Uri("https://www.google.com", UriKind.Absolute);
            webView21.TabIndex = 6;
            webView21.ZoomFactor = 1D;
            // 
            // webView22
            // 
            webView22.AllowExternalDrop = true;
            webView22.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webView22.BackColor = Color.White;
            webView22.CreationProperties = null;
            webView22.DefaultBackgroundColor = Color.White;
            webView22.Location = new Point(2, 64);
            webView22.Name = "webView22";
            webView22.Size = new Size(546, 285);
            webView22.Source = new Uri("https://www.google.com", UriKind.Absolute);
            webView22.TabIndex = 5;
            webView22.ZoomFactor = 1D;
            // 
            // panel8
            // 
            panel8.BackColor = Color.FromArgb(0, 100, 200);
            panel8.Dock = DockStyle.Top;
            panel8.Location = new Point(1, 0);
            panel8.Name = "panel8";
            panel8.Size = new Size(548, 1);
            panel8.TabIndex = 2;
            // 
            // panel10
            // 
            panel10.BackColor = Color.FromArgb(0, 100, 200);
            panel10.Controls.Add(panel11);
            panel10.Dock = DockStyle.Left;
            panel10.Location = new Point(0, 0);
            panel10.Name = "panel10";
            panel10.Size = new Size(1, 351);
            panel10.TabIndex = 4;
            // 
            // panel11
            // 
            panel11.BackColor = Color.Red;
            panel11.Location = new Point(680, 0);
            panel11.Name = "panel11";
            panel11.Size = new Size(13, 459);
            panel11.TabIndex = 3;
            // 
            // panel12
            // 
            panel12.BackColor = Color.FromArgb(0, 100, 200);
            panel12.Controls.Add(panel13);
            panel12.Dock = DockStyle.Right;
            panel12.Location = new Point(549, 0);
            panel12.Name = "panel12";
            panel12.Size = new Size(1, 351);
            panel12.TabIndex = 4;
            // 
            // panel13
            // 
            panel13.BackColor = Color.Red;
            panel13.Location = new Point(680, 0);
            panel13.Name = "panel13";
            panel13.Size = new Size(13, 459);
            panel13.TabIndex = 3;
            // 
            // panel14
            // 
            panel14.BackColor = Color.FromArgb(0, 100, 200);
            panel14.Dock = DockStyle.Bottom;
            panel14.Location = new Point(1, 350);
            panel14.Name = "panel14";
            panel14.Size = new Size(548, 1);
            panel14.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(0, 100, 162);
            ClientSize = new Size(550, 351);
            Controls.Add(webView21);
            Controls.Add(panel14);
            Controls.Add(webView22);
            Controls.Add(panel8);
            Controls.Add(panel12);
            Controls.Add(panel10);
            Controls.Add(panel6);
            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = new Size(550, 0);
            Name = "Form1";
            Text = "Form1";
            TransparencyKey = Color.Firebrick;
            MouseDown += Form1_MouseDown;
            panel6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            ((System.ComponentModel.ISupportInitialize)webView22).EndInit();
            panel10.ResumeLayout(false);
            panel12.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Panel panel4;
        private Panel panel6;
        private Panel panel7;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView22;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private Panel panel8;
        private Panel panel10;
        private Panel panel11;
        private Panel panel12;
        private Panel panel13;
        private Panel panel14;
    }
}
