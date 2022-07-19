using System;
using System.Collections.Generic;
using System.Drawing;
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
            return DateTime.Now.ToLongTimeString();
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

            addMessage(l, InfoPanelItemType.Warning);
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
            addMessage(l, InfoPanelItemType.Info);
        }

        void addMessage(ListViewItem l, InfoPanelItemType type)
        {
            Messages.Add(new MessageInfo(l, type));
            listView1.Invoke((Action)(() =>
            {
                listView1.Items.Add(l);
                listView1.EnsureVisible(listView1.Items.Count - 1);
                if (!status) Switch();
            }));
        }

        public void AddError(string info, string auxInfo = null)
        {
            List<string> ss = new List<string>();
            ss.Add(GetDate());


            ss.Add($"{info}");
            var l = new ListViewItem(ss.ToArray())
            {
                BackColor = Color.Red,
                ForeColor = Color.White,
                Tag = auxInfo
            };
            addMessage(l, InfoPanelItemType.Error);

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

        public void Clear()
        {
            Messages.Clear();
            listView1.Items.Clear();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            if(listView1.SelectedItems[0].Tag is string ss)
            {
                MessageBox.Show(ss, ParentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
