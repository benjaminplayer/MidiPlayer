using System;

namespace MidiPlayer
{
    internal class MusicItem : IComparable<MusicItem>
    {
        //private int id = 0;
        public int ID { get; private set; }
        public string path { get; private set; }

        public MusicItem(int id, string path)
        {
            this.ID = id;
            this.path = path;
        }

        public void SetId(int id)
        {
            this.ID = id;
        }

        public string GetFileName()
        {
            string[] temp = this.path.Trim().Split('\\');
            return temp[temp.Length-1];
        }

        public override bool Equals(object obj)
        {
            if (obj is MusicItem other)
                return this.path.Equals(other.path);
            return false;
        }

        public override int GetHashCode()
        {
            return path?.GetHashCode() ?? 0;
        }

        public int CompareTo(MusicItem other)
        { 
            return this.ID - other.ID;
        }

        public override string ToString()
        {
            return $"[id: {this.ID}; path: {this.path}]";
        }

    }
}
