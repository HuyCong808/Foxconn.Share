using Emgu.CV;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Foxconn.Editor.Camera
{
    public class Baumer : ICamera
    {
        #region CAMERA VARIABLES
        //DECLARATIONS OF VARIABLES
        private static BGAPI2.ImageProcessor imgProcessor = null;
        private static byte[] imageBufferCopy;
        private static byte[] transformImageBufferCopy;

        private static BGAPI2.SystemList systemList = null;
        private static BGAPI2.System mSystem = null;
        private static string sSystemID = "";

        private static BGAPI2.InterfaceList interfaceList = null;
        private static BGAPI2.Interface mInterface = null;
        private static string sInterfaceID = "";

        private static BGAPI2.DeviceList deviceList = null;
        private static BGAPI2.Device mDevice = null;
        private static string sDeviceID = "";

        private static BGAPI2.DataStreamList datastreamList = null;
        private static BGAPI2.DataStream mDataStream = null;
        private static string sDataStreamID = "";

        private static BGAPI2.BufferList bufferList = null;
        private static BGAPI2.Buffer mBuffer = null;
        private static BGAPI2.Buffer mBufferFilled = null;

        private static BGAPI2.Node mNode = null;
        private static BGAPI2.Events.DeviceEvent mdEvent = new BGAPI2.Events.DeviceEvent();

        private static bool bIsAvailableEventSelector = false;
        private static bool bIsAvaliableEventSelectorExposureStart = false;
        private static bool bIsAvaliableEventSelectorExposureEnd = false;
        private static bool bIsAvaliableEventSelectorFrameTransferEnd = false;
        private static int returnCode = 1;
        #endregion
        private readonly object _lockObj = new object();
        private string _userDefinedName = string.Empty;
        private bool _isConnected = false;
        private bool _isGrabbing = false;
        private bool _isStreaming = false;

        public bool IsConnected => _isConnected;

        public bool IsGrabbing => _isGrabbing;

        public bool IsStreaming => _isStreaming;

        public void DeviceListAcq()
        {
            try
            {
                GC.Collect();
                int nRet;
                nRet = BaumerInfo();
                nRet = BaumerLoadImageProcessor();
                nRet = BaumerSystemList();
                nRet = BaumerSystem();
                nRet = BaumerDeviceList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }
        }

        public int Open(string userDefinedName = "")
        {
            try
            {
                if (userDefinedName != "")
                    _userDefinedName = userDefinedName;

                if (_isConnected)
                    return 1;

                int nRet = -1;
                nRet = BaumerDevice(userDefinedName);
                nRet = BaumerDeviceParameterSetup();
                nRet = BaumerDataStreamList();
                nRet = BaumerDatastream();
                _isConnected = true;
                Thread.Sleep(1000);
                return nRet;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }
            return -1;
        }

        public int Close()
        {
            int nRet = -1;
            if (_isStreaming)
            {
                nRet = StopStreaming();
                if (nRet != MyCamera.MV_OK)
                {
                    System.Diagnostics.Trace.WriteLine("Stop Streaming Fail!");
                    return nRet;
                }
            }

            if (_isGrabbing)
            {
                nRet = StopGrabbing();
                if (nRet != MyCamera.MV_OK)
                {
                    System.Diagnostics.Trace.WriteLine("Stop Grabbing Fail!");
                    return nRet;
                }
            }
            nRet = BaumerRelease();
            if (nRet != 1)
            {
                System.Diagnostics.Trace.WriteLine("Destroy Device Fail!");
            }
            return 1;
        }

        public int StartGrabbing()
        {
            int nRet = -1;
            if (_isGrabbing)
            {
                return 0;
            }
            nRet = BaumerBufferList();
            nRet = BaumerCameraStart();
            if (nRet == 1)
            {
                _isGrabbing = true;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("Start Grabbing Fail!");
            }
            return nRet;
        }

        public int StopGrabbing()
        {
            int nRet = -1;
            nRet = BaumerCameraStop();
            nRet = BaumerStopDataStreamAcquisitionAndReleaseBuffer();
            if (nRet == 1)
            {
                _isGrabbing = false;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("Stop Grabbing Fail!");
            }
            return nRet;
        }

        public int StartStreaming()
        {
            return -1;
        }

        public int StopStreaming()
        {
            return -1;
        }

        public int GetParameter(KeyName key, ref object value)
        {
            if (_isConnected)
            {
                if (key == KeyName.ExposureTime)
                {
                    if (mDevice.GetRemoteNodeList().GetNodePresent("ExposureTime"))
                    {
                        return (int)mDevice.RemoteNodeList["ExposureTime"].Value;
                    }
                    else if (mDevice.GetRemoteNodeList().GetNodePresent("ExposureTimeAbs"))
                    {
                        return (int)mDevice.RemoteNodeList["ExposureTimeAbs"].Value;
                    }
                }
            }
            return 1;
        }

        public int SetParameter(KeyName key, object value)
        {
            if (_isConnected)
            {
                if (key == KeyName.ExposureTime)
                {
                    if (mDevice.GetRemoteNodeList().GetNodePresent("ExposureTime"))
                    {
                        mDevice.RemoteNodeList["ExposureTime"].Value = (double)Convert.ToInt64(value);
                    }
                    else if (mDevice.GetRemoteNodeList().GetNodePresent("ExposureTimeAbs"))
                    {
                        mDevice.RemoteNodeList["ExposureTimeAbs"].Value = (double)Convert.ToInt64(value);
                    }
                }
                if (key == KeyName.TriggerMode)
                {
                    if (Convert.ToInt64(value) == 1)
                    {
                        mDevice.RemoteNodeList["TriggerMode"].Value = "On";
                        Console.Write("          TriggerMode:            {0}\r\n", (string)mDevice.RemoteNodeList["TriggerMode"].Value);
                        Console.Write("  \r\n");
                    }
                    else
                    {
                        mDevice.RemoteNodeList["TriggerMode"].Value = "Off";
                        Console.Write("          TriggerMode:            {0}\r\n", (string)mDevice.RemoteNodeList["TriggerMode"].Value);
                        Console.Write("  \r\n");
                    }
                }
                if (key == KeyName.TriggerSource)
                {
                    mDevice.RemoteNodeList["TriggerSource"].Value = "SoftwareTrigger";
                    Console.Write("          TriggerSource:            {0}\r\n", (string)mDevice.RemoteNodeList["TriggerSource"].Value);
                    Console.Write("  \r\n");
                }
                if (key == KeyName.TriggerSoftware)
                {
                    mDevice.RemoteNodeList["TriggerSoftware"].Execute();
                }
            }
            return 1;
        }

        public void ClearImageBuffer()
        {

        }

        public System.Drawing.Bitmap GrabFrame(int timeout = 1000)
        {
            try
            {
                lock (_lockObj)
                {
                    mBufferFilled = mDataStream.GetFilledBuffer(1000); // image polling timeout 1000 msec
                    if (mBufferFilled == null)
                    {
                        Console.Write("Error: Buffer Timeout after 1000 msec\r\n");
                    }
                    else if (mBufferFilled.IsIncomplete == true)
                    {
                        Console.Write("Error: Image is incomplete\r\n");
                        // queue buffer again
                        mBufferFilled.QueueBuffer();
                    }
                    else
                    {
                        Console.Write(" Image {0, 5:d} received in memory address {1, 16:X16} {2}\r\n", mBufferFilled.FrameID, (ulong)mBufferFilled.MemPtr, mBufferFilled.PixelFormat);

                        if (mBufferFilled.PixelFormat == "Mono8" || mBufferFilled.PixelFormat == "BayerRG8" || mBufferFilled.PixelFormat == "BayerGB8") //openCV format (CV_8UC1)
                        {
                            using (var imOriginal = new Mat((int)mBufferFilled.Height, (int)mBufferFilled.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 1, (IntPtr)((ulong)mBufferFilled.MemPtr + mBufferFilled.ImageOffset), (int)mBufferFilled.Width * 1))    //PixelFormat == Mono8
                            using (var imConvert = new Mat((int)mBufferFilled.Height, (int)mBufferFilled.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 1))  //memory allcation
                            using (var imCopy = new Mat((int)mBufferFilled.Height, (int)mBufferFilled.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 1)) //memory allcation
                            {
                                ulong imConvert_FrameID = mBufferFilled.FrameID;
                                ulong imCopy_FrameID = mBufferFilled.FrameID;
                                ulong imClone_FrameID = mBufferFilled.FrameID;

                                imOriginal.ConvertTo(imConvert, Emgu.CV.CvEnum.DepthType.Cv8U, 1.0); //full copy with previous memory allocation AND scaling of 1.0
                                imOriginal.CopyTo(imCopy); //full copy with previous memory allocation in cv::Mat imCopy = cv::Mat(height, width, CV_8UC1)
                                Mat imClone = new Mat();
                                imClone = imOriginal.Clone();   //full copy with memory allocation     

                                // queue buffer again
                                Console.Write(" Image {0, 5:d} queue buffer \r\n", mBufferFilled.FrameID);
                                mBufferFilled.QueueBuffer(); //BGAPI2 buffer no longer used, image data copied to opencv Mat

                                return imClone.ToBitmap();
                            }
                        }
                        Console.Write("\r\n");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }
            return null;
        }

        private int BaumerInfo()
        {
            System.Console.Write("\r\n");
            System.Console.Write("#########################################################\r\n");
            System.Console.Write("# PROGRAMMER'S GUIDE BAUMER EXG50                       #\r\n");
            System.Console.Write("#########################################################\r\n");
            System.Console.Write("\r\n\r\n");
            return 1;
        }

        private int BaumerLoadImageProcessor()
        {
            //LOAD IMAGE PROCESSOR
            try
            {
                imgProcessor = new BGAPI2.ImageProcessor();
                Console.Write("ImageProcessor version:    {0} \r\n", imgProcessor.GetVersion());
                if (imgProcessor.NodeList.GetNodePresent("DemosaicingMethod") == true)
                {
                    imgProcessor.NodeList["DemosaicingMethod"].Value = "NearestNeighbor"; // NearestNeighbor, Bilinear3x3, Baumer5x5
                    Console.Write("    Demosaicing method:    {0} \r\n", (string)imgProcessor.NodeList["DemosaicingMethod"].Value);
                }
                Console.Write("\r\n");
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }
            return 1;
        }

        private int BaumerSystemList()
        {
            Console.Write("SYSTEM LIST\r\n");
            Console.Write("###########\r\n\r\n");

            //COUNTING AVAILABLE SYSTEMS (TL producers)
            try
            {
                systemList = BGAPI2.SystemList.Instance;
                systemList.Refresh();
                Console.Write("5.1.2   Detected systems:  {0}\r\n", systemList.Count);

                //SYSTEM DEVICE INFORMATION
                foreach (KeyValuePair<string, BGAPI2.System> sys_pair in BGAPI2.SystemList.Instance)
                {
                    Console.Write("  5.2.1   System Name:     {0}\r\n", sys_pair.Value.FileName);
                    Console.Write("          System Type:     {0}\r\n", sys_pair.Value.TLType);
                    Console.Write("          System Version:  {0}\r\n", sys_pair.Value.Version);
                    Console.Write("          System PathName: {0}\r\n\r\n", sys_pair.Value.PathName);
                }
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                returnCode = (1 == returnCode) ? -1 : returnCode;
                Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }
            return returnCode;
        }

        private int BaumerSystem()
        {
            //OPEN THE FIRST SYSTEM IN THE LIST WITH A CAMERA CONNECTED
            try
            {
                foreach (KeyValuePair<string, BGAPI2.System> sys_pair in BGAPI2.SystemList.Instance)
                {
                    Console.Write("SYSTEM\r\n");
                    Console.Write("######\r\n\r\n");

                    try
                    {
                        sys_pair.Value.Open();
                        Console.Write("5.1.3   Open next system \r\n");
                        Console.Write("  5.2.1   System Name:     {0}\r\n", sys_pair.Value.FileName);
                        Console.Write("          System Type:     {0}\r\n", sys_pair.Value.TLType);
                        Console.Write("          System Version:  {0}\r\n", sys_pair.Value.Version);
                        Console.Write("          System PathName: {0}\r\n\r\n", sys_pair.Value.PathName);
                        sSystemID = sys_pair.Key;
                        Console.Write("        Opened system - NodeList Information \r\n");
                        Console.Write("          GenTL Version:   {0}.{1}\r\n\r\n", (long)sys_pair.Value.NodeList["GenTLVersionMajor"].Value, (long)sys_pair.Value.NodeList["GenTLVersionMinor"].Value);


                        Console.Write("INTERFACE LIST\r\n");
                        Console.Write("##############\r\n\r\n");

                        try
                        {
                            interfaceList = sys_pair.Value.Interfaces;
                            //COUNT AVAILABLE INTERFACES
                            interfaceList.Refresh(100); // timeout of 100 msec
                            Console.Write("5.1.4   Detected interfaces: {0}\r\n", interfaceList.Count);

                            //INTERFACE INFORMATION
                            foreach (KeyValuePair<string, BGAPI2.Interface> ifc_pair in interfaceList)
                            {
                                Console.Write("  5.2.2   Interface Id:      {0}\r\n", ifc_pair.Value.Id);
                                Console.Write("          Interface Type:    {0}\r\n", ifc_pair.Value.TLType);
                                Console.Write("          Interface Name:    {0}\r\n\r\n", ifc_pair.Value.DisplayName);
                            }
                        }
                        catch (BGAPI2.Exceptions.IException ex)
                        {
                            returnCode = (1 == returnCode) ? -1 : returnCode;
                            Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                            Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                            Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
                        }


                        Console.Write("INTERFACE\r\n");
                        Console.Write("#########\r\n\r\n");

                        //OPEN THE NEXT INTERFACE IN THE LIST
                        try
                        {
                            foreach (KeyValuePair<string, BGAPI2.Interface> ifc_pair in interfaceList)
                            {
                                try
                                {
                                    Console.Write("5.1.5   Open interface \r\n");
                                    Console.Write("  5.2.2   Interface Id:      {0}\r\n", ifc_pair.Key);
                                    Console.Write("          Interface Type:    {0}\r\n", ifc_pair.Value.TLType);
                                    Console.Write("          Interface Name:    {0}\r\n", ifc_pair.Value.DisplayName);
                                    ifc_pair.Value.Open();
                                    //search for any camera is connetced to this interface
                                    deviceList = ifc_pair.Value.Devices;
                                    deviceList.Refresh(100);
                                    if (deviceList.Count == 0)
                                    {
                                        Console.Write("5.1.13   Close interface ({0} cameras found) \r\n\r\n", deviceList.Count);
                                        ifc_pair.Value.Close();
                                    }
                                    else
                                    {
                                        sInterfaceID = ifc_pair.Key;
                                        Console.Write("  \r\n");
                                        Console.Write("        Opened interface - NodeList Information \r\n");
                                        if (ifc_pair.Value.TLType == "GEV")
                                        {
                                            long iIPAddress = ifc_pair.Value.NodeList["GevInterfaceSubnetIPAddress"].Value;
                                            Console.Write("          GevInterfaceSubnetIPAddress: {0}.{1}.{2}.{3}\r\n", (iIPAddress & 0xff000000) >> 24,
                                                                                                                            (iIPAddress & 0x00ff0000) >> 16,
                                                                                                                            (iIPAddress & 0x0000ff00) >> 8,
                                                                                                                            iIPAddress & 0x000000ff);
                                            long iSubnetMask = ifc_pair.Value.NodeList["GevInterfaceSubnetMask"].Value;
                                            Console.Write("          GevInterfaceSubnetMask:      {0}.{1}.{2}.{3}\r\n", (iSubnetMask & 0xff000000) >> 24,
                                                                                                                            (iSubnetMask & 0x00ff0000) >> 16,
                                                                                                                            (iSubnetMask & 0x0000ff00) >> 8,
                                                                                                                            iSubnetMask & 0x000000ff);
                                        }
                                        if (ifc_pair.Value.TLType == "U3V")
                                        {
                                            Console.Write("          NodeListCount:               {0}\r\n", ifc_pair.Value.NodeList.Count);
                                        }
                                        Console.Write("  \r\n");
                                        break;
                                    }
                                }
                                catch (BGAPI2.Exceptions.ResourceInUseException ex)
                                {
                                    returnCode = (1 == returnCode) ? -1 : returnCode;
                                    Console.Write(" Interface {0} already opened \r\n", ifc_pair.Key);
                                    Console.Write(" ResourceInUseException {0} \r\n", ex.GetErrorDescription());
                                }
                            }
                        }
                        catch (BGAPI2.Exceptions.IException ex)
                        {
                            returnCode = (1 == returnCode) ? -1 : returnCode;
                            Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                            Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                            Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
                        }

                        //if a camera is connected to the system interface then leave the system loop
                        if (sInterfaceID != "")
                        {
                            break;
                        }
                    }
                    catch (BGAPI2.Exceptions.ResourceInUseException ex)
                    {
                        returnCode = (1 == returnCode) ? -1 : returnCode;
                        Console.Write(" System {0} already opened \r\n", sys_pair.Key);
                        Console.Write(" ResourceInUseException {0} \r\n", ex.GetErrorDescription());
                    }
                }
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                returnCode = (1 == returnCode) ? -1 : returnCode;
                Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }

            if (sSystemID == "")
            {
                Console.Write(" No System found \r\n");
                return returnCode;
            }
            else
            {
                mSystem = systemList[sSystemID];
            }

            if (sInterfaceID == "")
            {
                Console.Write(" No Interface of TLType 'GEV' found \r\n");
                mSystem.Close();
                return returnCode;
            }
            else
            {
                mInterface = interfaceList[sInterfaceID];
            }

            return returnCode;
        }

        private int BaumerDeviceList()
        {
            System.Console.Write("DEVICE LIST\r\n");
            System.Console.Write("###########\r\n\r\n");

            try
            {
                //COUNTING AVAILABLE CAMERAS
                deviceList = mInterface.Devices;
                deviceList.Refresh(100);
                System.Console.Write("5.1.6   Detected devices:         {0}\r\n", deviceList.Count);

                //DEVICE INFORMATION BEFORE OPENING
                foreach (KeyValuePair<string, BGAPI2.Device> dev_pair in deviceList)
                {
                    System.Console.Write("  5.2.3   Device DeviceID:        {0}\r\n", dev_pair.Key);
                    System.Console.Write("          Device Model:           {0}\r\n", dev_pair.Value.Model);
                    System.Console.Write("          Device SerialNumber:    {0}\r\n", dev_pair.Value.SerialNumber);
                    System.Console.Write("          Device Vendor:          {0}\r\n", dev_pair.Value.Vendor);
                    System.Console.Write("          Device TLType:          {0}\r\n", dev_pair.Value.TLType);
                    System.Console.Write("          Device AccessStatus:    {0}\r\n", dev_pair.Value.AccessStatus);
                    System.Console.Write("          Device UserID:          {0}\r\n\r\n", dev_pair.Value.DisplayName);
                }
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                returnCode = (1 == returnCode) ? -1 : returnCode;
                System.Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                System.Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                System.Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }
            return returnCode;
        }

        private int BaumerDevice(string userDefinedName)
        {
            Console.Write("DEVICE\r\n");
            Console.Write("######\r\n\r\n");

            //OPEN THE FIRST CAMERA IN THE LIST
            try
            {
                foreach (KeyValuePair<string, BGAPI2.Device> dev_pair in deviceList)
                {
                    try
                    {
                        if (dev_pair.Value.DisplayName == userDefinedName)
                        {
                            Console.Write("5.1.7   Open first device \r\n");
                            Console.Write("          Device DeviceID:        {0}\r\n", dev_pair.Value.Id);
                            Console.Write("          Device Model:           {0}\r\n", dev_pair.Value.Model);
                            Console.Write("          Device SerialNumber:    {0}\r\n", dev_pair.Value.SerialNumber);
                            Console.Write("          Device Vendor:          {0}\r\n", dev_pair.Value.Vendor);
                            Console.Write("          Device TLType:          {0}\r\n", dev_pair.Value.TLType);
                            Console.Write("          Device AccessStatus:    {0}\r\n", dev_pair.Value.AccessStatus);
                            Console.Write("          Device UserID:          {0}\r\n\r\n", dev_pair.Value.DisplayName);
                            dev_pair.Value.Open();
                            sDeviceID = dev_pair.Key;
                            Console.Write("        Opened device - RemoteNodeList Information \r\n");
                            Console.Write("          Device AccessStatus:    {0}\r\n", dev_pair.Value.AccessStatus);

                            //SERIAL NUMBER
                            if (dev_pair.Value.RemoteNodeList.GetNodePresent("DeviceSerialNumber") == true)
                                Console.Write("          DeviceSerialNumber:     {0}\r\n", (string)dev_pair.Value.RemoteNodeList["DeviceSerialNumber"].Value);
                            else if (dev_pair.Value.RemoteNodeList.GetNodePresent("DeviceID") == true)
                                Console.Write("          DeviceID (SN):          {0}\r\n", (string)dev_pair.Value.RemoteNodeList["DeviceID"].Value);
                            else
                                Console.Write("          SerialNumber:           Not Available.\r\n");

                            //DISPLAY DEVICEMANUFACTURERINFO
                            if (dev_pair.Value.RemoteNodeList.GetNodePresent("DeviceManufacturerInfo") == true)
                                Console.Write("          DeviceManufacturerInfo: {0}\r\n", (string)dev_pair.Value.RemoteNodeList["DeviceManufacturerInfo"].Value);

                            //DISPLAY DEVICEFIRMWAREVERSION OR DEVICEVERSION
                            if (dev_pair.Value.RemoteNodeList.GetNodePresent("DeviceFirmwareVersion") == true)
                                Console.Write("          DeviceFirmwareVersion:  {0}\r\n", (string)dev_pair.Value.RemoteNodeList["DeviceFirmwareVersion"].Value);
                            else if (dev_pair.Value.RemoteNodeList.GetNodePresent("DeviceVersion") == true)
                                Console.Write("          DeviceVersion:          {0}\r\n", (string)dev_pair.Value.RemoteNodeList["DeviceVersion"].Value);
                            else
                                Console.Write("          DeviceVersion:          Not Available.\r\n");

                            if (dev_pair.Value.TLType == "GEV")
                            {
                                Console.Write("          GevCCP:                 {0}\r\n", (string)dev_pair.Value.RemoteNodeList["GevCCP"].Value);
                                Console.Write("          GevCurrentIPAddress:    {0}.{1}.{2}.{3}\r\n", (dev_pair.Value.RemoteNodeList["GevCurrentIPAddress"].Value & 0xff000000) >> 24, (dev_pair.Value.RemoteNodeList["GevCurrentIPAddress"].Value & 0x00ff0000) >> 16, (dev_pair.Value.RemoteNodeList["GevCurrentIPAddress"].Value & 0x0000ff00) >> 8, dev_pair.Value.RemoteNodeList["GevCurrentIPAddress"].Value & 0x000000ff);
                                Console.Write("          GevCurrentSubnetMask:   {0}.{1}.{2}.{3}\r\n", (dev_pair.Value.RemoteNodeList["GevCurrentSubnetMask"].Value & 0xff000000) >> 24, (dev_pair.Value.RemoteNodeList["GevCurrentSubnetMask"].Value & 0x00ff0000) >> 16, (dev_pair.Value.RemoteNodeList["GevCurrentSubnetMask"].Value & 0x0000ff00) >> 8, dev_pair.Value.RemoteNodeList["GevCurrentSubnetMask"].Value & 0x000000ff);
                            }
                            Console.Write("          \r\n");
                            break;
                        }
                    }
                    catch (BGAPI2.Exceptions.ResourceInUseException ex)
                    {
                        returnCode = (1 == returnCode) ? -1 : returnCode;
                        Console.Write(" Device {0} already opened \r\n", dev_pair.Key);
                        Console.Write(" ResourceInUseException {0} \r\n", ex.GetErrorDescription());
                    }
                    catch (BGAPI2.Exceptions.AccessDeniedException ex)
                    {
                        returnCode = (1 == returnCode) ? -1 : returnCode;
                        Console.Write(" Device {0} already opened \r\n", dev_pair.Key);
                        Console.Write(" AccessDeniedException {0} \r\n", ex.GetErrorDescription());
                    }
                }
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                returnCode = (1 == returnCode) ? -1 : returnCode;
                Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }
            if (sDeviceID == "")
            {
                Console.Write(" No Device found \r\n");
                mInterface.Close();
                mSystem.Close();
            }
            else
            {
                mDevice = deviceList[sDeviceID];
            }
            return returnCode;
        }

        private int BaumerDeviceParameterSetup()
        {
            Console.Write("DEVICE PARAMETER SETUP\r\n");
            Console.Write("######################\r\n\r\n");

            try
            {
                //Stop Acquisition
                mDevice.RemoteNodeList["AcquisitionStop"].Execute();
                Console.Write("          AcquisitionStop:        Done \r\n");
                Console.Write("  \r\n");

                //SET TRIGGER MODE OFF (FreeRun)
                mDevice.RemoteNodeList["TriggerMode"].Value = "Off";
                Console.Write("          TriggerMode:            {0}\r\n", (string)mDevice.RemoteNodeList["TriggerMode"].Value);
                Console.Write("  \r\n");

                //SET PIXEL FORMAT 'Mono8' OR 'BayerRG8' OR 'BayerGB8'
                if (mDevice.RemoteNodeList["PixelFormat"].EnumNodeList.GetNodePresent("Mono8") == true &&
                    mDevice.RemoteNodeList["PixelFormat"].EnumNodeList["Mono8"].IsReadable == true)
                {
                    mDevice.RemoteNodeList["PixelFormat"].Value = "Mono8";
                }
                else if (mDevice.RemoteNodeList["PixelFormat"].EnumNodeList.GetNodePresent("BayerRG8") == true &&
                         mDevice.RemoteNodeList["PixelFormat"].EnumNodeList["BayerRG8"].IsReadable == true)
                {
                    mDevice.RemoteNodeList["PixelFormat"].Value = "BayerRG8";
                }
                else if (mDevice.RemoteNodeList["PixelFormat"].EnumNodeList.GetNodePresent("BayerGB8") == true &&
                         mDevice.RemoteNodeList["PixelFormat"].EnumNodeList["BayerGB8"].IsReadable == true)
                {
                    mDevice.RemoteNodeList["PixelFormat"].Value = "BayerGB8";
                }
                else
                {
                    Console.Write("          ERROR: neither Mono8, nor BayerRG8, nor BayerGB8 are available \r\n");
                }
                Console.Write("          PixelFormat:            {0}\r\n", (string)mDevice.RemoteNodeList["PixelFormat"].Value);
                Console.Write("  \r\n");
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                returnCode = (1 == returnCode) ? -1 : returnCode;
                Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }
            return returnCode;
        }

        private int BaumerDataStreamList()
        {
            Console.Write("DATA STREAM LIST\r\n");
            Console.Write("################\r\n\r\n");

            try
            {
                //COUNTING AVAILABLE DATASTREAMS
                datastreamList = mDevice.DataStreams;
                datastreamList.Refresh();
                Console.Write("5.1.8   Detected datastreams:     {0}\r\n", datastreamList.Count);

                //DATASTREAM INFORMATION BEFORE OPENING
                foreach (KeyValuePair<string, BGAPI2.DataStream> dst_pair in datastreamList)
                {
                    Console.Write("  5.2.4   DataStream Id:          {0}\r\n\r\n", dst_pair.Key);
                }
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                returnCode = (1 == returnCode) ? -1 : returnCode;
                Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }
            return returnCode;
        }

        private int BaumerDatastream()
        {
            Console.Write("DATA STREAM\r\n");
            Console.Write("###########\r\n\r\n");

            //OPEN THE FIRST DATASTREAM IN THE LIST
            try
            {
                foreach (KeyValuePair<string, BGAPI2.DataStream> dst_pair in datastreamList)
                {
                    Console.Write("5.1.9   Open first datastream \r\n");
                    Console.Write("          DataStream Id:          {0}\r\n\r\n", dst_pair.Key);
                    dst_pair.Value.Open();
                    sDataStreamID = dst_pair.Key;
                    Console.Write("        Opened datastream - NodeList Information \r\n");
                    Console.Write("          StreamAnnounceBufferMinimum:  {0}\r\n", dst_pair.Value.NodeList["StreamAnnounceBufferMinimum"].Value);
                    if (dst_pair.Value.TLType == "GEV")
                    {
                        Console.Write("          StreamDriverModel:            {0}\r\n", dst_pair.Value.NodeList["StreamDriverModel"].Value);
                    }
                    Console.Write("  \r\n");
                    break;
                }
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                returnCode = (1 == returnCode) ? -1 : returnCode;
                Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }

            if (sDataStreamID == "")
            {
                Console.Write(" No DataStream found \r\n");
                mDevice.Close();
                mInterface.Close();
                mSystem.Close();
                return returnCode;
            }
            else
            {
                mDataStream = datastreamList[sDataStreamID];
            }
            return returnCode;
        }

        private int BaumerBufferList()
        {
            Console.Write("BUFFER LIST\r\n");
            Console.Write("###########\r\n\r\n");

            try
            {
                //BufferList
                bufferList = mDataStream.BufferList;

                // 4 buffers using internal buffer mode
                for (int i = 0; i < 4; i++)
                {
                    mBuffer = new BGAPI2.Buffer();
                    bufferList.Add(mBuffer);
                }
                Console.Write("5.1.10   Announced buffers:       {0} using {1} [bytes]\r\n", bufferList.AnnouncedCount, mBuffer.MemSize * bufferList.AnnouncedCount);
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                returnCode = (1 == returnCode) ? -1 : returnCode;
                Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }

            try
            {
                foreach (KeyValuePair<string, BGAPI2.Buffer> buf_pair in bufferList)
                {
                    buf_pair.Value.QueueBuffer();
                }
                Console.Write("5.1.11   Queued buffers:          {0}\r\n", bufferList.QueuedCount);
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                returnCode = (1 == returnCode) ? -1 : returnCode;
                Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }
            Console.Write("\r\n");
            return returnCode;
        }

        private int BaumerCameraStart()
        {
            Console.Write("CAMERA START\r\n");
            Console.Write("############\r\n\r\n");

            //START DATASTREAM ACQUISITION
            try
            {
                mDataStream.StartAcquisition();
                Console.Write("5.1.12   DataStream started \r\n");
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                returnCode = (1 == returnCode) ? -1 : returnCode;
                Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }

            //START CAMERA
            try
            {
                mDevice.RemoteNodeList["AcquisitionStart"].Execute();
                Console.Write("5.1.12   {0} started \r\n", mDevice.Model);
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                returnCode = (1 == returnCode) ? -1 : returnCode;
                Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }
            Console.Write("\r\n");
            return returnCode;
        }

        private int BaumerCameraStop()
        {
            Console.Write("\r\n");
            Console.Write("CAMERA STOP\r\n");
            Console.Write("###########\r\n\r\n");

            //STOP CAMERA
            try
            {
                if (mDevice.RemoteNodeList.GetNodePresent("AcquisitionAbort") == true)
                {
                    mDevice.RemoteNodeList["AcquisitionAbort"].Execute();
                    Console.Write("5.1.12   {0} aborted\r\n", mDevice.Model);
                }

                mDevice.RemoteNodeList["AcquisitionStop"].Execute();
                Console.Write("5.1.12   {0} stopped \r\n", mDevice.Model);
                Console.Write("\r\n");

                string sExposureNodeName = "";
                if (mDevice.GetRemoteNodeList().GetNodePresent("ExposureTime"))
                {
                    sExposureNodeName = "ExposureTime";
                }
                else if (mDevice.GetRemoteNodeList().GetNodePresent("ExposureTimeAbs"))
                {
                    sExposureNodeName = "ExposureTimeAbs";
                }
                Console.Write("         ExposureTime:                     {0} [{1}]\r\n", (double)mDevice.RemoteNodeList[sExposureNodeName].Value, mDevice.RemoteNodeList[sExposureNodeName].Unit);
                if (mDevice.TLType == "GEV")
                {
                    if (mDevice.RemoteNodeList.GetNodePresent("DeviceStreamChannelPacketSize") == true)
                        Console.Write("         DeviceStreamChannelPacketSize:    {0} [bytes]\r\n", (long)mDevice.RemoteNodeList["DeviceStreamChannelPacketSize"].Value);
                    else
                        Console.Write("         GevSCPSPacketSize:                {0} [bytes]\r\n", (long)mDevice.RemoteNodeList["GevSCPSPacketSize"].Value);
                    Console.Write("         GevSCPD (PacketDelay):            {0} [tics]\r\n", (long)mDevice.RemoteNodeList["GevSCPD"].Value);
                }
                Console.Write("\r\n");
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                returnCode = (1 == returnCode) ? -1 : returnCode;
                Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }
            return returnCode;
        }

        private int BaumerStopDataStreamAcquisitionAndReleaseBuffer()
        {
            //STOP DataStream acquisition & release buffers
            try
            {
                if (mDataStream.TLType == "GEV")
                {
                    //DataStream Statistics
                    Console.Write("         DataStream Statistics \r\n");
                    Console.Write("           DataBlockComplete:              {0}\r\n", (long)mDataStream.NodeList["DataBlockComplete"].Value);
                    Console.Write("           DataBlockInComplete:            {0}\r\n", (long)mDataStream.NodeList["DataBlockInComplete"].Value);
                    Console.Write("           DataBlockMissing:               {0}\r\n", (long)mDataStream.NodeList["DataBlockMissing"].Value);
                    Console.Write("           PacketResendRequestSingle:      {0}\r\n", (long)mDataStream.NodeList["PacketResendRequestSingle"].Value);
                    Console.Write("           PacketResendRequestRange:       {0}\r\n", (long)mDataStream.NodeList["PacketResendRequestRange"].Value);
                    Console.Write("           PacketResendReceive:            {0}\r\n", (long)mDataStream.NodeList["PacketResendReceive"].Value);
                    Console.Write("           DataBlockDroppedBufferUnderrun: {0}\r\n", (long)mDataStream.NodeList["DataBlockDroppedBufferUnderrun"].Value);
                    Console.Write("           Bitrate:                        {0:f1}\r\n", (double)mDataStream.NodeList["Bitrate"].Value);
                    Console.Write("           Throughput:                     {0:f1}\r\n", (double)mDataStream.NodeList["Throughput"].Value);
                    Console.Write("\r\n");
                }
                if (mDataStream.TLType == "U3V")
                {
                    //DataStream Statistics
                    Console.Write("         DataStream Statistics \r\n");
                    Console.Write("           GoodFrames:                     {0}\r\n", (long)mDataStream.NodeList["GoodFrames"].Value);
                    Console.Write("           CorruptedFrames:                {0}\r\n", (long)mDataStream.NodeList["CorruptedFrames"].Value);
                    Console.Write("           LostFrames:                     {0}\r\n", (long)mDataStream.NodeList["LostFrames"].Value);
                    Console.Write("\r\n");
                }
                //BufferList Information
                Console.Write("         BufferList Information \r\n");
                Console.Write("           DeliveredCount:                 {0}\r\n", (long)bufferList.DeliveredCount);
                Console.Write("           UnderrunCount:                  {0}\r\n", (long)bufferList.UnderrunCount);
                Console.Write("\r\n");

                mDataStream.StopAcquisition();
                Console.Write("5.1.12   DataStream stopped \r\n");
                bufferList.DiscardAllBuffers();
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                returnCode = (1 == returnCode) ? -1 : returnCode;
                Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }
            Console.Write("\r\n");
            return returnCode;
        }

        private int BaumerRelease()
        {
            Console.Write("RELEASE\r\n");
            Console.Write("#######\r\n\r\n");

            //Release buffers
            Console.Write("5.1.13   Releasing the resources\r\n");
            try
            {
                while (bufferList.Count > 0)
                {
                    mBuffer = bufferList.Values.First();
                    bufferList.RevokeBuffer(mBuffer);
                }
                Console.Write("         buffers after revoke:             {0}\r\n", bufferList.Count);

                mDataStream.Close();
                mDevice.Close();
                mInterface.Close();
                mSystem.Close();
            }
            catch (BGAPI2.Exceptions.IException ex)
            {
                returnCode = (1 == returnCode) ? -1 : returnCode;
                Console.Write("ExceptionType:    {0} \r\n", ex.GetType());
                Console.Write("ErrorDescription: {0} \r\n", ex.GetErrorDescription());
                Console.Write("in function:      {0} \r\n", ex.GetFunctionName());
            }

            Console.Write("\r\nEnd\r\n\r\n");
            mDataStream = null;
            mDevice = null;
            mInterface = null;
            mSystem = null;
            return returnCode;
        }
    }
}
