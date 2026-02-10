using System;

namespace MidiPlayer
{
    internal class MusicItem : IComparable<MusicItem>
    {
        private int id = 0;
        private readonly string path;

        public MusicItem(int id, string path)
        {
            this.id = id;
            this.path = path;
        }

        public void SetId(int id)
        {
            this.id = id;
        }

        public string GetPath()
        {
            return this.path;
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
            return this.id - other.id;
        }

        public override string ToString()
        {
            return $"[id: {this.id}; path: {this.path}]";
        }

    }
}
