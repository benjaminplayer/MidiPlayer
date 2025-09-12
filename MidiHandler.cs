using NAudio.CoreAudioApi;
using NAudio.Midi;
using System;

namespace MidiPlayer
{
    internal class MidiHandler
    {
        private MidiIn device;


        MidiHandler(MidiIn device)
        {
            this.device = device;
        }

        public static string[] GetMidiInputDevies()
        {
            string[] devices = new string[MidiIn.NumberOfDevices];
            for (int i = 0; i < devices.Length; i++)
            {
                devices[i] = MidiIn.DeviceInfo(i).ProductName;
            }
            return devices;
        }

        public void test()
        {

            Console.WriteLine("Select the MIDI device");
            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                Console.WriteLine(i + ":" + MidiIn.DeviceInfo(i).ProductName);
            }
            int idx = Int32.Parse(Console.ReadLine());

            device = new MidiIn(idx);
            // Subscribe to events
            device.MessageReceived += MidiIn_MessageReceived;
            device.ErrorReceived += MidiIn_ErrorReceived;
            // Start receiving messages
            device.Start();
            Console.WriteLine("Press any key to stop");
            Console.ReadKey();
            device.Stop();
            device.Dispose();
        }

        void MidiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
        {
            Console.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}",
                e.Timestamp, e.RawMessage, e.MidiEvent));
        }

        void MidiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            Console.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}",
                e.Timestamp, e.RawMessage, e.MidiEvent));

            //values are storred in arr[5];
            string[] midiEvent = e.MidiEvent.ToString().Split(' ');

            if (int.Parse(midiEvent[5]) == 13)
            {
                //SoundManager sm = new SoundManager("E:\\MusicTest");
                //sm.PlayMusicFromDir();
            }

        }
        static void midiIn_SysexMessageReceived(object sender, MidiInSysexMessageEventArgs e)
        {
            byte[] sysexMessage = e.SysexBytes;
            Console.WriteLine(sysexMessage);
        }
    }
}
