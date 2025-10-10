using NAudio.Midi;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace MidiPlayer
{
    public partial class Settings : Form
    {
        #region Enums
        public enum DeviceType
        {
            WINDOWS_AUDIO,
            DIRECT_SOUND,
            ASIO
        }

        public enum InputMode
        { 
            MIDI,
            KEYBOARD
        }

        public enum DataDictionaryItems
        {
            INPUTTYPE,
            OUTPUTTYPE,
            OUTPUTDEVICE,
            MIDIIN
        }

        #endregion

        private bool midiAvailable;
        private MidiIn selected_midi;
        private DeviceType deviceType;
        private InputMode inputMode;
        public static Dictionary<string, string> dataMap = new Dictionary<string, string>();
        private static readonly string CONFIG_DIRECTORY_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MidiPlayer");
        public static readonly string CONFIG_FILE_PATH = Path.Combine(CONFIG_DIRECTORY_PATH, "settings.txt");
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            InitSettings();
        }

        #region Init methods
        private void InitSettings()
        {
            InitDictionary();
            InitMidiDropDown();
            InitDeviceTypeMenu();
            InitRadioBtns();
        }
        
        private void InitMidiDropDown()
        {
            comboBox1.Items.Clear();
            string[] devices = GetMidiInputDevies();
            string val = "";
            dataMap.TryGetValue("midiin", out val);
            int idx = 0;
            if (devices.Length > 0)
            {
                midiAvailable = true;
                foreach (string device in devices)
                {
                    comboBox1.Items.Add(device);
                    if (val.Equals(device))
                        idx = comboBox1.Items.IndexOf(device);
                }

            }
            else
            {
                midiAvailable = false;
                comboBox1.Items.Add("NO AVAILIBLE MIDI DEVICES");
            }
            if (idx == 0)
            {
                dataMap["midiin"] = "";
            }
            comboBox1.SelectedIndex = idx;
        }

        private void InitDeviceTypeMenu()
        {
            int idx = 0;
            string val = "";
            dataMap.TryGetValue("outputtype", out val);
            OutputTypeComboBox.Items.Clear();
            foreach (DeviceType dt in Enum.GetValues(typeof(DeviceType)))
            { 
                OutputTypeComboBox.Items.Add(dt.ToString());
                if (val.Equals(dt.ToString()))
                { 
                    idx = OutputTypeComboBox.Items.IndexOf(dt.ToString());
                    deviceType = dt;
                }
            }

            if (idx == 0)
            { 
                deviceType = DeviceType.WINDOWS_AUDIO; //default setting
                dataMap["outputtype"] = deviceType.ToString();
            }

            OutputTypeComboBox.SelectedIndex = idx;
        }

        //loads data from a settings file to a dictionary
        private void InitDictionary()
        {
            if (!File.Exists(CONFIG_FILE_PATH))
                CreateConfigFile();

            string content;

            using (StreamReader sr = new StreamReader(CONFIG_FILE_PATH))
            {
                content = sr.ReadToEnd();
            }

            dataMap = FormatData(content);

        }

        private void InitRadioBtns()
        {
            string val = "";
            dataMap.TryGetValue(DataDictionaryItems.INPUTTYPE.ToString().ToLower(), out val);
            if (val.Equals(InputMode.MIDI.ToString()))
            {
                radioButton1.Checked = true;
                inputMode = InputMode.MIDI;
            }
            else
            { 
                radioButton2.Checked = true;
                inputMode = InputMode.KEYBOARD;
            }

        }
        #endregion

        public void LoadDefaults()
        {
            if(!File.Exists(CONFIG_FILE_PATH))
                CreateConfigFile();
            string[] midiDevices = GetMidiInputDevies();
            if (midiDevices.Length > 0)
            { 
                dataMap[DataDictionaryItems.MIDIIN.ToString().ToLower()] = midiDevices[0];
                inputMode = InputMode.MIDI;
                selected_midi = new MidiIn(0);
                midiAvailable = true;
            }
            dataMap[DataDictionaryItems.MIDIIN.ToString().ToLower()] = "0";
            inputMode = InputMode.KEYBOARD;
            dataMap[DataDictionaryItems.OUTPUTTYPE.ToString().ToLower()] = DeviceType.WINDOWS_AUDIO.ToString();
            dataMap[DataDictionaryItems.OUTPUTDEVICE.ToString().ToLower()] = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)[0].ToString();

            SaveFile();

        }

        //MIDI selection changed
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Selected idx: " + comboBox1.SelectedIndex);
            if (!midiAvailable)
                return;
            dataMap["midiin"] = comboBox1.Text;
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
            int idx = 0;
            string val = "";
            dataMap.TryGetValue("outputdevice", out val);

            switch (deviceType = (DeviceType)Enum.Parse(typeof(DeviceType), OutputTypeComboBox.SelectedIndex.ToString()))
            {
                case DeviceType.WINDOWS_AUDIO:
                    var devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                    foreach (var device in devices)
                    {
                        OutputDevices.Items.Add(device.ToString());
                        if(val.Equals(device.ToString()))
                            idx = OutputDevices.Items.IndexOf(device.ToString());
                        Console.WriteLine(device.ToString());
                    }
                    break;
                case DeviceType.DIRECT_SOUND:
                    foreach (var dev in DirectSoundOut.Devices)
                    {
                        OutputDevices.Items.Add($" {dev.Description}");
                        if (val.Equals(dev.ToString()))
                            idx = OutputDevices.Items.IndexOf(dev.ToString());
                        Console.WriteLine($"{dev.Description}");
                    }
                    break;
                case DeviceType.ASIO:
                    if (AsioOut.GetDriverNames().Length == 0)
                    {
                        OutputDevices.Items.Add("NO ASIO DEVICES AVAILABLE!");
                        break;
                    }
                    foreach (var asio in AsioOut.GetDriverNames())
                    {
                        OutputDevices.Items.Add(asio.ToString());
                        if (val.Equals(asio.ToString()))
                            idx = OutputDevices.Items.IndexOf(asio.ToString());
                        Console.WriteLine(asio);
                    }
                    break;
                default:
                    Console.WriteLine("Something went wrong!");
                    break;
            }
            Console.WriteLine("Selected device: " + deviceType.ToString());
            dataMap["outputtype"] = deviceType.ToString();
            dataMap["outputdevice"] = OutputDevices.Items[idx].ToString();
            OutputDevices.SelectedIndex = idx;
        }

        private void OutputDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataMap["outputdevice"] = OutputDevices.Text;

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

            dataMap[DataDictionaryItems.INPUTTYPE.ToString().ToLower()] = inputMode.ToString();
            try
            {
                string content;
                using (StreamReader sr = new StreamReader(CONFIG_FILE_PATH))
                { 
                    content = sr.ReadToEnd();
                }

                // file structure: inputtype, outputtype, output device, midiin
                string[] splitString = content.Trim('\r').Split('\n'), d;
                using (StreamWriter sw = new StreamWriter(CONFIG_FILE_PATH))
                    for (int i = 0; i < splitString.Length; i++)
                    {
                        if (splitString[i].Length > 1 && splitString[i][0] != '#')
                        {
                            d = splitString[i].Split('=');
                            if (d.Length > 1 && !dataMap[d[0]].Equals(d[1]))
                            {
                                sw.Write(d[0] + "=" + dataMap[d[0]]);
                                Console.Write(d[0] + "=" + dataMap[d[0]]);
                            }
                            else
                            {
                                sw.Write(splitString[i]);
                                //Console.WriteLine(splitString[i]);
                            }
                            if (i != splitString.Length - 1)
                                sw.WriteLine();
                        }
                        else
                        {
                            //remove carrage return char, bc for some reason it exists there :/
                            PrintStringAsChars(splitString[i].Trim('\r'));
                            sw.WriteLine(splitString[i].Trim('\r'));
                        }

                    }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving settings: " + e.StackTrace);
            }
        }

        private void PrintStringAsChars(string s)
        {
            foreach (char c in s.ToCharArray())
            { 
                Console.Write((int)c + " ");
                if((int)c == 10)
                    Console.WriteLine();
            }
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
                File.Create (CONFIG_FILE_PATH).Dispose();

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
        private Dictionary<string,string> FormatData(string data)
        {
            Dictionary<string,string> dat = new Dictionary<string,string>();
            string[] splitData = data.Split('\n'), d;

            for(int i = 0; i < splitData.Length; i++)
            {
                if (splitData[i].Length > 1 && splitData[i][0] != '#')
                {
                    d = splitData[i].Trim('\r', '\n').Split('=');
                    if (d.Length <= 1) continue;
                    dat.Add(d[0], d[1]);
                }
            }
            return dat;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            inputMode = InputMode.MIDI;
            dataMap[DataDictionaryItems.INPUTTYPE.ToString().ToLower()] = inputMode.ToString();
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            inputMode = InputMode.KEYBOARD;
            dataMap[DataDictionaryItems.INPUTTYPE.ToString().ToLower()] = inputMode.ToString();
        }

        private void Settings_Leave(object sender, EventArgs e)
        {
            SaveFile();
        }

        public Dictionary<string,string> GetSettingsData()
        {
            if (dataMap == null || dataMap.Count == 0)
            {
                InitDictionary();
            }
            return dataMap;
        }

        public new void Dispose()
            { Dispose(true); }

        private void button2_Click(object sender, EventArgs e)
        {
            //InitDictionary();

        }
    }
}
