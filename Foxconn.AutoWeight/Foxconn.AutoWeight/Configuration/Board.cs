using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Foxconn.AutoWeight.Configuration
{
    public class Board
    {
        public string Name { get; set; }
        private Image _imageBoard { get; set; }
        public List<FOV> FOVs { get; set; }
        private ObservableCollection<FOV> _FOVs { get; set; }
        public Board()
        {
            Name = "";
            _imageBoard = new Image();
            FOVs = new List<FOV>();
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
                };
                currentFOV.SMDs.Add(smditem);
            }
        }

        public void SortFOV()
        {
            for (int i = 0; i < FOVs.Count; i++)
            {
                FOVs[i].ID = i;
                FOVs[i].Name = $"FOV_{i}";
            }
        }

        public void SortSMD(int smdId)
        {
            FOV currentFOV = FOVs.Find(x => x.ID == smdId);
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

        public Image<Bgr, byte> GetImageBlock(SMD itemSMD)
        {
            foreach (var itemFOV in _FOVs)
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
