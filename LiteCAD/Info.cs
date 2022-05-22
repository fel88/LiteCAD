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

        IEconomicsDetail[] details;

        public class TempInfo
        {
            public IEconomicsDetail Detail;
            public int Qty;
            public ProduceOperation Operation { get => Detail.Operation; set => Detail.Operation = value; }

            public decimal GetProductionCost()
            {
                if (Operation == ProduceOperation.LaserCutting)
                {
                    var price = (Detail.TotalCutLength / 1000m) * Qty * ProductionStuff.RunningMeterCutPrice;
                    return price;
                }
                else if (Operation == ProduceOperation.FDMPrinting)
                {
                    var price = (Detail.Volume * 1000000m) * Qty * ProductionStuff.FDMPrintingPrice;
                    return price;
                }
                /*else if (Operation == ProduceOperation.NotProducable)
                {
                    
                }*/
                return 0;
            }
        }
        TempInfo[] infos;
        public void Init(IEconomicsDetail[] details)
        {
            infos = details.GroupBy(z => z).Select(z => new TempInfo() { Detail = z.Key, Qty = z.Count() }).ToArray();
            foreach (var item in infos) //group by source  part
            {
                listView1.Items.Add(new ListViewItem(new string[] { (item.Detail as IDrawable).Name, item.Qty.ToString() })
                {
                    Tag = item
                });
            }

            this.details = details;
            //if (detail.Operation == ProduceOperation.LaserCutting)
            //{
            //    radioButton1.Checked = true;
            //}
            //if (detail is IDrawable dd)
            //{
            //    Text += ": " + dd.Name;
            //}

            //label1.Text = "Total cut length: " + Math.Round(detail.TotalCutLength, 4) + $" mm ({detail.TotalCutLength / 1000m:N2} m)";
            //var vol = detail.Volume;
            //vol *= 1000000m;
            ////label2.Text = "Total volume: " + detail.Volume + " m^2";
            //label2.Text = "Total volume: " + vol + $" cm^3 ({detail.Volume} m^3)";

            //recalc();
            InitGui(infos[0]);

        }

        public void InitGui(TempInfo detail)
        {
            this.detail = detail;
            if (detail.Operation == ProduceOperation.LaserCutting)
            {
                radioButton1.Checked = true;
            }
            if (detail.Operation == ProduceOperation.FDMPrinting)
            {
                radioButton2.Checked = true;
            }
            if (detail.Operation == ProduceOperation.NotProducable)
            {
                radioButton3.Checked = true;
            }
            if (detail is IDrawable dd)
            {
                Text += ": " + dd.Name;
            }

            textBox4.Text = detail.Qty.ToString();

            label1.Text = "Total cut length: " + Math.Round(detail.Detail.TotalCutLength, 4) + $" mm ({detail.Detail.TotalCutLength / 1000m:N2} m)";
            var vol = detail.Detail.Volume;
            vol *= 1000000m;
            //label2.Text = "Total volume: " + detail.Volume + " m^2";
            label2.Text = "Total volume: " + vol + $" cm^3 ({detail.Detail.Volume} m^3)";

            recalc(detail);
        }

        public void Init(IEconomicsDetail detail)
        {
            Init(new[] { detail });
        }

        void recalc(TempInfo t)
        {
            textBox3.Text = Math.Round(t.GetProductionCost(), 4).ToString();
            toolStripStatusLabel1.Text = "Total production cost: " + infos.Sum(z => z.GetProductionCost());

        }

        TempInfo detail;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ProductionStuff.RunningMeterCutPrice = decimal.Parse(textBox1.Text);
                recalc(detail);
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
                detail.Qty = int.Parse(textBox4.Text);
                recalc(detail);
            }
            catch (Exception)
            {

            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ProductionStuff.FDMPrintingPrice = decimal.Parse(textBox5.Text);
                recalc(detail);
            }
            catch (Exception)
            {

            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                detail.Operation = ProduceOperation.LaserCutting;

            recalc(detail);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                detail.Operation = ProduceOperation.FDMPrinting;

            recalc(detail);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV report (*.csv)|*.csv";
            if (sfd.ShowDialog() != DialogResult.OK) return;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            var ec = listView1.SelectedItems[0].Tag as TempInfo;
            InitGui(ec);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
                detail.Operation = ProduceOperation.NotProducable;

            recalc(detail);
        }
    }
}
