﻿// author：      Administrator
// created time：2014/1/15 14:34:49
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.Util;
using Emgu.CV;
using Emgu.CV.UI;
using System.Windows.Media.Imaging;
using Emgu.CV.Structure;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
namespace CURELab.SignLanguage.HandDetector
{
    public enum HandEnum
    {
        Right, Left, Both, Intersect, None
    }
    /// <summary>
    /// add summary here
    /// </summary>
    public class OpenCVController : INotifyPropertyChanged
    {
        public static double CANNY_THRESH;
        public static double CANNY_CONNECT_THRESH;
        public HOGDescriptor Hog_Descriptor;

        private static OpenCVController singletonInstance;
        private OpenCVController()
        {
            CANNY_THRESH = 10;
            CANNY_CONNECT_THRESH = 20;
            Hog_Descriptor = new HOGDescriptor(new Size(60, 60), new Size(10, 10), new Size(5, 5), new Size(5, 5), 9, 1, -1, 0.2, false);

        }

        public static OpenCVController GetSingletonInstance()
        {
            if (singletonInstance == null)
            {
                singletonInstance = new OpenCVController();
            }
            return singletonInstance;
        }

        public Rectangle RecogBlob(PointF pos, int radius, Bitmap bmp)
        {
            Image<Bgr, Byte> openCVImg = new Image<Bgr, byte>(bmp);
            openCVImg.ROI = new Rectangle((int)pos.X - radius, (int)pos.Y - radius, radius * 2, radius * 2);
            Image<Gray, byte> gray_image = openCVImg.Convert<Gray, byte>().PyrDown().PyrUp();
            Image<Gray, Byte> cannyEdges = openCVImg.Canny(CANNY_THRESH, CANNY_CONNECT_THRESH);
            using (MemStorage stor = new MemStorage())
            {
                //Find contours with no holes try CV_RETR_EXTERNAL to find holes
                Contour<System.Drawing.Point> contours = cannyEdges.FindContours(
                 Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                 Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST);
                //Contour<System.Drawing.Point> contours = cannyEdges.FindContours();
                Rectangle rtn_rec = new Rectangle(0, 0, 0, 0);
                double max_area = 0;
                for (int i = 0; contours != null; contours = contours.HNext)
                {
                    i++;
                    if ((contours.Area > Math.Pow(10, 2)) && (contours.Area < Math.Pow(radius * 2, 2)) && contours.Area > max_area)
                    {
                        // Console.WriteLine(contours.Area);
                        rtn_rec = contours.GetMinAreaRect().MinAreaRect();
                        max_area = contours.Area;
                    }
                }
                rtn_rec.X += (int)pos.X - radius;
                rtn_rec.Y += (int)pos.Y - radius;
                return rtn_rec;

            }

        }
        public void CalHistogram(Bitmap bmp)
        {
            Image<Gray, Byte> img = new Image<Gray, byte>(bmp);
            // DenseHistogram hist = new DenseHistogram(
        }

        public Bitmap Histogram(Image<Bgra, Byte> image)
        {
            Image<Gray, byte> gray_image = image.Convert<Gray, byte>();
            DenseHistogram Histo = new DenseHistogram(255, new RangeF(0, 255));
            Histo.Calculate(new Image<Gray, Byte>[] { gray_image }, true, null);
            //The data is here
            //Histo.MatND.ManagedArray
            float[] GrayHist = new float[256];
            Histo.MatND.ManagedArray.CopyTo(GrayHist, 0);
            float max = 1;
            for (int i = 0; i < 256; i++)
            {
                if (GrayHist[i] > max)
                {
                    max = GrayHist[i];
                }
            }
            int height = 200;
            Bitmap histbmp = new Bitmap(512, height);
            using (Graphics g = Graphics.FromImage(histbmp))
            {
                for (int i = 0; i < 256; i++)
                {
                    Point p1 = new Point(i, height - (int)(GrayHist[i] / max * height));
                    Point p2 = new Point(i, height);
                    g.DrawLine(new Pen(Brushes.Red, 2), p1, p2);

                }
            }
            return histbmp;
        }
        public Bitmap Histogram(Bitmap bmp)
        {
            Image<Bgra, Byte> openCVImg = new Image<Bgra, byte>(bmp);
            return Histogram(openCVImg);

        }
        public Bitmap Color2Gray(Bitmap bmp)
        {
            Image<Bgr, Byte> openCVImg = new Image<Bgr, byte>(bmp);
            return openCVImg.Convert<Gray, byte>().ToBitmap();
        }

