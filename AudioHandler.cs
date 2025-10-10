using NAudio.Wave;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MidiPlayer
{
    internal class AudioHandler
    {
        private WaveOutEvent waveOutputDevice;
        private DirectSoundOut dsOutputDevice;
        private AsioOut asioOutDevice;
        private IWavePlayer player;
        private int asioOutputChannel;
        private Thread audioThread;
        private float volume = 1f;

        #region Constructors
        public AudioHandler() 
        { 
            
        }

        public AudioHandler(Object outputDevice)
        {
            try
            {
                //try casting the object into a player object
                this.player = (IWavePlayer)outputDevice;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /*
        public AudioHandler(WaveOutEvent waveOutputDevice)
        {
            this.waveOutputDevice = waveOutputDevice;
        }

        public AudioHandler(DirectSoundOut dsOutputDevice)
        {
            this.dsOutputDevice = dsOutputDevice;
        }

        public AudioHandler(AsioOut asioOutDevice, int asioOutputChannel)
        {
            this.asioOutDevice = asioOutDevice;
            this.asioOutputChannel = asioOutputChannel;
        }*/
        #endregion
        public void Play(string path)
        {
            if (audioThread == null || !audioThread.IsAlive)
            {
                audioThread = new Thread(() => PlayAudio(path));
                audioThread.Start();
            }
            else
            {
                Stop();
                audioThread = new Thread(() => PlayAudio(path));
            }
        }

        public void Stop()
        { 

            if (waveOutputDevice != null)
            {
                waveOutputDevice.Stop();
                waveOutputDevice.Dispose();
                waveOutputDevice = null;
            }
            else if (dsOutputDevice != null)
            {
                dsOutputDevice.Stop();
                dsOutputDevice.Dispose();
                dsOutputDevice = null;
            }
            else
            {
                asioOutDevice.Stop();
                asioOutDevice.Dispose();
                asioOutDevice = null;
            }

            if (audioThread != null && audioThread.IsAlive)
            {
                try
                {
                    audioThread.Abort();
                    audioThread = null;

                }
                catch (ThreadAbortException e)
                {

                }
            }
        }

        public bool IsPlaying()
        {
            return audioThread != null && audioThread.IsAlive;
        }

        private void PlayAudio(string path)
        {
            //TODO: Instead of creating objects for specific audioDevices, just use a class
            //from which they inherit from -> player
            //test this code
            if (player != null)
            {
                using (var audioFile = new AudioFileReader(path))
                {
                    player.Init(audioFile);
                    player.Volume = volume;
                    player.Play();
                    while (player.PlaybackState == PlaybackState.Playing)
                        Thread.Sleep(1000);
                }
            }

            if (waveOutputDevice != null)
            {
                try
                {
                    using (var audioFile = new AudioFileReader(path))
                    {
                        waveOutputDevice.Init(audioFile);
                        waveOutputDevice.Volume = volume;
                        waveOutputDevice.Play();
                        while (waveOutputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(1000);
                        }
                    
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing music: {ex.Message}");
                }
            }
            else if (asioOutDevice != null)
            {
                asioOutDevice.ChannelOffset = asioOutputChannel;
                using (var reader = new AudioFileReader(path))
                { 
                    asioOutDevice.Init(reader);
                    reader.Volume = volume;
                    asioOutDevice.Play();
                    while(asioOutDevice.PlaybackState == PlaybackState.Playing)
                        Thread.Sleep(1000);
                }
            }
            else if (dsOutputDevice != null)
            {
                try
                {
                    using (var audioFile = new AudioFileReader(path))
                    {
                        dsOutputDevice.Init(audioFile);
                        dsOutputDevice.Volume = volume;
                        waveOutputDevice.Play();
                        while (waveOutputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing music: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No audio playback selected");
            }

            
        }

    }
}
