using NAudio.CoreAudioApi;
using NAudio.Midi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MidiPlayer
{
    //TODO: Refractor -> ASIO store names; others -> store INTS
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
            dataMap.TryGetValue("midiin", out string val);
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
            dataMap.TryGetValue("outputtype", out string val);

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

            if (!deviceType.Equals(DeviceType.ASIO))
            {
                AsioChannel_Label.Visible = false;
                AsioChannel_Drop.Visible = false;
            }
            else
            {
                AsioChannel_Label.Visible = true;
                AsioChannel_Drop.Visible = true;
            }

        }

        //loads data from a settings file to a dictionary
        private static void InitDictionary()
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

        //TODO: Make this static
        public void LoadDefaults()
        {
            Console.WriteLine("Load defaults");
            if (!File.Exists(CONFIG_FILE_PATH))
                CreateConfigFile();
            string[] midiDevices = GetMidiInputDevies();
            if (midiDevices.Length > 0)
            {
                dataMap[DataDictionaryItems.MIDIIN.ToString().ToLower()] = midiDevices[0];
                inputMode = InputMode.MIDI;
                midiAvailable = true;
            }
            dataMap[DataDictionaryItems.MIDIIN.ToString().ToLower()] = "0";
            dataMap[DataDictionaryItems.INPUTTYPE.ToString().ToLower()] = InputMode.KEYBOARD.ToString();
            inputMode = InputMode.KEYBOARD;
            dataMap[DataDictionaryItems.OUTPUTTYPE.ToString().ToLower()] = DeviceType.WINDOWS_AUDIO.ToString();
            dataMap[DataDictionaryItems.OUTPUTDEVICE.ToString().ToLower()] = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)[0].ToString();
            dataMap["asio_channel"] = "0";
            SaveFile();

        }

        //MIDI selection changed
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Selected idx: " + comboBox1.SelectedIndex);
            if (!midiAvailable)
                return;
            dataMap["midiin"] = comboBox1.Text;
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
            if (!dataMap.TryGetValue("outputdevice", out _))
            {
                throw new Exception("The device does not exist!");
            }

            switch (deviceType = (DeviceType)Enum.Parse(typeof(DeviceType), OutputTypeComboBox.SelectedIndex.ToString()))
            {
                case DeviceType.WINDOWS_AUDIO:
                    var devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                    string target;

                    //tries to parse the data
                    try { target = new MMDeviceEnumerator().GetDevice(dataMap["outputdevice"]).ToString();}
                    catch (ArgumentException)
                    {
                        //if parsing fails, sets the target as the default audio output
                        var tgt = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                        dataMap["outputdevice"] = tgt.ID;
                        target = tgt.ToString();
                        tgt.Dispose();
                    }

                    for(int i = 0; i < devices.Count;i++)
                    {
                        OutputDevices.Items.Add(devices[i].ToString());
                        if (target.Equals(devices[i].ToString()))
                            idx = i;
                    }
                    Device.UpdateOutput(new WasapiOut(new MMDeviceEnumerator().GetDevice(dataMap["outputdevice"]),AudioClientShareMode.Shared,true,100));
                    break;
                case DeviceType.DIRECT_SOUND:
                    foreach (var dev in DirectSoundOut.Devices)
                    {
                        OutputDevices.Items.Add($" {dev.Description}");
                        if (dev.Guid.ToString().Equals(dataMap["outputdevice"]))
                            idx = OutputDevices.Items.IndexOf(dev.Description);

                    }
                    break;
                    //TODO: Figure out how to fill the menu :)
                case DeviceType.ASIO:
                    if (AsioOut.GetDriverNames().Length == 0)
                    {
                        OutputDevices.Items.Add("NO ASIO DEVICES AVAILABLE!");
                        break;
                    }
                    foreach (var asio in AsioOut.GetDriverNames())
                    {
                        OutputDevices.Items.Add(asio.ToString());
                        if (dataMap["outputdevice"].Equals(asio))
                            idx = OutputDevices.Items.IndexOf(asio);
                    }
                    break;
                default:
                    Console.WriteLine("Something went wrong!");
                    break;
            }
            dataMap["outputtype"] = deviceType.ToString();
            OutputDevices.SelectedIndex = idx;

            if (!deviceType.Equals(DeviceType.ASIO))
            {
                AsioChannel_Label.Visible = false;
                AsioChannel_Drop.Visible = false;
            }
            else
            {
                AsioChannel_Label.Visible = true;
                AsioChannel_Drop.Visible = true;
                Console.WriteLine("Passing arg: "+ OutputDevices.SelectedItem.ToString());
                GetAsioChannelOutputs(OutputDevices.SelectedItem.ToString());
            }

        }

        private void GetAsioChannelOutputs(string deviceName)
        {
            Console.WriteLine("Called with arg: "+deviceName);
            try
            {
                using (AsioOut ao = new AsioOut(deviceName))
                {
                    Console.WriteLine("out:"+ao.NumberOfOutputChannels);
                    Console.WriteLine("in:"+ao.NumberOfInputChannels);
                    for (int i = 0; i < ao.NumberOfOutputChannels; i++)
                        AsioChannel_Drop.Items.Add("OUT: "+(i+1));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }

        private void OutputDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (deviceType)
            { 
                case DeviceType.WINDOWS_AUDIO:
                    //Updates the device info in the datamap, and the device class
                    string deviceID = GetOutputID(OutputDevices.Items[OutputDevices.SelectedIndex].ToString());
                    Device.UpdateOutput(new WasapiOut(new MMDeviceEnumerator().GetDevice(deviceID), AudioClientShareMode.Shared,true,100));
                    break;
                case DeviceType.DIRECT_SOUND:
                    dataMap["outputdevice"] = GetWaveOutGuid(OutputDevices.SelectedIndex); 
                    break;
                case DeviceType.ASIO:
                    GetAsioChannelOutputs(OutputDevices.SelectedItem.ToString());
                    dataMap["outputdevice"] = OutputDevices.SelectedItem.ToString();
                    dataMap["asio_channel"] = AsioChannel_Drop.SelectedIndex.ToString();
                    break;
            }
        }

        private string GetOutputID(string name)
        {
            var devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render,DeviceState.Active);
            foreach (var device in devices)
            {
                if (device.ToString().Equals(name)) return device.ID;
            }
            return "";
        }

        private string GetWaveOutGuid(int idx)
        {
            int i = 0;
            foreach (var dev in DirectSoundOut.Devices)
            {
                if(i == idx)
                    return dev.Guid.ToString();
                i++;
            }

            return null;
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

        private void RefreshMidiInBtn_Click(object sender, EventArgs e)
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
                    content = sr.ReadToEnd();

                // file structure: inputtype, outputtype, output device, midiin
                string[] splitString = content.Trim('\r').Split('\n'), d;
                using (StreamWriter sw = new StreamWriter(CONFIG_FILE_PATH))
                    for (int i = 0; i < splitString.Length; i++)
                    {
                        if (splitString[i].Length > 1 && splitString[i][0] != '#')
                        {
                            //d[0] == key; d[1] == value(probs)
                            d = splitString[i].Split('=');
                            if (d.Length > 1 && !dataMap[d[0]].Equals(d[1]))
                                sw.Write(d[0] + "=" + dataMap[d[0]]);
                            else
                                sw.Write(splitString[i]);
                            if (i != splitString.Length - 2)
                                sw.WriteLine();
                        }
                        else
                            sw.WriteLine(splitString[i].Trim('\r')); //remove carage return char, bc for some reason it exists there :/

                    }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving settings: " + e.StackTrace);
            }
        }

        private static void CreateConfigFile()
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
                "#ASIO output channel used: (Default = 0)",
                "asio_channel=",
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
                    foreach (string line in defaultData)
                        sw.WriteLine(line);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error while trying to create a config file" + e.Message);
            }

        }

        //Formats the read data from the config file
        private static Dictionary<string,string> FormatData(string data)
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

        public static Dictionary<string,string> GetSettingsData()
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
