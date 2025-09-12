using System;
using System.IO;
using NAudio.Wave;

namespace MidiTest
{
    public class SoundManager
    {
        private string directoryPath;

        public SoundManager(string directoryPath)
        {
            this.directoryPath = directoryPath;
        }


        public void PlayMusicFromDir()
        {
            if (directoryPath == null)
            {
                Console.WriteLine("The directory path was not specified!");
                return;
            }

            try
            {
                var mp3files = Directory.EnumerateFiles(directoryPath, "*.mp3"); //gets all the .mp3 files from the directory

                //Plays every song in the directory specified
                foreach (string current in mp3files)
                {
                    using (var audioFile = new AudioFileReader(current))
                    {
                        using (var outputDevice = new WaveOutEvent())
                        {
                            outputDevice.Init(audioFile);
                            outputDevice.Volume = .1f;
                            outputDevice.Play();

                            Console.WriteLine("Playing... press any key to stop");
                            Console.ReadKey();

                            outputDevice.Stop();
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("An error accured: " + ex.Message);
            }
        }

    }
}

