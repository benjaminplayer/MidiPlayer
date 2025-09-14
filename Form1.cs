using NAudio.Midi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MidiPlayer
{
    public partial class Form1 : Form
    {
    
        private MidiIn selected_midi;
        private Dictionary<int, string> musicItems = new Dictionary<int, string>();
        private bool midiAvailable = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listBox1.AllowDrop = true;
            listBox1.Items.Add("Drop 1");
            listBox1.Items.Add("Drop 2");
            listBox1.Items.Add("Drop 3");

            listView1.ItemSelectionChanged += listViewItemChanged;
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
                    // v string da paths od selected files
                    for (int i = 0; i < ofd.FileNames.Length; i++)
                    { 
                        Console.WriteLine("Selected file: " + ofd.FileNames[i]);
                        musicItems.Add(i, ofd.FileNames[i]);
                    }
                }

                Console.WriteLine("Music items in dictionary:");
                PrintDictionary();

                //formatta, da list displaya item horizontally
                listView1.Items.Clear();
                listView1.Columns.Clear();
                listView1.Columns.Add("");
                listView1.View = View.Details;
                listView1.HeaderStyle = ColumnHeaderStyle.None;
                

                foreach (var item in musicItems)
                {
                    ListViewItem lvi = new ListViewItem(item.Key.ToString()+": " +item.Value);
                    listView1.Items.Add(lvi);
                }
                listView1.Columns[0].Width = -1;
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            Console.WriteLine("Selected idx: "+comboBox1.SelectedIndex);
            if (!midiAvailable)
                return;
            if
                (selected_midi !=null)
            { 
                selected_midi.Stop();
                selected_midi.Dispose();
            }
            selected_midi = new MidiIn(comboBox1.SelectedIndex);
            selected_midi.MessageReceived += midiIn_MessageReceived;
            selected_midi.ErrorReceived += midiIn_ErrorReceived;
            selected_midi.Start();
        }

        void midiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
        {
            Console.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}",
                e.Timestamp, e.RawMessage, e.MidiEvent));
        }

        void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
           /* Console.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}",
                e.Timestamp, e.RawMessage, e.MidiEvent));*/

            Console.WriteLine("Event: "+ e.MidiEvent.ToString());
            string[] midiEvent = e.MidiEvent.ToString().Split(' ');

            //try parsing the string value to int
            try
            {
                string path = @"C:\Users\benja\Music\Illuminate Intro Sample FinalMix.mp3"; //default file

                if (int.Parse(midiEvent[5]) == 13 && int.Parse(midiEvent[midiEvent.Length-1]) == 127)
                {
                    
                    using (var audioFile = new AudioFileReader(path))
                    using (var outputDevice = new WaveOutEvent())
                    {
                        outputDevice.Init(audioFile);
                        outputDevice.Play();
                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(1000);
                        }
                    }

                }

            } catch (Exception ex)
            { 
                Console.WriteLine("Exception: "+ex.Message);
            }

        }

        void listViewItemChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            Console.WriteLine("Selection changerd\n" + e.ItemIndex);
        }

        private void ListView1_ItemDrag(Object sender, ItemDragEventArgs e)
            {
            // Start the drag-and-drop operation with the list item. 
            listView1.DoDragDrop(e.Item, DragDropEffects.Move);
            Console.WriteLine("Item dragged");
        }

        void PrintDictionary()
        {
            foreach (var item in musicItems)
            {
                Console.WriteLine("Key: "+item.Key+" Value: "+item.Value);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItem == null) return;
            listBox1.DoDragDrop(listBox1.SelectedItem, DragDropEffects.Move);
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(typeof(string)))
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
            this.listBox1.Items.Insert(index, data);
            Console.WriteLine("Item dropped" + data.ToString());
        }
    }
}
