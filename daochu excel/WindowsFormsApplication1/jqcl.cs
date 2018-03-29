using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApplication1;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication1
{
    public partial class jqcl : Form
    {
        FilterInfoCollection videoDevices;
        System.Drawing.Point point;

        byte close_flag = 1;
        private System.Drawing.Size m_szInit;//初始窗体大小
        private Dictionary<Control, Rectangle> m_dicSize
            = new Dictionary<Control, Rectangle>();

        //定义常量          
        public const int WM_DEVICECHANGE = 0x219;
        public const int DBT_DEVICEARRIVAL = 0x8000;
        public const int DBT_CONFIGCHANGECANCELED = 0x0019;
        public const int DBT_CONFIGCHANGED = 0x0018;
        public const int DBT_CUSTOMEVENT = 0x8006;
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;
        public const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public const int DBT_DEVICEREMOVEPENDING = 0x8003;
        public const int DBT_DEVICETYPESPECIFIC = 0x8005;
        public const int DBT_DEVNODES_CHANGED = 0x0007;
        public const int DBT_QUERYCHANGECONFIG = 0x0017;
        public const int DBT_USERDEFINED = 0xFFFF;

        protected override void OnLoad(EventArgs e)
        {
            m_szInit = this.Size;//获取初始大小
            this.GetInitSize(this);
            base.OnLoad(e);
        }

        private void GetInitSize(Control ctrl)
        {
            foreach (Control c in ctrl.Controls)
            {
                m_dicSize.Add(c, new Rectangle(c.Location, c.Size));
                this.GetInitSize(c);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            //计算当前大小和初始大小的比例
            float fx = (float)this.Width / m_szInit.Width;
            float fy = (float)this.Height / m_szInit.Height;
            foreach (var v in m_dicSize)
            {
                v.Key.Left = (int)(v.Value.Left * fx);
                v.Key.Top = (int)(v.Value.Top * fy);
                v.Key.Width = (int)(v.Value.Width * fx);
                v.Key.Height = (int)(v.Value.Height * fy);
            }
            base.OnResize(e);

    }


    public jqcl()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }



        private void Jqcl_Load(object sender, EventArgs e)
        {
            qjzc_cbo.SelectedIndex = 0;
            md_cbo.SelectedIndex = 0;

            jbcl_pnel.Visible = false;
            //jxhx_pnel.Visible = false;
            dwpd_pnel.Visible = false;
            jggd_pnel.Visible = false;
            cxcl_pnel.Visible = false;
            mdgj_pnel.Visible = false;
            gc_pnel.Visible = false;
            kz_pnel.Visible = false;


            /// <summary>
            /// 查询摄像头设备，如果有多个设备，则默认第二个启动
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                foreach (FilterInfo device in videoDevices)//挨个提取videoDevices中的元素，按顺序带入以下语句
                {
                    // toolStripComboBox.Items.Add(videoDevices[i].Name.ToString());//将视频驱动依次放入ComBox
                    //choosedevices.DropDownItems.Add(videoDevices[i].Name.ToString());

                    ToolStripMenuItem video_item = new ToolStripMenuItem(device.Name.ToString());
                    video_item.Click += new EventHandler(video_item_click);

                    choosedevices.DropDownItems.Add(video_item);
                    //i++;
                
                }

                FilterInfo info;

                if (choosedevices.DropDownItems.Count >= 2)
                {
                    // choosedevices.DropDownItems{}
                    info = videoDevices[1];//启动第二个摄像头
                    VideoCaptureDevice videoSource = new VideoCaptureDevice(info.MonikerString);
                    videoSource.DesiredFrameRate = 1;
                    videoSourcePlayer_jqcl.VideoSource = videoSource;
                    videoSourcePlayer_jqcl.Start();

                    ToolStripMenuItem[] video = new ToolStripMenuItem[choosedevices.DropDownItems.Count];
                    choosedevices.DropDownItems.CopyTo(video, 0);
                    video[1].Checked = true;

                }
                else
                {
                    info = videoDevices[0];//启动第一个摄像头
                    VideoCaptureDevice videoSource = new VideoCaptureDevice(info.MonikerString);
                    videoSource.DesiredFrameRate = 1;
                    videoSourcePlayer_jqcl.VideoSource = videoSource;
                    videoSourcePlayer_jqcl.Start();

                    ToolStripMenuItem[] video = new ToolStripMenuItem[choosedevices.DropDownItems.Count];
                    choosedevices.DropDownItems.CopyTo(video, 0);
                    video[0].Checked = true;
                }
            }
            catch//没有发现摄像头
            {
                MessageBox.Show("摄像头回家吃饭去了！！");
            }

            if (!Directory.Exists("d:\\照片"))//创建照片文件夹
            {
                Directory.CreateDirectory("d:\\照片");
            }

            //查询串口
            //string[] names = SerialPort.GetPortNames();
            //port_cbo.Items.Clear();
            //CheckForIllegalCrossThreadCalls = false;
            //for (int i = 0; i < names.Length; i++)
            //{
            //    port_cbo.Items.Add(names[i]);
            //}

            //btl_cbo.SelectedIndex = 6;
            //tzw_cbo.SelectedIndex = 0;
            //jojy_cbo.SelectedIndex = 0;
            //if (names.Length > 0)
            //    port_cbo.SelectedIndex = 0;
            //else if (serialPort.IsOpen)
            //    serialPort.Close();

              try
            {
                string[] names = SerialPort.GetPortNames();//查询串口名字
                MessageBox.Show(names.Length.ToString());
                foreach (string device in names)
                {
                    ToolStripMenuItem port_item = new ToolStripMenuItem(device.ToString());
                    port_item.Click += new EventHandler(port_item_click);
                   

                    chooseport.DropDownItems.Add(port_item);
                    //i++;
                
                }

                if (names.Length > 0)
                {
                    serialPort.PortName = names[0];
                    serialPort.BaudRate = Convert.ToInt32("115200");
                    serialPort.DataBits = 8;
                    serialPort.Parity = Parity.None;
                    serialPort.StopBits = StopBits.One;

                    serialPort.Open();
                    ToolStripMenuItem[] port = new ToolStripMenuItem[names.Length];
                    chooseport.DropDownItems.CopyTo(port, 0);
                    port[0].Checked = true;

                }
                else MessageBox.Show("没有搜索到串口设备！！");


            }
            catch//没有发现摄像头
            {
                MessageBox.Show("串口回家吃饭去了！！");
            }
        }

        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == WM_DEVICECHANGE)
                {
                    switch (m.WParam.ToInt32())
                    {
                        case WM_DEVICECHANGE:   break;
                        //case DBT_DEVICEARRIVAL:
                        //    /*
                        //    DriveInfo[] s = DriveInfo.GetDrives();
                        //    foreach (DriveInfo drive in s) {
                        //        if (drive.DriveType == DriveType.Removable)
                        //        {
                        //            label3.Text = ("U盘已插入，盘符是" + drive.Name.ToString() + "\r\n");
                        //            break;
                        //        }
                        //    }*/
                   
                        //    break;
                        case DBT_CONFIGCHANGECANCELED:
                            MessageBox.Show("2");
                            break;
                        case DBT_CONFIGCHANGED:
                            MessageBox.Show("3");
                            break;
                        case DBT_CUSTOMEVENT:
                            MessageBox.Show("4");
                            break;
                        case DBT_DEVICEQUERYREMOVE:
                            MessageBox.Show("5");
                            break;
                        case DBT_DEVICEQUERYREMOVEFAILED:
                            MessageBox.Show("6");
                            break;
                        //case DBT_DEVICEREMOVECOMPLETE:
                        //    //label3.Text = ("U盘已卸载");

                        //    break;
                        case DBT_DEVICEREMOVEPENDING:
                            MessageBox.Show("7");
                            break;
                        case DBT_DEVICETYPESPECIFIC:
                            MessageBox.Show("8");
                            break;
                        case DBT_DEVNODES_CHANGED:
                            //串口热拔插
                            string[] names = SerialPort.GetPortNames();//当有串口改变的时候重新加载串口号
                            int i, flag = 0;

                            try//重新加载点击事件
                            {
                                chooseport.DropDownItems.Clear();
                                foreach (string device in names)
                                {
                                    ToolStripMenuItem port_item = new ToolStripMenuItem(device.ToString());
                                    port_item.Click += new EventHandler(port_item_click);
                                    //i++;
                                    chooseport.DropDownItems.Add(port_item);
                                }

                                ToolStripMenuItem[] port = new ToolStripMenuItem[names.Length];
                                chooseport.DropDownItems.CopyTo(port, 0);
                                for (i = 0; i < names.Length; i++)
                                {
                                    if (serialPort.PortName == port[i].ToString())
                                    {
                                        port[i].Checked = true;
                                        if (serialPort.IsOpen)//如果串口未打开则打开串口
                                        {

                                        }
                                        else serialPort.Open();
                                        flag = 1;
                                    }
                                }
                                if (flag == 0) MessageBox.Show("请选择端口！");
                            }
                            catch//没有发现串口
                            {
                                MessageBox.Show("串口回家吃饭去了！！");
                            }

                            //摄像头热拔插
                            try
                            {
                                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                                choosedevices.DropDownItems.Clear();
                                foreach (FilterInfo device in videoDevices)//挨个提取videoDevices中的元素，按顺序带入以下语句
                                {
                                    // toolStripComboBox.Items.Add(videoDevices[i].Name.ToString());//将视频驱动依次放入ComBox
                                    //choosedevices.DropDownItems.Add(videoDevices[i].Name.ToString());

                                    ToolStripMenuItem video_item = new ToolStripMenuItem(device.Name.ToString());
                                    video_item.Click += new EventHandler(video_item_click);

                                    choosedevices.DropDownItems.Add(video_item);
                                    //i++;
                                }
                            }
                            catch//没有发现摄像头
                            {
                                MessageBox.Show("请连接视频设备！！");
                            }



                            break;
                        case DBT_QUERYCHANGECONFIG:
                            MessageBox.Show("10");
                            break;
                        case DBT_USERDEFINED:
                            MessageBox.Show("11");
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            base.WndProc(ref m);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            point = videoSourcePlayer_jqcl.PointToClient(Control.MousePosition);//实时返回鼠标相对与videoSourcePlayer_jqcl的坐标

        }


        // 菜单-摄像头选择事件
        private void video_item_click(object sender, EventArgs args)
        {
            ToolStripMenuItem video_item = (ToolStripMenuItem)sender;
            videoSourcePlayer_jqcl.Stop();

            foreach (FilterInfo device in videoDevices)
            {
                if (video_item.Text == device.Name.ToString())
                {
                    VideoCaptureDevice videoSource = new VideoCaptureDevice(device.MonikerString);
                    videoSource.DesiredFrameRate = 1;
                    videoSourcePlayer_jqcl.VideoSource = videoSource;
                    videoSourcePlayer_jqcl.Start();
                    VideoCheck(sender);
                }
            }
        }




        private void port_item_click(object sender, EventArgs args)
        {
            ToolStripMenuItem port_item = (ToolStripMenuItem)sender;
            //videoSourcePlayer_jqcl.Stop();
            string[] names = SerialPort.GetPortNames();
            foreach (string port in names)
            {
                if (port_item.Text == port.ToString())
                {
                    try
                    {
                        serialPort.Close();

                        serialPort.PortName = port_item.Text;//串口参数设置
                        serialPort.BaudRate = Convert.ToInt32("115200");
                        serialPort.DataBits = 8;
                        serialPort.Parity = Parity.None;
                        serialPort.StopBits = StopBits.One;

                        serialPort.Open();
                        PortCheck(sender);
                       
                        //shezhi.show();
                        //toolstripmenuitem ts = (toolstripmenuitem)contextmenustrip1.items[7];
                        port_item.ShowDropDown();
                    }
                    catch
                    {
                        MessageBox.Show("串口分身失败，请关掉占用该串口的软件", "ERROR", MessageBoxButtons.OK,
                                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        return;
                    }
                }
            }
        }

       

        private void PortCheck(object sender)   //自定义函数   标识选择的端口号
        {
            int i;
            ToolStripMenuItem[]port = new ToolStripMenuItem[chooseport.DropDownItems.Count];
            chooseport.DropDownItems.CopyTo(port,0);
          
            for (i=0; i<port.Length; i++)
            {
                port[i].Checked = false;
            }
            
            choosedevices.Checked = false;

            ((ToolStripMenuItem)sender).Checked = true;
        }

        private void VideoCheck(object sender)   //自定义函数   标识选择的视频源
        {
            int i;
            ToolStripMenuItem[] a = new ToolStripMenuItem[choosedevices.DropDownItems.Count];
            choosedevices.DropDownItems.CopyTo(a, 0);

            for (i = 0; i < a.Length; i++)
            {
                a[i].Checked = false;
            }


            ((ToolStripMenuItem)sender).Checked = true;
        }
        private void Jqcl_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (close_flag == 1)
            {
                DialogResult result = MessageBox.Show("你确定要关闭吗！", "提示信息", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (result == DialogResult.OK)
                {
                    videoSourcePlayer_jqcl.Stop();
                    e.Cancel = false;  //点击OK 
                }
                else
                {
                    e.Cancel = true;
                }
            }

        }

        private void Jqcl_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (close_flag == 1)
            {
                Application.Exit();
            }
        }

        private void fh_bto_Click(object sender, EventArgs e)
        {
            try {
                videoSourcePlayer_jqcl.Stop();
                serialPort.Close();
            }
            catch { }
            close_flag = 0;
            this.Close();
            new Form1().Show();
        }

        private void md_cbo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void jbcl_pne_Paint(object sender, PaintEventArgs e)
        {

        }
  

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            
        }

        private void zycl_bto_Click(object sender, EventArgs e)
        {
            if (zycl_bto.Font.Bold == true)
            {
                return;
            }
            else
            {
                jbcl_pnel.Visible = false;
                //jxhx_pnel.Visible = false;
                dwpd_pnel.Visible = false;
                jggd_pnel.Visible = false;
                cxcl_pnel.Visible = false;
                mdgj_pnel.Visible = false;
                gc_pnel.Visible = false;
                kz_pnel.Visible = false;
                zycl_pnel.Visible = true;

                kz_bto.Font = new Font(kz_bto.Font, kz_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jxhx_bto.Font = new Font(jxhx_bto.Font, jxhx_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                dwpd_bto.Font = new Font(dwpd_bto.Font, dwpd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jggd_bto.Font = new Font(jggd_bto.Font, jggd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                cxcl_bto.Font = new Font(cxcl_bto.Font, cxcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                mdgj_bto.Font = new Font(mdgj_bto.Font, mdgj_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                gc_bto.Font = new Font(gc_bto.Font, gc_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jbcl_bto.Font = new Font(jbcl_bto.Font, jbcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                zycl_bto.Font = new Font(zycl_bto.Font, zycl_bto.Font.Style | System.Drawing.FontStyle.Bold);//显示粗体

            }

        }

        private void kz_bto_Click(object sender, EventArgs e)
        {

           if(kz_bto.Font.Bold == true)
            {
                return;
            }
           else
            {
                zycl_bto.Font = new Font(zycl_bto.Font, zycl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jxhx_bto.Font = new Font(jxhx_bto.Font, jxhx_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                kz_bto.Font = new Font(dwpd_bto.Font, dwpd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jggd_bto.Font = new Font(jggd_bto.Font, jggd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                cxcl_bto.Font = new Font(cxcl_bto.Font, cxcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                mdgj_bto.Font = new Font(mdgj_bto.Font, mdgj_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                gc_bto.Font = new Font(gc_bto.Font, gc_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jbcl_bto.Font = new Font(jbcl_bto.Font, jbcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                dwpd_bto.Font = new Font(kz_bto.Font, kz_bto.Font.Style | System.Drawing.FontStyle.Bold);//显示粗体

                jbcl_pnel.Visible = false;
                //jxhx_pnel.Visible = false;
                dwpd_pnel.Visible = false;
                jggd_pnel.Visible = false;
                cxcl_pnel.Visible = false;
                mdgj_pnel.Visible = false;
                gc_pnel.Visible = false;
                zycl_pnel.Visible = false;
                kz_pnel.Visible = true;

            }
     
        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        private void dwpd_bto_Click(object sender, EventArgs e)
        {
            if (dwpd_bto.Font.Bold == true)
            {
                return;
            }
            else
            {
                zycl_bto.Font = new Font(zycl_bto.Font, zycl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jxhx_bto.Font = new Font(jxhx_bto.Font, jxhx_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                kz_bto.Font = new Font(dwpd_bto.Font, dwpd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jggd_bto.Font = new Font(jggd_bto.Font, jggd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                cxcl_bto.Font = new Font(cxcl_bto.Font, cxcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                mdgj_bto.Font = new Font(mdgj_bto.Font, mdgj_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                gc_bto.Font = new Font(gc_bto.Font, gc_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jbcl_bto.Font = new Font(jbcl_bto.Font, jbcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                dwpd_bto.Font = new Font(kz_bto.Font, kz_bto.Font.Style | System.Drawing.FontStyle.Bold);//显示粗体

                jbcl_pnel.Visible = false;
               // jxhx_pnel.Visible = false;
                zycl_pnel.Visible = false;
                jggd_pnel.Visible = false;
                cxcl_pnel.Visible = false;
                mdgj_pnel.Visible = false;
                gc_pnel.Visible = false;
                kz_pnel.Visible = false;

                dwpd_pnel.Visible = true;

            }
        }

        private void jggd_bto_Click(object sender, EventArgs e)
        {
            if (jggd_bto.Font.Bold == true)
            {
                return;
            }
            else
            {
                zycl_bto.Font = new Font(zycl_bto.Font, zycl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jxhx_bto.Font = new Font(jxhx_bto.Font, jxhx_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                kz_bto.Font = new Font(dwpd_bto.Font, dwpd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                dwpd_bto.Font = new Font(jggd_bto.Font, jggd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                cxcl_bto.Font = new Font(cxcl_bto.Font, cxcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                mdgj_bto.Font = new Font(mdgj_bto.Font, mdgj_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                gc_bto.Font = new Font(gc_bto.Font, gc_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jbcl_bto.Font = new Font(jbcl_bto.Font, jbcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jggd_bto.Font = new Font(kz_bto.Font, kz_bto.Font.Style | System.Drawing.FontStyle.Bold);//显示粗体

                jbcl_pnel.Visible = false;
               // jxhx_pnel.Visible = false;
                zycl_pnel.Visible = false;
                dwpd_pnel.Visible = false;
                cxcl_pnel.Visible = false;
                mdgj_pnel.Visible = false;
                gc_pnel.Visible = false;
                kz_pnel.Visible = false;

                jggd_pnel.Visible = true;

            }
        }

        private void cxcl_bto_Click(object sender, EventArgs e)
        {
            if (cxcl_bto.Font.Bold == true)
            {
                return;
            }
            else
            {
                zycl_bto.Font = new Font(zycl_bto.Font, zycl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jxhx_bto.Font = new Font(jxhx_bto.Font, jxhx_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                kz_bto.Font = new Font(dwpd_bto.Font, dwpd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                dwpd_bto.Font = new Font(jggd_bto.Font, jggd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jggd_bto.Font = new Font(cxcl_bto.Font, cxcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                mdgj_bto.Font = new Font(mdgj_bto.Font, mdgj_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                gc_bto.Font = new Font(gc_bto.Font, gc_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jbcl_bto.Font = new Font(jbcl_bto.Font, jbcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                cxcl_bto.Font = new Font(kz_bto.Font, kz_bto.Font.Style | System.Drawing.FontStyle.Bold);//显示粗体

                jbcl_pnel.Visible = false;
                //jxhx_pnel.Visible = false;
                zycl_pnel.Visible = false;
                dwpd_pnel.Visible = false;
                jggd_pnel.Visible = false;
                mdgj_pnel.Visible = false;
                gc_pnel.Visible = false;
                kz_pnel.Visible = false;

                cxcl_pnel.Visible = true;

            }
        }

        private void mdgj_bto_Click(object sender, EventArgs e)
        {
            if (mdgj_bto.Font.Bold == true)
            {
                return;
            }
            else
            {
                zycl_bto.Font = new Font(zycl_bto.Font, zycl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jxhx_bto.Font = new Font(jxhx_bto.Font, jxhx_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                kz_bto.Font = new Font(dwpd_bto.Font, dwpd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                dwpd_bto.Font = new Font(jggd_bto.Font, jggd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jggd_bto.Font = new Font(cxcl_bto.Font, cxcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                cxcl_bto.Font = new Font(mdgj_bto.Font, mdgj_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                gc_bto.Font = new Font(gc_bto.Font, gc_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jbcl_bto.Font = new Font(jbcl_bto.Font, jbcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                mdgj_bto.Font = new Font(kz_bto.Font, kz_bto.Font.Style | System.Drawing.FontStyle.Bold);//显示粗体

                jbcl_pnel.Visible = false;
                //jxhx_pnel.Visible = false;
                zycl_pnel.Visible = false;
                dwpd_pnel.Visible = false;
                jggd_pnel.Visible = false;
                cxcl_pnel.Visible = false;
                gc_pnel.Visible = false;
                kz_pnel.Visible = false;

                mdgj_pnel.Visible = true;
            }
        }

        private void gc_bto_Click(object sender, EventArgs e)
        {
            if (gc_bto.Font.Bold == true)
            {
                return;
            }
            else
            {
                zycl_bto.Font = new Font(zycl_bto.Font, zycl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jxhx_bto.Font = new Font(jxhx_bto.Font, jxhx_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                kz_bto.Font = new Font(dwpd_bto.Font, dwpd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                dwpd_bto.Font = new Font(jggd_bto.Font, jggd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jggd_bto.Font = new Font(cxcl_bto.Font, cxcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                cxcl_bto.Font = new Font(mdgj_bto.Font, mdgj_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                mdgj_bto.Font = new Font(gc_bto.Font, gc_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jbcl_bto.Font = new Font(jbcl_bto.Font, jbcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                gc_bto.Font = new Font(kz_bto.Font, kz_bto.Font.Style | System.Drawing.FontStyle.Bold);//显示粗体

                jbcl_pnel.Visible = false;
               // jxhx_pnel.Visible = false;
                zycl_pnel.Visible = false;
                dwpd_pnel.Visible = false;
                jggd_pnel.Visible = false;
                cxcl_pnel.Visible = false;
                mdgj_pnel.Visible = false;
                kz_pnel.Visible = false;

                gc_pnel.Visible = true;
            }
        }

        private void label57_Click(object sender, EventArgs e)
        {

        }

        private void jbcl_bto_Click(object sender, EventArgs e)
        {
            if (jbcl_bto.Font.Bold == true)
            {
                return;
            }
            else
            {
                zycl_bto.Font = new Font(zycl_bto.Font, zycl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jxhx_bto.Font = new Font(jxhx_bto.Font, jxhx_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                kz_bto.Font = new Font(dwpd_bto.Font, dwpd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                dwpd_bto.Font = new Font(jggd_bto.Font, jggd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jggd_bto.Font = new Font(cxcl_bto.Font, cxcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                cxcl_bto.Font = new Font(mdgj_bto.Font, mdgj_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                mdgj_bto.Font = new Font(gc_bto.Font, gc_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                gc_bto.Font = new Font(jbcl_bto.Font, jbcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jbcl_bto.Font = new Font(kz_bto.Font, kz_bto.Font.Style | System.Drawing.FontStyle.Bold);//显示粗体

         
                gc_pnel.Visible = false;
                zycl_pnel.Visible = false;
                dwpd_pnel.Visible = false;
                jggd_pnel.Visible = false;
                cxcl_pnel.Visible = false;
                mdgj_pnel.Visible = false;
                kz_pnel.Visible = false;

                jbcl_pnel.Visible = true;
            }
        }

        private void jqcl_sjgl_bto_Click(object sender, EventArgs e)
        {
            close_flag = 0;
            try
            {
                videoSourcePlayer_jqcl.Stop();
            }
            catch { }
            jqcl_sjgl f = new jqcl_sjgl();
            f.Show();
            this.Close();
        }

        private void videoSourcePlayer_jqcl_Click(object sender, EventArgs e)
        {
            if (point.X > 0 && point.X < videoSourcePlayer_jqcl.Width / 2)
            {
                MessageBox.Show("Left");
            }
            else if (point.X > videoSourcePlayer_jqcl.Width / 2 && point.X < videoSourcePlayer_jqcl.Width)
            {
                MessageBox.Show("Right");
            }
        }

        

        private void tpzp_tbo_Click(object sender, EventArgs e)
        {
            if (videoSourcePlayer_jqcl.IsRunning)
            {
                string path = "d:\\照片\\";
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                videoSourcePlayer_jqcl.GetCurrentVideoFrame().GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
                PngBitmapEncoder pE = new PngBitmapEncoder();
                pE.Frames.Add(BitmapFrame.Create(bitmapSource));
                string picName = path + $"{System.DateTime.Now.ToString("yyyy - MM - dd HH：m：ss")}.jpg";
                if (File.Exists(picName))
                {
                    File.Delete(picName);
                }
                using (Stream stream = File.Create(picName))
                {
                    pE.Save(stream);
                }
            }
        }

        private void choosedevices_Click(object sender, EventArgs e)
        {
            
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
        }

        private void toolStripMenuItem1_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void choosedevices_DropDownOpening(object sender, EventArgs e)
        {

        }

        private void choosedevices_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            
        }

        private void jqcl_cssz_bto_Click(object sender, EventArgs e)
        {

        }

        private void return_dtcl_Click(object sender, EventArgs e)
        {
            close_flag = 0;
            try
            {
                videoSourcePlayer_jqcl.Stop();
            }
            catch { }
            dtcl m = new dtcl();
            m.Show();
            this.Close();
        }

        private void jbcl_cg_lab_Click(object sender, EventArgs e)
        {

        }

        private void jbcl_dg_lab_Click(object sender, EventArgs e)
        {

        }

        private void label55_Click(object sender, EventArgs e)
        {

        }

        private void jbcl_lcz_lab_Click(object sender, EventArgs e)
        {

        }

        private void label71_Click(object sender, EventArgs e)
        {

        }

        private void label84_Click(object sender, EventArgs e)
        {

        }

        private void jbcl_kj_lab_Click(object sender, EventArgs e)
        {

        }

        private void label82_Click(object sender, EventArgs e)
        {

        }

        private void jbcl_kjql_bto_Click(object sender, EventArgs e)
        {

        }

        private void label68_Click(object sender, EventArgs e)
        {

        }

        private void label81_Click(object sender, EventArgs e)
        {

        }

        private void label78_Click(object sender, EventArgs e)
        {

        }

        private void jbcl_gj_lab_Click(object sender, EventArgs e)
        {

        }

        private void label69_Click(object sender, EventArgs e)
        {

        }

        private void label74_Click(object sender, EventArgs e)
        {

        }

        private void jbcl_pnel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void shezhi_Click(object sender, EventArgs e)
        {

        }

        private void shezhi_MouseHover(object sender, EventArgs e)
        {
            shezhi.ShowDropDown();
        }

        private void shezhi_MouseLeave(object sender, EventArgs e)
        {
         
        }

        private void chooseport_MouseLeave(object sender, EventArgs e)
        {
            
          

            
         
        }

        private void jqcl_MouseEnter(object sender, EventArgs e)
        {
           
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }

    public class ButtonX : Button
    {


        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {

            base.OnPaint(e);
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(0, 0, this.Width, this.Height);
            this.Region = new Region(path);

        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Graphics g = this.CreateGraphics();
            g.DrawEllipse(new Pen(Color.Blue), 0, 0, this.Width, this.Height);
            g.Dispose();
        }

    }

}
