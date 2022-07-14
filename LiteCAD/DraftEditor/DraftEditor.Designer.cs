namespace LiteCAD.DraftEditor
{
    partial class DraftEditorControl
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
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.detectCOMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.approxByCircleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.translateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.offsetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.detectCOMToolStripMenuItem,
            this.approxByCircleToolStripMenuItem,
            this.translateToolStripMenuItem,
            this.offsetToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 136);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.deleteToolStripMenuItem.Text = "delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // detectCOMToolStripMenuItem
            // 
            this.detectCOMToolStripMenuItem.Name = "detectCOMToolStripMenuItem";
            this.detectCOMToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.detectCOMToolStripMenuItem.Text = "detect COM";
            this.detectCOMToolStripMenuItem.Click += new System.EventHandler(this.detectCOMToolStripMenuItem_Click);
            // 
            // approxByCircleToolStripMenuItem
            // 
            this.approxByCircleToolStripMenuItem.Name = "approxByCircleToolStripMenuItem";
            this.approxByCircleToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.approxByCircleToolStripMenuItem.Text = "approx by circle";
            this.approxByCircleToolStripMenuItem.Click += new System.EventHandler(this.approxByCircleToolStripMenuItem_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 15;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // translateToolStripMenuItem
            // 
            this.translateToolStripMenuItem.Name = "translateToolStripMenuItem";
            this.translateToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.translateToolStripMenuItem.Text = "translate";
            this.translateToolStripMenuItem.Click += new System.EventHandler(this.translateToolStripMenuItem_Click);
            // 
            // offsetToolStripMenuItem
            // 
            this.offsetToolStripMenuItem.Name = "offsetToolStripMenuItem";
            this.offsetToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.offsetToolStripMenuItem.Text = "offset";
            // 
            // DraftEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "DraftEditorControl";
            this.Size = new System.Drawing.Size(654, 433);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem detectCOMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem approxByCircleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem translateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem offsetToolStripMenuItem;
    }
}
