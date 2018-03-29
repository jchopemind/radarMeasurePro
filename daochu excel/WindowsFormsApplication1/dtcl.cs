using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApplication1
{
    public partial class dtcl : Form
    {
        byte close_flag = 1;

        private Queue<double> dataQueue1 = new Queue<double>(100);

        private Queue<double> dataQueue2 = new Queue<double>(100);

        private Queue<double> dataQueue3 = new Queue<double>(100);

        private int curValue = 0;

        private int num = 5;//每次删除增加几个点

        public dtcl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtcl_Load(object sender, EventArgs e)
        {
            tzcl_bto.Enabled = false;
            kscl_bto.Enabled = true;
            bctp_bto.Enabled = false;
            bcbx_bto.Enabled = false;

            Init_Chart1();
            Init_Chart2();
            Init_Chart3();

            UpdateQueueValue_chart1();
            UpdateQueueValue_chart2();
            UpdateQueueValue_chart3();

            this.chart1.Series[0].Points.AddXY(0,0);
            this.chart2.Series[0].Points.AddXY(0,0);
            this.chart3.Series[0].Points.AddXY(0,0);



            if (!Directory.Exists("d:\\波形图"))//创建照片文件夹
            {
                Directory.CreateDirectory("d:\\波形图");
            }
        }

        private void dtcl_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (close_flag == 1)
            {
                Application.Exit();
            }
        }

        private void dtcl_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (close_flag == 1)
            //{
            //    DialogResult result = MessageBox.Show("你确定要关闭吗！", "提示信息", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            //    if (result == DialogResult.OK)
            //    {
            //        e.Cancel = false;  //点击OK 
            //    }
            //    else
            //    {
            //        e.Cancel = true;
            //    }
            //}
            
        }

        private void fh_bto_Click(object sender, EventArgs e)
        {
            close_flag = 0;
            this.Close();
            new Form1().Show();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void bctp_bto_Click(object sender, EventArgs e)
        {
            Save_Chart_pictures();
        }

        private void chart3_Click(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 开始事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            this.timer1.Start();
        }

        /// <summary>
        /// 停止事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, EventArgs e)
        {
            this.timer1.Stop();
        }

        //更新队列中的值
        private void UpdateQueueValue_chart2()
        {

            if (dataQueue2.Count > 100)
            {
                //先出列
                for (int i = 0; i < num; i++)
                {
                    dataQueue2.Dequeue();
                }
            }

                Random r = new Random();
                for (int i = 0; i < num; i++)
                {
                    dataQueue2.Enqueue(r.Next(0, 100));
                }
        }

        private void UpdateQueueValue_chart1()
        {

            if (dataQueue1.Count > 100)
            {
                //先出列
                for (int i = 0; i < num; i++)
                {
                    dataQueue1.Dequeue();
                }
            }

            for (int i = 0; i < num; i++)
            {
                //对curValue只取[0,360]之间的值
                curValue = curValue % 360;
                //对得到的正玄值，放大50倍，并上移50
                dataQueue1.Enqueue((50 * Math.Sin(curValue * Math.PI / 180)) + 50);
                curValue = curValue + 10;
            }
        }

        private void UpdateQueueValue_chart3()//更新队列值
        {
            if (dataQueue3.Count > 100)
            {
                //先出列
                for (int i = 0; i < num; i++)
                {
                    dataQueue3.Dequeue();
                }
            }

            Random r = new Random();
            for (int i = 0; i < num; i++)
            {
                dataQueue3.Enqueue(r.Next(0, 100));
            }
        }

        private void kscl_bto_Click(object sender, EventArgs e)
        {
            this.timer1.Start();
            //return_jqcl.Enabled = false;
            kscl_bto.Enabled = false;
            tzcl_bto.Enabled = true;
            bctp_bto.Enabled = true;
            bcbx_bto.Enabled = true;
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            UpdateQueueValue_chart1();//获取数据并储存
            UpdateQueueValue_chart2();
            UpdateQueueValue_chart3();

            this.chart1.Series[0].Points.Clear();//清除上一次画点
            this.chart2.Series[0].Points.Clear();
            this.chart3.Series[0].Points.Clear();

            for (int i = 0; i < dataQueue1.Count; i++)
            {
                this.chart1.Series[0].Points.AddXY((i + 1), dataQueue1.ElementAt(i));//描点
                this.chart2.Series[0].Points.AddXY((i + 1), dataQueue2.ElementAt(i));
                this.chart3.Series[0].Points.AddXY((i + 1), dataQueue3.ElementAt(i));
            }

        }

        private void tzcl_bto_Click(object sender, EventArgs e)
        {
            this.timer1.Stop();
            tzcl_bto.Enabled = false;
            kscl_bto.Enabled = true;
            //bctp_bto.Enabled = false;
            //bcbx_bto.Enabled = false;
        }

        private void Init_Chart1()
        {
            //定义图表区域
            this.chart1.ChartAreas.Clear();
            ChartArea chartArea1 = new ChartArea("C1");
            this.chart1.ChartAreas.Add(chartArea1);
            //定义存储和显示点的容器
            this.chart1.Series.Clear();
            Series series1 = new Series("  导高");
            series1.ChartArea = "C1";
            this.chart1.Series.Add(series1);
            //设置图表显示样式
            this.chart1.ChartAreas[0].AxisY.Minimum = 0;
            this.chart1.ChartAreas[0].AxisY.Maximum = 100;
            this.chart1.ChartAreas[0].AxisX.Interval = 5;
            this.chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            this.chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            //设置标题
            ////this.chart1.Titles.Clear();
            ////this.chart1.Titles.Add("S01");
            ////this.chart1.Titles[0].Text = "XXX显示";
            ////this.chart1.Titles[0].ForeColor = Color.RoyalBlue;
            ////this.chart1.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            //设置图表显示样式
            this.chart1.Series[0].Color = Color.Red;
            this.chart1.Series[0].ChartType = SeriesChartType.Spline;
            this.chart1.Series[0].Points.Clear();
        }

        private void Init_Chart2()
        {
            //定义图表区域
            this.chart2.ChartAreas.Clear();
            ChartArea chartArea2 = new ChartArea("C2");
            this.chart2.ChartAreas.Add(chartArea2);
            //定义存储和显示点的容器
            this.chart2.Series.Clear();
            Series series2 = new Series("拉出值");
            series2.ChartArea = "C2";
            this.chart2.Series.Add(series2);
            //设置图表显示样式
            this.chart2.ChartAreas[0].AxisY.Minimum = 0;
            this.chart2.ChartAreas[0].AxisY.Maximum = 100;
            this.chart2.ChartAreas[0].AxisX.Interval = 5;
            this.chart2.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            this.chart2.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            //设置标题
            ////this.chart1.Titles.Clear();
            ////this.chart1.Titles.Add("S01");
            ////this.chart1.Titles[0].Text = "XXX显示";
            ////this.chart1.Titles[0].ForeColor = Color.RoyalBlue;
            ////this.chart1.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            //设置图表显示样式
            this.chart2.Series[0].Color = Color.Blue;
            this.chart2.Series[0].ChartType = SeriesChartType.Line;
            this.chart2.Series[0].Points.Clear();
        }

        private void Init_Chart3()
        {
            //定义图表区域
            this.chart3.ChartAreas.Clear();
            ChartArea chartArea3 = new ChartArea("C3");
            this.chart3.ChartAreas.Add(chartArea3);
            //定义存储和显示点的容器
            this.chart3.Series.Clear();
            Series series3 = new Series("  高差");
            series3.ChartArea = "C3";
            this.chart3.Series.Add(series3);
            //设置图表显示样式
            this.chart3.ChartAreas[0].AxisY.Minimum = 0;
            this.chart3.ChartAreas[0].AxisY.Maximum = 100;
            this.chart3.ChartAreas[0].AxisX.Interval = 5;
            this.chart3.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            this.chart3.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            //设置标题
            ////this.chart3.Titles.Clear();
            ////this.chart3.Titles.Add("S01");
            ////this.chart3.Titles[0].Text = "XXX显示";
            ////this.chart3.Titles[0].ForeColor = Color.RoyalBlue;
            ////this.chart3.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            //设置图表显示样式
            this.chart3.Series[0].Color = Color.Green;
            this.chart3.Series[0].ChartType = SeriesChartType.Column;
            this.chart3.Series[0].Points.Clear();
        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 保存Chart波形图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Chart_pictures()
        {
            Graphics g1 = panel3.CreateGraphics();
            Image myImage = new Bitmap(this.panel3.Width, this.panel3.Height, g1);
            Graphics g2 = Graphics.FromImage(myImage);
            IntPtr dc1 = g1.GetHdc();
            IntPtr dc2 = g2.GetHdc();
            BitBlt(dc2, 0, 0, this.panel3.Width, this.panel3.Height, dc1, 0, 0, 13369376);
            g1.ReleaseHdc(dc1);
            g2.ReleaseHdc(dc2);
            // myImage.Save(@"d:\1.bmp", ImageFormat.Bmp);
            myImage.Save("d:\\波形图\\"+$"{System.DateTime.Now.ToString("yyyy - MM - dd HH：m：ss")}.jpg");
        }

        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        private static extern bool BitBlt(
                   IntPtr hdcDest, // handle to destination DC
                   int nXDest, // x-coord of destination upper-left corner
                   int nYDest, // y-coord of destination upper-left corner
                   int nWidth, // width of destination rectangle
                   int nHeight, // height of destination rectangle
                   IntPtr hdcSrc, // handle to source DC
                   int nXSrc, // x-coordinate of source upper-left corner
                   int nYSrc, // y-coordinate of source upper-left corner
                   System.Int32 dwRop // raster operation code
              );

        private void return_jqcl_Click(object sender, EventArgs e)
        {
            close_flag = 0;
            jqcl m = new jqcl();
            m.Show();
            this.Close();
            
        }
    }
}