        public Bitmap Color2Edge(PointF pos, int radius, Bitmap bmp)
        {
            Image<Bgr, Byte> openCVImg = new Image<Bgr, byte>(bmp);
            Image<Gray, byte> gray_image = openCVImg.Convert<Gray, byte>().PyrDown().PyrUp();
            gray_image.ROI = new Rectangle((int)pos.X - radius, (int)pos.Y - radius, radius * 2, radius * 2);
            Image<Gray, Byte> cannyEdges = openCVImg.Canny(50, 150);
            cannyEdges.ROI = Rectangle.Empty;
            return cannyEdges.ToBitmap();
        }


        public Image<Gray, Byte> RecogEdge(BitmapSource bs)
        {
            return RecogEdge(bs.ToBitmap());
        }

        public Image<Gray, Byte> RecogEdge(Bitmap bs)
        {
            Image<Bgr, Byte> openCVImg = new Image<Bgr, byte>(bs);
            return CannyEdge(openCVImg, CANNY_THRESH, CANNY_CONNECT_THRESH);
        }

        private Image<Gray, Byte> CannyEdge(Image<Bgr, Byte> img, double cannyThresh, double cannyConnectThresh)
        {
            Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();
            return gray.Canny(cannyThresh, cannyConnectThresh);
        }

        public Image<Gray, Byte> RecogEdgeBgra(Bitmap bmp)
        {
            Image<Bgra, Byte> openCVImg = new Image<Bgra, byte>(bmp);
            return CannyEdgeBgra(openCVImg, CANNY_THRESH, CANNY_CONNECT_THRESH);
        }

        private Image<Gray, Byte> CannyEdgeBgra(Image<Bgra, Byte> img, double cannyThresh, double cannyConnectThresh)
        {
            Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();
            return gray.Canny(cannyThresh, cannyConnectThresh);
        }


        #region Hand recognition


