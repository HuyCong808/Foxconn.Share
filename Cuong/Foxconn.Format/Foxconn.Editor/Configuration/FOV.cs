using Foxconn.Editor.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Foxconn.Editor.Configuration
{
    public class FOV :NotifyProperty
    {
        private int _id { get; set; }
        private string _name { get; set; }
        private string _description { get; set; }
        private bool _isEnabled { get; set; }
        private string _imageBlockName { get; set; }
        private int _exposureTime { get; set; }
        private FOVType _FOVType { get; set; }
        private CameraMode _cameraMode { get; set; }
        private ObservableCollection<SMD> _SMDs { get; set; }

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                NotifyPropertyChanged(nameof(Id));
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



        public FOVType FOVType
        {
            get => _FOVType;
            set
            {
                _FOVType = value;
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
        public ObservableCollection<SMD> SMDs
        {
            get => _SMDs;
            set
            {
                _SMDs = value;
                NotifyPropertyChanged(nameof(SMDs));
            }
        }

        public FOV()
        {
            _id = 0;
            _name = "";
            _description = string.Empty;
            _isEnabled = true;
            _imageBlockName = DateTime.Now.ToString("yyyyMMddHHmmssffffff");
            _exposureTime = 1000;
            _FOVType = FOVType.Unknow;
            _cameraMode = CameraMode.Unknow;
            _SMDs = new ObservableCollection<SMD>();

        }
       
        public void SortByName()
        {
            var sorted = _SMDs.OrderBy(i => i.Name);
            _SMDs = new ObservableCollection<SMD>();
            for (int i = 0; i < sorted.Count(); i++)
            {
                SMD item = sorted.ElementAt(i);
                item.Id = i;
                item.Name = $"SMD_{i}";
                _SMDs.Add(item);
            }
        }


    }
}
