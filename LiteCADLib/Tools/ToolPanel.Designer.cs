namespace LiteCAD.Tools
{
    partial class ToolPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolHeader1 = new ToolHeader();
            this.panel2 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // toolHeader1
            // 
            this.toolHeader1.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolHeader1.Location = new System.Drawing.Point(0, 0);
            this.toolHeader1.Name = "toolHeader1";
            this.toolHeader1.Size = new System.Drawing.Size(155, 23);
            this.toolHeader1.TabIndex = 0;
            this.toolHeader1.Text = "toolHeader1";
            this.toolHeader1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolHeader1.TopColor = System.Drawing.Color.White;
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 23);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(155, 110);
            this.panel2.TabIndex = 1;
            // 
            // ToolPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.toolHeader1);
            this.Name = "ToolPanel";
            this.Size = new System.Drawing.Size(155, 133);
            this.ResumeLayout(false);

        }

        #endregion

        private ToolHeader toolHeader1;
        private System.Windows.Forms.Panel panel2;
    }
}