        private bool Intersect = false;
        int minSize = 1000;
        Point RightHandCenter = new Point();
        Point LeftHandCenter = new Point();
        int hogSize = 4356;
        Image<Gray, Byte> grayImg;
        public unsafe HandShapeModel FindHandPart(
            ref Image<Bgra, Byte> image,
            out Image<Gray, Byte> rightFront,
            out Image<Gray, Byte> leftFront,
            int handDepth
       )
        {
            //process hand size
            if (handDepth > 0)
            {
                begin = (int)(100.0 * 240.0 / handDepth / 0.39);
                end = (int)(200 * 240.0 / handDepth / 0.39);
                minLength = (int)(150 * 240.0 / handDepth / 0.39);
            }
            else
            {
                rightFront = null;
                leftFront = null;
                return null;
            }

            //Console.WriteLine(begin);
            //Console.WriteLine(end);
            //Console.WriteLine(minLength);
            rightFront = new Image<Gray, byte>(new Size(60, 60));
            leftFront = new Image<Gray, byte>(new Size(60, 60));
            HandShapeModel model = null;
            //find contour
            grayImg = image.Convert<Gray, byte>();
            Image<Gray, Byte> binaryImg = GetBinaryImg(image);
            List<MCvBox2D> rectList = FindContourMBox(binaryImg);
            //draw contour
            foreach (var rect in rectList)
            {
                DrawPoly(rect.GetVertices().ToPoints(), image, new MCvScalar(255, 0, 0));
            }
            // find hand
            rectList = rectList.OrderByDescending(x => x.GetTrueArea()).ToList();
            MCvBox2D rightHand;
            MCvBox2D leftHand;

            Font textFont = new Font(FontFamily.Families[0], 20);
            // count hands number
            using (Graphics g = Graphics.FromImage(image.Bitmap))
            {
                // 3 conditions in total
                if (rectList.Count() >= 2)//two hands
                {
                    if (rectList[0].center.X > rectList[1].center.X)
                    {
                        rightHand = rectList[0];
                        leftHand = rectList[1];
                    }
                    else
                    {
                        rightHand = rectList[1];
                        leftHand = rectList[0];
                    }
                    // mark intersect state
                    Intersect = rightHand.MinAreaRect().IsCloseTo(leftHand.MinAreaRect(), 5);
                    //right hand
                    Point[] SplittedRightHand = SplitHand(rightHand, HandEnum.Right);
                    rightFront = GetSubImage<Gray>(binaryImg, SplittedRightHand, rightHand.angle);
                    float[] rightHog = CalHog(rightFront);
                    DrawHand(SplittedRightHand, image, HandEnum.Right);
                    //left hand
                    Point[] SplittedLeftHand = SplitHand(leftHand, HandEnum.Left);
                    leftFront = GetSubImage<Gray>(binaryImg, SplittedLeftHand, leftHand.angle);
                    float[] leftHog = CalHog(leftFront);
                    DrawHand(SplittedLeftHand, image, HandEnum.Left);
                    g.DrawString("left and right", textFont, Brushes.Red, 0, 20);

                    model = new HandShapeModel(hogSize, HandEnum.Both);
                    model.hogLeft = leftHog;
                    model.hogRight = rightHog;
                }
                else if (rectList.Count() == 1) // one rectangle
                {
                    string text = "";
                    leftFront = null;


                    if (Intersect)
                    {
                        text = "Two hands";
                        //Point[] SplittedHand = SplitHand(rectList[0], HandEnum.Intersect);
                        Point[] SplittedHand = SplitHand(rectList[0], HandEnum.Right);
                        rightFront = GetSubImage<Gray>(binaryImg, SplittedHand, rectList[0].angle);
                        DrawHand(SplittedHand, image, HandEnum.Intersect);
                        float[] TwoHandHOG = CalHog(rightFront);
                        model = new HandShapeModel(hogSize, HandEnum.Intersect);
                        model.hogRight = TwoHandHOG;
                    }
                    else
                    {
                        text = "right";
                        Point[] SplittedRightHand = SplitHand(rectList[0], HandEnum.Right);
                        rightFront = GetSubImage<Gray>(binaryImg, SplittedRightHand, rectList[0].angle);
                        DrawHand(SplittedRightHand, image, HandEnum.Right);
                        float[] TwoHandHOG = CalHog(rightFront);
                        model = new HandShapeModel(hogSize, HandEnum.Right);
                        model.hogRight = TwoHandHOG;
                    }
                    g.DrawString(text, textFont, Brushes.Red, 0, 20);

                }
            }


            return model;
        }

        public float[] ResizeAndCalHog(ref Image<Bgr, byte> image)
        {
            Image<Gray, byte> binaryImg = GetBinaryImg(image);
            List<Rectangle> rectList = FindContourRect(binaryImg);
            rectList = rectList.OrderByDescending(x => x.GetRectArea()).ToList();
            if (rectList.Count() >= 1)
            {
                Rectangle r = rectList[0];
                Image<Gray, byte> s_image = (binaryImg.Copy(r) * 255);
                image = ResizeImage<Gray>(s_image, 60, 60).Convert<Bgr, byte>();
                return CalHog(image);

            }
            return null;
        }

        public float[] ResizeAndCalHog(Image<Gray, byte> image)
        {
            if (image == null)
            {
                return null;
            }
            Image<Bgr, byte> bgr = image.Convert<Bgr, byte>();
            return ResizeAndCalHog(ref bgr);
        }

        public float[] CalHog(Image<Gray, byte> image)
        {
            if (image == null)
            {
                return null;
            }
            return CalHog(image.Convert<Bgr, byte>());
        }

        public float[] CalHog(Image<Bgr, byte> image)
        {
            if (image == null)
            {
                return null;
            }
            return Hog_Descriptor.Compute(image, new Size(1, 1), new Size(0, 0), null);
        }



        public Image<Gray, byte> GetBinaryImg(Image<Bgr, byte> image)
        {
            if (image == null)
            {
                return null;
            }
            return image.Convert<Gray, byte>().ThresholdBinaryInv
                (new Gray(200), new Gray(255));
        }

