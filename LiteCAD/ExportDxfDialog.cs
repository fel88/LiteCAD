using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiteCAD
{
    public partial class ExportDxfDialog : Form
    {
        public ExportDxfDialog()
        {
            InitializeComponent();
        }
        public bool MmUnitEnabled;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            MmUnitEnabled = checkBox1.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
