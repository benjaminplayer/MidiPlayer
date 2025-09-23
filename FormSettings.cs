using NAudio.Midi;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using System.IO;

namespace MidiPlayer
{
    public partial class Settings : Form
    {
        public enum DeviceType
        {
            WINDOWS_AUDIO,
            DIRECT_SOUND,
            ASIO
        }

        private bool midiAvailable;
        public MidiIn selected_midi;
        public DeviceType deviceType;
        private static AudioHandler audioHandler;
        private static Dictionary<string, string> dataMap = new Dictionary<string, string>();
        private static readonly string CONFIG_DIRECTORY_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MidiPlayer");
        private static readonly string CONFIG_FILE_PATH = Path.Combine(CONFIG_DIRECTORY_PATH, "settings.txt");
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            InitDictionary();
            InitMidiDropDown();
            InitDeviceTypeMenu();
        }

        private void InitMidiDropDown()
        {
            comboBox1.Items.Clear();
            string[] devices = GetMidiInputDevies();
            if (devices.Length > 0)
            {
                midiAvailable = true;
                foreach (string device in devices)
                {
                    comboBox1.Items.Add(device);
                    //ce je item enak settings shrani idx in ga iz loopa dodaj
                }

            }
            else
            {
                midiAvailable = false;
                comboBox1.Items.Add("NO AVAILIBLE MIDI DEVICES");
            }
            
            //comboBox1.SelectedIndex = 0;
        }

        private void InitDeviceTypeMenu()
        {
            foreach (DeviceType dt in Enum.GetValues(typeof(DeviceType)))
                OutputTypeComboBox.Items.Add(dt.ToString());
            deviceType = DeviceType.WINDOWS_AUDIO;
            OutputTypeComboBox.SelectedIndex = 0;
        }

        //MIDI selection changed
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Selected idx: " + comboBox1.SelectedIndex);
            if (!midiAvailable)
                return;
            dataMap["midiin"] = comboBox1.Text.ToUpper();
            if
                (selected_midi != null)
            {
                selected_midi.Stop();
                selected_midi.Dispose();
            }
            selected_midi = new MidiIn(comboBox1.SelectedIndex);
            selected_midi.MessageReceived += MainContainer.midiIn_MessageReceived;
            selected_midi.ErrorReceived += MainContainer.midiIn_ErrorReceived;
            selected_midi.Start();
        }

        public string[] GetMidiInputDevies()
        {
            string[] devices = new string[MidiIn.NumberOfDevices];
            for (int i = 0; i < devices.Length; i++)
            {
                devices[i] = MidiIn.DeviceInfo(i).ProductName;
            }
            return devices;
        }

        //Device type selection changed
        private void OutputTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            OutputDevices.Items.Clear();
            switch (deviceType = (DeviceType)Enum.Parse(typeof(DeviceType), OutputTypeComboBox.SelectedIndex.ToString()))
            {
                case DeviceType.WINDOWS_AUDIO:
                    var devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                    foreach (var device in devices)
                    {
                        OutputDevices.Items.Add(device.ToString());
                        Console.WriteLine(device.ToString());
                    }
                    OutputDevices.SelectedIndex = 0;
                    break;
                case DeviceType.DIRECT_SOUND:
                    foreach (var dev in DirectSoundOut.Devices)
                    {
                        OutputDevices.Items.Add($" {dev.Description}");
                        Console.WriteLine($"{dev.Description}");
                    }
                    OutputDevices.SelectedIndex = 0;
                    break;
                case DeviceType.ASIO:
                    if (AsioOut.GetDriverNames().Length == 0)
                    {
                        OutputDevices.Items.Add("NO ASIO DEVICES AVAILABLE!");
                        OutputDevices.SelectedIndex = 0;
                        break;
                    }
                    foreach (var asio in AsioOut.GetDriverNames())
                    {
                        OutputDevices.Items.Add(asio.ToString());
                        Console.WriteLine(asio);
                    }
                    OutputDevices.SelectedIndex = 0;
                    break;
                default:
                    Console.WriteLine("Something went wrong!");
                    break;
            }
            Console.WriteLine("Selected device: " + deviceType.ToString());
            dataMap["outputtype"] = deviceType.ToString();
        }


        private void OutputDevices_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void OutputDevices_DropDown(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            int width = cb.DropDownWidth;
            Graphics g = cb.CreateGraphics();
            foreach (var item in cb.Items)
            {
                // + sbwidth => adds extra space so the scrollbar doesn't cover the item name
                int newWidth = (int)g.MeasureString(item.ToString(), cb.Font).Width + SystemInformation.VerticalScrollBarWidth;
                if (newWidth > width)
                    width = newWidth;
            }
            g.Dispose();

            cb.DropDownWidth = width;
        }

        private void refreshMidiInBtn_Click(object sender, EventArgs e)
        {
            InitMidiDropDown();
        }

        public void SaveFile()
        {
            try
            {
                if (!File.Exists(CONFIG_FILE_PATH))
                    CreateConfigFile();

                string content;
                using (StreamReader sr = new StreamReader(CONFIG_FILE_PATH))
                { 
                    content = sr.ReadToEnd();
                }

                FormatData(content);



                // file structure: inputtype, outputtype, output device, midiin
                foreach (var item in dataMap)
                { 
                    
                }

            }
            catch (Exception e)
            { 
                Console.WriteLine("Error saving settings: " + e.Message);
            }
           // StreamWriter sw = new StreamWriter();
        }

        private void InitDictionary()
        {
            string content;

            using (StreamReader sr = new StreamReader(CONFIG_FILE_PATH))
            {
                content = sr.ReadToEnd();
            }

            FormatData(content);

        }

        private void CreateConfigFile()
        {
            string[] defaultData =
            {
                "#Settings configuration file for MidiPlayer v.0.1.0",
                "#DO NOT EDIT THE FILES CONTENT, AS IT COULD BREAK FUNCTIONALITY OF THE APP",
                "",
                "#INPUT TYPE",
                "inputtype=",
                "",
                "#TYPE OF OUTPUT DEVICE",
                "outputtype=",
                "",
                "#OUTPUT DEVICE",
                "outputdevice=",
                "",
                "#MIDI INPUT DEVICE",
                "midiin=0"
            };

            try
            {
                if(!Directory.Exists(CONFIG_DIRECTORY_PATH))
                    Directory.CreateDirectory (CONFIG_DIRECTORY_PATH);
                File.Create (CONFIG_FILE_PATH);

                using (StreamWriter sw = new StreamWriter(CONFIG_FILE_PATH))
                { 
                    foreach(string line in defaultData)
                        sw.WriteLine(line);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error while trying to create a config file" + e.Message);
            }

        }

        //Formats the read data from the config file
        private void FormatData(string data)
        {
            string[] splitData = data.Split('\n'), d;
            
            for(int i = 0; i < splitData.Length; i++)
            {
                if (splitData[i].Length != 1 && splitData[i][0] != '#')
                {
                    d = splitData[i].Trim('\r','\n').Split('=');
                    if (!dataMap.ContainsKey(d[0]))
                        dataMap.Add(d[0], d[1]);
                    else
                    {
                        dataMap[d[0]] = d[1];
                    }
                        Console.WriteLine("Key:" + d[0] + "; Value:" + d[1]);
                }
            }

        }

        private Dictionary<string, string> FormDataToDictionary()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();

            return data;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFile();
        }
    }
}
