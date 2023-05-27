using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Foxconn.App.Controllers.Service
{
    public class Flask
    {
        private readonly MainWindow Root = MainWindow.Current;
        public int Index
        {
            get => _index;
            set => _index = value;
        }
        public string Alias
        {
            get => _alias;
            set => _alias = value;
        }
        public string UserId
        {
            get => _userId;
            set => _userId = value;
        }
        public string Password
        {
            get => _password;
            set => _password = value;
        }
        public string Host
        {
            get => _host;
            set => _host = value;
        }
        public int Port
        {
            get => _port;
            set => _port = value;
        }
        public string ServiceName
        {
            get => _serviceName;
            set => _serviceName = value;
        }
        public ConnectionStatus Status
        {
            get => _status;
            set => _status = value;
        }
        private int _index { get; set; }
        private string _alias { get; set; }
        private string _userId { get; set; }
        private string _password { get; set; }
        private string _host { get; set; }
        private int _port { get; set; }
        private string _serviceName { get; set; }
        private ConnectionStatus _status { get; set; }

        public Flask()
        {
            _status = ConnectionStatus.None;
        }

        public bool StartFlask()
        {
            _status = ConnectionStatus.Disconnected;
            if (IsPingSuccessAsync())
            {
                _status = ConnectionStatus.Connected;
                Root.ShowMessage($"[Flask {_index}] Connected ({_host}:{_port})");
            }
            else
            {
                Root.ShowMessage($"[Flask] Cannot ping to ({_host}:{_port})", AppColor.Red);
            }
            return _status == ConnectionStatus.Connected;
        }

        /// <summary>
        /// Ping to host & port
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool IsPingSuccessAsync(string host = null, int port = 0)
        {
            try
            {
                using (var client = new System.Net.Sockets.TcpClient())
                {
                    client.SendTimeout = 500;
                    client.ReceiveTimeout = 500;
                    return client.ConnectAsync(host ?? _host, port != 0 ? port : _port).Wait(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Ping to host
        /// </summary>
        /// <param name="_ip"></param>
        /// <returns></returns>
        public bool IsPingSuccess(string host = null)
        {
            try
            {
                var ping = new Ping();
                var pingReply = ping.Send(host ?? _host);
                return pingReply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        #region How to use predict
        /*
        var requestUri = "http://172.20.10.8:5000/debug/U10C153T00";
        var path = @"D:\01.Core\01.Github\Foxconn.AI\AI\cnn_cv_platform\test\images\1288&R14@1_side.jpeg";
        var bitmap = new System.Drawing.Bitmap(path);
        var fileInfo = new System.IO.FileInfo(path);
        AppUi.ShowImageBox(this, imbCamera0, bitmap);
        await Flask.PredictDebug(requestUri, bitmap);
        */
        #endregion
        /// <summary>
        /// Convert bitmap to byte array
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            var byteArray = default(byte[]);
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                byteArray = memoryStream.ToArray();
            }
            return byteArray;
        }
        /// <summary>
        /// Debug
        /// </summary>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        public static async Task<bool> PredictDebug(string requestUri = "http://172.20.10.8:5000/debug/new-model")
        {
            using (var ofd = new System.Windows.Forms.OpenFileDialog())
            {
                ofd.Filter = "Image file (*.jpg, *.png, *.tiff)|*.jpg; *.png; *.tiff|All files (*.*)|*.*";
                ofd.FilterIndex = 0;
                ofd.RestoreDirectory = true;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var image = new Bitmap(ofd.FileName))
                    {
                        return await Predict(requestUri, image);
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Debug
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static async Task<bool> PredictDebug(string requestUri, Bitmap bitmap)
        {
            var boundary = "chenmin666";
            var productName = "product_name";
            var direction = "direction";
            var serialNumber = "serial_number";
            var pictureName = "picture_name";
            object[] objectArray = new object[5]
            {
                boundary,
                productName,
                direction,
                serialNumber,
                pictureName
            };
            try
            {
                var httpClientHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = true,
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    //httpClient.BaseAddress = new Uri(requestUri);
                    httpClient.DefaultRequestVersion = HttpVersion.Version11;
                    httpClient.Timeout = TimeSpan.FromSeconds(15);
                    var byteArray1 = Encoding.UTF8.GetBytes(string.Format("--{0}\r\nContent-Disposition: form-data; name=\"image\"; filename=\"{1},{2},{3},{4}\"\r\nContent-Type: image/jpeg\r\n\r\n", objectArray));
                    var byteArray2 = BitmapToByteArray(bitmap);
                    var byteArray3 = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");

                    var byteArrayContent1 = new ByteArrayContent(byteArray1, 0, byteArray1.Length);
                    var byteArrayContent2 = new ByteArrayContent(byteArray2, 0, byteArray2.Length);
                    var byteArrayContent3 = new ByteArrayContent(byteArray3, 0, byteArray3.Length);
                    byteArray1 = null;
                    byteArray2 = null;
                    byteArray3 = null;

                    var content = new MultipartFormDataContent
                    {
                        byteArrayContent1,
                        byteArrayContent2,
                        byteArrayContent3,
                    };

                    using (var httpClientResult = await httpClient.PostAsync(requestUri, content).ConfigureAwait(false))
                    {
                        AppUi.ShowConsole($"Time: {DateTime.Now:yyyy/MM/dd HH:mm:ss.ffff}", ConsoleColor.Yellow);
                        Console.WriteLine(httpClientResult);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return false;
            }
            finally
            {
                bitmap?.Dispose();
            }
        }
        /// <summary>
        /// Predict
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static async Task<bool> Predict(string requestUri, Bitmap bitmap)
        {
            try
            {
                var boundary = "chenmin666";
                var productName = "product_name";
                var direction = "direction";
                var serialNumber = "serial_number";
                var pictureName = "picture_name";
                object[] objectArray = new object[5]
                {
                    boundary,
                    productName,
                    direction,
                    serialNumber,
                    pictureName
                };
                var webRequest = WebRequest.CreateHttp(requestUri);
                webRequest.AllowAutoRedirect = true;
                webRequest.Method = "POST";
                webRequest.ProtocolVersion = HttpVersion.Version11;
                webRequest.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.KeepAlive = false;
                webRequest.Timeout = 15000;
                webRequest.ReadWriteTimeout = 15000;
                var content = new List<byte>();
                var byteArray1 = Encoding.UTF8.GetBytes(string.Format("--{0}\r\nContent-Disposition: form-data; name=\"image\"; filename=\"{1},{2},{3},{4}\"\r\nContent-Type: image/jpeg\r\n\r\n", objectArray));
                var byteArray2 = BitmapToByteArray(bitmap);
                var byteArray3 = Encoding.UTF8.GetBytes(string.Format("\r\n--{0}--\r\n", boundary));
                content.AddRange(byteArray1);
                content.AddRange(byteArray2);
                content.AddRange(byteArray3);
                byteArray1 = null;
                byteArray2 = null;
                byteArray3 = null;
                webRequest.ContentLength = content.Count;
                using (var requestStream = await webRequest.GetRequestStreamAsync().ConfigureAwait(false))
                {
                    await requestStream.WriteAsync(content.ToArray(), 0, content.Count).ConfigureAwait(false);
                }
                var webResponse = (HttpWebResponse)await webRequest.GetResponseAsync().ConfigureAwait(false);
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    var result = "";
                    using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                    {
                        result = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                    webRequest.Abort();
                    webRequest = null;
                    webResponse.Close();
                    webResponse = null;
                    Console.WriteLine(result.Trim());
                    var predictResult = JsonConvert.DeserializeObject<PredictResult>(result);
                    return predictResult.success.ToLower() == "true" && predictResult.result.ToLower() == "pass";
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return false;
            }
            finally
            {
                bitmap?.Dispose();
            }
        }
        /// <summary>
        /// Predict
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static async Task<bool> Predict(string requestUri, FileInfo fileInfo)
        {
            try
            {
                string boundary = "chenmin666";
                string productName = "product_name";
                string direction = "direction";
                string serialNumber = "serial_number";
                string pictureName = "picture_name";
                object[] objectArray = new object[5]
                {
                    boundary,
                    productName,
                    direction,
                    serialNumber,
                    pictureName
                };
                var webRequest = WebRequest.CreateHttp(requestUri);
                webRequest.AllowAutoRedirect = true;
                webRequest.Method = "POST";
                webRequest.ProtocolVersion = HttpVersion.Version11;
                webRequest.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.KeepAlive = false;
                webRequest.Timeout = 15000;
                webRequest.ReadWriteTimeout = 15000;
                var content = new List<byte>();
                var stream = File.Open(string.Format("{0}\\{1}_side.jpeg", fileInfo.DirectoryName, pictureName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] byteArray2;
                try
                {
                    byteArray2 = new byte[stream.Length];
                    var number = await stream.ReadAsync(byteArray2, 0, byteArray2.Length).ConfigureAwait(false);
                    var byteArray1 = Encoding.UTF8.GetBytes(string.Format("--{0}\r\nContent-Disposition: form-data; name=\"side_image\"; filename=\"{1},{2},{3},{4}\"\r\nContent-Type: image/jpeg\r\n\r\n", objectArray));
                    content.AddRange(byteArray1);
                    content.AddRange(byteArray2);
                    byteArray2 = null;
                }
                finally
                {
                    stream?.Dispose();
                }
                stream = null;
                stream = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                try
                {
                    byteArray2 = new byte[stream.Length];
                    var number = await stream.ReadAsync(byteArray2, 0, byteArray2.Length).ConfigureAwait(false);
                    var byteArray1 = Encoding.UTF8.GetBytes(string.Format("\r\n--{0}\r\nContent-Disposition: form-data; name=\"top_image\"; filename=\"{1},{2},{3},{4}\"\r\nContent-Type: image/jpeg\r\n\r\n", objectArray));
                    content.AddRange(byteArray1);
                    content.AddRange(byteArray2);
                    byteArray2 = null;
                }
                finally
                {
                    stream?.Dispose();
                }
                stream = null;
                var byteArray3 = Encoding.UTF8.GetBytes(string.Format("\r\n--{0}--\r\n", boundary));
                content.AddRange(byteArray3);
                webRequest.ContentLength = content.Count;
                using (var requestStream = await webRequest.GetRequestStreamAsync().ConfigureAwait(false))
                {
                    await requestStream.WriteAsync(content.ToArray(), 0, content.Count).ConfigureAwait(false);
                }
                var webResponse = (HttpWebResponse)await webRequest.GetResponseAsync().ConfigureAwait(false);
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    var result = "";
                    using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                    {
                        result = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                    webRequest.Abort();
                    webRequest = null;
                    webResponse.Close();
                    webResponse = null;
                    Console.WriteLine(result.Trim());
                    var predictResult = JsonConvert.DeserializeObject<PredictResult>(result);
                    return predictResult.success.ToLower() == "true" && predictResult.result.ToLower() == "pass";
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return false;
            }
        }
    }

    public class PredictResult
    {
        public string success { get; set; }
        public string result { get; set; }
    }
}
