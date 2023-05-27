using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Foxconn.App.Helper;
using Microsoft.Win32.SafeHandles;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Foxconn.App.Controllers.Image
{
    public class FeatureResult : IDisposable
    {
        private readonly SafeHandle _handle = new SafeFileHandle(IntPtr.Zero, true);
        public VectorOfKeyPoint ModelKeyPoints { get; set; }
        public VectorOfKeyPoint ObservedKeyPoints { get; set; }
        public VectorOfVectorOfDMatch Matches { get; set; }
        public Point[] Vertices { get; set; }
        /// <summary>
        /// Number of Points which matched
        /// </summary>
        public int NumberPointMatch { get; set; }
        public Mat ImageResult { get; set; }
        private bool _disposed { get; set; }

        public FeatureResult()
        {
            ModelKeyPoints = new VectorOfKeyPoint();
            ObservedKeyPoints = new VectorOfKeyPoint();
            Matches = new VectorOfVectorOfDMatch();
            Vertices = Array.Empty<Point>();
        }

        #region Disposable
        ~FeatureResult()
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
                _handle?.Dispose();
                ModelKeyPoints?.Dispose();
                ObservedKeyPoints?.Dispose();
                Matches?.Dispose();
                ImageResult?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
            _disposed = true;
        }
        #endregion

        public void Init()
        {
            try
            {
                _disposed = false;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }
    }

    public class FeatureMatching
    {
        public static FeatureResult Matching(Mat modelImage, Mat observedImage, bool debug = false)
        {
            FeatureResult fResult = new FeatureResult();
            using (var featureDetectorExtractor = new SIFT())
            //using (var modelKeyPoints = new VectorOfKeyPoint())
            //using (var observedKeyPoints = new VectorOfKeyPoint())
            using (var matches = new VectorOfVectorOfDMatch())
            using (var modelDescriptors = new Mat())
            using (var uModelImage = modelImage.GetUMat(AccessType.Read))
            using (var observedDescriptors = new Mat())
            using (var uObservedImage = observedImage.GetUMat(AccessType.Read))
            using (var good = new VectorOfDMatch())
            //using (var mask = new Mat())
            //using (var homography = new Mat())
            {
                int k = 2;
                double uniquenessThreshold = 0.80;
                int ransacThreshold = 5;
                System.Diagnostics.Stopwatch watch;
                Mat mask = null;
                Mat homography = null;
                // extract features from the object image 
                featureDetectorExtractor.DetectAndCompute(uModelImage, null, fResult.ModelKeyPoints, modelDescriptors, false);

                watch = System.Diagnostics.Stopwatch.StartNew();

                // extract features from the observed image
                featureDetectorExtractor.DetectAndCompute(uObservedImage, null, fResult.ObservedKeyPoints, observedDescriptors, false);

                // Brute force, slower but more accurate
                // You can use KDTree for faster matching with slight loss in accuracy
                using (var ip = new LinearIndexParams())
                using (var sp = new SearchParams())
                using (var matcher = new FlannBasedMatcher(ip, sp))
                //using (var matcher = new BFMatcher(DistanceType.Hamming, false))
                {
                    matcher.Add(modelDescriptors);
                    matcher.KnnMatch(observedDescriptors, matches, k, null);
                    mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(255));
                    Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                    int nonZeroCount = CvInvoke.CountNonZero(mask);
                    if (nonZeroCount >= 4)
                    {
                        nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(fResult.ModelKeyPoints, fResult.ObservedKeyPoints,
                            matches, mask, 1.5, 20);
                        if (nonZeroCount >= 4)
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(fResult.ModelKeyPoints, fResult.ObservedKeyPoints,
                                matches, mask, 2);
                    }

                    // Store all the good matches as per lowe's ratio test.
                    for (int i = 0; i < matches.Size; i++)
                    {
                        var item = matches[i].ToArray();
                        if (item[0].Distance < 0.7 * item[1].Distance)
                        {
                            good.Push(item);
                        }
                    }
                    Console.WriteLine($"Matches Size: {matches.Size}");
                    Console.WriteLine($"Good Size: {good.Size}");

                    if (debug)
                    {
                        fResult.ImageResult = new Mat();
                        Features2DToolbox.DrawMatches(modelImage, fResult.ModelKeyPoints, observedImage, fResult.ObservedKeyPoints,
                            matches, fResult.ImageResult, new MCvScalar(255, 255, 255), new MCvScalar(255, 255, 255), mask,
                            Features2DToolbox.KeypointDrawType.NotDrawSinglePoints);
                        //Features2DToolbox.DrawMatches(modelImage, fResult.ModelKeyPoints, observedImage, fResult.ObservedKeyPoints,
                        //    new VectorOfVectorOfDMatch(good), fResult.ImageResult, new MCvScalar(255, 255, 255),
                        //    new MCvScalar(255, 255, 255), null, Features2DToolbox.KeypointDrawType.NotDrawSinglePoints);
                    }

                    if (good.Size >= 4)
                    {
                        MKeyPoint[] kptsModel = fResult.ModelKeyPoints.ToArray();
                        MKeyPoint[] kptsTest = fResult.ObservedKeyPoints.ToArray();

                        PointF[] srcPoints = new PointF[good.Size];
                        PointF[] destPoints = new PointF[good.Size];

                        for (int i = 0; i < good.Size; i++)
                        {
                            srcPoints[i] = kptsModel[good[i].TrainIdx].Point;
                            destPoints[i] = kptsTest[good[i].QueryIdx].Point;
                        }

                        homography = CvInvoke.FindHomography(srcPoints, destPoints, RobustEstimationAlgorithm.Ransac, ransacThreshold);
                        if (homography != null)
                        {
                            Rectangle rect = new Rectangle(Point.Empty, modelImage.Size);
                            var pts = new PointF[]
                            {
                                new PointF(rect.Left, rect.Bottom),
                                new PointF(rect.Right, rect.Bottom),
                                new PointF(rect.Right, rect.Top),
                                new PointF(rect.Left, rect.Top)
                            };
                            pts = CvInvoke.PerspectiveTransform(pts, homography);
                            fResult.Vertices = Array.ConvertAll(pts, Point.Round);
                            if (debug)
                            {
                                using (VectorOfPoint vp = new VectorOfPoint(fResult.Vertices))
                                {
                                    CvInvoke.Polylines(fResult.ImageResult, vp, true, new MCvScalar(255, 0, 0, 255), 5);
                                }
                            }
                            homography.Dispose();
                        }
                    }
                }
                watch.Stop();
                var matchTime = watch.ElapsedMilliseconds;
                Console.WriteLine($"Match Time: {matchTime} ms");
                mask?.Dispose();
                homography?.Dispose();
            }
            return fResult;
        }

        public static FeatureResult OldMatching(Mat modelImage, Mat observedImage, bool debug = false)
        {
            FeatureResult fResult = new FeatureResult();
            Feature2D featureDetectorExtractor = new SIFT();
            VectorOfDMatch good = new VectorOfDMatch();
            int k = 2; int ransacThreshold = 5;
            long matchTime;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            VectorOfVectorOfDMatch matches;
            Mat mask;
            Mat homography;
            FindMatch(modelImage, observedImage, featureDetectorExtractor, out matchTime,
                out modelKeyPoints, out observedKeyPoints, out matches, out mask, out homography);
            Console.WriteLine($"Match Time: {matchTime} ms");
            fResult.ModelKeyPoints = modelKeyPoints;
            fResult.ObservedKeyPoints = observedKeyPoints;
            // Store all the good matches as per Lowe's ratio test.
            for (int i = 0; i < matches.Size; i++)
            {
                var item = matches[i].ToArray();
                if (item[0].Distance < 0.7 * item[1].Distance)
                {
                    good.Push(item);
                }
            }
            if (debug)
            {
                fResult.ImageResult = new Mat();
                Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
                    matches, fResult.ImageResult, new MCvScalar(255, 255, 255), new MCvScalar(255, 255, 255), mask,
                    Features2DToolbox.KeypointDrawType.NotDrawSinglePoints);
            }
            if (good.Size >= 4)
            {
                MKeyPoint[] kptsModel = fResult.ModelKeyPoints.ToArray();
                MKeyPoint[] kptsTest = fResult.ObservedKeyPoints.ToArray();

                PointF[] srcPoints = new PointF[good.Size];
                PointF[] destPoints = new PointF[good.Size];

                for (int i = 0; i < good.Size; i++)
                {
                    srcPoints[i] = kptsModel[good[i].TrainIdx].Point;
                    destPoints[i] = kptsTest[good[i].QueryIdx].Point;
                }

                homography = CvInvoke.FindHomography(srcPoints, destPoints, RobustEstimationAlgorithm.Ransac, ransacThreshold);
                if (homography != null)
                {
                    Rectangle rect = new Rectangle(Point.Empty, modelImage.Size);
                    PointF[] pts = new PointF[]
                    {
                            new PointF(rect.Left, rect.Bottom),
                            new PointF(rect.Right, rect.Bottom),
                            new PointF(rect.Right, rect.Top),
                            new PointF(rect.Left, rect.Top)
                    };
                    pts = CvInvoke.PerspectiveTransform(pts, homography);
                    fResult.Vertices = Array.ConvertAll(pts, Point.Round);
                    if (debug)
                    {
                        using (VectorOfPoint vp = new VectorOfPoint(fResult.Vertices))
                        {
                            CvInvoke.Polylines(fResult.ImageResult, vp, true, new MCvScalar(255, 0, 0, 255), 5);
                        }
                    }
                    homography.Dispose();
                }
            }
            matches?.Dispose();
            mask?.Dispose();
            fResult.Matches = new VectorOfVectorOfDMatch(good);
            fResult.NumberPointMatch = fResult.Matches.Size;
            return fResult;
        }

        public static void FindMatch(
            Mat modelImage,
            Mat observedImage,
            Feature2D featureDetectorExtractor,
            out long matchTime,
            out VectorOfKeyPoint modelKeyPoints,
            out VectorOfKeyPoint observedKeyPoints,
            out VectorOfVectorOfDMatch matches,
            out Mat mask,
            out Mat homography)
        {
            int k = 2;
            double uniquenessThreshold = 0.80;

            System.Diagnostics.Stopwatch watch;
            homography = null;

            modelKeyPoints = new VectorOfKeyPoint();
            observedKeyPoints = new VectorOfKeyPoint();
            matches = new VectorOfVectorOfDMatch();

            using (UMat uModelImage = modelImage.GetUMat(AccessType.Read))
            using (UMat uObservedImage = observedImage.GetUMat(AccessType.Read))
            {
                // extract features from the object image
                Mat modelDescriptors = new Mat();
                featureDetectorExtractor.DetectAndCompute(uModelImage, null, modelKeyPoints, modelDescriptors, false);

                watch = System.Diagnostics.Stopwatch.StartNew();

                // extract features from the observed image
                Mat observedDescriptors = new Mat();
                featureDetectorExtractor.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);

                // Brute force, slower but more accurate
                // You can use KDTree for faster matching with slight loss in accuracy
                using (LinearIndexParams ip = new LinearIndexParams())
                using (SearchParams sp = new SearchParams())
                using (DescriptorMatcher matcher = new FlannBasedMatcher(ip, sp))
                {
                    matcher.Add(modelDescriptors);

                    matcher.KnnMatch(observedDescriptors, matches, k, null);
                    mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(255));
                    Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                    int nonZeroCount = CvInvoke.CountNonZero(mask);
                    if (nonZeroCount >= 4)
                    {
                        nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
                            matches, mask, 1.5, 20);
                        if (nonZeroCount >= 4)
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
                                observedKeyPoints, matches, mask, 2);
                    }
                }
                watch.Stop();

            }
            matchTime = watch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Draw the model image and observed image, the matched features and homography projection.
        /// </summary>
        /// <param name="modelImage">The model image</param>
        /// <param name="observedImage">The observed image</param>
        /// <param name="matchTime">The output total time for computing the homography matrix.</param>
        /// <param name="featureDetectorExtractor">The feature detector extractor</param>
        /// <returns>The model image and observed image, the matched features and homography projection.</returns>
        public static Mat Draw(Mat modelImage, Mat observedImage, VectorOfVectorOfDMatch matches, Feature2D featureDetectorExtractor, out long matchTime)
        {
            Mat homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            Mat mask;
            FindMatch(modelImage, observedImage, featureDetectorExtractor, out matchTime, out modelKeyPoints, out observedKeyPoints, out matches,
               out mask, out homography);

            //Draw the matched keypoints
            Mat result = new Mat();
            Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
               matches, result, new MCvScalar(255, 255, 255), new MCvScalar(255, 255, 255), mask);

            #region draw the projected region on the image
            if (homography != null)
            {
                //draw a rectangle along the projected model
                Rectangle rect = new Rectangle(Point.Empty, modelImage.Size);
                PointF[] pts = new PointF[]
                {
                        new PointF(rect.Left, rect.Bottom),
                        new PointF(rect.Right, rect.Bottom),
                        new PointF(rect.Right, rect.Top),
                        new PointF(rect.Left, rect.Top)
                };
                pts = CvInvoke.PerspectiveTransform(pts, homography);

                Point[] points = new Point[pts.Length];
                for (int i = 0; i < points.Length; i++)
                    points[i] = Point.Round(pts[i]);

                using (VectorOfPoint vp = new VectorOfPoint(points))
                {
                    CvInvoke.Polylines(result, vp, true, new MCvScalar(255, 0, 0, 255), 5);
                }
            }
            #endregion

            return result;
        }
    }
}
