using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace MidiPlayer
{
    //TODO: Reimpliment audio with wasapi -> Done?
    public class Device : IDisposable
    {
        private bool _disposed = false;

        private static dynamic output;
        private int asioOutputChannel;
        private string track;
        private TimeSpan trackLength;
        private Thread t_playing;
        private Dictionary<string, string> data;
        private int SongsPlayed = 0;

        public Device(int asioOutChannel = 0)
        {
            this.data = Settings.GetSettingsData();
            this.asioOutputChannel = asioOutChannel;
        }

        //test, mayhaps obselete
        public Device(dynamic outputDevice, int asioOutChannel = 0)
        {
            output = outputDevice;
            this.asioOutputChannel = asioOutChannel;
            this.data = Settings.GetSettingsData();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //TODO: take a look at cancelation tokens
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing) 
            {
                if (output != null)
                {
                    try { output.Stop(); }catch (Exception) { }
                    if (output is IDisposable dis) dis.Dispose();
                    output = null;
                }
                if (t_playing != null && t_playing.IsAlive)
                    t_playing = null;

                if (data != null)
                {
                    data.Clear();
                    data = null;
                }
            
            }

            _disposed = true;
        }

        //fix dis a tad
        /// <summary>
        /// Initializes an audio device, based on data provided from settings configuration file
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="Exception"></exception>
        private void InitAudioDevice()
        {
            string deviceID = data["outputdevice"];
            switch (data["outputtype"])
            {
                case "WINDOWS_AUDIO":
                    output = new WasapiOut(new MMDeviceEnumerator().GetDevice(deviceID), AudioClientShareMode.Shared, true, 100);
                    break;
                case "DIRECT_SOUND":
                    throw new NotImplementedException();
                case "ASIO":
                    Console.WriteLine("Outputdevice:" + data["outputdevice"]);
                    string[] s = AsioOut.GetDriverNames();

                    foreach (string s1 in s)
                        Console.WriteLine(s1);

                    Thread asio_creation = new Thread(() =>
                    {
                        output = new AsioOut(data["outputdevice"]);
                        output.ChannelOffset = asioOutputChannel;

                    });
                    asio_creation.SetApartmentState(ApartmentState.STA);
                    asio_creation.Start();
                    asio_creation.Join();
                    break;
                default:
                    throw new Exception("Incorrect output device type!\nProvided type:" + data["outputtype"]);
            }
        }

        /// <summary>
        /// Plays an audio based on provided path
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public void Play(string path)
        {

            if (!File.Exists(path))
                throw new FileNotFoundException();
            this.track = path;

            if (t_playing != null && t_playing.IsAlive)
                Stop();
            else
            {
                InitAudioDevice();
                t_playing = new Thread(PlayThread);
                t_playing.Start();
            }

        }

        private void PlayThread()
        { 
            using (var ar = new AudioFileReader(this.track))
            using (output)
            {
                trackLength = ar.TotalTime;
                int elapsed = 0;
                //idk why dis has to be like this but otherwise it doesn't work :)
                output.Init(ar);
                output.Play();
                while (output.PlaybackState == PlaybackState.Playing && elapsed <= (int)trackLength.TotalSeconds)
                {
                    //Console.WriteLine("Outputdevice playback state:" + outputDevice.PlaybackState.ToString());
                    elapsed++;
                    Thread.Sleep(1000);
                }
            }
            SongsPlayed++;
        }

        public void Stop()
        {
            output.Stop();
            output.Dispose();
            SongsPlayed++;

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

        public int GetSongsPlayed()
        {
            return this.SongsPlayed;
        }

        public void SetSongsPlayed(int idx = 0)
        { 
            this.SongsPlayed = idx;
        }

        public new string ToString()
        {
            return "Device: [Output device: "+output.toString()+";asioChannel: "+this.asioOutputChannel + "; track: "+track+"; thread_alive: "+this.t_playing.IsAlive+"; songsPlayed: "+this.SongsPlayed+"]";
        }

        //TODO: Implement this in settings after updating text box or after leave
        /// <summary>
        /// Updates the output mode of the device. Throws an exception if the old device was unable to be disposed
        /// </summary>
        /// <param name="newOut"></param>
        /// <exception cref="Exception"></exception>
        public static void UpdateOutput(dynamic newOut)
        {
            if (output != null && output is IDisposable)
            {
                output.Dispose();
                output = newOut;
            }
            else if (output != null && !(output is IDisposable))
                throw new Exception("The output cannot be disposed!");
            /*else
                throw new Exception("Exception reached while trying to update output device!");*/
        }


    }
}