using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Foxconn.App.Controllers.Variable;
using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using Foxconn.App.ViewModels;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Foxconn.App.Models
{
    public class DatabaseManager : IDisposable
    {
        private readonly MainWindow Root = MainWindow.Current;
        public string ModelName
        {
            get => _modelName;
            set
            {
                _modelName = value;
                _basicConfiguration.ModelName = value;
                _runtimeConfiguration.ModelName = value;
                _statisticsData.ModelName = value;
                _storageData.ModelName = value;
            }
        }
        public BasicConfiguration Basic => _basicConfiguration;
        public RuntimeConfiguration Runtime => _runtimeConfiguration;
        public StatisticsData Statistics => _statisticsData;
        public StorageData Storage => _storageData;
        private MongoClient _mongoClient { get; set; }
        private IMongoDatabase _database { get; set; }
        private bool _isConnected { get; set; }
        private string _modelName { get; set; }
        private BasicConfiguration _basicConfiguration { get; set; }
        private RuntimeConfiguration _runtimeConfiguration { get; set; }
        private StatisticsData _statisticsData { get; set; }
        private StorageData _storageData { get; set; }
        private bool _disposed { get; set; }

        public DatabaseManager()
        {
            _mongoClient = new MongoClient(GlobalVariable.ConnectionString);
            _database = _mongoClient.GetDatabase(GlobalVariable.DatabaseName);
            _isConnected = false;
            _modelName = string.Empty;
            _basicConfiguration = new BasicConfiguration();
            _runtimeConfiguration = new RuntimeConfiguration();
            _statisticsData = new StatisticsData();
            _storageData = new StorageData();
        }

        #region Disposable
        ~DatabaseManager()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        // Public implementation of Dispose pattern callable by consumers.
        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                _mongoClient.Cluster?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
            _disposed = true;
        }
        #endregion

        public async void Init()
        {
            try
            {
                _disposed = false;

                #region Ping to Database
                _isConnected = _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);
                if (_isConnected)
                {
                    Console.WriteLine($"Connected ({GlobalVariable.ConnectionString})");
                    Root.ShowMessage($"[Database] Connected ({GlobalVariable.ConnectionString})");
                }
                else
                {
                    Console.WriteLine($"Cannot connect to ({GlobalVariable.ConnectionString})");
                    Root.ShowMessage($"[Database] Disconnected ({GlobalVariable.ConnectionString})", AppColor.Red);
                }
                #endregion

                #region Read Basic Configuration
                var modelName = Appsettings.Config.DefaultDocumentName;
                var taskRead = await ReadAsync(Collection.Basic, modelName, DateTime.Now);
                if (taskRead)
                {
                    Root.ShowMessage($"[Database] Loaded basic ({modelName})");
                }
                else
                {
                    Root.ShowMessage($"[Database] Cannot loaded basic ({modelName})", AppColor.Red);
                }
                #endregion

                #region Show List Runtime
                var listRuntime = GetListRuntime();
                if (listRuntime != null)
                {
                    AppUi.ShowListToComboBox(Root, Root.tbcmbModelName, listRuntime);
                }
                #endregion

                #region Clear Statistics
                var taskDelete = await DeleteOldAsync(Collection.Statistics, DateTime.Now.Date.AddDays(-31));
                if (taskDelete)
                {
                    Root.ShowMessage($"[Database] Clear old statistics data");
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        public async void Start(string modelName)
        {
            ModelName = modelName;

            #region Basic Configuration
            var taskRead = false;
            //taskRead = await ReadAsync(Collection.Basic, modelName, DateTime.Now);
            //if (taskRead)
            //{
            //    Root.ShowMessage($"[Database] Loaded basic ({modelName})");
            //}
            //else
            //{
            //    Root.ShowMessage($"[Database] Cannot loaded basic ({modelName})", AppColor.Red);
            //}
            #endregion

            #region Runtime Configuration
            taskRead = await ReadAsync(Collection.Runtime, modelName, DateTime.Now);
            if (taskRead)
            {
                Root.ShowMessage($"[Database] Loaded runtime ({modelName})");
            }
            else
            {
                Root.ShowMessage($"[Database] Cannot loaded runtime ({modelName})", AppColor.Red);
            }
            #endregion

            #region Statistics Data
            taskRead = await ReadAsync(Collection.Statistics, modelName, DateTime.Now);
            if (taskRead)
            {
                Root.ShowMessage($"[Database] Loaded statistics ({modelName})");
            }
            else
            {
                Root.ShowMessage($"[Database] Cannot loaded statistics ({modelName})", AppColor.Red);
            }
            ShowCounter();
            #endregion

            #region Storage Data
            taskRead = await ReadAsync(Collection.Storage, modelName, DateTime.Now);
            if (taskRead)
            {
                Root.ShowMessage($"[Database] Loaded storage ({modelName})");
            }
            else
            {
                Root.ShowMessage($"[Database] Cannot loaded storage ({modelName})", AppColor.Red);
            }
            #endregion

            Logger.ClearLog();
            Logger.ClearImage(modelName);
        }

        public List<string> GetListRuntime()
        {
            try
            {
                if (!_isConnected)
                    return null;

                var modelNames = new List<string>();
                var collection = _database.GetCollection<BsonDocument>(GlobalVariable.RuntimeConfigurationName);
                var docs = collection.FindAsync(new BsonDocument()).Result.ToList();
                foreach (var item in docs)
                {
                    modelNames.Add(item["ModelName"].AsString);
                }
                var result = modelNames.OrderBy(x => x).ToList();
                return result;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return null;
            }
        }

        public async Task<bool> ReadAsync(Collection collectionName, string modelName, DateTime dateTime)
        {
            try
            {
                if (!_isConnected)
                    return false;

                switch (collectionName)
                {
                    case Collection.Basic:
                        {
                            var collection = _database.GetCollection<BasicConfiguration>(GlobalVariable.BasicConfigurationName);
                            var filter = Builders<BasicConfiguration>.Filter.Eq(x => x.ModelName, modelName);
                            var result = await collection.FindAsync(filter).Result.FirstAsync();
                            _basicConfiguration = result;
                            return result != null;
                        }
                    case Collection.Runtime:
                        {
                            var collection = _database.GetCollection<RuntimeConfiguration>(GlobalVariable.RuntimeConfigurationName);
                            var filter = Builders<RuntimeConfiguration>.Filter.Eq(x => x.ModelName, modelName);
                            var result = await collection.FindAsync(filter).Result.FirstAsync();
                            _runtimeConfiguration = result;
                            return result != null;
                        }
                    case Collection.Statistics:
                        {
                            var collection = _database.GetCollection<StatisticsData>(GlobalVariable.StatisticsDataName);
                            var filter1 = Builders<StatisticsData>.Filter.Eq(x => x.DateCreated, dateTime.Date);
                            var filter2 = Builders<StatisticsData>.Filter.Eq(x => x.ModelName, modelName);
                            var result = await collection.FindAsync(filter1 & filter2).Result.FirstAsync();
                            _statisticsData = result;
                            return result != null;
                        }
                    case Collection.Storage:
                        {
                            var collection = _database.GetCollection<StorageData>(GlobalVariable.StorageDataName);
                            var filter = Builders<StorageData>.Filter.Eq(x => x.ModelName, modelName);
                            var result = await collection.FindAsync(filter).Result.FirstAsync();
                            _storageData = result;
                            return result != null;
                        }
                    default:
                        break;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> WriteAsync(Collection collectionName, Command command, string modelName = "")
        {
            try
            {
                if (!_isConnected)
                    return false;

                switch (collectionName)
                {
                    case Collection.Basic:
                        {
                            _basicConfiguration.DateModified = DateTime.Now;
                            var collection = _database.GetCollection<BasicConfiguration>(GlobalVariable.BasicConfigurationName);
                            if (command == Command.Replace)
                            {
                                var filter = Builders<BasicConfiguration>.Filter.Eq(x => x._id, _basicConfiguration._id);
                                var result = await collection.ReplaceOneAsync(filter, _basicConfiguration);
                            }
                            else if (command == Command.Insert)
                            {
                                _basicConfiguration.ModelName = modelName;
                                await collection.InsertOneAsync(_basicConfiguration);
                            }
                            else
                            {
                                _basicConfiguration.ModelName = modelName;
                                await collection.InsertOneAsync(_basicConfiguration.Clone());
                            }
                            break;
                        }
                    case Collection.Runtime:
                        {
                            _runtimeConfiguration.DateModified = DateTime.Now;
                            var collection = _database.GetCollection<RuntimeConfiguration>(GlobalVariable.RuntimeConfigurationName);
                            if (command == Command.Replace)
                            {
                                var filter = Builders<RuntimeConfiguration>.Filter.Eq(x => x._id, _runtimeConfiguration._id);
                                var result = await collection.ReplaceOneAsync(filter, _runtimeConfiguration);
                            }
                            else if (command == Command.Insert)
                            {
                                _runtimeConfiguration.ModelName = modelName;
                                await collection.InsertOneAsync(_runtimeConfiguration);
                            }
                            else
                            {
                                _runtimeConfiguration.ModelName = modelName;
                                await collection.InsertOneAsync(_runtimeConfiguration.Clone());
                            }
                            break;
                        }
                    case Collection.Statistics:
                        {
                            _statisticsData.DateModified = DateTime.Now;
                            var collection = _database.GetCollection<StatisticsData>(GlobalVariable.StatisticsDataName);
                            if (command == Command.Replace)
                            {
                                var filter = Builders<StatisticsData>.Filter.Eq(x => x._id, _statisticsData._id);
                                var result = await collection.ReplaceOneAsync(filter, _statisticsData);
                            }
                            else if (command == Command.Insert)
                            {
                                _statisticsData.ModelName = modelName;
                                await collection.InsertOneAsync(_statisticsData);
                            }
                            else
                            {
                                _statisticsData.ModelName = modelName;
                                await collection.InsertOneAsync(_statisticsData.Clone());
                            }
                            break;
                        }
                    case Collection.Storage:
                        {
                            _storageData.DateModified = DateTime.Now;
                            var collection = _database.GetCollection<StorageData>(GlobalVariable.StorageDataName);
                            if (command == Command.Replace)
                            {
                                var filter = Builders<StorageData>.Filter.Eq(x => x._id, _storageData._id);
                                var result = await collection.ReplaceOneAsync(filter, _storageData);
                            }
                            else if (command == Command.Insert)
                            {
                                _storageData.ModelName = modelName;
                                await collection.InsertOneAsync(_storageData);
                            }
                            else
                            {
                                _storageData.ModelName = modelName;
                                await collection.InsertOneAsync(_storageData.Clone());
                            }
                            break;
                        }
                    default:
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Collection collectionName, string modelName)
        {
            try
            {
                if (!_isConnected)
                    return false;

                switch (collectionName)
                {
                    case Collection.Basic:
                        {
                            var collection = _database.GetCollection<BasicConfiguration>(GlobalVariable.BasicConfigurationName);
                            var filter = Builders<BasicConfiguration>.Filter.Where(x => x.ModelName == modelName);
                            var result = await collection.DeleteManyAsync(filter);
                            break;
                        }
                    case Collection.Runtime:
                        {
                            var collection = _database.GetCollection<RuntimeConfiguration>(GlobalVariable.RuntimeConfigurationName);
                            var filter = Builders<RuntimeConfiguration>.Filter.Where(x => x.ModelName == modelName);
                            var result = await collection.DeleteManyAsync(filter);
                            break;
                        }
                    case Collection.Statistics:
                        {
                            var collection = _database.GetCollection<StatisticsData>(GlobalVariable.StatisticsDataName);
                            var filter = Builders<StatisticsData>.Filter.Where(x => x.ModelName == modelName);
                            var result = await collection.DeleteManyAsync(filter);
                            break;
                        }
                    case Collection.Storage:
                        {
                            var collection = _database.GetCollection<StorageData>(GlobalVariable.StorageDataName);
                            var filter = Builders<StorageData>.Filter.Where(x => x.ModelName == modelName);
                            var result = await collection.DeleteManyAsync(filter);
                            break;
                        }
                    default:
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> DeleteOldAsync(Collection collectionName, DateTime dateTime)
        {
            try
            {
                if (!_isConnected)
                    return false;

                switch (collectionName)
                {
                    case Collection.Basic:
                        {
                            var collection = _database.GetCollection<BasicConfiguration>(GlobalVariable.BasicConfigurationName);
                            var filter = Builders<BasicConfiguration>.Filter.Where(x => x.DateCreated <= dateTime);
                            var result = await collection.DeleteManyAsync(filter);
                            break;
                        }
                    case Collection.Runtime:
                        {
                            var collection = _database.GetCollection<RuntimeConfiguration>(GlobalVariable.RuntimeConfigurationName);
                            var filter = Builders<RuntimeConfiguration>.Filter.Where(x => x.DateCreated <= dateTime);
                            var result = await collection.DeleteManyAsync(filter);
                            break;
                        }
                    case Collection.Statistics:
                        {
                            var collection = _database.GetCollection<StatisticsData>(GlobalVariable.StatisticsDataName);
                            var filter = Builders<StatisticsData>.Filter.Where(x => x.DateCreated <= dateTime);
                            var result = collection.DeleteManyAsync(filter);
                            break;
                        }
                    case Collection.Storage:
                        {
                            var collection = _database.GetCollection<StorageData>(GlobalVariable.StorageDataName);
                            var filter = Builders<StorageData>.Filter.Where(x => x.DateCreated <= dateTime);
                            var result = await collection.DeleteManyAsync(filter);
                            break;
                        }
                    default:
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return false;
            }
        }

        private bool IsNewDay()
        {
            var now = DateTime.Now.Date;
            var dateCreated = _statisticsData.DateCreated.Date;
            if ((now - dateCreated).Days > 0)
            {
                _statisticsData._id = ObjectId.GenerateNewId();
                _statisticsData.ModelName = _modelName;
                _statisticsData.Total = 0;
                _statisticsData.Pass = 0;
                _statisticsData.Fail = 0;
                _statisticsData.History.Clear();
                _statisticsData.DateCreated = DateTime.Now.Date;
                _statisticsData.DateModified = DateTime.Now;
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task UpdateCounter(int passNumber, int failNumber)
        {
            if (IsNewDay())
            {
                _statisticsData.Pass = passNumber;
                _statisticsData.Fail = failNumber;
                _statisticsData.Total = _statisticsData.Pass + _statisticsData.Fail;
                await WriteAsync(Collection.Statistics, Command.Insert, ModelName);
            }
            else
            {
                _statisticsData.Pass = _statisticsData.Pass + passNumber;
                _statisticsData.Fail = _statisticsData.Fail + failNumber;
                _statisticsData.Total = _statisticsData.Pass + _statisticsData.Fail;
                await WriteAsync(Collection.Statistics, Command.Replace, ModelName);
            }
        }

        public void ShowCounter()
        {
            AppUi.ShowLabel(Root, Root.lblPass, $"Pass: {_statisticsData.Pass}");
            AppUi.ShowLabel(Root, Root.lblFail, $"Fail: {_statisticsData.Fail}");
            Root.Dispatcher.Invoke(() =>
            {
                Root.Plot1.DataContext = new PieViewModel(_statisticsData.Pass, _statisticsData.Fail);
            });
        }

        public void ClearCounter()
        {
            _statisticsData.Pass = 0;
            _statisticsData.Fail = 0;
            _statisticsData.Total = 0;
            _ = UpdateCounter(0, 0);
        }

        public async Task UpdateHistory(string message)
        {
            if (IsNewDay())
            {
                _statisticsData.History.Add(message);
                await WriteAsync(Collection.Statistics, Command.Insert);
            }
            else
            {
                _statisticsData.History.Add(message);
                await WriteAsync(Collection.Statistics, Command.Replace);
            }
        }

        public void SaveCameraConfiguration()
        {
            var camera0 = _runtimeConfiguration.StepsForCamera0;
            var camera1 = _runtimeConfiguration.StepsForCamera1;
            var camera2 = _runtimeConfiguration.StepsForCamera2;
            CreateImagePath(camera0, Enum.GetName(typeof(CameraName), 0));
            CreateImagePath(camera1, Enum.GetName(typeof(CameraName), 1));
            CreateImagePath(camera2, Enum.GetName(typeof(CameraName), 2));
            SaveImageForCamera(camera0);
            SaveImageForCamera(camera1);
            SaveImageForCamera(camera2);
        }

        private void CreateImagePath(List<RuntimeConfiguration.StepConfiguration> steps, string cameraName)
        {
            // Steps
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                step.ImagePath = $@"{GlobalVariable.Data}\{_modelName}\{cameraName}\{i}\{GlobalVariable.ImageName}";
                CreateFolderFromFilePath(step.ImagePath);
                // Components
                for (int j = 0; j < step.Components.Count; j++)
                {
                    var component = step.Components[j];
                    if (component.Algorithm == Algorithm.FeatureMatching)
                    {
                        component.FeatureMatching.ImagePath = $@"{GlobalVariable.Data}\{_modelName}\{cameraName}\{i}\{j}_{GlobalVariable.FeatureImageName}";
                        CreateFolderFromFilePath(component.FeatureMatching.ImagePath);
                    }
                    if (component.Algorithm == Algorithm.TemplateMatching)
                    {
                        component.TemplateMatching.ImagePath = $@"{GlobalVariable.Data}\{_modelName}\{cameraName}\{i}\{j}_{GlobalVariable.TemplateImageName}";
                        CreateFolderFromFilePath(component.TemplateMatching.ImagePath);
                    }
                }
            }
        }

        private void CreateFolderFromFilePath(string filePath)
        {
            var path = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void SaveImageForCamera(List<RuntimeConfiguration.StepConfiguration> steps)
        {
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                SaveImage(step.Image, step.ImagePath);
                for (int j = 0; j < step.Components.Count; j++)
                {
                    var component = step.Components[j];
                    if (component.Algorithm == Algorithm.FeatureMatching)
                    {
                        SaveImage(component.FeatureMatching.Image, component.FeatureMatching.ImagePath);
                    }
                    if (component.Algorithm == Algorithm.TemplateMatching)
                    {
                        SaveImage(component.TemplateMatching.Image, component.TemplateMatching.ImagePath);
                    }
                }
            }
        }

        private void SaveImage(Image<Bgr, byte> image, string path)
        {
            if (image != null && path != null)
            {
                CvInvoke.Imwrite(Path.GetFullPath(path), image, new KeyValuePair<ImwriteFlags, int>(ImwriteFlags.PngCompression, 0));
            }
        }

        public void SetNullImageVariable(List<RuntimeConfiguration.StepConfiguration> steps)
        {
            // Steps
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                step.Image = null;
                // Components
                for (int j = 0; j < step.Components.Count; j++)
                {
                    var component = step.Components[j];
                    if (component.Algorithm == Algorithm.FeatureMatching)
                    {
                        component.FeatureMatching.Image = null;
                    }
                    if (component.Algorithm == Algorithm.TemplateMatching)
                    {
                        component.TemplateMatching.Image = null;
                    }
                }
            }
        }

        public void CreateNewFile(string modelName, User user = User.None)
        {
            Task.Run(async () =>
            {
                var hasCreated = GetListRuntime().Contains(modelName);
                if (hasCreated)
                {
                    AppUi.ShowMessage($"Cannot create: {modelName}.", MessageBoxImage.Error);
                }
                else
                {
                    if (user == User.Admin)
                    {
                        await WriteAsync(Collection.Basic, Command.Create, modelName);
                        await WriteAsync(Collection.Runtime, Command.Create, modelName);
                        await WriteAsync(Collection.Statistics, Command.Create, modelName);
                        await WriteAsync(Collection.Storage, Command.Create, modelName);
                    }
                    else
                    {
                        if (modelName == Appsettings.Config.DefaultDocumentName)
                        {
                            AppUi.ShowMessage("Login with administrator privileges and try again.", MessageBoxImage.Error);
                            return;
                        }
                        else
                        {
                            //await WriteAsync(Collection.Basic, Command.Create, modelName);
                            await WriteAsync(Collection.Runtime, Command.Create, modelName);
                            await WriteAsync(Collection.Statistics, Command.Create, modelName);
                            await WriteAsync(Collection.Storage, Command.Create, modelName);
                        }
                    }
                    var listRuntime = GetListRuntime();
                    if (listRuntime != null)
                        AppUi.ShowListToComboBox(Root, Root.tbcmbModelName, listRuntime);
                }
            });
        }

        public void DeleteFile(string modelName, User user = User.None)
        {
            Task.Run(async () =>
            {
                if (user == User.Admin)
                {
                    await DeleteAsync(Collection.Basic, modelName);
                    await DeleteAsync(Collection.Runtime, modelName);
                    await DeleteAsync(Collection.Statistics, modelName);
                    await DeleteAsync(Collection.Storage, modelName);
                    // Clear data in disk
                    var path = Path.GetFullPath($@"{AppDomain.CurrentDomain.BaseDirectory}Data\{modelName}");
                    if (Directory.Exists(path))
                    {
                        Logger.ClearFolder(path);
                        Directory.Delete(path);
                    }
                }
                else
                {
                    if (modelName == Appsettings.Config.DefaultDocumentName)
                    {
                        AppUi.ShowMessage("Login with administrator privileges and try again.", MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        await DeleteAsync(Collection.Basic, modelName);
                        await DeleteAsync(Collection.Runtime, modelName);
                        await DeleteAsync(Collection.Statistics, modelName);
                        await DeleteAsync(Collection.Storage, modelName);
                        // Clear data in disk
                        var path = Path.GetFullPath($@"{AppDomain.CurrentDomain.BaseDirectory}Data\{modelName}");
                        if (Directory.Exists(path))
                        {
                            Logger.ClearFolder(path);
                            Directory.Delete(path);
                        }
                    }
                }
                var listRuntime = GetListRuntime();
                if (listRuntime != null)
                    AppUi.ShowListToComboBox(Root, Root.tbcmbModelName, listRuntime);
            });
        }

        public void SaveAsFile(string modelName, User user = User.None)
        {
            Task.Run(async () =>
            {
                if (user == User.Admin)
                {
                    SaveCameraConfiguration();
                    await WriteAsync(Collection.Basic, Command.Replace);
                    await WriteAsync(Collection.Runtime, Command.Replace);
                    await WriteAsync(Collection.Statistics, Command.Replace);
                    await WriteAsync(Collection.Storage, Command.Replace);
                }
                else
                {
                    if (modelName == Appsettings.Config.DefaultDocumentName)
                    {
                        AppUi.ShowMessage("Login with administrator privileges and try again.", MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        SaveCameraConfiguration();
                        //await WriteAsync(Collection.Basic, Command.Replace);
                        await WriteAsync(Collection.Runtime, Command.Replace);
                        await WriteAsync(Collection.Statistics, Command.Replace);
                        await WriteAsync(Collection.Storage, Command.Replace);
                    }
                }
            });
        }

        public void SelectedFile(string modelName)
        {
            Task.Run(async () =>
            {
                //await ReadAsync(Collection.Basic, modelName, DateTime.Now);
                await ReadAsync(Collection.Runtime, modelName, DateTime.Now);
                if (await ReadAsync(Collection.Statistics, modelName, DateTime.Now) == false)
                    await WriteAsync(Collection.Statistics, Command.Insert, modelName);
                if (await ReadAsync(Collection.Storage, modelName, DateTime.Now) == false)
                    await WriteAsync(Collection.Storage, Command.Insert, modelName);
            });
        }
    }
}
