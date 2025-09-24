using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MidiPlayer
{
    public partial class MainContainer : Form
    {
        private MidiControlls midiControllsForm;
        private Settings SettingsForm;

        public MainContainer()
        {
            InitializeComponent();
        }

        private void MainContainer_Load(object sender, EventArgs e)
        {
            if (!File.Exists(Settings.CONFIG_FILE_PATH))
            { 
                Settings s = new Settings();
                s.LoadDefaults();
                s.Dispose();
            }

            midiControllsForm = new MidiControlls();
            midiControllsForm.MdiParent = this;
            midiControllsForm.FormClosed += MidiControllsClosed;
            midiControllsForm.Dock = DockStyle.Fill;
            midiControllsForm.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (midiControllsForm == null)
            {
                midiControllsForm = new MidiControlls();
                midiControllsForm.FormClosed += MidiControllsClosed;
                midiControllsForm.MdiParent = this;
                midiControllsForm.Dock = DockStyle.Fill;
                midiControllsForm.Show();
            }
            else
            {
                midiControllsForm.Activate();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (SettingsForm == null)
            {
                SettingsForm = new Settings();
                SettingsForm.FormClosed += SettingsClosed;
                SettingsForm.MdiParent = this;
                SettingsForm.Dock = DockStyle.Fill;
                SettingsForm.Show();
            }
            else
            {
                SettingsForm.Activate();
            }
        }


        #region Settings Close handlers

        private void MidiControllsClosed(object sender, FormClosedEventArgs e)
        { 
            midiControllsForm = null;
        }

        private void SettingsClosed(object sender, FormClosedEventArgs e)
        {
            SettingsForm = null;
        }
        #endregion

        #region Midi Signal Handlers
        public static void midiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
        {
            Console.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}",
                e.Timestamp, e.RawMessage, e.MidiEvent));
        }

        public static void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {

            Console.WriteLine("Event: " + e.MidiEvent.ToString());
            string[] midiEvent = e.MidiEvent.ToString().Split(' ');

            //try parsing the string value to int
            /*try
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
            }*/

        }
        #endregion
    }
}
