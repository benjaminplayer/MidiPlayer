using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Midi;

namespace MidiPlayer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] devices = MidiHandler.GetMidiInputDevies();

            foreach (string device in devices)
            {
                comboBox1.Items.Add(device);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = "c:\\";
                ofd.Filter = "Audio files | *.MP3; *WAV | All files |*.*";
                ofd.Multiselect = true;
                ofd.Title = "Select the audio files to be played";
                ofd.ShowDialog();
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Selected idx: "+comboBox1.SelectedIndex);
        }
    }
}
