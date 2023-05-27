using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Foxconn.App.Controllers.Camera;
using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ZXing;
using ZXing.Common;
using static Foxconn.App.Helper.Utilities;
using static Foxconn.App.Models.RuntimeConfiguration;
using static Foxconn.App.Models.RuntimeConfiguration.StepConfiguration;

namespace Foxconn.App.Controllers.Image
{
    public class Image
    {
        public int Index { get; set; }

        public bool IsStarted { get; set; }
        public ICamera Camera { get; set; }
        public List<StepConfiguration> Steps { get; set; }
        public StepConfiguration Step { get; set; }
        public ComponentConfiguration Component { get; set; }
        public Mat InputImage { get; set; }
        public Mat OutputImage { get; set; }
        /// <summary>
        /// List of object's vertices.
        /// </summary>
        public Point[] ListVertices { get; set; }
        public Point[] TwoPoints { get; set; }

        public Image()
        {
            Index = -1;
            IsStarted = false;
            Camera = null;
            Steps = new List<StepConfiguration>();
            Step = new StepConfiguration();
            Component = new ComponentConfiguration();
            InputImage = new Mat();
            OutputImage = new Mat();
            ListVertices = Array.Empty<Point>();
            TwoPoints = Array.Empty<Point>();
        }

        public void Init(List<StepConfiguration> steps, bool loadImage)
        {
            Step = new StepConfiguration();
            Steps.Clear();
            for (int i = 0; i < steps.Count; i++)
            {
                Steps.Add(steps[i]);
            }
            if (Steps.Count > 0)
            {
                Step = Steps[0];
            }
            foreach (var step in Steps)
            {
                if (loadImage)
                {
                    step.LoadImage();
                }
                else
                {
                    // Because resolution of step.Image is large so it used a lot of RAM.
                    step.Image?.Dispose();
                }
                foreach (var component in step.Components)
                {
                    component.FeatureMatching.LoadImage();
                    component.TemplateMatching.LoadImage();
                }
            }
        }

        public bool Start()
        {
            try
            {
                if (IsStarted)
                    return true;
                IsStarted = Camera.OpenStrategies(true, null);
                return IsStarted;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return false;
            }
        }

        public bool Stop()
        {
            try
            {
                if (!IsStarted)
                    return true;
                if (Camera.IsConnected)
                {
                    Camera.Disconnect();
                    Camera.Release();
                }
                IsStarted = false;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return false;
            }
        }

        #region Draw Image
        public Image<Bgr, byte> Draw(Image<Bgr, byte> image)
        {
            if (image == null)
                return null; 

            var outputImage = image.Clone();
            {
                if (!Component.Region.IsEmpty)
                {
                    outputImage.Draw(Component.Region.Rectangle, new Bgr(255, 122, 0), 2);
                }
                return outputImage;
            }
        }
        #endregion

        #region Preprocessing
        public Image<Gray, byte> Preprocessing(Image<Bgr, byte> image)
        {
            try
            {
                using (var gray = image.Convert<Gray, byte>())
                {
                    var config = Component.Preprocessing;
                    if (config.Threshold.Enable)
                    {
                        using (var blur = config.BlurProp.Enable ? gray.SmoothGaussian(config.BlurProp.Value) : gray)
                        {
                            switch (config.Threshold.Type)
                            {
                                case Helper.Enums.ThresholdType.None:
                                    {
                                        return blur.Clone();
                                    }
                                case Helper.Enums.ThresholdType.Binary:
                                    {
                                        using (var thresh = !config.Threshold.Inverse ?
                                            blur.ThresholdBinary(new Gray(config.Threshold.Binary.Value), new Gray(255)) :
                                            blur.ThresholdBinaryInv(new Gray(config.Threshold.Binary.Value), new Gray(255)))
                                        {
                                            if (config.BlobProp.Enable)
                                            {
                                                var element = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(config.BlobProp.Value, config.BlobProp.Value), new Point(-1, -1));
                                                CvInvoke.Dilate(thresh, thresh, element, new Point(-1, -1), config.BlobProp.DilateIteration, BorderType.Reflect, default);
                                                CvInvoke.Erode(thresh, thresh, element, new Point(-1, -1), config.BlobProp.ErodeIteration, BorderType.Reflect, default);
                                            }
                                            return thresh.Clone();
                                        }
                                    }
                                case Helper.Enums.ThresholdType.Adaptive:
                                    {
                                        using (var thresh = !config.Threshold.Inverse ?
                                            blur.ThresholdAdaptive(new Gray(255), AdaptiveThresholdType.MeanC, Emgu.CV.CvEnum.ThresholdType.Binary, config.Threshold.Adaptive.BlockSize, new Gray(config.Threshold.Adaptive.Param1)) :
                                            blur.ThresholdAdaptive(new Gray(255), AdaptiveThresholdType.MeanC, Emgu.CV.CvEnum.ThresholdType.BinaryInv, config.Threshold.Adaptive.BlockSize, new Gray(config.Threshold.Adaptive.Param1)))
                                        {
                                            if (config.BlobProp.Enable)
                                            {
                                                var element = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(config.BlobProp.Value, config.BlobProp.Value), new Point(-1, -1));
                                                CvInvoke.Dilate(thresh, thresh, element, new Point(-1, -1), config.BlobProp.DilateIteration, BorderType.Reflect, default);
                                                CvInvoke.Erode(thresh, thresh, element, new Point(-1, -1), config.BlobProp.ErodeIteration, BorderType.Reflect, default);
                                            }
                                            return thresh.Clone();
                                        }
                                    }
                                case Helper.Enums.ThresholdType.Otsu:
                                    using (var thresh = new Image<Gray, byte>(blur.Size))
                                    {
                                        if (!config.Threshold.Inverse)
                                        {
                                            CvInvoke.Threshold(blur, thresh, config.Threshold.Otsu.Value, 255, Emgu.CV.CvEnum.ThresholdType.Binary | Emgu.CV.CvEnum.ThresholdType.Otsu);
                                        }
                                        else
                                        {
                                            CvInvoke.Threshold(blur, thresh, config.Threshold.Otsu.Value, 255, Emgu.CV.CvEnum.ThresholdType.BinaryInv | Emgu.CV.CvEnum.ThresholdType.Otsu);
                                        }
                                        if (config.BlobProp.Enable)
                                        {
                                            var element = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(config.BlobProp.Value, config.BlobProp.Value), new Point(-1, -1));
                                            CvInvoke.Dilate(thresh, thresh, element, new Point(-1, -1), config.BlobProp.DilateIteration, BorderType.Reflect, default);
                                            CvInvoke.Erode(thresh, thresh, element, new Point(-1, -1), config.BlobProp.ErodeIteration, BorderType.Reflect, default);
                                        }
                                        return thresh.Clone();
                                    }
                                default:
                                    return blur.Clone();
                            }
                        }
                    }
                    else
                    {
                        return gray.Clone();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return null;
            }
        }
        #endregion

