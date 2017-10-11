using System;
using System.Configuration;
using System.Data.Linq.Mapping;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Dapper;
using Dapper.Contrib.Extensions;
using PhraseHelper.Properties;

namespace PhraseHelper
{
    public partial class MainForm : Form
    {
        private const int ActionHotkeyId = 1;
        private const int ActionHotKeyEscId = 2;
        private const int ActionHotKeyAltEnter = 3;
        private static readonly string SQLiteFileLocation = ConfigurationManager.AppSettings["SQLiteFileLocation"]
            ?? "phrase.db";
        public MainForm()
        {
            InitializeComponent();

            TopMost = true;

            Icon = Resources.Icon;
            notifyIcon.Icon = Resources.Icon;
            RegisterHotKey(Handle, ActionHotkeyId, 1, (int)Keys.Oem3);
            RegisterHotKey(Handle, ActionHotKeyEscId, 0, (int)Keys.Escape);
            RegisterHotKey(Handle, ActionHotKeyAltEnter, 1, (int)Keys.Enter);
            listBox.MouseDoubleClick += (sender, args) =>
            {
                HideThenPasteSelectedItem();
            };

            Dapper.SqlMapper.SetTypeMap(
                typeof(Phrase),
                new CustomPropertyTypeMap(
                    typeof(Phrase),
                    (type, columnName) =>
                        type.GetProperties().FirstOrDefault(prop =>
                            prop.GetCustomAttributes(false)
                                .OfType<ColumnAttribute>()
                                .Any(attr => attr.Name == columnName))));
            RefreshDataSource(string.Empty);
            textBox.TextChanged += (sender, args) =>
            {
                RefreshDataSource(textBox.Text);
            };
            textBox.KeyUp += (sender, args) =>
            {
                switch (args.KeyCode)
                {
                    case Keys.Down:
                        if (listBox.SelectedIndex < listBox.Items.Count - 1)
                        {
                            listBox.SelectedIndex++;
                        }
                        break;
                    case Keys.Up:
                        if (listBox.SelectedIndex > 0)
                        {
                            listBox.SelectedIndex--;
                        }
                        break;
                    case Keys.Enter:
                        if (args.Alt)
                        {
                            if (!string.IsNullOrWhiteSpace(textBox.Text))
                            {
                                AddPhrase(textBox.Text);
                                RefreshDataSource(textBox.Text);
                            }
                        }
                        else
                        {
                            HideThenPasteSelectedItem();
                        }
                        break;
                }
            };

            notifyIcon.ShowBalloonTip(1600, "Phrase Helper", "Application is running in the background.", ToolTipIcon.Info);
        }

        protected override void OnShown(EventArgs e)
        {
            Hide();
        }

        private void HideThenPasteSelectedItem()
        {
            if (!(listBox.SelectedItem is Phrase phrase))
            {
                return;
            }
            Debug.Assert(phrase != null, nameof(phrase) + " != null");
            Clipboard.SetDataObject(phrase.Text);
            Hide();
            SendKeys.Send("^v");
            phrase.Timestamp = (long?)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            UpdatePhrase(phrase);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {
                switch (m.WParam.ToInt32())
                {
                    case ActionHotkeyId:
                        Location = MousePosition;
                        textBox.Clear();
                        textBox.Focus();
                        RefreshDataSource(textBox.Text);
                        Show();
                        break;
                    case ActionHotKeyEscId:
                        Hide();
                        break;
                }
            }
            base.WndProc(ref m);
        }

        private void RefreshDataSource(string filterKey)
        {
            var conn = new SQLiteConnection("DbLinqProvider=Sqlite;Data Source=" + SQLiteFileLocation);
            var phrases = conn.Query<Phrase>("select * from phrase_tb where text like '%" + filterKey + "%' order by timestamp desc");
            listBox.DataSource = null;
            listBox.DataSource = phrases;
            listBox.DisplayMember = "Text";
            if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndex = 0;
            }
        }

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private void AddPhrase(string phrase)
        {
            var conn = new SQLiteConnection("DbLinqProvider=Sqlite;Data Source=" + SQLiteFileLocation);
            conn.Insert(new Phrase { Text = phrase });
        }

        private void UpdatePhrase(Phrase phrase)
        {
            var conn = new SQLiteConnection("DbLinqProvider=Sqlite;Data Source=" + SQLiteFileLocation);
            conn.Update<Phrase>(phrase);
        }
    }

    [Dapper.Contrib.Extensions.Table("phrase_tb")]
    public class Phrase
    {
        [Key]
        [Column(Name = "code", IsPrimaryKey = true)]
        public int? Code { get; set; }
        [Column(Name = "text")]
        public string Text { get; set; }
        [Column(Name = "timestamp")]
        public long? Timestamp { get; set; }
    }
}
