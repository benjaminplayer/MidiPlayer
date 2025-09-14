using NAudio.Wave;
using System;
using System.Threading;

namespace MidiPlayer
{
    internal class AudioHandler
    {
        private WaveOutEvent outputDevice;
        private Thread audioThread;
        public AudioHandler() { }

        public AudioHandler(WaveOutEvent outputDevice)
        {
            this.outputDevice = outputDevice;
        }

        public void Play(string path)
        {
            Console.WriteLine("musicThread == null || !musicThread.IsAlive"+(audioThread == null || !audioThread.IsAlive));
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
            if (outputDevice != null)
            {
                outputDevice.Stop();
                outputDevice.Dispose();
                outputDevice = null;
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
            try
            {
                using (var audioFile = new AudioFileReader(path))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
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

    }
}
