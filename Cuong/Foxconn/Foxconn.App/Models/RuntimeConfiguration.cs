using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.App.Controllers.Variable;
using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Foxconn.App.Models
{
    public class RuntimeConfiguration
    {
        #region Position
        public class Position
        {

            #region Status
            public class Status
            {
                public string Init { get; set; }
                public string Pass { get; set; }
                public string Fail { get; set; }
                public string Repair { get; set; }
                public string InitInit { get; set; }
                public string PassPass { get; set; }
                public string FailFail { get; set; }
                public string PassFail { get; set; }
                public string FailPass { get; set; }
                public string Ready { get; set; }

                public Status()
                {
                    Init = null;
                    Pass = null;
                    Fail = null;
                    Repair = null;
                    InitInit = null;
                    PassPass = null;
                    FailFail = null;
                    PassFail = null;
                    FailPass = null;
                    Ready = null;
                }

                public Status Clone()
                {
                    return new Status()
                    {
                        Init = Init,
                        Pass = Pass,
                        Fail = Fail,
                        Repair = Repair,
                        InitInit = InitInit,
                        PassPass = PassPass,
                        FailFail = FailFail,
                        PassFail = PassFail,
                        FailPass = FailPass,
                        Ready = Ready,
                    };
                }
            }
            #endregion

            #region PLC
            public class PlcConfiguration
            {
                public string MovePickup { get; set; }
                public string SavePickup { get; set; }
                public string MoveDropdown { get; set; }
                public string SaveDropdown { get; set; }
                public Status Status { get; set; }

                public PlcConfiguration()
                {
                    MovePickup = null;
                    SavePickup = null;
                    MoveDropdown = null;
                    SaveDropdown = null;
                    Status = new Status();
                }

                public PlcConfiguration Clone()
                {
                    return new PlcConfiguration()
                    {
                        MovePickup = MovePickup,
                        SavePickup = SavePickup,
                        MoveDropdown = MoveDropdown,
                        SaveDropdown = SaveDropdown,
                        Status = Status?.Clone(),
                    };
                }
            }
            #endregion

            #region Robot
            public class RobotConfiguration
            {
                public string MovePickup { get; set; }
                public string SavePickup { get; set; }
                public string MoveDropdown { get; set; }
                public string SaveDropdown { get; set; }
                public Status Status { get; set; }

                public RobotConfiguration()
                {
                    MovePickup = null;
                    SavePickup = null;
                    MoveDropdown = null;
                    SaveDropdown = null;
                    Status = new Status();
                }

                public RobotConfiguration Clone()
                {
                    return new RobotConfiguration()
                    {
                        MovePickup = MovePickup,
                        SavePickup = SavePickup,
                        MoveDropdown = MoveDropdown,
                        SaveDropdown = SaveDropdown,
                        Status = Status?.Clone(),
                    };
                }
            }
            #endregion

            #region Server
            public class ServerConfiguration
            {
                public string Host { get; set; }
                public int Port { get; set; }

                public ServerConfiguration()
                {
                    Host = "127.0.0.1";
                    Port = 27000;
                }

                public ServerConfiguration Clone()
                {
                    return new ServerConfiguration()
                    {
                        Host = Host,
                        Port = Port,
                    };
                }
            }
            #endregion

            #region Client
            public class ClientConfiguration
            {
                public string Host { get; set; }
                public int Port { get; set; }

                public ClientConfiguration()
                {
                    Host = "127.0.0.1";
                    Port = 27000;
                }

                public ClientConfiguration Clone()
                {
                    return new ClientConfiguration()
                    {
                        Host = Host,
                        Port = Port,
                    };
                }
            }
            #endregion

            public bool Enable { get; set; }
            public int Index { get; set; }
            public string Alias { get; set; }
            public bool IsServer { get; set; }
            public bool IsClient { get; set; }
            public ModelName ModelName { get; set; }
            public PlcConfiguration Plc { get; set; }
            public RobotConfiguration Robot { get; set; }
            public ServerConfiguration Server { get; set; }
            public ClientConfiguration Client { get; set; }

            public Position()
            {
                Enable = true;
                Index = 0;
                Alias = null;
                IsServer = false;
                IsClient = false;
                ModelName = ModelName.None;
                Plc = new PlcConfiguration();
                Robot = new RobotConfiguration();
                Server = new ServerConfiguration();
                Client = new ClientConfiguration();
            }

            public Position Clone()
            {
                return new Position()
                {
                    Enable = Enable,
                    Index = Index,
                    Alias = Alias,
                    IsServer = IsServer,
                    IsClient = IsClient,
                    ModelName = ModelName,
                    Plc = Plc?.Clone(),
                    Robot = Robot?.Clone(),
                    Server = Server?.Clone(),
                    Client = Client?.Clone(),
                };
            }

        }
        #endregion

        #region Step
        public class StepConfiguration
        {
            public class CameraInformation
            {
                public string UserDefinedName { get; set; }
                public string ModelName { get; set; }
                public string SerialNumber { get; set; }
                public double ExposureTime { get; set; }
                public double Gain { get; set; }
                public double Rotate { get; set; }

                public CameraInformation()
                {
                    UserDefinedName = null;
                    ModelName = null;
                    SerialNumber = null;
                    ExposureTime = 10000;
                    Gain = 0;
                    Rotate = 0;
                }

                public CameraInformation Clone()
                {
                    return new CameraInformation()
                    {
                        UserDefinedName = UserDefinedName,
                        ModelName = ModelName,
                        SerialNumber = SerialNumber,
                        ExposureTime = ExposureTime,
                        Gain = Gain,
                        Rotate = Rotate
                    };
                }
            }

            public class ImagePreprocessing
            {
                public class Prop<T> : ICloneable
                {
                    public bool Enable { get; set; }
                    public T Value { get; set; }
                    public object Clone()
                    {
                        return MemberwiseClone();
                    }
                }

                public class ThresholdImage
                {
                    public bool Enable { get; set; }
                    public ThresholdType Type { get; set; }
                    public ThresholdImageBinary Binary { get; set; }
                    public ThresholdImageAdaptive Adaptive { get; set; }
                    public ThresholdImageOtsu Otsu { get; set; }
                    public bool Inverse { get; set; }

                    public ThresholdImage()
                    {
                        Enable = false;
                        Type = ThresholdType.None;
                        Binary = new ThresholdImageBinary();
                        Adaptive = new ThresholdImageAdaptive();
                        Otsu = new ThresholdImageOtsu();
                        Inverse = false;
                    }

                    public ThresholdImage Clone()
                    {
                        return new ThresholdImage()
                        {
                            Enable = Enable,
                            Type = Type,
                            Binary = Binary?.Clone(),
                            Adaptive = Adaptive?.Clone(),
                            Otsu = Otsu?.Clone(),
                            Inverse = Inverse,
                        };
                    }

                    ~ThresholdImage() { }

                    public class ThresholdImageBinary
                    {
                        public int Value { get; set; }

                        public ThresholdImageBinary()
                        {
                            Value = 180;
                        }

                        public ThresholdImageBinary Clone()
                        {
                            return new ThresholdImageBinary()
                            {
                                Value = Value,
                            };
                        }

                        ~ThresholdImageBinary() { }
                    }

                    public class ThresholdImageAdaptive
                    {
                        public int BlockSize { get; set; }
                        public int Param1 { get; set; }

                        public ThresholdImageAdaptive()
                        {
                            BlockSize = 15;
                            Param1 = 5;
                        }

                        public ThresholdImageAdaptive Clone()
                        {
                            return new ThresholdImageAdaptive()
                            {
                                BlockSize = BlockSize,
                                Param1 = Param1,
                            };
                        }

                        ~ThresholdImageAdaptive() { }
                    }

                    public class ThresholdImageOtsu
                    {
                        public int Value { get; set; }

                        public ThresholdImageOtsu()
                        {
                            Value = 128;
                        }

                        public ThresholdImageOtsu Clone()
                        {
                            return new ThresholdImageOtsu()
                            {
                                Value = Value,
                            };
                        }

                        ~ThresholdImageOtsu() { }
                    }
                }

                public class BlurPropImage<T> : Prop<T>
                {
                    public BlurPropImage()
                    {
                        Enable = false;
                        Value = (T)(object)3;
                    }

                    public new BlurPropImage<T> Clone()
                    {
                        return new BlurPropImage<T>()
                        {
                            Enable = Enable,
                            Value = Value,
                        };
                    }
                }

                public class BlobPropImage<T> : Prop<T>
                {
                    public int DilateIteration { get; set; }
                    public int ErodeIteration { get; set; }

                    public BlobPropImage()
                    {
                        Enable = false;
                        Value = (T)(object)3;
                        DilateIteration = 1;
                        ErodeIteration = 2;
                    }

                    public new BlobPropImage<T> Clone()
                    {
                        return new BlobPropImage<T>()
                        {
                            Enable = Enable,
                            Value = Value,
                            DilateIteration = DilateIteration,
                            ErodeIteration = ErodeIteration,
                        };
                    }
                }

                public ThresholdImage Threshold { get; set; }
                public BlurPropImage<int> BlurProp { get; set; }
                public BlobPropImage<int> BlobProp { get; set; }

                public ImagePreprocessing()
                {
                    Threshold = new ThresholdImage();
                    BlurProp = new BlurPropImage<int>();
                    BlobProp = new BlobPropImage<int>();
                }

                public ImagePreprocessing Clone()
                {
                    return new ImagePreprocessing()
                    {
                        Threshold = Threshold?.Clone(),
                        BlurProp = BlurProp?.Clone(),
                        BlobProp = BlobProp?.Clone(),
                    };
                }
            }

            public class FeatureMatching
            {
                [BsonIgnore]
                public Image<Bgr, byte> Image { get; set; }
                public string ImagePath { get; set; }
                public double Score { get; set; }
                public bool Inverted { get; set; }

                public FeatureMatching()
                {
                    Image = null;
                    ImagePath = null;
                    Score = 0.8;
                    Inverted = false;
                }

                public FeatureMatching Clone()
                {
                    return new FeatureMatching()
                    {
                        Image = Image?.Clone(),
                        ImagePath = ImagePath,
                        Score = Score,
                        Inverted = Inverted,
                    };
                }

                ~FeatureMatching()
                {
                    Image?.Dispose();
                }

                /// <summary>
                /// Load image from disk
                /// </summary>
                public void LoadImage()
                {
                    if (System.IO.File.Exists(ImagePath))
                    {
                        Image = new Image<Bgr, byte>(ImagePath);
                    }
                }
            }

            public class TemplateMatching
            {
                [BsonIgnore]
                public Image<Bgr, byte> Image { get; set; }
                public string ImagePath { get; set; }
                public double Score { get; set; }
                public bool Inverted { get; set; }

                public TemplateMatching()
                {
                    Image = null;
                    ImagePath = null;
                    Score = 0.8;
                    Inverted = false;
                }

                public TemplateMatching Clone()
                {
                    return new TemplateMatching()
                    {
                        Image = Image?.Clone(),
                        ImagePath = ImagePath,
                        Score = Score,
                        Inverted = Inverted,
                    };
                }

                ~TemplateMatching()
                {
                    Image?.Dispose();
                }

                /// <summary>
                /// Load image from disk
                /// </summary>
                public void LoadImage()
                {
                    if (System.IO.File.Exists(ImagePath))
                    {
                        Image = new Image<Bgr, byte>(ImagePath);
                    }
                }
            }

            public class BarcodeDetection
            {
                public BarcodeMode Mode { get; set; }
                public BarcodeType Type { get; set; }
                public int Length { get; set; }

                public BarcodeDetection()
                {
                    Mode = BarcodeMode.ZXing;
                    Type = BarcodeType.Linear;
                    Length = 0;
                }

                public BarcodeDetection Clone()
                {
                    return new BarcodeDetection()
                    {
                        Mode = Mode,
                        Type = Type,
                        Length = Length,
                    };
                }
            }

            public class ContoursDetection
            {
                public int MinWidth { get; set; }
                public int MaxWidth { get; set; }
                public int MinHeight { get; set; }
                public int MaxHeight { get; set; }

                public ContoursDetection()
                {
                    MinWidth = 100;
                    MaxWidth = 2448;
                    MinHeight = 100;
                    MaxHeight = 2048;
                }

                public ContoursDetection Clone()
                {
                    return new ContoursDetection()
                    {
                        MinWidth = MinWidth,
                        MaxWidth = MaxWidth,
                        MinHeight = MinHeight,
                        MaxHeight = MaxHeight,
                    };
                }
            }

            public class ComponentConfiguration
            {
                public bool Enable { get; set; }
                public int Index { get; set; }
                public BRectangle Region { get; set; }
                public BRectangle ObjectRegion { get; set; }
                public ImagePreprocessing Preprocessing { get; set; }
                public Algorithm Algorithm { get; set; }
                public Function Function { get; set; }
                public FeatureMatching FeatureMatching { get; set; }
                public TemplateMatching TemplateMatching { get; set; }
                public BarcodeDetection BarcodeDetection { get; set; }
                public ContoursDetection ContoursDetection { get; set; }
                public bool IsFinish => !Region.IsEmpty;

                public ComponentConfiguration()
                {
                    Enable = true;
                    Index = 0;
                    Region = new BRectangle();
                    ObjectRegion = new BRectangle();
                    Preprocessing = new ImagePreprocessing();
                    Algorithm = Algorithm.None;
                    Function = Function.None;
                    FeatureMatching = new FeatureMatching();
                    TemplateMatching = new TemplateMatching();
                    BarcodeDetection = new BarcodeDetection();
                    ContoursDetection = new ContoursDetection();
                }

                public ComponentConfiguration Clone()
                {
                    return new ComponentConfiguration()
                    {
                        Enable = Enable,
                        Index = Index,
                        Region = Region?.Clone(),
                        ObjectRegion = ObjectRegion?.Clone(),
                        Preprocessing = Preprocessing?.Clone(),
                        Algorithm = Algorithm,
                        Function = Function,
                        FeatureMatching = FeatureMatching?.Clone(),
                        TemplateMatching = TemplateMatching?.Clone(),
                        BarcodeDetection = BarcodeDetection?.Clone(),
                        ContoursDetection = ContoursDetection?.Clone(),
                    };
                }
            }

            public bool Enable { get; set; }
            public int Index { get; set; }
            public CameraInformation Camera { get; set; }
            [BsonIgnore]
            public Image<Bgr, byte> Image { get; set; }
            public string ImagePath { get; set; }
            public List<ComponentConfiguration> Components { get; set; }

            public StepConfiguration()
            {
                Enable = true;
                Index = 0;
                Camera = new CameraInformation();
                ImagePath = null;
                Components = new List<ComponentConfiguration>();
            }

            public StepConfiguration Clone()
            {
                return new StepConfiguration()
                {
                    Enable = Enable,
                    Index = Index,
                    Camera = Camera?.Clone(),
                    Image = Image?.Clone(),
                    ImagePath = ImagePath,
                    Components = Components != null ? new List<ComponentConfiguration>() { new ComponentConfiguration().Clone() } : null,
                };
            }
            /// <summary>
            /// Build a final configuration for saving in database
            /// </summary>
            /// <param name="pathData">Use for only save image in some method like Feature Matching</param>
            /// <returns></returns>
            public StepConfiguration Archive(string pathName, int index)
            {
                var step = Clone();
                if (step.Image != null)
                {
                    var path = GlobalVariable.Data + "\\" + pathName + "\\" + index.ToString() + "\\" + GlobalVariable.ImageName;
                    step.ImagePath = path;
                }
                for (int i = 0; i < step.Components.Count; i++)
                {
                    var component = step.Components[i];
                    if (component.Algorithm == Algorithm.TemplateMatching && component.TemplateMatching.Image != null)
                    {
                        var path = GlobalVariable.Data + "\\" + pathName + "\\" + index.ToString() + "\\" + i.ToString() + "_" + GlobalVariable.TemplateImageName;
                        component.TemplateMatching.ImagePath = path;
                    }
                    if (component.Algorithm == Algorithm.FeatureMatching && component.TemplateMatching.Image != null)
                    {
                        var path = GlobalVariable.Data + "\\" + pathName + "\\" + index.ToString() + "\\" + i.ToString() + "_" + GlobalVariable.FeatureImageName;
                        component.FeatureMatching.ImagePath = path;
                    }
                }
                return step;
            }
            /// <summary>
            /// Load image from disk
            /// </summary>
            public void LoadImage()
            {
                if (System.IO.File.Exists(ImagePath))
                {
                    Image = new Image<Bgr, byte>(ImagePath);
                }
            }
        }
        #endregion

        public BsonObjectId _id { get; set; }
        public string ModelName { get; set; }
        public List<Position> Positions { get; set; }
        public List<StepConfiguration> StepsForCamera0 { get; set; }
        public List<StepConfiguration> StepsForCamera1 { get; set; }
        public List<StepConfiguration> StepsForCamera2 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime DateCreated { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime DateModified { get; set; }

        public RuntimeConfiguration()
        {
            ModelName = string.Empty;
            Positions = new List<Position>();
            StepsForCamera0 = new List<StepConfiguration>();
            StepsForCamera1 = new List<StepConfiguration>();
            StepsForCamera2 = new List<StepConfiguration>();
            DateCreated = DateTime.Now.Date;
            DateModified = DateTime.Now;
        }

        public RuntimeConfiguration Clone()
        {
            return new RuntimeConfiguration()
            {
                ModelName = ModelName,
                Positions = Positions != null ? new List<Position>() { new Position().Clone() } : null,
                StepsForCamera0 = StepsForCamera0 != null ? new List<StepConfiguration>() { new StepConfiguration().Clone() } : null,
                StepsForCamera1 = StepsForCamera1 != null ? new List<StepConfiguration>() { new StepConfiguration().Clone() } : null,
                StepsForCamera2 = StepsForCamera1 != null ? new List<StepConfiguration>() { new StepConfiguration().Clone() } : null,
                DateCreated = DateCreated,
                DateModified = DateModified,
            };
        }
    }
}
