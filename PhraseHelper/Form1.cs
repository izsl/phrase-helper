using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace PhraseHelper
{
    public partial class Form1 : Form
    {
        private const int ActionHotkeyId = 1;
        public Form1()
        {
            InitializeComponent();

            RegisterHotKey(Handle, ActionHotkeyId, 1, (int)Keys.Oem3);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == ActionHotkeyId)
            {
                TopMost = true;
                MessageBox.Show("Key down.");
            }
            base.WndProc(ref m);
        }

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