        #region Barcode Text
        public bool BarcodeText(Image<Bgr, byte> inputImage, ref string barcodeText, Image<Bgr, byte> outputImage = null)
        {
            try
            {
                using (var image = Preprocessing(inputImage))
                {
                    var rectangle = new Rectangle(0, 0, Component.Region.Rectangle.Width, Component.Region.Rectangle.Height);
                    var config = Component.BarcodeDetection;
                    switch (config.Mode)
                    {
                        case Helper.Enums.BarcodeMode.ZXing:
                            {
                                if (config.Type == Helper.Enums.BarcodeType.Linear)
                                {
                                    barcodeText = ZXing1D(image);
                                }
                                else if (config.Type == Helper.Enums.BarcodeType.QRCode)
                                {
                                    barcodeText = ZXing2D(image);
                                }
                                else if (config.Type == Helper.Enums.BarcodeType.DataMatrix)
                                {
                                    barcodeText = ZXing2D(image);
                                }
                                break;
                            }
                        case Helper.Enums.BarcodeMode.Halcon:
                            {
                                if (config.Type == Helper.Enums.BarcodeType.Linear)
                                {
                                    barcodeText = Halcon1D(image);
                                }
                                else if (config.Type == Helper.Enums.BarcodeType.QRCode)
                                {
                                    barcodeText = HalconQR(image);
                                }
                                else if (config.Type == Helper.Enums.BarcodeType.DataMatrix)
                                {
                                    barcodeText = HalconMatrix(image);
                                }
                                break;
                            }
                        case Helper.Enums.BarcodeMode.All:
                            {
                                if (config.Type == Helper.Enums.BarcodeType.Linear)
                                {
                                    barcodeText = ZXing1D(image);
                                    if (barcodeText.Length != config.Length)
                                    {
                                        barcodeText = Halcon1D(image);
                                    }
                                }
                                else if (config.Type == Helper.Enums.BarcodeType.QRCode)
                                {
                                    barcodeText = ZXing2D(image);
                                    if (barcodeText.Length != config.Length)
                                    {
                                        barcodeText = HalconQR(image);
                                    }
                                }
                                else if (config.Type == Helper.Enums.BarcodeType.DataMatrix)
                                {
                                    barcodeText = ZXing2D(image);
                                    if (barcodeText.Length != config.Length)
                                    {
                                        barcodeText = HalconMatrix(image);
                                    }
                                }
                                break;
                            }
                        default:
                            break;
                    }
                    if (outputImage != null)
                    {
                        if (barcodeText.Length == config.Length)
                        {
                            outputImage.Draw(rectangle, new Bgr(40, 205, 65), 3);
                        }
                        else
                        {
                            outputImage.Draw(rectangle, new Bgr(48, 59, 255), 3);
                        }
                    }
                    return barcodeText.Length == config.Length;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return false;
            }
            finally
            {
                inputImage?.Dispose();
            }
        }
        /// <summary>
        /// ZXing 1D
        /// </summary>
        /// <param name="inputImage"></param>
        /// <returns></returns>
        public string ZXing1D(Image<Gray, byte> inputImage)
        {
            using (var bitmap = inputImage.ToBitmap())
            {
                var reader = new BarcodeReader()
                {
                    AutoRotate = true,
                    TryInverted = true,
                    Options = new DecodingOptions
                    {
                        TryHarder = true,
                    }
                };
                reader.Options.PossibleFormats = new List<BarcodeFormat> {
                    BarcodeFormat.CODE_39,
                    BarcodeFormat.CODE_93,
                    BarcodeFormat.CODE_128,
                    BarcodeFormat.UPC_A,
                    BarcodeFormat.UPC_E,
                    BarcodeFormat.UPC_EAN_EXTENSION
                };
                if (bitmap != null)
                {
                    var result = reader.Decode(bitmap);
                    return result != null ? result.Text.Trim() : string.Empty;
                }
                return string.Empty;
            }
        }
        /// <summary>
        /// ZXing 2D
        /// </summary>
        /// <param name="inputImage"></param>
        /// <returns></returns>
        public string ZXing2D(Image<Gray, byte> inputImage)
        {
            using (var bmp = inputImage.ToBitmap())
            {
                var reader = new BarcodeReader(null, newbitmap => new BitmapLuminanceSource(bmp), luminance => new GlobalHistogramBinarizer(luminance));
                reader.AutoRotate = true;
                reader.TryInverted = true;
                reader.Options = new DecodingOptions
                {
                    TryHarder = true
                };
                reader.Options.PossibleFormats = new List<BarcodeFormat>
                {
                    BarcodeFormat.QR_CODE,
                    BarcodeFormat.DATA_MATRIX
                };
                if (bmp != null)
                {
                    var result = reader.Decode(bmp);
                    return result != null ? result.Text.Trim() : string.Empty;
                }
                return string.Empty;
            }
        }
        /// <summary>
        /// Halcon 1D
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public string Halcon1D(Image<Gray, byte> image)
        {
            // Local iconic variables 
            HObject ho_Image;
            HObject ho_SymbolRegions;
            // Local control variables 
            var hv_BarCodeHandle = new HTuple();
            var hv_DecodedDataStrings = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_SymbolRegions);

            //Read an image
            ho_Image.Dispose();
            // Convert bitmap to ho_Image
            ho_Image = Halcon.BitmapToHalconImage(image.ToBitmap());

            //Create a model for reading 1D barcode
            hv_BarCodeHandle.Dispose();
            HOperatorSet.CreateBarCodeModel(new HTuple(), new HTuple(), out hv_BarCodeHandle);

            //Read the symbol in the image
            ho_SymbolRegions.Dispose(); hv_DecodedDataStrings.Dispose();
            HOperatorSet.FindBarCode(ho_Image, out ho_SymbolRegions, hv_BarCodeHandle, "auto", out hv_DecodedDataStrings);
            var strings = hv_DecodedDataStrings.ToSArr();
            string barcodeText = strings.Length > 0 ? strings[0].ToString().Trim() : string.Empty;

            ho_Image.Dispose();
            ho_SymbolRegions.Dispose();

            hv_BarCodeHandle.Dispose();
            hv_DecodedDataStrings.Dispose();

            return barcodeText;
        }
        /// <summary>
        /// Halcon QR
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public string HalconQR(Image<Gray, byte> image)
        {
            // Local iconic variables 
            HObject ho_Image;
            HObject ho_SymbolXLDs;
            // Local control variables 
            var hv_DataCodeHandle = new HTuple();
            var hv_ResultHandles = new HTuple();
            var hv_DecodedDataStrings = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_SymbolXLDs);

