using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiteCAD
{
    public partial class InfoPanel : UserControl
    {
        public InfoPanel()
        {
            InitializeComponent();
        }
        private bool status = true;
        private void button1_Click(object sender, EventArgs e)
        {
            Switch();
        }
        private float olds = 150;
        string GetDate()
        {
            return DateTime.Now.ToShortTimeString();
        }
        public List<MessageInfo> Messages = new List<MessageInfo>();
        public class MessageInfo
        {
            public MessageInfo(ListViewItem l, InfoPanelItemType t)
            {
                Item = l;
                Type = t;
            }

            public ListViewItem Item;
            public InfoPanelItemType Type;
        }
        public enum InfoPanelItemType
        {
            Warning, Error, Info
        }
        public void AddWarning(string info)
        {
            List<string> ss = new List<string>();
            ss.Add(GetDate());

            ss.Add($"{info}");
            var l = new ListViewItem(ss.ToArray())
            {
                BackColor = Color.Yellow,
                ForeColor = Color.Blue
            };

            Messages.Add(new MessageInfo(l, InfoPanelItemType.Error));

            listView1.Invoke((Action)(() =>
            {
                listView1.Items.Add(l);
                if (!status) Switch();
            }));

        }
        public void AddInfo(string info)
        {
            List<string> ss = new List<string>();
            ss.Add(GetDate());

            ss.Add($"{info}");
            var l = new ListViewItem(ss.ToArray())
            {
                BackColor = Color.White,
                ForeColor = Color.Blue
            };
            Messages.Add(new MessageInfo(l, InfoPanelItemType.Info));
            listView1.Invoke((Action)(() =>
            {
                listView1.Items.Add(l);
                if (!status) Switch();
            }));
            

        }
        public void AddError(string info)
        {

            List<string> ss = new List<string>();
            ss.Add(GetDate());


            ss.Add($"{info}");
            var l = new ListViewItem(ss.ToArray())
            {
                BackColor = Color.Red,
                ForeColor = Color.White
            };

            Messages.Add(new MessageInfo(l, InfoPanelItemType.Error));
            listView1.Invoke((Action)(() =>
            {
                listView1.Items.Add(l);
                if (!status) Switch();
            }));

        }
        public void Switch()
        {
            if (status)
            {
                olds = tableLayoutPanel1.RowStyles[1].Height;
                tableLayoutPanel1.RowStyles[1].Height = 0;
                Height = (int)tableLayoutPanel1.RowStyles[0].Height;
            }
            else
            {
                tableLayoutPanel1.RowStyles[1].Height = olds;
                Height = (int)(tableLayoutPanel1.RowStyles[0].Height + olds);

            }
            status = !status;
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Messages.Clear();
            listView1.Items.Clear();
        }
    }
}
