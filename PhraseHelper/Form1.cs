using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Data.SQLite;
using System.Data.SQLite.Linq;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Data.Linq.Mapping;
using System.Windows.Forms;

namespace PhraseHelper
{
    public partial class Form1 : Form
    {
        private const int ActionHotkeyId = 1;
        private const int ActionHotKeyEscId = 2;
        public Form1()
        {
            InitializeComponent();

            TopMost = true;

            Icon = Properties.Resources.Icon;
            notifyIcon.Icon = Properties.Resources.Icon;
            RegisterHotKey(Handle, ActionHotkeyId, 1, (int)Keys.Oem3);
            RegisterHotKey(Handle, ActionHotKeyEscId, 0, (int)Keys.Escape);

            var conn = new SQLiteConnection("DbLinqProvider=Sqlite;Data Source=phrase_db.db");
            var context = new DataContext(conn);
            listBox1.DataSource = context.GetTable<Phrase>().Select(p => p.Text);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {
                if (m.WParam.ToInt32() == ActionHotkeyId)
                {
                    Show();
                }
                else if (m.WParam.ToInt32() == ActionHotKeyEscId)
                {
                    Hide();
                }
            }
            base.WndProc(ref m);
        }

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

    [Table(Name = "phrase")]
    public class Phrase
    {
        [Column(Name = "code")]
        public int Code { get; set; }
        [Column(Name = "text")]
        public string Text { get; set; }
    }
}
