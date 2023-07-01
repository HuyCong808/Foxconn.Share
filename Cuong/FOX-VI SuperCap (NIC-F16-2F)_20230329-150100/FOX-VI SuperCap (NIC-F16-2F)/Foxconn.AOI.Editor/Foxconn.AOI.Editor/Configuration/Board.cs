using Emgu.CV;
using Emgu.CV.Structure;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Foxconn.AOI.Editor.Configuration
{
    public class Board : NotifyProperty
    {
        private string _name { get; set; }
        private double _boardLength { get; set; }
        private double _boardWidth { get; set; }
        private double _boardThickness { get; set; }
        private Image _imageBoard { get; set; }
        private ObservableCollection<FOV> _FOVs { get; set; }
        private BackupDevice _backupDevices { get; set; }
        private ObservableCollection<Server> _servers { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public double BoardLength
        {
            get => _boardLength;
            set
            {
                _boardLength = value;
                NotifyPropertyChanged(nameof(BoardLength));
            }
        }

        public double BoardWidth
        {
            get => _boardWidth;
            set
            {
                _boardWidth = value;
                NotifyPropertyChanged(nameof(_boardWidth));
            }
        }

        public double BoardThickness
        {
            get => _boardThickness;
            set
            {
                _boardThickness = value;
                NotifyPropertyChanged(nameof(BoardThickness));
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

        public BackupDevice BackupDevices
        {
            get => _backupDevices;
            set
            {
                _backupDevices = value;
                NotifyPropertyChanged(nameof(BackupDevices));
            }
        }

        public ObservableCollection<Server> Servers
        {
            get => _servers;
            set
            {
                _servers = value;
                NotifyPropertyChanged(nameof(Servers));
            }
        }

        public Board()
        {
            _name = "DEFAULT_PROGRAM";
            _boardLength = 200;
            _boardWidth = 150;
            _boardThickness = 1;
            _imageBoard = new Image();
            _FOVs = new ObservableCollection<FOV>();
            _backupDevices = new BackupDevice();
            _servers = new ObservableCollection<Server>();
        }

        public void Init()
        {
        }

        public void Dispose()
        {
            foreach (var item in _FOVs)
                item?.Dispose();
            _imageBoard?.Dispose();
        }

        public Board Load(string filename)
        {
            try
            {
                Board program = null;
                FileInfo fileInfo = new FileInfo(filename);
                string tempPath = @$"temp\models";
                string tempFilename = @$"{tempPath}\{fileInfo.Name}";
                string tempProgramPath = tempFilename.Replace(".jbn", "");
                string tempProgramFile = @$"{tempProgramPath}\program.json";
                FileExplorer.DeleteDirectory(tempPath);
                FileExplorer.CreateDirectory(tempPath);
                File.Copy(filename, tempFilename);
                ZipFile.ExtractToDirectory(filename, tempFilename.Replace(".jbn", ""));
                string contents = File.ReadAllText(tempProgramFile);
                program = JsonConvert.DeserializeObject<Board>(contents);
                program.Init();
                if (program.ImageBoard != null)
                {
                    for (int i = 0; i < program.ImageBoard.Blocks.Count; i++)
                    {
                        bool loaded = program.ImageBoard.Blocks[i].Load(tempProgramPath);
                        if (!loaded)
                        {
                            program.ImageBoard.Dispose();
                            return null;
                        }
                    }
                }
                FileExplorer.DeleteFiles(tempPath, true);
                return program;
            }
            catch
            {
                return null;
            }
        }

        public bool SaveProgram(string filename, bool includeImages = false)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filename);
                string tempPath = @$"temp\models";
                string tempFilename = @$"{tempPath}\{fileInfo.Name}";
                string tempProgramPath = tempFilename.Replace(".jbn", "");
                string tempProgramFile = @$"{tempProgramPath}\program.json";
                FileExplorer.CreateDirectory(tempProgramPath);
                FileExplorer.DeleteFiles(tempProgramPath, false);
                Image tempImages = _imageBoard;
                Image<Bgr, byte>[] imageArray = new Image<Bgr, byte>[0];
                if (includeImages)
                {
                    if (_imageBoard != null)
                    {
                        imageArray = new Image<Bgr, byte>[_imageBoard.Blocks.Count];
                        for (int i = 0; i < _imageBoard.Blocks.Count; i++)
                        {
                            _imageBoard.Blocks[i].Save(tempProgramPath);
                            imageArray[i] = _imageBoard.Blocks[i].Image;
                            _imageBoard.Blocks[i].Image = null;
                        }
                    }
                }
                else
                {
                    _imageBoard = null;
                }
                string contents = JsonConvert.SerializeObject(this);
                File.WriteAllText(tempProgramFile, contents);
                File.Delete(fileInfo.FullName);
                ZipFile.CreateFromDirectory(tempProgramPath, fileInfo.FullName);
                if (includeImages)
                {
                    if (_imageBoard != null)
                    {
                        for (int i = 0; i < _imageBoard.Blocks.Count; i++)
                        {
                            _imageBoard.Blocks[i].Image = imageArray[i];
                        }
                    }
                }
                else
                {
                    _imageBoard = tempImages;
                }
                FileExplorer.DeleteFiles(tempProgramPath, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SaveProgramAndImage(string filename)
        {
            return false;
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
            FOV item = new FOV
            {
                ID = id,
                Name = $"FOV_{id:D3}"
            };
            _FOVs.Add(item);
            AddImageBlock(item.ImageBlockName);
            SortByName();
            return item;
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
                ID = id,
                Name = $"SMD_{id:D3}",
                FOV_ID = id
            };
            _FOVs[FOVID].SMDs.Add(item);
            SortByName();
            return item;
        }

        public void RemoveSMD(SMD item)
        {
            _FOVs[item.FOV_ID].SMDs.Remove(item);
            SortByName();
        }

        public void SortByName()
        {
            IOrderedEnumerable<FOV> sorted = _FOVs.OrderBy(i => i.Name);
            _FOVs = new ObservableCollection<FOV>();
            for (int i = 0; i < sorted.Count(); i++)
            {
                FOV itemFOV = sorted.ElementAt(i);
                itemFOV.ID = i;
                itemFOV.Name = $"FOV_{i:D3}";
                itemFOV.SortByName();
                _FOVs.Add(itemFOV);
                foreach (SMD itemSMD in itemFOV.SMDs)
                {
                    itemSMD.FOV_ID = i;
                }
            }
        }

        public object Find(string obj)
        {
            foreach (FOV itemFOV in _FOVs)
            {
                if (itemFOV.Name.Contains(obj))
                {
                    return itemFOV;
                }
                else
                {
                    foreach (SMD itemSMD in itemFOV.SMDs)
                    {
                        if (itemSMD.Name.Contains(obj))
                        {
                            return itemSMD;
                        }
                    }
                }
            }
            return null;
        }

        public int GetExposureTime(int FOV_ID)
        {
            int exposureTime = 0;
            foreach (var item in _FOVs)
            {
                if (item.ID == FOV_ID)
                {
                    exposureTime = item.ExposureTime;
                    break;
                }
            }
            return exposureTime;
        }

        public void SetExposureTime(int FOV_ID, int value)
        {
            foreach (var item in _FOVs)
            {
                if (item.ID == FOV_ID)
                {
                    item.ExposureTime = value;
                }
            }
        }

        private void AddImageBlock(string blockName)
        {
            ImageBlock block = _imageBoard.Blocks.Find(x => x.Name == blockName);
            if (block == null)
            {
                ImageBlock item = new ImageBlock(0, blockName, null);
                _imageBoard.Blocks.Add(item);
            }
        }

        private void RemoveImageBlock(string blockName)
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

        public void AddServer()
        {
            int id = _servers.Count;
            Server item = new Server
            {
                ID = id,
                Name = $"SERVER_{id:D3}"
            };
            _servers.Add(item);
            SortServerByName();
        }

        public void RemoveServer(Server item)
        {
            _servers.Remove(item);
            SortServerByName();
        }

        public void SortServerByName()
        {
            IOrderedEnumerable<Server> sorted = _servers.OrderBy(i => i.Name);
            _servers = new ObservableCollection<Server>();
            for (int i = 0; i < sorted.Count(); i++)
            {
                Server item = sorted.ElementAt(i);
                item.ID = i;
                item.Name = $"SERVER_{i:D3}";
                _servers.Add(item);
            }
        }
    }
}
