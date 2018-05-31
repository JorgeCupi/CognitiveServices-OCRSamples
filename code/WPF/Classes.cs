using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF
{
    public class CognitiveResult
    {
        public string TextAngle { get; set; }
        public string Orientation { get; set; }
        public string Language { get; set; }
        public List<Region> Regions { get; set; }
    }
    public class Region
    {
        public string BoundingBox { get; set; }
        public List<Line> Lines { get; set; }
    }

    public class Line
    {
        public string BoundingBox { get; set; }
        public List<Word> Words { get; set; }
    }

    public class Word
    {
        public string BoundingBox { get; set; }
        public string Text { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }
    }
}
