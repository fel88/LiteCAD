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
            textBox1.Text = LastOffset.ToString();
            textBox2.Text = LastCurveTolerance.ToString();
            StartPosition = FormStartPosition.CenterParent;
            var vls = Enum.GetValues(typeof(JoinType));
            foreach (var item in vls)
            {
                comboBox1.Items.Add(new ComboBoxItem() { Tag = item, Name = item.ToString() });
            }
            comboBox1.SelectedIndex = Array.IndexOf(vls, JoinType.jtRound);
        }

        public double Offset;
        static double LastCurveTolerance = 0.25;
        public double CurveTolerance;
        public JoinType JoinType = JoinType.jtRound;

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null) return;
            var t = (ClipperLib.JoinType)(comboBox1.SelectedItem as ComboBoxItem).Tag;
            JoinType = t;
        }
        static double LastOffset = 1;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Offset = Helpers.ParseDouble(textBox1.Text);
                LastOffset = Offset;
                textBox1.SetNormalStyle();
            }
            catch (Exception ex)
            {
                textBox1.SetErrorStyle();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                CurveTolerance = Helpers.ParseDouble(textBox2.Text);
                LastCurveTolerance = CurveTolerance;
                textBox2.SetNormalStyle();
            }
            catch (Exception ex)
            {
                textBox2.SetErrorStyle();
            }
        }
    }
}
