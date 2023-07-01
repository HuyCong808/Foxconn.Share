using Foxconn.AOI.Editor.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Foxconn.AOI.Editor.Configuration
{
    public class FOV : NotifyProperty
    {
        private int _id { get; set; }
        private string _name { get; set; }
        private string _description { get; set; }
        private bool _isEnabled { get; set; }
        private string _imageBlockName { get; set; }
        private int _exposureTime { get; set; }
        private ObservableCollection<SMD> _SMDs { get; set; }
        private FOVType _type { get; set; }
        private CameraMode _cameraMode { get; set; }
        private FOVPosition _FOVPosition { get; set; }

        public int ID
        {
            get => _id;
            set
            {
                _id = value;
                NotifyPropertyChanged(nameof(ID));
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                NotifyPropertyChanged(nameof(Description));
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                if (_SMDs != null)
                {
                    for (int i = 0; i < _SMDs.Count; i++)
                    {
                        _SMDs[i].IsEnabled = value;
                    }
                }
                NotifyPropertyChanged(nameof(IsEnabled));
            }
        }

        public string ImageBlockName
        {
            get => _imageBlockName;
            set
            {
                _imageBlockName = value;
                NotifyPropertyChanged(nameof(ImageBlockName));
            }
        }

        public int ExposureTime
        {
            get => _exposureTime;
            set
            {
                _exposureTime = value;
                NotifyPropertyChanged(nameof(ExposureTime));
            }
        }

        public ObservableCollection<SMD> SMDs
        {
            get => _SMDs;
            set
            {
                _SMDs = value;
                NotifyPropertyChanged(nameof(SMDs));
            }
        }

        public FOVType Type
        {
            get => _type;
            set
            {
                _type = value;
                NotifyPropertyChanged(nameof(Type));
            }
        }

        public CameraMode CameraMode
        {
            get => _cameraMode;
            set
            {
                _cameraMode = value;
                NotifyPropertyChanged(nameof(CameraMode));
            }
        }

        public FOVPosition FOVPosition
        {
            get => _FOVPosition;
            set
            {
                _FOVPosition = value;
                NotifyPropertyChanged(nameof(FOVPosition));
            }
        }

        public FOV()
        {
            _id = 0;
            _name = "FOV";
            _description = string.Empty;
            _isEnabled = true;
            _imageBlockName = DateTime.Now.ToString("yyyyMMddHHmmssffffff");
            _exposureTime = 1000;
            _SMDs = new ObservableCollection<SMD>();
            _type = FOVType.Unknow;
            _cameraMode = CameraMode.Unknow;
            _FOVPosition = new FOVPosition();
        }

        public void Dispose()
        {
            foreach (var item in SMDs)
                item.Dispose();
        }

        public void SortByName()
        {
            var sorted = _SMDs.OrderBy(i => i.Name);
            _SMDs = new ObservableCollection<SMD>();
            for (int i = 0; i < sorted.Count(); i++)
            {
                SMD item = sorted.ElementAt(i);
                item.ID = i;
                item.Name = $"SMD_{i:D3}";
                _SMDs.Add(item);
            }
        }
    }
}