        public Image<Gray, byte> GetBinaryImg(Image<Bgra, byte> image)
        {
            if (image == null)
            {
                return null;
            }
            return image.Convert<Gray, byte>().ThresholdBinaryInv
                (new Gray(200), new Gray(255));
        }

        private unsafe List<Rectangle> FindContourRect(Image<Gray, byte> image)
        {
            if (image == null)
            {
                return null;
            }
            Seq<System.Drawing.Point> DyncontourTemp = FindContourSeq(image);
            List<Rectangle> rectList = new List<Rectangle>();
            for (; DyncontourTemp != null && DyncontourTemp.Ptr.ToInt32() != 0; DyncontourTemp = DyncontourTemp.HNext)
            {
                //iterate contours
                if (DyncontourTemp.GetMinAreaRect().GetTrueArea() < minSize)
                {
                    continue;
                }
                rectList.Add(DyncontourTemp.BoundingRectangle);
            }
            return rectList;
        }

        private unsafe List<MCvBox2D> FindContourMBox(Image<Gray, byte> image)
        {
            Seq<System.Drawing.Point> DyncontourTemp = FindContourSeq(image);
            List<MCvBox2D> rectList = new List<MCvBox2D>();
            for (; DyncontourTemp != null && DyncontourTemp.Ptr.ToInt32() != 0; DyncontourTemp = DyncontourTemp.HNext)
            {
                //iterate contours
                if (DyncontourTemp.GetMinAreaRect().GetTrueArea() < minSize)
                {
                    continue;
                }
                rectList.Add(DyncontourTemp.GetMinAreaRect());
            }
            return rectList;
        }

        private unsafe Seq<System.Drawing.Point> FindContourSeq(Image<Gray, byte> image)
        {
            //Find contours with no holes try CV_RETR_EXTERNAL to find holes
            IntPtr Dyncontour = new IntPtr();//存放检测到的图像块的首地址
            IntPtr Dynstorage = CvInvoke.cvCreateMemStorage(0);
            int n = CvInvoke.cvFindContours(image.Ptr, Dynstorage, ref Dyncontour, sizeof(MCvContour),
                Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL, Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, new System.Drawing.Point(0, 0));
            Seq<System.Drawing.Point> DyncontourTemp1 = new Seq<System.Drawing.Point>(Dyncontour, null);//方便对IntPtr类型进行操作
            Seq<System.Drawing.Point> DyncontourTemp = DyncontourTemp1;
            return DyncontourTemp;
        }



        private Image<T, Byte> GetSubImage<T>(Image<T, Byte> image, Point[] p, float angle) where T : struct, IColor
        {

            if (p == null)
            {
                return null;
            }
            // ensure the low most side being horizontal. 
            // angle is between the horizontal axis and the first side (i.e. width) in degrees
            if (angle < -45)
            {
                angle += 90;
            }
            Point lowP = p.OrderByDescending(x => x.Y).First();
            Point highP = p.OrderByDescending(x => x.DistanceTo(lowP)).First();
            Point widthP = p.OrderBy(x => x.TanWith(lowP)).First();
            SizeF size = new SizeF(lowP.DistanceTo(widthP), highP.DistanceTo(widthP));
            MCvBox2D box = new MCvBox2D(p.GetCenter(), size, angle);
            Image<T, Byte> mask = (image.Copy(box) * 255);

            return ResizeImage<T>(mask, 60, 60);

        }

