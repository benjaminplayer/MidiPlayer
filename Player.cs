using NAudio.Midi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MidiPlayer
{
    public partial class Player : Form
    {
        private ArrayList musicPaths = new ArrayList();
        private Dictionary<string, string> settings_data;
        private Timer musicPlayTimer;
        private MidiIn midiDevice;
        public Player()
        {
            InitializeComponent();
        }

        private void Player_Load(object sender, EventArgs e)
        {
            //load settings config into a dictionary
            Settings settings = new Settings();
            settings_data = settings.GetSettingsData();
            settings.Dispose();
        }

        private void fileAddButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = "c:\\";
                ofd.Filter = "Audio files | *.MP3; *WAV | All files |*.*";
                ofd.Multiselect = true;
                ofd.Title = "Select the audio files to be played";
                DialogResult dr = ofd.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    // string stores the path of selected files
                    for (int i = 0; i < ofd.FileNames.Length; i++)
                    {
                        if (!musicPaths.Contains(ofd.FileNames[i]))
                            musicPaths.Add(ofd.FileNames[i]);
                    }
                }

                Console.WriteLine("Items in an array list:");
                PrintArrayList(musicPaths);

                ListBox1.Items.Clear();
                string[] data;
                for (int i = 0; i < musicPaths.Count; i++)
                {
                    data = musicPaths[i].ToString().Split('\\');
                    ListBox1.Items.Add(i + ": " + data[data.Length - 1]); // formats the string so only the name of the file is displayed
                }
            }

        }

        private void PrintArrayList(ArrayList ar)
        {
            foreach(var item in ar)
                Console.WriteLine(item.ToString());
        }

        #region List box drag and drop fucntionality
        private void ListBox1_DragDrop(object sender, DragEventArgs e)
        {
            Point point = ListBox1.PointToClient(new Point(e.X, e.Y));
            int index = this.ListBox1.IndexFromPoint(point);
            if (index < 0) index = this.ListBox1.Items.Count - 1;
            object data = e.Data.GetData(typeof(string));
            this.ListBox1.Items.Remove(data);
            this.ListBox1.Items.Insert(index, data.ToString());

            //update the musicPaths arraylist accordingly
            int oldIdx = int.Parse(data.ToString().Split(':')[0]);
            UpdateListBox();
            UpdatePathList(oldIdx, index);
        }
        private void UpdateListBox()
        {
            for (int i = 0; i < ListBox1.Items.Count; i++)
                ListBox1.Items[i] = i + ":" + ListBox1.Items[i].ToString().Split(':')[1];
        }

        private void UpdatePathList(int oldIdx, int newIdx)
        {
            ArrayList sorted = (ArrayList)musicPaths.Clone();
            for (int i = newIdx; i > oldIdx; i--)
                sorted[i - 1] = musicPaths[i].ToString();
            sorted[newIdx] = musicPaths[oldIdx];

            musicPaths = (ArrayList)sorted.Clone();
        }

        private void ListBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string)))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void ListBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (ListBox1.SelectedItem == null) return;
                ListBox1.DoDragDrop(ListBox1.SelectedItem, DragDropEffects.Move);
        }
        #endregion

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string device = settings_data["outputdevice"];
            AudioHandler ah = new AudioHandler();
        }
    }

}