            //Read an image
            ho_Image.Dispose();
            // Convert bitmap to ho_Image
            ho_Image = Halcon.BitmapToHalconImage(image.ToBitmap());

            //Create a model for reading QR Code
            hv_DataCodeHandle.Dispose();
            HOperatorSet.CreateDataCode2dModel("QR Code", new HTuple(), new HTuple(), out hv_DataCodeHandle);

            //Read the symbol in the image
            ho_SymbolXLDs.Dispose();
            hv_ResultHandles.Dispose();
            hv_DecodedDataStrings.Dispose();
            HOperatorSet.FindDataCode2d(ho_Image, out ho_SymbolXLDs, hv_DataCodeHandle, new HTuple(), new HTuple(), out hv_ResultHandles, out hv_DecodedDataStrings);
            var strings = hv_DecodedDataStrings.ToSArr();
            string barcodeText = strings.Length > 0 ? strings[0].ToString().Trim() : string.Empty;

            ho_Image.Dispose();
            ho_SymbolXLDs.Dispose();

            hv_DataCodeHandle.Dispose();
            hv_ResultHandles.Dispose();
            hv_DecodedDataStrings.Dispose();

            return barcodeText;
        }
        /// <summary>
        /// Halcon matrix
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public string HalconMatrix(Image<Gray, byte> image)
        {
            // Local iconic variables 
            HObject ho_Image;
            HObject ho_SymbolXLDs;
            // Local control variables 
            var hv_DataCodeHandle = new HTuple();
            var hv_ResultHandles = new HTuple();
            var hv_DecodedDataStrings = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_SymbolXLDs);

            //Read an image
            ho_Image.Dispose();
            // Convert bitmap to ho_Image
            ho_Image = Halcon.BitmapToHalconImage(image.ToBitmap());

            //Create a model for reading Dat Matrix
            hv_DataCodeHandle.Dispose();
            HOperatorSet.CreateDataCode2dModel("Data Matrix ECC 200", new HTuple(), new HTuple(), out hv_DataCodeHandle);

            //Read the symbol in the image
            ho_SymbolXLDs.Dispose();
            hv_ResultHandles.Dispose();
            hv_DecodedDataStrings.Dispose();
            HOperatorSet.FindDataCode2d(ho_Image, out ho_SymbolXLDs, hv_DataCodeHandle, new HTuple(), new HTuple(), out hv_ResultHandles, out hv_DecodedDataStrings);
            var strings = hv_DecodedDataStrings.ToSArr();
            string barcodeText = strings.Length > 0 ? strings[0].ToString().Trim() : string.Empty;

            ho_Image.Dispose();
            ho_SymbolXLDs.Dispose();
            hv_DataCodeHandle.Dispose();
            hv_ResultHandles.Dispose();
            hv_DecodedDataStrings.Dispose();

            return barcodeText;
        }
        #endregion

        #region Feature Matching
        /// <summary>
        /// Feature matching
        /// </summary>
        /// <param name="inputImage"></param>
        /// <param name="outputImage"></param>
        /// <returns></returns>
        public bool FeatureImage(Image<Bgr, byte> inputImage, Image<Bgr, byte> outputImage = null)
        {
            try
            {
                if (!Component.Enable)
                {
                    return false;
                }
                else
                {
                    var config = Component.FeatureMatching;
                    using (Image<Gray, byte> image = Preprocessing(inputImage))
                    using (Image<Gray, byte> mark = Preprocessing(config.Image))
                    {
                        AppUi.ShowConsole("Find ROI", ConsoleColor.Yellow);
                        var rectangle = FindROIByFeatureMatching(image, mark, null, true);
                        AppUi.ShowConsole($"Rectangle: {rectangle}");

                        //AppUI.ShowConsole("Find Boundary", ConsoleColor.Yellow);
                        //var points = FindBoundaryFeatureMatching(image, mark, Component.Region.Rectangle, null, true);
                        //for (int i = 0; i < points.Count(); i++)
                        //{
                        //    AppUI.ShowConsole($"X: {points[i].X}");
                        //    AppUI.ShowConsole($"Y: {points[i].Y}");
                        //}
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return false;
            }
            finally
            {
                inputImage?.Dispose();
            }
        }
        /// <summary>
        /// Find ROI by feature matching algorithm
        /// </summary>
        /// <param name="model"></param>
        /// <param name="observe"></param>
        /// <param name="imageDraw"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public Rectangle FindROIByFeatureMatching(Image<Gray, byte> model, Image<Gray, byte> observe, Image<Bgr, byte> imageDraw = null, bool debug = false)
        {
            using (var result = FeatureMatching.Matching(model.Mat, observe.Mat, debug))
            {
                if (result.ImageResult != null)
                {
                    imageDraw = result.ImageResult.ToImage<Bgr, byte>();
                    CvInvoke.Imwrite("out.jpg", imageDraw);
                }
                var area = CvInvoke.MinAreaRect(new VectorOfPoint(result.Vertices));
                return area.MinAreaRect();
            }
        }
        /// <summary>
        /// Find boundary feature matching
        /// </summary>
        /// <param name="model"></param>
        /// <param name="observe"></param>
        /// <param name="imageDraw"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public Point[] FindBoundaryFeatureMatching(Image<Gray, byte> model, Image<Gray, byte> observe, Image<Bgr, byte> imageDraw = null, bool debug = false)
        {
            using (var result = FeatureMatching.Matching(model.Mat, observe.Mat, debug))
            {
                if (result.ImageResult != null)
                {
                    imageDraw = result.ImageResult.ToImage<Bgr, byte>();
                    CvInvoke.Imwrite("out.jpg", imageDraw);
                }
                var area = CvInvoke.MinAreaRect(new VectorOfPoint(result.Vertices));
                var vertices = Array.ConvertAll(area.GetVertices(), Point.Round);
                ListVertices = Util.OrderVertices(vertices);
                return Util.CopyPoints(ListVertices);
            }
        }
        /// <summary>
        /// Find boundary feature matching by ROI
        /// </summary>
        /// <param name="model"></param>
        /// <param name="observe"></param>
        /// <param name="ROI"></param>
        /// <param name="outputImage"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public Point[] FindBoundaryFeatureMatching(Image<Gray, byte> model, Image<Gray, byte> observe, Rectangle ROI, Image<Bgr, byte> outputImage = null, bool debug = false)
        {
            var vertices = FindBoundaryFeatureMatching(model, observe, outputImage, debug);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].X += ROI.X;
                vertices[i].Y += ROI.Y;
            }
            ListVertices = Util.OrderVertices(vertices);
            return Util.CopyPoints(ListVertices);
        }
        #endregion

        #region Template Matching
        /// <summary>
        /// Template matching
        /// </summary>
        /// <param name="inputImage"></param>
        /// <param name="outputImage"></param>
        /// <returns></returns>
        public bool TemplateMatching(Image<Bgr, byte> inputImage, Image<Bgr, byte> outputImage = null)
        {
            try
            {
                return Component.Enable && TemplateMatchingOpenCV(inputImage, outputImage);
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return false;
            }
            finally
            {
                inputImage?.Dispose();
            }
        }
        /// <summary>
        /// Template matching opencv
        /// </summary>
        /// <param name="inputImage"></param>
        /// <param name="outputImage"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public bool TemplateMatchingOpenCV(Image<Bgr, byte> inputImage, Image<Bgr, byte> outputImage = null, bool debug = false)
        {
            var rectangle = new Rectangle(0, 0, Component.Region.Rectangle.Width, Component.Region.Rectangle.Height);
            var config = Component.TemplateMatching;
            bool result = false;
            using (Image<Gray, byte> image = Preprocessing(inputImage))
            using (Image<Gray, byte> mark = Preprocessing(config.Image))
            {
                using (Image<Gray, float> resultTemplate = image.MatchTemplate(mark, TemplateMatchingType.CcoeffNormed))
                {
                    if (resultTemplate != null)
                    {
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        resultTemplate.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                        // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                        if (maxValues[0] > config.Score)
                        {
                            var x = maxLocations[0].X + Component.Region.X + Component.ObjectRegion.Width / 2;
                            var y = maxLocations[0].Y + Component.Region.Y + Component.ObjectRegion.Height / 2;
                            Console.WriteLine($"Center: X[{x}], Y[{y}]");
                            if (config.Inverted)
                            {
                                outputImage.Draw(rectangle, new Bgr(48, 59, 255), 2);
                                result = false;
                            }
                            else
                            {
                                var match = new Rectangle(maxLocations[0], mark.Size);
                                outputImage.Draw(match, new Bgr(255, 122, 0), 2);
                                outputImage.Draw(rectangle, new Bgr(65, 205, 40), 2);
                                result = true;
                            }
                        }
                        else
                        {
                            if (config.Inverted)
                            {
                                outputImage.Draw(rectangle, new Bgr(65, 205, 40), 2);
                                result = true;
                            }
                            else
                            {
                                outputImage.Draw(rectangle, new Bgr(48, 59, 255), 2);
                                result = false;
                            }
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region Contours
        /// <summary>
        /// Process boundary
        /// </summary>
        /// <param name="image"></param>
        /// <param name="method"></param>
        /// <param name="ROI"></param>
        /// <returns></returns>
        public Mat ProcessBoundaryDebug(Image<Bgr, byte> image, Algorithm method, Rectangle ROI = new Rectangle())
        {
            if (image == null)
                return new Mat();
            using (var inputImage = Preprocessing(image))
            {
                var outputImage = image.Clone();
                outputImage.Draw(ROI, new Bgr(0, 128, 255), 2);
                //if (rdoShowContours.IsChecked == true)
                if (true)
                {
                    inputImage.ROI = ROI;
                    outputImage.ROI = ROI;
                    var vertices = new Point[4];
                    var twoPoints = new Point[2];
                    switch (method)
                    {
                        case Algorithm.None:
                            break;
                        case Algorithm.BoundaryText:
                            {
                                vertices = FindBoundaryText(inputImage, outputImage, debug: true);
                                twoPoints = FindTwoSpecialPoints(inputImage, outputImage, debug: true);
                                for (int i = 0; i < twoPoints.Length; i++)
                                {
                                    CvInvoke.Circle(outputImage, twoPoints[i], 3, new MCvScalar(0, 0, 255), 3);
                                }
                                break;
                            }
                        case Algorithm.BoundaryBox:
                            {
                                vertices = FindBoundaryBox(inputImage, outputImage, debug: true);
                                break;
                            }
                        case Algorithm.BoundaryTextBox:
                            {
                                vertices = FindBoundaryTextBox(inputImage, outputImage, debug: true);
                                break;
                            }
                        case Algorithm.FeatureMatching:
                            {
                                vertices = FindBoundaryFeatureMatching(inputImage, Component.FeatureMatching.Image.Convert<Gray, byte>(), outputImage, debug: true);
                                break;
                            }
                        case Algorithm.TemplateMatching:
                            break;
                        case Algorithm.BarcodeDetection:
                            break;
                        case Algorithm.ColorDetection:
                            break;
                        default:
                            break;
                    }
                    CvInvoke.Polylines(outputImage, vertices, true, new MCvScalar(0, 255, 0), 4);
                    inputImage.ROI = Rectangle.Empty;
                    outputImage.ROI = Rectangle.Empty;
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        var p = new Point()
                        {
                            X = vertices[i].X + ROI.X,
                            Y = vertices[i].Y + ROI.Y
                        };
                        CvInvoke.PutText(outputImage, string.Format("{0}.({1},{2})", i + 1, p.X, p.Y), p,
                            FontFace.HersheyPlain, 3.5, new MCvScalar(0, 0, 255), 2);
                    }
                }
                else
                {
                    outputImage = inputImage.Convert<Bgr, byte>();
                }
                return outputImage.Mat.Clone();
            }
        }
        /// <summary>
        /// Find two special points
        /// </summary>
        /// <param name="inputImage"></param>
        /// <param name="outputImage"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public Point[] FindTwoSpecialPoints(Image<Gray, byte> inputImage, Image<Bgr, byte> outputImage = null, bool debug = false)
        {
            using (var contours = new VectorOfVectorOfPoint())
            {
                var config = Component.ContoursDetection;
                CvInvoke.FindContours(inputImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
                var listRect = new List<RotatedRect>();
                var points = new Point[2];
                for (int i = 0; i < contours.Size; i++)
                {
                    var rect = CvInvoke.BoundingRectangle(contours[i]);
                    if ((rect.Width > config.MinWidth) && (rect.Height > config.MinHeight))
                    {
                        if (debug && outputImage != null)
                        {
                            CvInvoke.DrawContours(outputImage, contours, i, new MCvScalar(255, 0, 0), 2);
                        }
                        var l = contours[i].ToArray();
                        var list = new List<PointF>();
                        foreach (var item in l)
                        {
                            list.Add(new PointF(item.X, item.Y));
                        }
                        var hull = CvInvoke.ConvexHull(list.ToArray(), true);
                        var area = CvInvoke.MinAreaRect(hull);
                        listRect.Add(area);
                        //Util.OrderVertices(Array.ConvertAll<PointF, Point>(area.GetVertices(), Point.Round))[3]
                        if (debug && outputImage != null)
                        {
                            outputImage.Draw(area, new Bgr(0, 255, 0), 2);
                        }

                    }
                }
                if (listRect.Count > 1)
                {
                    var sort = listRect.OrderBy(item => item.Center.X).ToList();
                    points[0] = Util.OrderVertices(Array.ConvertAll(sort[0].GetVertices(), Point.Round))[0];
                    points[1] = Util.OrderVertices(Array.ConvertAll(sort[sort.Count - 1].GetVertices(), Point.Round))[3];
                    //points[0] = Point.Round(sort[0].Center);
                    //points[1] = Point.Round(sort[sort.Count - 1].Center);
                }
                TwoPoints = Util.CopyPoints(points);
                return Util.CopyPoints(points);
            }
        }
        /// <summary>
        /// Find boundary of text. Support for method aligment BoundaryText
        /// </summary>
        /// <param name="inputImage">Source image</param>
        /// <param name="ouputImage">The function will use this image from drawing debug</param>
        /// <param name="debug">debug mode</param>
        /// <returns>Vertices of rectangle surround text</returns>
        public Point[] FindBoundaryText(Image<Gray, byte> inputImage, Image<Bgr, byte> ouputImage = null, bool debug = false)
        {
            try
            {
                var config = Component.ContoursDetection;
                using (var contours = new VectorOfVectorOfPoint())
                {
                    ListVertices = Array.Empty<Point>();
                    CvInvoke.FindContours(inputImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
                    List<PointF> lst = new List<PointF>();
                    for (int i = 0; i < contours.Size; i++)
                    {
                        var rect = CvInvoke.BoundingRectangle(contours[i]);
                        if (rect.Width > config.MinWidth &&
                            rect.Height > config.MinHeight &&
                            rect.Width < config.MaxWidth &&
                            rect.Height < config.MaxHeight)
                        {
                            if (debug && ouputImage != null)
                            {
                                CvInvoke.DrawContours(ouputImage, contours, i, new MCvScalar(255, 0, 0), 1);
                            }

                            var l = contours[i].ToArray();
                            foreach (var item in l)
                            {
                                lst.Add(new PointF(item.X, item.Y));
                            }
                        }
                    }
                    if (lst.Count > 0)
                    {
                        PointF[] hull = CvInvoke.ConvexHull(lst.ToArray(), true);
                        RotatedRect area = CvInvoke.MinAreaRect(hull);
                        var vertices = Array.ConvertAll(area.GetVertices(), Point.Round);
                        ListVertices = Util.OrderVertices(vertices);
                    }
                    return Util.CopyPoints(ListVertices);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return Array.Empty<Point>();
            }
        }
        /// <summary>
        /// Find boundary of box. Support for method aligment BoundaryBox
        /// </summary>
        /// <param name="inputImage">Source image</param>
        /// <param name="outputImage">The function will use this image from drawing debug</param>
        /// <param name="debug">debug mode</param>
        /// <returns>Vertices of rectangle surround box</returns>
        public Point[] FindBoundaryBox(Image<Gray, byte> inputImage, Image<Bgr, byte> outputImage = null, bool debug = false)
        {
            try
            {
                using (var contours = new VectorOfVectorOfPoint())
                {
                    ListVertices = Array.Empty<Point>();
                    CvInvoke.FindContours(inputImage, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                    if (debug && outputImage != null)
                    {
                        CvInvoke.DrawContours(outputImage, contours, -1, new MCvScalar(255, 0, 0), 1);
                    }
                    if (contours.Size > 0)
                    {
                        int largest_idx = -1; double maxSize = 0;
                        for (int i = 0; i < contours.Size; i++)
                        {
                            double mSize = CvInvoke.ContourArea(contours[i]);
                            if (mSize > maxSize)
                            {
                                maxSize = mSize;
                                largest_idx = i;
                            }
                        }
                        var area = CvInvoke.MinAreaRect(contours[largest_idx]);
                        var vertices = Array.ConvertAll(area.GetVertices(), Point.Round);
                        ListVertices = Util.OrderVertices(vertices);
                        return Util.CopyPoints(ListVertices);
                    }
                    else
                    {
                        return Array.Empty<Point>();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return null;
            }
        }
        /// <summary>
        /// Find boundary textbox
        /// </summary>
        /// <param name="inputImage"></param>
        /// <param name="outputImage"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public Point[] FindBoundaryTextBox(Image<Gray, byte> inputImage, Image<Bgr, byte> outputImage = null, bool debug = false)
        {
            var config = Component.ContoursDetection;
            using (var contours = new VectorOfVectorOfPoint())
            {
                ListVertices = Array.Empty<Point>();
                CvInvoke.FindContours(inputImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
                List<PointF> lst = new List<PointF>();
                for (int i = 0; i < contours.Size; i++)
                {
                    var rect = CvInvoke.BoundingRectangle(contours[i]);
                    if (rect.Width > config.MinWidth &&
                        rect.Height > config.MinHeight &&
                        rect.Width < config.MaxWidth &&
                        rect.Height < config.MaxHeight)
                    {
                        if (debug && outputImage != null)
                        {
                            CvInvoke.DrawContours(outputImage, contours, i, new MCvScalar(255, 0, 0), 1);
                        }

                        var l = contours[i].ToArray();
                        foreach (var item in l)
                        {
                            lst.Add(new PointF(item.X, item.Y));
                        }
                    }
                }
                if (lst.Count > 0)
                {
                    var sort_lst_x = lst.OrderBy(elem => elem.X).ToList();
                    var sort_lst_y = lst.OrderBy(elem => elem.Y).ToList();

                    var top_left = new Point()
                    {
                        X = (int)Math.Round(sort_lst_x[0].X),
                        Y = (int)Math.Round(sort_lst_y[0].Y)
                    };

                    var bottom_right = new Point()
                    {
                        X = (int)Math.Round(sort_lst_x[sort_lst_x.Count - 1].X),
                        Y = (int)Math.Round(sort_lst_y[sort_lst_x.Count - 1].Y)
                    };
                    var vertices = new Point[]
                    {
                        new Point() {X = top_left.X, Y = top_left.Y},
                        new Point() {X = bottom_right.X, Y = top_left.Y},
                        new Point() {X = bottom_right.X, Y = bottom_right.Y},
                        new Point() {X = top_left.X, Y = bottom_right.Y},
                    };
                    ListVertices = Util.OrderVertices(vertices);
                }
                return Util.CopyPoints(ListVertices);
            }
        }
        #endregion

        #region Text
        ///// <summary>
        ///// Text detection
        ///// </summary>
        ///// <param name="inputImage"></param>
        ///// <returns></returns>
        //public string TextDetection(Image<Bgr, byte> inputImage, ref string text, Image<Bgr, byte> outputImage = null)
        //{
        //    try
        //    {
        //        var rectangle = new Rectangle(0, 0, Component.Region.Rectangle.Width, Component.Region.Rectangle.Height);
        //        var config = Component.TextDetection;
        //        using (var image = Preprocessing(inputImage))
        //        {
        //            switch (config.Mode)
        //            {
        //                case Helper.Enums.TextMode.Tesseract:
        //                    {
        //                        text = TesseractText(image);
        //                        break;
        //                    }
        //                case Helper.Enums.TextMode.Halcon:
        //                    {
        //                        text = HalconText(image);
        //                        break;
        //                    }
        //                default:
        //                    break;
        //            }
        //            if (text == config.textDetection.textTemplate)
        //            {
        //                outputImage.Draw(rectangle, new Bgr(89, 199, 52), 3);
        //            }
        //            else
        //            {
        //                outputImage.Draw(rectangle, new Bgr(48, 59, 255), 3);
        //            }
        //            return text;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Instance.Write(ex.StackTrace);
        //        return string.Empty;
        //    }
        //    finally
        //    {
        //        inputImage?.Dispose();
        //    }
        //}
        ///// <summary>
        ///// Text detection by tesseract
        ///// </summary>
        ///// <param name="inputImage"></param>
        ///// <returns></returns>
        //public string TesseractText(Image<Gray, byte> inputImage)
        //{
        //    var ocrtext = string.Empty;
        //    using (var engine = new Tesseract.TesseractEngine(@"./tessdata", "eng", Tesseract.EngineMode.Default))
        //    {
        //        using (var image = Tesseract.PixConverter.ToPix(inputImage.ToBitmap()))
        //        {
        //            using (var page = engine.Process(image))
        //            {
        //                ocrtext = page.GetText();
        //            }
        //        }
        //    }
        //    return ocrtext;
        //}
        ///// <summary>
        ///// Text detection by halcon
        ///// </summary>
        ///// <param name="inputImage"></param>
        ///// <returns></returns>
        //public string HalconText(Image<Gray, byte> inputImage)
        //{
        //    // Local iconic variables 
        //    HObject ho_Image;
        //    HObject ho_Characters;
        //    // Local control variables 
        //    var hv_TextModel = new HTuple();
        //    var hv_TextResultID = new HTuple();
        //    var hv_Class = new HTuple();

        //    // Initialize local and output iconic variables 
        //    HOperatorSet.GenEmptyObj(out ho_Image);
        //    HOperatorSet.GenEmptyObj(out ho_Characters);
        //    ho_Image.Dispose();
        //    ho_Image = Halcon.BitmapToHalconImage(inputImage.ToBitmap());
        //    hv_TextModel.Dispose();
        //    HOperatorSet.CreateTextModelReader("auto", "Document_Rej.omc", out hv_TextModel);

        //    //Optionally specify text properties
        //    HOperatorSet.SetTextModelParam(hv_TextModel, "min_char_height", 20);
        //    hv_TextResultID.Dispose();
        //    HOperatorSet.FindText(ho_Image, hv_TextModel, out hv_TextResultID);

        //    //Return character regions and corresponding classification results
        //    ho_Characters.Dispose();
        //    HOperatorSet.GetTextObject(out ho_Characters, hv_TextResultID, "all_lines");
        //    hv_Class.Dispose();
        //    HOperatorSet.GetTextResult(hv_TextResultID, "class", out hv_Class);
        //    string text = string.Join("", hv_Class.ToSArr());

        //    ho_Image.Dispose();
        //    ho_Characters.Dispose();
        //    hv_TextModel.Dispose();
        //    hv_TextResultID.Dispose();
        //    hv_Class.Dispose();

        //    return text;
        //}
        #endregion

        #region Color
        ///// <summary>
        ///// Get range hsv color
        ///// </summary>
        ///// <param name="inputImage"></param>
        ///// <returns></returns>
        //public List<int> GetHsvValue(Image<Bgr, byte> inputImage)
        //{
        //    var hsvList = new List<int>();
        //    var src = inputImage.Mat;
        //    using (Mat hsv = new Mat())
        //    {
        //        CvInvoke.CvtColor(src, hsv, ColorConversion.Bgr2Hsv);
        //        Mat[] channels = hsv.Split();
        //        RangeF H = channels[0].GetValueRange();
        //        RangeF S = channels[1].GetValueRange();
        //        RangeF V = channels[2].GetValueRange();
        //        hsvList.Add((int)H.Min);
        //        hsvList.Add((int)H.Max);
        //        hsvList.Add((int)S.Min);
        //        hsvList.Add((int)S.Max);
        //        hsvList.Add((int)V.Min);
        //        hsvList.Add((int)V.Max);
        //    };
        //    return hsvList;
        //}
        ///// <summary>
        ///// Get image by hsv
        ///// </summary>
        ///// <param name="inputImage"></param>
        ///// <param name="minHue"></param>
        ///// <param name="maxHue"></param>
        ///// <param name="minSaturation"></param>
        ///// <param name="maxSaturation"></param>
        ///// <param name="minValue"></param>
        ///// <param name="maxValue"></param>
        ///// <returns></returns>
        //public Image<Gray, byte> GetImageByHsv(Image<Bgr, byte> inputImage, int minHue, int maxHue, int minSaturation, int maxSaturation, int minValue, int maxValue)
        //{
        //    using var hsv = inputImage.Convert<Hsv, byte>();
        //    using var lowerScalarArray = new ScalarArray(new MCvScalar(minHue, minSaturation, minValue));
        //    using var upperScalarArray = new ScalarArray(new MCvScalar(maxHue, maxSaturation, maxValue));
        //    using var dst = new Image<Gray, byte>(inputImage.Width, inputImage.Height);
        //    CvInvoke.InRange(hsv, lowerScalarArray, upperScalarArray, dst);
        //    using (var backColor = new Image<Bgr, byte>(inputImage.Width, inputImage.Height))
        //    {
        //        // Background is color after threshold color
        //        CvInvoke.BitwiseAnd(inputImage, inputImage, backColor, dst);
        //    }
        //    return dst.Clone();
        //}
        ///// <summary>
        ///// Calculate color
        ///// </summary>
        ///// <param name="inputImage"></param>
        ///// <param name="roi"></param>
        ///// <returns></returns>
        //public int CalculateColor(Image<Bgr, byte> inputImage, Rectangle roi)
        //{
        //    inputImage.ROI = roi;
        //    using (var imgDst = PreprocessingColor(inputImage))
        //    {
        //        var image_region = imgDst.GetInputArray().GetMat();
        //        return CvInvoke.CountNonZero(imgDst);
        //    }
        //    inputImage.ROI = Rectangle.Empty;
        //}
        #endregion

        #region Circle
        ///// <summary>
        ///// Circle
        ///// </summary>
        ///// <param name="src"></param>
        ///// <param name="outputImage"></param>
        ///// <param name="debug"></param>
        ///// <returns></returns>
        //public CircleF Circle(Image<Bgr, byte> inputImage, Image<Bgr, byte> outputImage = null, bool debug = false)
        //{
        //    var rectangle = new Rectangle(0, 0, Component.Region.Rectangle.Width, Component.Region.Rectangle.Height);
        //    var config = Component.CircleDetection;
        //    using (var image = Preprocessing(inputImage))
        //    {
        //        var circles = CvInvoke.HoughCircles(image, HoughModes.Gradient,
        //                                            config.dp, config.minDist,
        //                                            config.param1, config.param2,
        //                                            config.minRadius, config.maxRadius);
        //        foreach (var circle in circles)
        //        {
        //            if (circle.Radius >= config.threshRadius)
        //            {
        //                outputImage.Draw(rectangle, new Bgr(89, 199, 52), 3);
        //                outputImage.Draw(circle, new Bgr(89, 199, 52), 3);
        //                return circle;
        //            }
        //        }
        //        outputImage.Draw(rectangle, new Bgr(48, 59, 255), 3);
        //        return new CircleF();
        //    }
        //    //double dp = 1.0;    //dp la ti so nghich dao cua do phan giai (ap dung trong phuong phap hien tai)
        //    //double minDist = 200;    //min_dist la khoang cach nho nhat giua hai tam duong tron phat hien duoc
        //    //var param1 = 150;   //nguong tren phuc vu cho viec phat hien bien bang phuong phap candy
        //    //var param2 = 30;    //nguong duoi phuc vu cho viec phat hien bien bang phuong phap candy
        //    //int minRadius = 10;     //ban kinh gioi han nho nhat
        //    //int maxRadius = 100;    //ban kinh gioi han lon nhat
        //}
        #endregion

        #region Edge
        ///// <summary>
        ///// Edge
        ///// </summary>
        ///// <param name="src">Source image</param>
        ///// <param name="outputImage">The function will use this image from drawing debug</param>
        ///// <param name="debug">debug mode</param>
        ///// <returns>Vertices of rectangle surround box</returns>
        //public Mat FinEdge(Image<Gray, byte> inputImage, Image<Bgr, byte> outputImage = null, bool debug = false)
        //{
        //    try
        //    {
        //        if (outputImage != null)
        //        {
        //            return inputImage.Canny("threshold1", "threshold1").Mat.Clone();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Instance.Write(ex.StackTrace);
        //        return null;
        //    }
        //}
        #endregion

        #region Padding
        /// <summary>
        /// Padding Rectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="maxSize"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static Rectangle PaddingRegion(Rectangle rect, Size maxSize, SizeF padding)
        {
            var x = (int)Math.Max(0, rect.X - rect.Width * 0.5 * padding.Width);
            var y = (int)Math.Max(0, rect.Y - rect.Height * 0.5 * padding.Height);
            var width = (int)(rect.Width + rect.Width * padding.Width);
            var height = (int)(rect.Height + rect.Height * padding.Height);

            if (x + width >= maxSize.Width)
            {
                x = Math.Max(0, maxSize.Width - width);
            }
            if (y + height >= maxSize.Height)
            {
                y = Math.Max(0, maxSize.Height - height);
            }
            return new Rectangle()
            {
                X = x,
                Y = y,
                Width = width,
                Height = height
            };
        }
        #endregion
    }
}
