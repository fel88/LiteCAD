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
            label1.Text = "Total cut length: " + Math.Round(detail.TotalCutLength, 4) + $" mm ({detail.TotalCutLength / 1000m:N2} m)";
            var vol = detail.Volume;
            vol *= 1000000m;
            //label2.Text = "Total volume: " + detail.Volume + " m^2";
            label2.Text = "Total volume: " + vol + $" cm^3 ({detail.Volume} m^3)";

            recalc();

        }
        void recalc()
        {
            if (radioButton1.Checked)
            {
                var price = (detail.TotalCutLength / 1000m) * qty * runningMeterCutPrice;
                textBox3.Text = Math.Round(price, 4).ToString();
            }
            else
            {
                var price = (detail.Volume * 1000000m) * qty * _3dPrintingPrice;
                textBox3.Text = Math.Round(price, 4).ToString();
            }
        }
        decimal runningMeterCutPrice = 40;
        decimal _3dPrintingPrice = 40;
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

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _3dPrintingPrice = decimal.Parse(textBox5.Text);
                recalc();
            }
            catch (Exception)
            {

            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            recalc();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            recalc();

        }
    }
}
