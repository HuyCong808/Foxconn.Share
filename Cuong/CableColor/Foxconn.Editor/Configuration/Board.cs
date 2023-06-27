using Emgu.CV;
using Emgu.CV.Structure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Foxconn.Editor.Configuration
{
    public class Board
    {
        private string _name { get; set; }
        private Image _imageBoard { get; set; }
        private List<FOV> _FOVs { get; set; }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public Image ImageBoard
        {
            get => _imageBoard;
            set => _imageBoard = value;
        }

        public List<FOV> FOVs
        {
            get => _FOVs;
            set => _FOVs = value;
        }

        public Board()
        {
            _name = "";
            _imageBoard = new Image();
            _FOVs = new List<FOV>();
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

                    //if (_program.ImageBoard != null)
                    //{
                    //    for (int i = 0; i < _program.ImageBoard.Blocks.Count; i++)
                    //    {
                    //        bool loaded = _program.ImageBoard.Blocks[i].Load($"data\\images\\image_{_imageBoard.Blocks[i].Name}.png");
                    //        if (!loaded)
                    //        {
                    //            _program.ImageBoard.Dispose();
                    //        }
                    //    }
                    //}
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
                        _imageBoard.Blocks[i].Image = null;
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

        public void AddFOV()
        {
            int id = FOVs.Count;
            FOV fovitem = new FOV()
            {
                ID = id,
                Name = $"FOV_{id}"
            };
            FOVs.Add(fovitem);
            AddImageBlock(fovitem.ImageBlockName);
        }

        public void RemoveFOV(FOV item)
        {
            FOVs.Remove(item);
            RemoveImageBlock(item.ImageBlockName);
            SortFOV();
        }

        public void AddSMD(int fovID)
        {
            var currentFOV = FOVs.Find(x => x.ID == fovID);
            if (currentFOV != null)
            {
                int id = currentFOV.SMDs.Count;
                SMD smditem = new SMD()
                {
                    Id = id,
                    name = $"SMD_{id}",
                    FOV_ID = fovID,
                };
                currentFOV.SMDs.Add(smditem);
            }
        }

        public void RemoveSMD(SMD smd)
        {
            _FOVs[smd.FOV_ID].SMDs.Remove(smd);
            SortSMD(smd.FOV_ID);
        }

        public void SortFOV()
        {
            for (int i = 0; i < FOVs.Count; i++)
            {
                FOVs[i].ID = i;
                FOVs[i].Name = $"FOV_{i}";
            }
        }

        public void SortSMD(int fovID)
        {
            FOV currentFOV = FOVs.Find(x => x.ID == fovID);
            int i = 0;
            if (currentFOV != null)
            {
                foreach (var smd in currentFOV.SMDs)
                {
                    smd.Id = i;
                    smd.name = $"SMD_{i}";
                    i++;
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
                if (itemFOV.ID == itemSMD.FOV_ID)
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