        private Image<T, Byte> ResizeImage<T>(Image<T, Byte> mask, int width, int height) where T : struct, IColor
        {
            try
            {
                int l = mask.Width > mask.Height ? mask.Width : mask.Height;
                Image<T, Byte> result = new Image<T, byte>(l, l);
                Rectangle rect = new Rectangle();
                rect.X = result.Width / 2 - mask.Width / 2;
                rect.Y = result.Height / 2 - mask.Height / 2;
                rect.Width = mask.Width;
                rect.Height = mask.Height;
                result.ROI = rect;
                rect.X = rect.X < 0 ? 0 : rect.X;
                rect.Y = rect.Y < 0 ? 0 : rect.Y;
                CvInvoke.cvResize(mask.Ptr, result.Ptr, Emgu.CV.CvEnum.INTER.CV_INTER_NN);
                result.ROI = Rectangle.Empty;
                result = result.Resize(width, height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
                return result;

            }
            catch (Exception)
            {
                return null;
            }

        }
        private Image<T, Byte> GetSubImageByRect<T>(Image<T, Byte> image, Rectangle rect) where T : struct, IColor
        {

            if (rect == null)
            {
                return null;
            }

            Image<T, Byte> result = (image.Copy(rect) * 255).Resize(60, 60, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            return result.PyrDown().PyrUp();

        }

        /// <summary>
        /// 将IplImage*转换为Bitmap（注：在OpenCV中IplImage* 对应EmguCV的IntPtr类型）       
        /// </summary>
        /// <param name="ptrImage"></param>
        /// <returns>Bitmap对象</returns>
        public static Image<T, byte> ConvertIntPrToBitmap<T>(IntPtr ptrImage) where T : struct,IColor
        {
            //将IplImage指针转换成MIplImage结构
            MIplImage mi = (MIplImage)Marshal.PtrToStructure(ptrImage, typeof(MIplImage));

            Image<T, byte> image = new Image<T, byte>(mi.width, mi.height, mi.widthStep, mi.imageData);
            return image;
        }


        private Point GetCenterPoint(Point[] points)
        {
            try
            {
                if (points.Length <= 0)
                {
                    return Point.Empty;
                }
                int X = (int)points.Average((x => x.X));
                int Y = (int)points.Average((x => x.Y));
                return new Point(X, Y);
            }
            catch (Exception)
            {

                return Point.Empty;
            }

        }

        private void DrawHand(System.Drawing.Point[] rect, Image<Bgra, Byte> image, HandEnum handEnum)
        {
            DrawPoly(rect, image, new MCvScalar(0, 0, 255));
            Point center = GetCenterPoint(rect);
            DrawPoint(image, center, new MCvScalar(255, 0, 0));

            if (handEnum == HandEnum.Right)
            {
                RightHandCenter = center;
            }
            if (handEnum == HandEnum.Left)
            {
                LeftHandCenter = center;
            }
            if (handEnum == HandEnum.Intersect)
            {
                RightHandCenter = center;
            }
        }

        private void DrawPoly(System.Drawing.Point[] points, Image<Bgra, Byte> image, MCvScalar color)
        {

            if (points == null || points.Length <= 0)
            {
                return;
            }
            for (int j = 0; j < points.Length; j++)
            {
                CvInvoke.cvLine(image, points[j], points[(j + 1) % points.Length], color, 2, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
            }
        }

        private void DrawPoint(Image<Bgra, Byte> image, Point point, MCvScalar color)
        {
            if (point == null || point == Point.Empty)
            {
                return;
            }
            CvInvoke.cvCircle(image, point, 3, color, -1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
        }


        int begin = 35;
        int end = 70;
        int minLength = 50;

        private System.Drawing.Point[] SplitHand(MCvBox2D rect, HandEnum handEnum)
        {
            if (handEnum == HandEnum.Both || handEnum == HandEnum.Intersect)
            {
                return rect.MinAreaRect().GetPoints();
            }
            PointF[] pl = rect.GetVertices();
            Point[] splittedHands = new Point[4];
            //find angle of long edge
            PointF startP = pl[1];
            PointF shortP = pl[0];
            PointF longP = pl[2];
            PointF ap1 = new PointF();
            PointF ap2 = new PointF();

            if (pl[0].DistanceTo(startP) > pl[2].DistanceTo(startP))
            {
                shortP = pl[2];
                longP = pl[0];
            }

            float longDis = longP.DistanceTo(startP);
            if (longDis < minLength)
            {
                return rect.MinAreaRect().GetPoints();
            }
            float shortDis = shortP.DistanceTo(startP);
            // x and long edge slope 
            float longslope = Math.Abs(longP.X - startP.X) / longDis;
            float min = float.MaxValue;

            float factor = Math.Max(Math.Abs(longP.Y - startP.Y) / longDis, Math.Abs(longP.X - startP.X) / longDis);
            int TransformEnd = Convert.ToInt32(factor * end);
            int TransformBegin = Convert.ToInt32(factor * begin);
            // > 45
            if (longslope < 0.707)//vert
            {
                pl = pl.OrderBy((x => x.Y)).ToArray();
                startP = pl[0];
                shortP = pl[1];
                longP = pl[2];
                for (int y = TransformBegin; y < Convert.ToInt32(Math.Abs(longP.Y - startP.Y)) && Math.Abs(y) < TransformEnd; y++)
                {
                    PointF p1 = InterPolateP(startP, longP, y / Math.Abs(longP.Y - startP.Y));
                    PointF p2 = new PointF(p1.X + shortP.X - startP.X, p1.Y + shortP.Y - startP.Y);
                    float dis = GetHandWidthBetween(p1, p2);
                    if (dis < min)
                    {
                        min = dis;
                        ap1 = p1;
                        ap2 = p2;
                    }
                }
            }
            else // horizontal 
            {

                if (handEnum == HandEnum.Right)
                {
                    pl = pl.OrderBy((x => x.X)).ToArray();

                }
                else if (handEnum == HandEnum.Left)
                {
                    pl = pl.OrderByDescending((x => x.X)).ToArray();
                }
                startP = pl[0];
                shortP = pl[1];
                longP = pl[2];
                for (int X = TransformBegin; X < Convert.ToInt32(Math.Abs(longP.X - startP.X)) && Math.Abs(X) < TransformEnd; X++)
                {
                    PointF p1 = InterPolateP(startP, longP, X / Math.Abs(longP.X - startP.X));
                    PointF p2 = new PointF(p1.X + shortP.X - startP.X, p1.Y + shortP.Y - startP.Y);
                    float dis = GetHandWidthBetween(p1, p2);
                    if (dis < min)
                    {
                        min = dis;
                        ap1 = p1;
                        ap2 = p2;
                    }
                }
            }
            if (ap1 == null || ap1 == PointF.Empty)
            {
                return rect.MinAreaRect().GetPoints();
            }
            splittedHands[0] = startP.ToPoint();
            splittedHands[1] = ap1.ToPoint();
            splittedHands[2] = ap2.ToPoint();
            splittedHands[3] = shortP.ToPoint();
            return splittedHands;
        }

        private PointF InterPolateP(PointF p1, PointF p2, float disToP1)
        {
            float x = (p2.X - p1.X) * Math.Abs(disToP1) + p1.X;
            float y = (p2.Y - p1.Y) * Math.Abs(disToP1) + p1.Y;
            return new PointF(x, y);
        }

        private float GetHandWidthBetween(PointF p1, PointF p2)
        {
            float slope = Math.Abs(p2.X - p1.X) / p2.DistanceTo(p1);
            PointF p3 = new PointF();
            PointF p4 = new PointF();
            if (slope < 0.707)//vert
            {

                for (int Y = 0; Y < Math.Abs(p2.Y - p1.Y); Y++)
                {
                    p3 = InterPolateP(p1, p2, Y / (p2.Y - p1.Y));
                    if (IsHand(p3)) break;

                }
                for (int Y = 0; Y < Math.Abs(p2.Y - p1.Y); Y++)
                {
                    p4 = InterPolateP(p2, p1, Y / (p2.Y - p1.Y));
                    if (IsHand(p4)) break;

                }
                return p3.DistanceTo(p4);
            }
            else//hori
            {
                for (int x = 0; x < Math.Abs(p2.X - p1.X); x++)
                {
                    p3 = InterPolateP(p1, p2, x / (p2.X - p1.X));
                    if (IsHand(p3)) break;

                }
                for (int x = 0; x < Math.Abs(p2.X - p1.X); x++)
                {
                    p4 = InterPolateP(p2, p1, x / (p2.X - p1.X));
                    if (IsHand(p4)) break;

                }
                return p3.DistanceTo(p4);
            }
        }

        private bool IsHand(PointF p)
        {
            try
            {
                if (grayImg[p.ToPoint()].Intensity < 200)
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        #endregion

        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}