using ClipperLib;
using LiteCAD.Common;
using LiteCAD.DraftEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiteCAD.Dialogs
{
    public partial class OffsetDialog : Form
    {
        public OffsetDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;
            var vls = Enum.GetValues(typeof(JoinType));
            foreach (var item in vls)
            {
                comboBox1.Items.Add(new ComboBoxItem() { Tag = item, Name = item.ToString() });
            }
            comboBox1.SelectedIndex = Array.IndexOf(vls, JoinType.jtRound);
        }

        public double Offset;
        public JoinType JoinType = JoinType.jtRound;
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null) return;
            var t = (ClipperLib.JoinType)(comboBox1.SelectedItem as ComboBoxItem).Tag;
            JoinType = t;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Offset = Helpers.ParseDouble(textBox1.Text);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
