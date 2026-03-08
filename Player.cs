using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MidiPlayer
{
    public partial class Player : Form
    {
        private readonly List<MusicItem> queue = new List<MusicItem>();
        private Dictionary<string, string> settings_data;
        private MidiIn midiDevice;
        Device d = null;
        Object lb_sel_item;

        public Player()
        {
            InitializeComponent();
        }

        private void Player_Load(object sender, EventArgs e)
        {
            //load settings config into a dictionary
            settings_data = Settings.GetSettingsData();
            InitMidiDevice();

        }

        private void InitMidiDevice()
        {
            if (settings_data["midiin"] == null || settings_data["midiin"].Length == 0) return;
            if (!TryCreateMIDIDevice(out midiDevice)) 
                throw new Exception("Unable to create device!");

            midiDevice.MessageReceived += midiIn_MessageReceived;
            midiDevice.ErrorReceived += midiIn_ErrorReceived;
            midiDevice.Start();
        }

        private void midiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
        {
            Console.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}",
                e.Timestamp, e.RawMessage, e.MidiEvent));
        }

        void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            Console.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}",
                e.Timestamp, e.RawMessage, e.MidiEvent));
            string[] split_message = e.MidiEvent.ToString().Split(' ');
            try
            {
                if (e.MidiEvent.CommandCode == MidiCommandCode.ControlChange && int.Parse(split_message[split_message.Length - 3]) == 33 && int.Parse(split_message[split_message.Length - 1]) == 127)
                {
                    if (d == null)
                        d = new Device();
                    Console.WriteLine(d.GetSongsPlayed());
                    if (d.GetSongsPlayed() >= queue.Count)
                        return;
                    d.Play(queue[d.GetSongsPlayed()].path);

                    if (d.Playing())
                    {
                        Console.WriteLine("Updating lb");
                        this.Invoke((MethodInvoker)(()=> ListBox1.Items.RemoveAt(0)));
                        UpdateListBox();
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid format");
                Console.WriteLine(ex.ToString());
            }

        }

        private bool TryCreateMIDIDevice(out MidiIn device)
        {
            device = null;
            try
            {
                int idx;
                if ((idx = FindMidiDeviceIndex()) != -1)
                {
                    device = new MidiIn(idx);
                    return true;
                }
                else
                {
                    Console.WriteLine("idx: "+idx);
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private int FindMidiDeviceIndex()
        {
            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                if (settings_data["midiin"].Equals(MidiIn.DeviceInfo(i).ProductName))
                    return i;
            }
            return -1;
        }

        private void fileAddButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = "c:\\";
                ofd.Filter = "Audio files (.mp3,.wav)|*.mp3;*.wav|All files|*.*";
                ofd.Multiselect = true;
                ofd.Title = "Select the audio files to be played";
                DialogResult dr = ofd.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    // string stores the path of selected files
                    MusicItem item;
                    int itemIndex = (queue.Count == 0) ? 0: queue.Count+1;
                    for (int i = 0; i < ofd.FileNames.Length; i++, itemIndex++)
                    {
                        item = new MusicItem(i, ofd.FileNames[i]);
                        if (!queue.Contains(item))
                            queue.Add(item);
                    }
                }

                InitQueueListBox();
            }

        }

        private void InitQueueListBox()
        {
            ListBox1.Items.Clear();
            //ListBox1.DrawMode = DrawMode.OwnerDrawFixed;
            //ListBox1.DrawItem += ListBox_DrawItem;
            for (int i = 0; i < queue.Count; i++)
                ListBox1.Items.Add(i + ": " + queue[i].GetFileName()); // formats the string so only the name of the file is displayed
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

            //update the queue arraylist accordingly
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
            
            queue[oldIdx].SetId(newIdx);
            if (oldIdx < newIdx)
                for (int i = newIdx; i > oldIdx; i--)
                    queue[i].SetId(i - 1);
            else
                for (int i = newIdx; i < oldIdx; i++)
                    queue[i].SetId(i+1);

            queue.Sort();
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
            //Console.WriteLine(ListBox1.SelectedItem is null);
            if (ListBox1.SelectedItem is null) return;
            this.lb_sel_item = ListBox1.SelectedItem;
            ListBox1.DoDragDrop(ListBox1.SelectedItem, DragDropEffects.Move);
        }
        #endregion

        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            Color bg = ColorTranslator.FromHtml("#252525"), line = ColorTranslator.FromHtml("#3A3A3A");
            //line = Color.White;
            Rectangle rect = e.Bounds;
            float borderSize = 1.5f;

            e.Graphics.FillRectangle(new SolidBrush(bg),e.Bounds);
            e.Graphics.DrawString(ListBox1.Items[e.Index].ToString(), ListBox1.Font, Brushes.White, e.Bounds);
            if(e.Index != ListBox1.Items.Count - 1)
                using (Pen p = new Pen(line, borderSize))
                {
                    e.Graphics.DrawLine(
                        p,
                        rect.Left,
                        rect.Bottom - 1,     // y position
                        rect.Right,
                        rect.Bottom - 1
                    );
                }
            //e.DrawFocusRectangle();

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //TODO: Remove items when the song ends

            if (d == null)
                d = new Device();
            Console.WriteLine(d.GetSongsPlayed());
            if (d.GetSongsPlayed() >= queue.Count)
                return;
            d.Play(queue[d.GetSongsPlayed()].path);

            if (d.Playing())
            {
                Console.WriteLine("Updating lb");
                ListBox1.Items.RemoveAt(0);
                UpdateListBox();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (d == null) return;
            if (d.Playing()) d.Stop();

            //resets the songs played counter to 0 and reinitalizes the queue display
            d.SetSongsPlayed();
            InitQueueListBox();
        }
    

        //remove logic
        //needs testing
        private void RemoveItemBtn_Click(object sender, EventArgs e)
        {
            if (this.lb_sel_item is null) return;
            string formatted_item = this.lb_sel_item.ToString().Substring(this.lb_sel_item.ToString().IndexOf(':') + 1).Trim();
            //itterate through every item, and remove the selected one
            foreach (MusicItem item in queue)
            {
                //Console.WriteLine($"Filename: {item.GetFileName()}\nlb_selitem: {formatted_item}\nbool:{item.GetFileName().Equals(formatted_item)}");
                if (item.GetFileName().Equals(formatted_item))
                {
                    ListBox1.Items.Remove(this.lb_sel_item);
                    this.queue.Remove(new MusicItem(item.ID,item.path));
                    UpdateListBox();
                    lb_sel_item = null; //null the item
                    break;
                }
            }
        }
    }

}
