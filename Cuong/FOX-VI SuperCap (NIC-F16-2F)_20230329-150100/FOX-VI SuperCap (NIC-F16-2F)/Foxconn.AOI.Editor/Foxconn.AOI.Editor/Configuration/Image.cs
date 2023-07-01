using System.Collections.Generic;

namespace Foxconn.AOI.Editor.Configuration
{
    public class Image
    {
        private List<ImageBlock> _blocks { get; set; }
        private int _width { get; set; }
        private int _height { get; set; }
        private int _rows { get; set; }
        private int _columns { get; set; }

        public List<ImageBlock> Blocks
        {
            get => _blocks;
            set => _blocks = value;
        }

        public int Width
        {
            get => _width;
            set => _width = value;
        }

        public int Height
        {
            get => _height;
            set => _height = value;
        }

        public int Rows
        {
            get => _rows;
            set => _rows = value;
        }

        public int Columns
        {
            get => _columns;
            set => _columns = value;
        }

        public Image()
        {
            _blocks = new List<ImageBlock>();
            _width = 0;
            _height = 0;
            _rows = 0;
            _columns = 0;
        }

        public void Dispose()
        {
            for (int i = 0; i < _blocks.Count; i++)
                _blocks[i].Dispose();
        }
    }
}
