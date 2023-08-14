using Emgu.CV;
using Emgu.CV.Structure;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Foxconn.Editor.Configuration
{
    public class Board : NotifyProperty
    {
        private string _name { get; set; }
        private Image _imageBoard { get; set; }
        private ObservableCollection<FOV> _FOVs { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public Image ImageBoard
        {
            get => _imageBoard;
            set
            {
                _imageBoard = value;
                NotifyPropertyChanged(nameof(ImageBoard));
            }
        }

        public ObservableCollection<FOV> FOVs
        {
            get => _FOVs;
            set
            {
                _FOVs = value;
                NotifyPropertyChanged(nameof(FOVs));
            }
        }

        public Board()
        {
            _name = "DEFAULT_PROGRAM";
            _imageBoard = new Image();
            _FOVs = new ObservableCollection<FOV>();
        }


        public void LoadProgram()
        {
            try
            {
                Board _program = null;
                string _filePath = @"data\board.json";
                if (File.Exists(_filePath))
                {
                    Board data = JsonConvert.DeserializeObject<Board>(File.ReadAllText(_filePath));
                    if (data != null)
                    {
                        _program = data;
                    }
                }
                else
                {
                    _program = new Board();
                    _program.Name = "DEFAULT_PROGRAM";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SaveProgram()
        {
            try
            {
                string _filePath = @"data\board.json";
                Image<Bgr, byte>[] imageArray = new Image<Bgr, byte>[0];
                if (_imageBoard != null)
                {
                    imageArray = new Image<Bgr, byte>[_imageBoard.Blocks.Count];
                    for (int i = 0; i < _imageBoard.Blocks.Count; i++)
                    {
                        _imageBoard.Blocks[i].Save($"data\\images\\image_{_imageBoard.Blocks[i].Name}.png");
                        imageArray[i] = _imageBoard.Blocks[i].Image;
                        //_imageBoard.Blocks[i].Image = null;
                    }
                }
                else
                {
                    _imageBoard = null;
                }
                string data = JsonConvert.SerializeObject(this);
                File.WriteAllText(_filePath, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public FOV GetFOV(SMD smd)
        {
            if (smd != null)
            {
                return _FOVs[smd.FOV_ID];
            }
            else
            {
                return null;
            }
        }

        public FOV AddFOV()
        {
            int id = _FOVs.Count;
            FOV fovitem = new FOV()
            {
                Id = id,
                Name = $"FOV_{id}"
            };
            _FOVs.Add(fovitem);
            AddImageBlock(fovitem.ImageBlockName);
            SortByName();
            return fovitem;
        }

        public void RemoveFOV(FOV item)
        {
            _FOVs.Remove(item);
            RemoveImageBlock(item.ImageBlockName);
            SortByName();
        }

        public SMD AddSMD(int FOVID)
        {
            int id = _FOVs[FOVID].SMDs.Count;
            SMD item = new SMD
            {
                Id = id,
                Name = $"SMD_{id}",
                FOV_ID = id
            };
            _FOVs[FOVID].SMDs.Add(item);
            SortByName();
            return item;
        }

        public void RemoveSMD(SMD smd)
        {
            _FOVs[smd.FOV_ID].SMDs.Remove(smd);
            SortByName();
        }

        public void SortByName()
        {
            IOrderedEnumerable<FOV> sorted = _FOVs.OrderBy(i => i.Name);
            _FOVs = new ObservableCollection<FOV>();
            for (int i = 0; i < sorted.Count(); i++)
            {
                FOV itemFOV = sorted.ElementAt(i);
                itemFOV.Id = i;
                itemFOV.Name = $"FOV_{i}";
                itemFOV.SortByName();
                _FOVs.Add(itemFOV);
                foreach (SMD itemSMD in itemFOV.SMDs)
                {
                    itemSMD.FOV_ID = i;
                }
            }
        }

        public void AddImageBlock(string blockName)
        {
            ImageBlock block = _imageBoard.Blocks.Find(x => x.Name == blockName);
            if (block == null)
            {
                ImageBlock item = new ImageBlock(0, blockName, null);
                _imageBoard.Blocks.Add(item);
            }
        }

        public void RemoveImageBlock(string blockName)
        {
            ImageBlock block = _imageBoard.Blocks.Find(x => x.Name == blockName);
            if (block != null)
            {
                _imageBoard.Blocks.Remove(block);
            }
        }

        public Image<Bgr, byte> GetImageBlock(string blockName)
        {
            ImageBlock block = _imageBoard.Blocks.Find(x => x.Name == blockName);
            if (block != null)
            {
                return block.Image;
            }
            return null;
        }

        public Image<Bgr, byte> GetImageBlock(SMD itemSMD)
        {
            foreach (var itemFOV in FOVs)
            {
                if (itemFOV.Id == itemSMD.FOV_ID)
                {
                    ImageBlock block = _imageBoard.Blocks.Find(x => x.Name == itemFOV.ImageBlockName);
                    if (block != null)
                    {
                        return block.Image;
                    }
                    break;
                }
            }
            return null;
        }

        public void SetImageBlock(string blockName, System.Drawing.Bitmap bmp)
        {
            ImageBlock block = _imageBoard.Blocks.Find(x => x.Name == blockName);
            if (block != null)
            {
                block.Image = bmp.ToImage<Bgr, byte>();
            }
        }



    }
}
