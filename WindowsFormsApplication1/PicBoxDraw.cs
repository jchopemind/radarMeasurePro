
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    class PicBoxDraw
    {
        private Graphics graphic;
        Pen myPen;
        Point center0;
        static double OneDegree = 0.0174533;
        System.Drawing.SolidBrush myBrush;
        float width;
        float height;


        public PicBoxDraw(Graphics graphics, int width, int height)
        {
            this.graphic = graphics;
            this.width = width;
            this.height = height;
            myPen = new Pen(Color.Blue, 1);
            myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green);//画刷

        }

        public void draw5_185_base(Pen MyPen, int sdegree, int edegree)
        {
            graphic.SmoothingMode = SmoothingMode.AntiAlias;  //使绘图质量最高，即消除锯齿
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.CompositingQuality = CompositingQuality.HighQuality;
            sdegree = sdegree + 180;
            edegree = edegree + 180;

            center0.X = (int)(width / 2);
            center0.Y = (int)(height * 0.5);

            int x2 = center0.X - (int)((width) * Math.Cos(-OneDegree * sdegree));
            int y2 = center0.Y - (int)((width) * Math.Sin(-OneDegree * sdegree));

            graphic.DrawLine(MyPen, center0.X, center0.Y, x2, y2);

            x2 = center0.X - (int)((width) * Math.Cos(-OneDegree * edegree));
            y2 = center0.Y - (int)((width) * Math.Sin(-OneDegree * edegree));
            graphic.DrawLine(MyPen, center0.X, center0.Y, x2, y2);

        }

        Point ConvertPoint(int length, double degree)
        {
            Point cedPoint = new Point();

            cedPoint.X = (int)(center0.X + length * Math.Cos(degree * OneDegree));
            cedPoint.Y = (int)(center0.Y - length * Math.Sin(degree * OneDegree));


            return cedPoint;
        }

        public void drawAPoint(int length, double degree)
        {
            Point poi = ConvertPoint(length, degree);
            graphic.FillEllipse(myBrush, poi.X, poi.Y, 3, 3);//画实心圆
        }
    }
}
