using NAudio.Midi;
using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
namespace MidiPlayer
{
    public partial class MidiControlls : Form
    {

        private MidiIn selected_midi;
        private ArrayList musicPaths = new ArrayList();
        private bool midiAvailable = false;
        private AudioHandler audioHandler = new AudioHandler();
        private int audioPlayingIdx = 0;
        private Settings settingsForm;
        public MidiControlls()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listBox1.AllowDrop = true;
            //InitMidiDropDown();
        }

        private void InitMidiDropDown()
        {
            comboBox1.Items.Clear();
            string[] devices = MidiHandler.GetMidiInputDevies();
            if (devices.Length > 0)
            {
                midiAvailable = true;
                foreach (string device in devices)
                {
                    comboBox1.Items.Add(device);
                }
            }
            else
            {
                midiAvailable = false;
                comboBox1.Items.Add("NO AVAILIBLE MIDI DEVICES");
            }
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
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

                listBox1.Items.Clear();
                string[] data;
                for (int i = 0; i < musicPaths.Count; i++)
                {
                    data = musicPaths[i].ToString().Split('\\');
                    listBox1.Items.Add(i + ": " + data[data.Length - 1]); // formats the string so only the name of the file is displayed
                }
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            Console.WriteLine("Selected idx: " + comboBox1.SelectedIndex);
            if (!midiAvailable)
                return;
            if
                (selected_midi != null)
            {
                selected_midi.Stop();
                selected_midi.Dispose();
            }
            selected_midi = new MidiIn(comboBox1.SelectedIndex);
            selected_midi.MessageReceived += midiIn_MessageReceived;
            selected_midi.ErrorReceived += midiIn_ErrorReceived;
            selected_midi.Start();
        }

        #region Midi Signal Handlers
        void midiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
        {
            Console.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}",
                e.Timestamp, e.RawMessage, e.MidiEvent));
        }

        void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {

            Console.WriteLine("Event: " + e.MidiEvent.ToString());
            string[] midiEvent = e.MidiEvent.ToString().Split(' ');

            //try parsing the string value to int
            try
            {

                if (int.Parse(midiEvent[5]) == 13 && int.Parse(midiEvent[midiEvent.Length - 1]) == 127)
                {
                    //handle later
                    if (musicPaths.Count == 0)
                    {
                        return;
                    }
                    if (!audioHandler.IsPlaying() && audioPlayingIdx < musicPaths.Count)
                    {
                        audioHandler.Play(musicPaths[audioPlayingIdx].ToString());
                        //invokes Method Invoker so the update will be run on the original thread
                        listBox1.Invoke((MethodInvoker)(() =>
                        {
                            listBox1.Items.RemoveAt(0);
                            UpdateListBox();
                        }));
                        audioPlayingIdx++;
                    }
                    else if (audioHandler.IsPlaying())
                    {
                        audioHandler.Stop();
                    }
                    
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

        }
        #endregion
        void listViewItemChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            Console.WriteLine("Selection changerd\n" + e.ItemIndex);
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItem == null) return;
            listBox1.DoDragDrop(listBox1.SelectedItem, DragDropEffects.Move);
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string)))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            Point point = listBox1.PointToClient(new Point(e.X, e.Y));
            int index = this.listBox1.IndexFromPoint(point);
            if (index < 0) index = this.listBox1.Items.Count - 1;
            object data = e.Data.GetData(typeof(string));
            this.listBox1.Items.Remove(data);
            this.listBox1.Items.Insert(index, data.ToString());

            //update the musicPaths arraylist accordingly
            int oldIdx = int.Parse(data.ToString().Split(':')[0]);
            UpdateListBox();
            UpdatePathList(oldIdx, index);

        }

        private void UpdateListBox()
        {
            for (int i = 0; i < listBox1.Items.Count; i++)
                listBox1.Items[i] = i + ":" + listBox1.Items[i].ToString().Split(':')[1];
        }

        private void UpdatePathList(int oldIdx, int newIdx)
        {
            ArrayList sorted = (ArrayList)musicPaths.Clone();
            for (int i = newIdx; i > oldIdx; i--)
                sorted[i - 1] = musicPaths[i].ToString();
            sorted[newIdx] = musicPaths[oldIdx];

            musicPaths = (ArrayList)sorted.Clone();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (selected_midi != null)
            {
                selected_midi.Stop();
                selected_midi.Dispose();
            }
        }

        #region Debugging functions
        void PrintArrayList(ArrayList a)
        {
            foreach (var item in a)
                Console.WriteLine(item.ToString());
        }
        #endregion

        private void refreshMidiInBtn_Click(object sender, EventArgs e)
        {
            InitMidiDropDown();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (settingsForm == null)
            {
                settingsForm = new Settings();
                settingsForm.FormClosed += SettingsFormClosed;
                settingsForm.MdiParent = this;
                settingsForm.Dock = DockStyle.Fill;
                settingsForm.Show();
                //settingsForm.Show();
            }else
            {
                settingsForm.Activate();
            }
        }

        private void SettingsFormClosed(object sender, FormClosedEventArgs e)
        {
            settingsForm = null;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
