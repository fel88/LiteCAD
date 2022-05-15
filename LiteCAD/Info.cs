using LiteCAD.Common;
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
    public partial class Info : Form
    {
        public Info()
        {
            InitializeComponent();
        }

        IEconomicsDetail detail;
        public void Init(IEconomicsDetail detail)
        {
            if (detail is IDrawable dd)
            {
                Text += ": " + dd.Name;
            }
            this.detail = detail;
            label1.Text = "Total cut length: " + Math.Round(detail.TotalCutLength, 4) + $" mm ({detail.TotalCutLength / 100m:N2} m)";
            label2.Text = "Total volume: " + detail.Volume;

            recalc();

        }
        void recalc()
        {
            var price = (detail.TotalCutLength / 100m) * qty * runningMeterCutPrice;
            textBox3.Text = Math.Round(price, 4).ToString();
        }
        decimal runningMeterCutPrice = 10;
        int qty = 1;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                runningMeterCutPrice = decimal.Parse(textBox1.Text);
                recalc();
            }
            catch (Exception)
            {

            }
        }

        private void Info_Load(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            try
            {
                qty = int.Parse(textBox4.Text);
                recalc();
            }
            catch (Exception)
            {

            }
        }
    }
}
