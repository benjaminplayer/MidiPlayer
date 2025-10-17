using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;

namespace MidiPlayer
{
    public class Device
    {
        private dynamic outputDevice;
        private int AsioOutChannel;
        private string track;
        private TimeSpan trackLength;
        private Thread t_playing;
        private Dictionary<string, string> data;

        public Device()
        { }

        public Device(Dictionary<string, string> data, int asioOutChannel = 0)
        {
            // set the output device type
            // TODO refractor the code a bit 
            this.AsioOutChannel = asioOutChannel;
            this.data = data;
        }

        // figure this shit out
        public void Play(string path)
        {

            if (!File.Exists(path))
                throw new FileNotFoundException();
            track = path;

            if (t_playing != null && t_playing.IsAlive)
            { 
                Stop();
                return;
            }

            t_playing = new Thread(PlayThread);
            t_playing.Start();

        }

        private void PlayThread()
        {
            //AsioOut a = new AsioOut();

            using (var ar = new AudioFileReader(track))
            using (outputDevice = CreateAudioDevice())
            {

                trackLength = ar.TotalTime;
                int elapsed = 0;
                //idk why dis has to be like this but otherwise it doesn't work :)
                outputDevice.Init(ar);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing && elapsed <= (int)trackLength.TotalSeconds)
                {
                    //Console.WriteLine("Outputdevice playback state:" + outputDevice.PlaybackState.ToString());
                    elapsed++;
                    Thread.Sleep(1000);
                }
            }
            //Stop();
        }

        private dynamic CreateAudioDevice()
        {
            int deviceIdx;
            int.TryParse(data["outputdevice"], out deviceIdx);

            switch (data["outputtype"])
            {
                case "WINDOWS_AUDIO":
                    return outputDevice = new WaveOutEvent() { DeviceNumber = deviceIdx };
                case "DIRECT_SOUND":
                    throw new NotImplementedException();
                //break;
                case "ASIO":
                    Console.WriteLine("Outputdevice:" + data["outputdevice"]);
                    outputDevice = new AsioOut(data["outputdevice"]);
                    outputDevice.ChannelOffset = AsioOutChannel;
                    return outputDevice;
                default:
                    throw new Exception("Incorrect output device type!\nProvided type:" + data["outputtype"]);
            }
        }

        public void Stop()
        {
            outputDevice.Stop();
            outputDevice.Dispose();

            try
            {
                t_playing.Abort();
                t_playing = null;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("An error accured while trying to stop the thread\n" + e.Message);
            }
        }

        public dynamic GetOutputDevice()
        {
            return outputDevice.DeviceNumber;
        }

        public Type GetDeviceType()
        {
            return outputDevice.GetType();
        }

        public TimeSpan GetSongLength()
        {
            return trackLength;
        }

        public bool Playing()
        {
            if (t_playing != null)
                return t_playing.IsAlive;
            return false;
        }
    }
}