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
using SpeechLib;
using System.Drawing.Drawing2D;
using System.Threading;

namespace WindowsFormsApplication1
{

    public partial class jqcl : Form
    {
        FilterInfoCollection videoDevices;
        System.Drawing.Point point;

        string databaseDetails;
        private byte close_flag = 1, overtime_flag = 1, change_flag = 1, right_flag = 1, left_flag = 1, jbVerify_celiang = 0, cgVerify_celiang = 0, save_flag = 0, start_flag=0;//right_flag,left_flag:电机正反转标志；
                                                                                                                                                                  //close_flag:窗体关闭弹出提示窗口标志，1为弹出，0为不弹；
                                                                                                                                                                  //overtime_flag,接收消息超时标志位，overtime_flag=0表示接收成功;
                                                                                                                                                                  //change_flag=1,不触发串口波特率改变事件;
                                                                                                                                                                  //jbVerify_celiang:基本校验测量标志;
                                                                                                                                                                  // cgVerify_celinag:超高校验测量标志
                                                                                                                                                                  //save_flag:保存标志位
                                                                                                                                                                  //start_flag:程序开始标志位
        private System.Drawing.Size m_szInit;//初始窗体大小
        private Dictionary<Control, Rectangle> m_dicSize
            = new Dictionary<Control, Rectangle>();
        byte steer;

        //十字光标参数    
        int move = 0;               //十字光标  0:不移动；1：向左移动；2：向右移动；3：向上移动；4：向下移动
        int pic_lct_x,              //初始载入时的picture.location.x
            pic_lct_y;              //初始载入时的picture.location.y

        int Cross_width,            //实际观察的十字光标的宽度
            Cross_height;           //实际观察的十字光标的高度
                                    //video参数
        int Video_width,            //Videoplayer的宽度
            Video_height;           //Videoplayer的高度
        //精测指令
        private byte[] command1 = { 0XEE, 0XEE, 0X01, 0XFC, 0XFC };//绝对编码器角度置零指令
        private byte[] command2 = { 0XEE, 0XEE, 0X02, 0X00, 0X05, 0XFC, 0XFC };//电机正转5度
        private byte[] command3 = { 0XEE, 0XEE, 0X02, 0X01, 0X05, 0XFC, 0XFC };//电机反转5度
        private byte[] command4 = { 0XEE, 0XEE, 0X03, 0X00, 0XFC, 0XFC };//电机正转0.1度
        private byte[] command5 = { 0XEE, 0XEE, 0X03, 0X01, 0XFC, 0XFC };//电机反转0.1度
        private byte[] command6 = { 0XEE, 0XEE, 0X04, 0XFC, 0XFC };//获取拉出值、导高、轨距、超高、侧面限界5个数据
        private byte[] command7 = { 0XEE, 0XEE, 0X05, 0XFC, 0XFC };//获取增量编码器距离值、绝对值编码器角度值、激光测距距离值、导轨距离值、倾角角度值
        private byte[] command8 = { 0XEE, 0XEE, 0X06, 0XFC, 0XFC };//增量编码器距离数据置零指令
        private byte[] command9 = { 0XEE, 0XEE, 0X0A, 0x00, 0XFC, 0XFC };//关激光测距
        private byte[] command10 = { 0XEE, 0XEE, 0X0A, 0x01, 0XFC, 0XFC };//开激光测距
        private byte[] command11 = { 0XEE, 0XEE, 0X0B, 0XFC, 0XFC };//读取固定参数指令
        private byte[] command12 = { 0XEE, 0XEE, 0X0C, 0X01, 0XFC, 0XFC };//读取实时值
        private byte[] command13 = { 0XEE, 0XEE, 0X0D, 0XFC, 0XFC };//超高清零
        private byte[] command14 = { 0XEE, 0XEE, 0X0C, 0X00, 0XFC, 0XFC };//关闭读取实时值

        private byte[] command15 = { 0XEE, 0XEE, 0X10, 0XFC, 0XFC };//反馈给精测板激光开关
        private byte[] command16 = { 0XEE, 0XEE, 0X11, 0XFC, 0XFC };//反馈给精测板收到测量数据


        //测量数据
        AllParameter parameter, parameter1, parameter2;

        //协议功能码
        byte founctioncode = 0;
        //超时时间
        int overtime = 5000;
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

        //数据库类
        shujuku database_operate;



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
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }


        private void Cross_Shape()                      //画十字线
        {
            GraphicsPath Cross = new GraphicsPath();

            System.Drawing.Point[] array =
            {
                new System.Drawing.Point(((Video_width / 150) * 85) - 1, (Video_height / 2) - (Cross_height - 1) / 2),
                new System.Drawing.Point(((Video_width / 150) * 85) + 1, (Video_height / 2) - (Cross_height - 1) / 2),
                new System.Drawing.Point(((Video_width / 150) * 85) + 1, Video_height / 2 + 1),
                new System.Drawing.Point(((Video_width / 150) * 88) - 1, Video_height / 2 + 1),
                new System.Drawing.Point(((Video_width / 150) * 88) - 1, (Video_height / 2) - (Cross_height - 1) / 2),
                new System.Drawing.Point(((Video_width / 150) * 88) + 1, (Video_height / 2) - (Cross_height - 1) / 2),
                new System.Drawing.Point(((Video_width / 150) * 88) + 1, (Video_height / 2) - 1),
                new System.Drawing.Point(Video_width / 2 + (Cross_width - 1) / 2, (Video_height / 2) - 1),
                new System.Drawing.Point(Video_width / 2 + (Cross_width - 1) / 2, (Video_height / 2) + 1),
                new System.Drawing.Point((Video_width / 2) + 1, (Video_height / 2) + 1),
                new System.Drawing.Point((Video_width / 2) + 1, Video_height / 2 + (Cross_height - 1) / 2),
                new System.Drawing.Point((Video_width / 2) - 1, Video_height / 2 + (Cross_height - 1) / 2),
                new System.Drawing.Point((Video_width / 2) - 1, (Video_height / 2) + 1),
                new System.Drawing.Point(Video_width / 2 - (Cross_width - 1) / 2, Video_height / 2 + 1),
                new System.Drawing.Point(Video_width / 2 - (Cross_width - 1) / 2, (Video_height / 2) - 1),
                new System.Drawing.Point(((Video_width / 150) * 85) - 1, (Video_height / 2) - 1)
        };
            Cross.AddLines(array);

            pictureBox1.Region = new Region(Cross);
        }
        private void Jqcl_Load(object sender, EventArgs e)
        {
            int i = 0;
            pic_lct_x = pictureBox1.Location.X;
            pic_lct_y = pictureBox1.Location.Y;

            Video_width = videoSourcePlayer_jqcl.Width;
            Video_height = videoSourcePlayer_jqcl.Height;

            Cross_width = 61;               //修改十字光标实际观察宽度
            Cross_height = 61;              //修改十字光标实际观察高度

            // qjzc_cbo.SelectedIndex = 0;
            //  md_cbo.SelectedIndex = 0;

            parameter1 = new AllParameter();
            parameter2 = new AllParameter();
            parameter = new AllParameter();
            database_operate = new shujuku();

            zycl_pnel.Visible = false;
            //jxhx_pnel.Visible = false;
            dwpd_pnel.Visible = false;
            jggd_pnel.Visible = false;
            cxcl_pnel.Visible = false;
            mdgj_pnel.Visible = false;
            gc_pnel.Visible = false;
            kz_pnel.Visible = false;
            jqcl_cssz_pnl.Visible = false;

            Cross_Shape();


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
                                                           //MessageBox.Show(names.Length.ToString());
                string parameters = parameter.fileToString("serialportname(重要资料，请勿删改！！！).txt");

                if (names.Length > 0)
                {

                    foreach (string device in names)
                    {
                       // MessageBox.Show(device);
                        ToolStripMenuItem port_item = new ToolStripMenuItem(device.ToString());
                        port_item.Click += new EventHandler(port_item_click);


                        chooseport.DropDownItems.Add(port_item);
                    }

                    if (parameters != "")//如果已保存串口号
                    {
                        foreach (string device in names)
                        {
                            if (device == parameters)
                            {
                                change_flag = 0;
                                serialPort.PortName = device;
                                //serialPort.BaudRate = Convert.ToInt32(boundrate_cbo.Text);
                                serialPort.BaudRate = 115200;
                                serialPort.DataBits = 8;
                                serialPort.Parity = Parity.None;
                                serialPort.StopBits = StopBits.One;

                                serialPort.Open();

                                founctioncode = 0x0A;
                                serialPort.Write(command9, 0, command9.Length);//关激光测距
                                Thread.Sleep(1000);
                                //start_flag = 1;
                            }
                        }
                    }

                    for (i = 0; i < names.Count(); i++)
                    {
                        if (start_flag == 0)
                        { //boundrate_cbo.SelectedIndex = 4;
                            serialPort.Close();

                            change_flag = 0;
                            serialPort.PortName = names[i];
                            //serialPort.BaudRate = Convert.ToInt32(boundrate_cbo.Text);
                            serialPort.BaudRate = 115200;
                            serialPort.DataBits = 8;
                            serialPort.Parity = Parity.None;
                            serialPort.StopBits = StopBits.One;

                            serialPort.Open();

                            founctioncode = 0x0A;
                            serialPort.Write(command9, 0, command9.Length);//关激光测距
                            Thread.Sleep(1000);
                        }
                    }

                    if (start_flag == 0)
                    {
                        Application.Exit();
                    }


                    if (parameters == "" || parameters != serialPort.PortName)//如果串口号未保存或已更改
                    {
                        parameter.SaveProcess("serialportName(重要资料，请勿删改！！！).txt", serialPort.PortName);
                    }

                    ToolStripMenuItem[] port = new ToolStripMenuItem[names.Length];
                    chooseport.DropDownItems.CopyTo(port, 0);
                    port[i-1].Checked = true;



                    if (serialPort.IsOpen)
                    {
                        founctioncode = 5;
                        serialPort.Write(command7, 0, command7.Length);

                        while (founctioncode != 0)
                        {

                        }
                        jqcl_jd_lab.Text = parameter.jueduizhi.ToString() + "°";
                        //founctioncode = 0x0C;
                        serialPort.Write(command12, 0, command12.Length);
                    }

            }
            else
            {
                MessageBox.Show("没有搜索到串口设备！！");
                    //boundrate_cbo.SelectedIndex = 4;
                    Application.Exit();
                }


        }
            catch//没有发现串口
            {
                MessageBox.Show("打开串口失败！！");
                Application.Exit();
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
                        case WM_DEVICECHANGE: break;
                        case DBT_DEVICEARRIVAL:
                            /*
                            DriveInfo[] s = DriveInfo.GetDrives();
                            foreach (DriveInfo drive in s) {
                                if (drive.DriveType == DriveType.Removable)
                                {
                                    label3.Text = ("U盘已插入，盘符是" + drive.Name.ToString() + "\r\n");
                                    break;
                                }
                            }*/
                            break;
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
                        case DBT_DEVICEREMOVECOMPLETE:
                            //label3.Text = ("U盘已卸载");
                            break;
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

                            // MessageBox.Show(names.Length.ToString());
                            try
                            {
                                if (names.Length > 0)
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
                                            else
                                            {
                                                serialPort.Open();

                                            }
                                            flag = 1;


                                        }
                                    }
                                    if (flag == 0)
                                    {
                                        MessageBox.Show("请选择端口！");
                                    }
                                }
                                else
                                {
                                    serialPort.Close();
                                    chooseport.DropDownItems.Clear();
                                }
                            }
                            catch//没有发现串口
                            {
                                serialPort.Close();
                                //chooseport.DropDownItems.Clear();
                                MessageBox.Show("串口打开失败，有可能是因为拔出正在使用的串口！！");
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

                        //serialPort.BaudRate = Convert.ToInt32(boundrate_cbo.Text);
                        serialPort.BaudRate = 115200;
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
            ToolStripMenuItem[] port = new ToolStripMenuItem[chooseport.DropDownItems.Count];
            chooseport.DropDownItems.CopyTo(port, 0);

            for (i = 0; i < port.Length; i++)
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
                if (start_flag == 0)
                {
                    DialogResult result = MessageBox.Show("未发现可用串口，程序即将关闭！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    DialogResult result = MessageBox.Show("你确定要关闭吗！", "提示信息", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (result == DialogResult.OK)
                    {
                        serialPort.Close();
                        videoSourcePlayer_jqcl.Stop();
                        e.Cancel = false;  //点击OK 
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
            }

        }

        private void Jqcl_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (close_flag == 1)
            {
                //Application.Exit();
                Environment.Exit(0);
               
            }
            if (serialPort.IsOpen)
            {
                    founctioncode = 0x0C;
                    ack_timer3.Enabled = true;
                    ack_timer3.Interval = overtime;
                    serialPort.Write(command14, 0, command14.Length);
           
                serialPort.Close();
            }
        }

        private void fh_bto_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)//关闭实时测量
            {
                    //founctioncode = 0x0C;
                    //ack_timer3.Enabled = true;
                    //ack_timer3.Interval = overtime;
                serialPort.Write(command14, 0, command14.Length);
          
                serialPort.Close();

                try
                {

                    serialPort.Close();
                    videoSourcePlayer_jqcl.Stop();
                }
                catch { }
                close_flag = 0;
                this.Close();
                new Form1().Show();
            }
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
            if (serialPort.IsOpen)
            {
                if (founctioncode == 0)
                {
                    founctioncode = 3;
                    ack_timer3.Enabled = true;
                    ack_timer3.Interval = overtime;
                    serialPort.Write(command4, 0, command4.Length);//电机正转0.1度
                    right_flag = 0;
                }

            }
        }

        private void zycl_bto_Click(object sender, EventArgs e)
        {
            /*两次测量标志位复位*/
            parameter1.celiang_flag = 0;
            parameter2.celiang_flag = 0;
            parameter1.clEnd_flag = 0;
            parameter2.clEnd_flag = 0;

            clts_lab.Text = "请开始第一次测量！";

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
                //jqcl_cl_bto.Visible = false;

                //jqcl_cl_1.Visible = true;
                //jqcl_cl_2.Visible = true;
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
            /*两次测量标志位复位*/
            parameter1.celiang_flag = 0;
            parameter2.celiang_flag = 0;
            parameter1.clEnd_flag = 0;
            parameter2.clEnd_flag = 0;

            clts_lab.Text = "";

            if (kz_bto.Font.Bold == true)
            {
                return;
            }
            else
            {
                zycl_bto.Font = new Font(zycl_bto.Font, zycl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jxhx_bto.Font = new Font(jxhx_bto.Font, jxhx_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                dwpd_bto.Font = new Font(dwpd_bto.Font, dwpd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jggd_bto.Font = new Font(jggd_bto.Font, jggd_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                cxcl_bto.Font = new Font(cxcl_bto.Font, cxcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                mdgj_bto.Font = new Font(mdgj_bto.Font, mdgj_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                gc_bto.Font = new Font(gc_bto.Font, gc_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                jbcl_bto.Font = new Font(jbcl_bto.Font, jbcl_bto.Font.Style & ~System.Drawing.FontStyle.Bold);//取消粗体
                kz_bto.Font = new Font(kz_bto.Font, kz_bto.Font.Style | System.Drawing.FontStyle.Bold);//显示粗体

                jbcl_pnel.Visible = false;
                //jxhx_pnel.Visible = false;
                dwpd_pnel.Visible = false;
                jggd_pnel.Visible = false;
                cxcl_pnel.Visible = false;
                mdgj_pnel.Visible = false;
                gc_pnel.Visible = false;
                zycl_pnel.Visible = false;
                jqcl_cl_1.Visible = false;
                jqcl_cl_2.Visible = false;

                jqcl_cl_bto.Visible = true;
                kz_pnel.Visible = true;

            }

        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        private void dwpd_bto_Click(object sender, EventArgs e)
        {
            /*两次测量标志位复位*/
            parameter1.celiang_flag = 0;
            parameter2.celiang_flag = 0;
            parameter1.clEnd_flag = 0;
            parameter2.clEnd_flag = 0;

            clts_lab.Text = "请开始第一次测量！";

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
                //jqcl_cl_bto.Visible = false;

                dwpd_pnel.Visible = true;
                //jqcl_cl_1.Visible = true;
                //jqcl_cl_2.Visible = true;

            }
        }

        private void jggd_bto_Click(object sender, EventArgs e)
        {
            /*两次测量标志位复位*/
            parameter1.celiang_flag = 0;
            parameter2.celiang_flag = 0;
            parameter1.clEnd_flag = 0;
            parameter2.clEnd_flag = 0;

            clts_lab.Text = "请开始第一次测量！";

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
                //jqcl_cl_bto.Visible = false;

                //jqcl_cl_1.Visible = true;
                //jqcl_cl_2.Visible = true;
                jggd_pnel.Visible = true;

            }
        }

        private void cxcl_bto_Click(object sender, EventArgs e)
        {
            /*两次测量标志位复位*/
            parameter1.celiang_flag = 0;
            parameter2.celiang_flag = 0;
            parameter1.clEnd_flag = 0;
            parameter2.clEnd_flag = 0;

            clts_lab.Text = "";

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
                jqcl_cl_1.Visible = false;
                jqcl_cl_2.Visible = false;

                jqcl_cl_bto.Visible = true;
                cxcl_pnel.Visible = true;

            }
        }

        private void mdgj_bto_Click(object sender, EventArgs e)
        {
            /*两次测量标志位复位*/
            parameter1.celiang_flag = 0;
            parameter2.celiang_flag = 0;
            parameter1.clEnd_flag = 0;
            parameter2.clEnd_flag = 0;

            clts_lab.Text = "请开始第一次测量！";

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
                //jqcl_cl_bto.Visible = false;

                //jqcl_cl_1.Visible = true;
                //jqcl_cl_2.Visible = true;
                mdgj_pnel.Visible = true;
            }
        }

        private void gc_bto_Click(object sender, EventArgs e)
        {
            /*两次测量标志位复位*/
            parameter1.celiang_flag = 0;
            parameter2.celiang_flag = 0;
            parameter1.clEnd_flag = 0;
            parameter2.clEnd_flag = 0;

            clts_lab.Text = "请开始第一次测量！";

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
                //jqcl_cl_bto.Visible = false;

                //jqcl_cl_1.Visible = true;
                //jqcl_cl_2.Visible = true;
                gc_pnel.Visible = true;
            }
        }

        private void label57_Click(object sender, EventArgs e)
        {

        }

        private void jbcl_bto_Click(object sender, EventArgs e)
        {
            /*两次测量标志位复位*/
            parameter1.celiang_flag = 0;
            parameter2.celiang_flag = 0;
            parameter1.clEnd_flag = 0;
            parameter2.clEnd_flag = 0;

            clts_lab.Text = "";

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
                jqcl_cl_1.Visible = false;
                jqcl_cl_2.Visible = false;

                jqcl_cl_bto.Visible = true;
                jbcl_pnel.Visible = true;
            }
        }

        private void videoSourcePlayer_jqcl_Click(object sender, EventArgs e)
        {

            if (point.X > 0 && point.X < videoSourcePlayer_jqcl.Width / 2)
            {
                if (serialPort.IsOpen)
                {
                    if (founctioncode == 0)
                    {
                        founctioncode = 2;
                        ack_timer3.Enabled = true;
                        ack_timer3.Interval = overtime;
                        steer = 4;
                        serialPort.Write(command3, 0, command3.Length);//电机反转5度
                        left_flag = 0;
                    }
                }


            }
            else if (point.X > videoSourcePlayer_jqcl.Width / 2 && point.X < videoSourcePlayer_jqcl.Width)
            {
                if (serialPort.IsOpen)
                {
                    if (founctioncode == 0)
                    {
                        founctioncode = 2;
                        ack_timer3.Enabled = true;
                        ack_timer3.Interval = overtime;
                        steer = 4;
                        serialPort.Write(command2, 0, command2.Length);//电机正转5度
                        right_flag = 0;
                    }
                }
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

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
        }

        private void choosedevices_DropDownOpening(object sender, EventArgs e)
        {

        }

        private void choosedevices_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void jbcl_cg_lab_Click(object sender, EventArgs e)
        {

        }

        private void jbcl_dg_lab_Click(object sender, EventArgs e)
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

        private void jbcl_kjql_bto_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                if (founctioncode == 0)
                {
                    founctioncode = 6;
                    ack_timer3.Enabled = true;
                    ack_timer3.Interval = overtime;
                    serialPort.Write(command8, 0, command8.Length);
                }
            }
        }

        private void label68_Click(object sender, EventArgs e)
        {

        }

        private void label81_Click(object sender, EventArgs e)
        {

        }

        private void jbcl_gj_lab_Click(object sender, EventArgs e)
        {

        }

        private void label69_Click(object sender, EventArgs e)
        {

        }

        private void jbcl_pnel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void shezhi_Click(object sender, EventArgs e)
        {

        }

        private void gc_jj_lab_Click(object sender, EventArgs e)
        {
            SpeechVoiceSpeakFlags flag = SpeechVoiceSpeakFlags.SVSFlagsAsync;
            SpVoice voice = new SpVoice();
            voice.Voice = voice.GetVoices(string.Empty, string.Empty).Item(0);
            string speak = gc_jj_lab.Text + "毫米";
            voice.Speak(speak, flag);
        }

        private void jxhx_bto_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            //if (serialPort.IsOpen)//开启实时测量
            //{
            //    if (founctioncode == 0)
            //    {
            //         //founctioncode = 0x0C;
            //        //ack_timer3.Enabled = true;
            //        //ack_timer3.Interval = overtime;
            //        serialPort.Write(command12, 0, command12.Length);
            //    }
            //}
            jqcl_cssz_pnl.Visible = false;
        }

        private void button2_MouseDown(object sender, MouseEventArgs e)
        {
            timer2.Enabled = true;
            timer2.Interval = 10;
            move = 1;
        }

        private void button2_MouseUp(object sender, MouseEventArgs e)
        {
            timer2.Enabled = false;
        }

        private void button6_MouseDown(object sender, MouseEventArgs e)
        {
            timer2.Enabled = true;
            timer2.Interval = 10;
            move = 3;
        }

        private void button6_MouseUp(object sender, MouseEventArgs e)
        {
            timer2.Enabled = false;
        }

        private void button7_MouseDown(object sender, MouseEventArgs e)
        {
            timer2.Enabled = true;
            timer2.Interval = 10;
            move = 4;
        }

        private void button7_MouseUp(object sender, MouseEventArgs e)
        {
            timer2.Enabled = false;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            byte[] bytes = new byte[29];
            byte[] bytess = new byte[4];
            float x1, x2, y1, y2, c1, d;
            bytes[0] = 0XEE;
            bytes[1] = 0XEE;
            bytes[2] = 0X07;

            try
            {
                /*Y2*/
                y2 = Convert.ToSingle(jqcl_y2_txt.Text);
                x2 = Convert.ToSingle(jqcl_x2_txt.Text);
                y1 = Convert.ToSingle(jqcl_y1_txt.Text);
                x1 = Convert.ToSingle(jqcl_x1_txt.Text);
                c1 = Convert.ToSingle(jqcl_gjsz_txt.Text);
                d = Convert.ToSingle(jqcl_txlzj_txt.Text);

                bytess = BitConverter.GetBytes(y2);
                for (int i = 0; i < 4; i++)
                {
                    bytes[3 + i] = bytess[i];
                }

                /*X2*/
                bytess = BitConverter.GetBytes(x2);
                for (int i = 0; i < 4; i++)
                {
                    bytes[7 + i] = bytess[i];
                }

                /*X1*/
                bytess = BitConverter.GetBytes(x1);
                for (int i = 0; i < 4; i++)
                {
                    bytes[11 + i] = bytess[i];
                }

                /*Y1*/
                bytess = BitConverter.GetBytes(y1);
                for (int i = 0; i < 4; i++)
                {
                    bytes[15 + i] = bytess[i];
                }

                /*C1*/
                bytess = BitConverter.GetBytes(c1);
                for (int i = 0; i < 4; i++)
                {
                    bytes[19 + i] = bytess[i];
                }
                /*d*/
                bytess = BitConverter.GetBytes(d);
                for (int i = 0; i < 4; i++)
                {
                    bytes[23 + i] = bytess[i];
                }

                bytes[27] = 0xfc;
                bytes[28] = 0xfc;
                if (serialPort.IsOpen)
                {
                    // string parameter;
                    if (founctioncode == 0)
                    {
                        serialPort.Write(bytes, 0, bytes.Length);
                        founctioncode = 7;
                        ack_timer3.Enabled = true;
                        ack_timer3.Interval = overtime;

                        //parameter = jqcl_x1_txt.Text + " " + jqcl_x2_txt.Text + " " + jqcl_y1_txt.Text + " " + jqcl_y2_txt.Text + " " + jqcl_gjsz_txt.Text + " " + jqcl_txlzj_txt.Text;
                        //parameter1.SaveProcess("32parameter(重要资料，请勿删改！！！).txt", parameter);
                    }
                }
                else
                {
                    MessageBox.Show("保存失败！");
                }
            }
            catch
            {
                MessageBox.Show("输入有误！");
                return;
            }






            //   serialPort.Write(bytes, 0, bytes.Length);

        }

        private void jqcl_cl_bto_Click(object sender, EventArgs e)
        {
            if (dwpd_pnel.Visible == true || jggd_pnel.Visible == true || mdgj_pnel.Visible == true || gc_pnel.Visible == true || zycl_pnel.Visible == true)
            {
                if (parameter1.celiang_flag != 1 && parameter2.celiang_flag != 1)
                {
                    if (serialPort.IsOpen)
                    {
                        if (founctioncode == 0)
                        {
                            ack_timer3.Enabled = true;
                            ack_timer3.Interval = overtime;
                            founctioncode = 4;
                            serialPort.Write(command6, 0, command6.Length);
                            parameter1.celiang_flag = 1;
                        }
                    }
                }
                else if (parameter1.celiang_flag == 1 && parameter2.celiang_flag != 1)
                {
                    if (serialPort.IsOpen)
                    {
                        if (founctioncode == 0)
                        {
                            ack_timer3.Enabled = true;
                            ack_timer3.Interval = overtime;
                            founctioncode = 4;
                            serialPort.Write(command6, 0, command6.Length);
                            parameter1.celiang_flag = 0;
                            parameter2.celiang_flag = 1;
                        }
                    }
                }
            }
            else if (serialPort.IsOpen)
            {
                if (founctioncode == 0)
                {
                    founctioncode = 4;
                    ack_timer3.Enabled = true;
                    ack_timer3.Interval = overtime;
                    serialPort.Write(command6, 0, command6.Length);
                    parameter.celiang_flag = 1;
                }
            }

            //if (save_flag == 1)
            //{
            //    database_operate.Add("'2','3','4','5','6','7','8','9','10','11','12','13','14','15','16','17','18',''");
            //}
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void r_bto_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void r_bto_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void l_bto_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void l_bto_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            //switch (steer)
            //{
            //    case 0:

            //        break;
            //    case 1:

            //        break;
            //    case 2:

            //        break;
            //    case 3:

            //        break;
            //    case 4:
            ack_timer3.Enabled = false;
            MessageBox.Show("消息接收超时！");
            //button13.Enabled = true;
            //jqcl_cl_2.Enabled = true;
            //jqcl_cl_1.Enabled = true;
            //jqcl_cl_bto.Enabled = true;
            //jbcl_kjql_bto.Enabled = true;
            //l_bto.Enabled = true;
            //r_bto.Enabled = true;
            //button10.Enabled = true;
            //jg_bto.Enabled = true;
            founctioncode = 0;
            // break;
            // }
        }





        private void videoSourcePlayer_jqcl_MouseUp_1(object sender, MouseEventArgs e)
        {

        }

        private void videoSourcePlayer_jqcl_MouseDown_1(object sender, MouseEventArgs e)
        {
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                if (founctioncode == 0)
                {
                    //button14.Enabled = false;
                    founctioncode = 1;
                    ack_timer3.Enabled = true;
                    ack_timer3.Interval = overtime;
                    //steer = 4;
                    serialPort.Write(command1, 0, command1.Length);
                }
            }
        }

        private void label97_Click(object sender, EventArgs e)
        {

        }

        private void jqcl_cl_1_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                if (founctioncode == 0)
                {
                    ack_timer3.Enabled = true;
                    ack_timer3.Interval = overtime;
                    founctioncode = 4;
                    serialPort.Write(command6, 0, command6.Length);
                    parameter1.celiang_flag = 1;
                }
            }
        }

        private void jqcl_cl_2_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                if (founctioncode == 0)
                {
                    ack_timer3.Enabled = true;
                    ack_timer3.Interval = overtime;
                    founctioncode = 4;
                    serialPort.Write(command6, 0, command6.Length);
                    parameter2.celiang_flag = 1;
                }
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                if (founctioncode == 0)
                {
                    //button14.Enabled = false;
                    founctioncode = 0x0D;
                    ack_timer3.Enabled = true;
                    ack_timer3.Interval = overtime;
                    //steer = 4;
                    serialPort.Write(command13, 0, command13.Length);
                }
            }
        }

        private void l_bto_Click(object sender, EventArgs e)
        {

            if (serialPort.IsOpen)
            {
                if (founctioncode == 0)
                {
                    founctioncode = 3;
                    ack_timer3.Enabled = true;
                    ack_timer3.Interval = overtime;
                    serialPort.Write(command5, 0, command5.Length);//电机反转0.1度
                    left_flag = 0;
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            parameter1.customAngel_flag = 1;

            //如果为正转
            if (jqcl_xzjd_txt.Text != "")
            {
                if (jqcl_djzz_rbto.Checked == true)
                {
                    if (serialPort.IsOpen)
                    {
                        if (founctioncode == 0)
                        {
                            try
                            {
                                if (Convert.ToInt16(jqcl_xzjd_txt.Text) > 90 || Convert.ToInt16(jqcl_xzjd_txt.Text) < 0)
                                {
                                    MessageBox.Show("电机旋转角度范围为：0°到 90°！");
                                    jqcl_xzjd_txt.Text = "";
                                    return;
                                }
                                else
                                {
                                    string hexOutput = Convert.ToByte(jqcl_xzjd_txt.Text).ToString("X2");
                                    command2[4] = Convert.ToByte(hexOutput, 16);

                                    //MessageBox.Show(command2[4].ToString());
                                    founctioncode = 2;
                                    ack_timer3.Enabled = true;
                                    ack_timer3.Interval = overtime;
                                    serialPort.Write(command2, 0, command2.Length);
                                    command2[4] = 0x05;
                                }
                            }
                            catch
                            {
                                MessageBox.Show("旋转角度输入有误！");
                                jqcl_xzjd_txt.Text = "";
                                return;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("请先打开串口！");
                        return;
                    }
                }

                //如果为反转
                else if (jqcl_djfz_rbto.Checked == true)
                {
                    if (serialPort.IsOpen)
                    {
                        if (founctioncode == 0)
                        {
                            try
                            {
                                if (Convert.ToInt16(jqcl_xzjd_txt.Text) > 90 || Convert.ToInt16(jqcl_xzjd_txt.Text) < 0)
                                {
                                    MessageBox.Show("电机旋转角度范围为：0°到 90°！");
                                    jqcl_xzjd_txt.Text = "";
                                    return;
                                }
                                else
                                {
                                    string hexOutput = Convert.ToByte(jqcl_xzjd_txt.Text).ToString("X2");
                                    command3[4] = Convert.ToByte(hexOutput, 16);

                                    founctioncode = 2;
                                    ack_timer3.Enabled = true;
                                    ack_timer3.Interval = overtime;
                                    serialPort.Write(command3, 0, command3.Length);
                                    command3[4] = 0x05;
                                }
                            }
                            catch
                            {
                                MessageBox.Show("旋转角度输入有误！");
                                jqcl_xzjd_txt.Text = "";
                                return;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("请先打开串口！");
                        return;
                    }
                }
            }
        }

        private void button13_Click_1(object sender, EventArgs e)
        {
            //if (serialPort.IsOpen)
            //{
            //    if (founctioncode == 0)
            //    {
            //        button13.Enabled = false;
            //        founctioncode = 1;
            //        steer_timer3.Enabled = true;
            //        steer_timer3.Interval = overtime;
            //        steer = 4;
            //        serialPort.Write(command1, 0, command1.Length);
            //    }
            //}
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {

        }

        private void jqcl_cssz_pnl_Paint(object sender, PaintEventArgs e)
        {

        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                if (founctioncode == 0)
                {
                    if (jg_bto.BackColor == Color.Red)
                    {
                        founctioncode = 10;
                        ack_timer3.Enabled = true;
                        ack_timer3.Interval = overtime;
                        serialPort.Write(command9, 0, command9.Length);//关激光
                    }
                    else
                    {
                        founctioncode = 10;
                        ack_timer3.Enabled = true;
                        ack_timer3.Interval = overtime;
                        serialPort.Write(command10, 0, command10.Length);//开激光
                    }
                }
            }
        }

        private void jggd_dg2_lab_Click(object sender, EventArgs e)
        {

        }

        private void jggd_gc_lab_Click(object sender, EventArgs e)
        {

        }

        private void jggd_cg_lab_Click(object sender, EventArgs e)
        {

        }

        private void jggd_gj_lab_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label103_Click(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void label80_Click(object sender, EventArgs e)
        {

        }

        private void jggd_dg1_lab_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void jqcl_sjgl_bto_Click(object sender, EventArgs e)
        {
            close_flag = 0;

            if (serialPort.IsOpen)//关闭实时测量
            {
                if (founctioncode == 0)
                {
                    founctioncode = 0x0C;
                    ack_timer3.Enabled = true;
                    ack_timer3.Interval = overtime;
                    serialPort.Write(command14, 0, command14.Length);
                }

                try
                {

                    serialPort.Close();
                    videoSourcePlayer_jqcl.Stop();
                }
                catch { }
                jqcl_sjgl f = new jqcl_sjgl();
                f.StartPosition = FormStartPosition.CenterScreen;
                f.Show();
                this.Close();
            }
        }

        private void jqcl_cssz_bto_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)//关闭实时测量
            {
              
                if (founctioncode == 0)
                {
                    

                    //serialPort.Write(command14, 0, command14.Length);

                    //Thread.Sleep(200);


                  

                    founctioncode = 0x0B;
                    ack_timer3.Enabled = true;
                    ack_timer3.Interval = overtime;
                    //steer = 4;
                    serialPort.Write(command11, 0, command11.Length);

                    jqcl_cssz_pnl.Visible = true;
                    clts_lab.Text = "";
                    /*两次测量标志位复位*/
                    parameter1.celiang_flag = 0;
                    parameter2.celiang_flag = 0;
                    parameter1.clEnd_flag = 0;
                    parameter2.clEnd_flag = 0;


                    cssz_jd_lab.Text = jqcl_jd_lab.Text;
                }
            }
          
            //string [] parameter =  parameter1.fileToString("32parameter(重要资料，请勿删改！！！).txt").Split(' ');

            //if (parameter.Length == 6)
            //{
            //    jqcl_x1_txt.Text = parameter[0];
            //    jqcl_x2_txt.Text = parameter[1];
            //    jqcl_y1_txt.Text = parameter[2];
            //    jqcl_y2_txt.Text = parameter[3];
            //    jqcl_gjsz_txt.Text = parameter[4];
            //    jqcl_txlzj_txt.Text = parameter[5];
            //}
            //else
            //{
            //    MessageBox.Show("参数读取失败！");
            //}
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void return_dtcl_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)//关闭实时测量
            {
               
                    //founctioncode = 0x0C;
                    //ack_timer3.Enabled = true;
                    //ack_timer3.Interval = overtime;
                    serialPort.Write(command14, 0, command14.Length);
         

                close_flag = 0;
                try
                {
                    serialPort.Close();
                    videoSourcePlayer_jqcl.Stop();
                }
                catch { }
                dtcl m = new dtcl();
                m.Show();
                this.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            save_flag = 0;
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void jqcl_xzjd_txt_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Convert.ToInt16(jqcl_xzjd_txt.Text);
            }
            catch
            {
                jqcl_xzjd_txt.Text = "";
                MessageBox.Show("输入有误！");
            }
        }

        private void cxcl_srz_tbo_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Convert.ToSingle(cxcl_srz_tbo.Text);
            }
            catch
            {
                cxcl_srz_tbo.Text = "";
                MessageBox.Show("输入有误！");
            }
        }

        private void cssz_jdsz_ql_tbo_Click(object sender, EventArgs e)
        {
            byte[] bytes = new byte[9];
            byte[] bytess = new byte[4];
            bytes[0] = 0XEE;
            bytes[1] = 0XEE;
            bytes[2] = 0X0E;

            if (serialPort.IsOpen)
            {
                if (founctioncode == 0)
                {
                    if (cssz_jdsz_txt.Text != "")
                    {
                        string changeAngel = cssz_jdsz_txt.Text.Replace("°", "");
                        bytess = BitConverter.GetBytes( Convert.ToSingle(changeAngel) );

                        for (int i = 0; i < 4; i++)
                        {
                            bytes[3 + i] = bytess[i];
                        }

                        bytes[7] = 0xFC;
                        bytes[8] = 0xFC;

                        founctioncode = 0x0E;
                        ack_timer3.Enabled = true;
                        ack_timer3.Interval = overtime;
                        serialPort.Write(bytes, 0, bytes.Length);
                    }
                    else
                    {
                        MessageBox.Show("请输入角度设置值！");
                    }
                }
            }
        }

        private void jqcl_x1_txt_TextChanged(object sender, EventArgs e)
        {
            if (jqcl_x1_txt.Text != "")
            {
                try
                {
                    Convert.ToSingle(jqcl_x1_txt.Text);
                }
                catch
                {
                    MessageBox.Show("输入值有误！");
                    jqcl_x1_txt.Text = "";
                }
            }
        }

        private void jqcl_x2_txt_TextChanged(object sender, EventArgs e)
        {
            if (jqcl_x2_txt.Text != "")
            {
                try
                {
                    Convert.ToSingle(jqcl_x2_txt.Text);
                }
                catch
                {
                    MessageBox.Show("输入值有误！");
                    jqcl_x2_txt.Text = "";
                }
            }
        }

        private void jqcl_y1_txt_TextChanged(object sender, EventArgs e)
        {
            if (jqcl_y1_txt.Text != "")
            {
                try
                {
                    Convert.ToSingle(jqcl_y1_txt.Text);
                }
                catch
                {
                    MessageBox.Show("输入值有误！");
                    jqcl_y1_txt.Text = "";
                }
            }
        }

        private void jqcl_y2_txt_TextChanged(object sender, EventArgs e)
        {
            if (jqcl_y2_txt.Text != "")
            {
                try
                {
                    Convert.ToSingle(jqcl_y2_txt.Text);
                }
                catch
                {
                    MessageBox.Show("输入值有误！");
                    jqcl_y2_txt.Text = "";
                }
            }
        }

        private void jqcl_gjsz_txt_TextChanged(object sender, EventArgs e)
        {
            if (jqcl_gjsz_txt.Text != "")
            {
                try
                {
                    Convert.ToSingle(jqcl_gjsz_txt.Text);
                }
                catch
                {
                    MessageBox.Show("输入值有误！");
                    jqcl_gjsz_txt.Text = "";
                }
            }
        }

        private void jqcl_txlzj_txt_TextChanged(object sender, EventArgs e)
        {
            if (jqcl_txlzj_txt.Text != "")
            {
                try
                {
                    Convert.ToSingle(jqcl_txlzj_txt.Text);
                }
                catch
                {
                    MessageBox.Show("输入值有误！");
                    jqcl_txlzj_txt.Text = "";
                }
            }
        }

        private void cssz_jdsz_txt_TextChanged(object sender, EventArgs e)
        {
            if (cssz_jdsz_txt.Text != "")
            {
                try
                {
                    string angel = cssz_jdsz_txt.Text.Replace("°", "");
                    Convert.ToSingle(angel);
                }
                catch
                {
                    MessageBox.Show("输入值有误！");
                    cssz_jdsz_txt.Text = "";
                }
            }
        }

        private void gc_gc_lab_Click(object sender, EventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {
            save_flag = 1;
            MessageBox.Show("设置成功，");
        }

        private void kz_pnel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            clts_lab.Text = "请开始第一次测量！";
            jbVerify_celiang = 1;
            cgVerify_celiang = 0;

        }

        private void jg_bto_Click(object sender, EventArgs e)
        {

            if (serialPort.IsOpen)
            {
                if (founctioncode == 0)
                {
                    if (jg_bto.BackColor == Color.Red)
                    {
                        founctioncode = 10;
                        ack_timer3.Enabled = true;
                        ack_timer3.Interval = overtime;
                        serialPort.Write(command9, 0, command9.Length);//关激光
                    }
                    else
                    {
                        founctioncode = 10;
                        ack_timer3.Enabled = true;
                        ack_timer3.Interval = overtime;
                        serialPort.Write(command10, 0, command10.Length);//开激光
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (jbVerify_celiang == 1 || jbVerify_celiang == 2 || cgVerify_celiang == 1 || cgVerify_celiang == 2)
            {
                if (serialPort.IsOpen)
                {
                    if (founctioncode == 0)
                    {
                        founctioncode = 4;
                        ack_timer3.Enabled = true;
                        ack_timer3.Interval = overtime;
                        serialPort.Write(command6, 0, command6.Length);
                    }
                }

                //jbVerify_celiang = 0;
                //cgVerify_celiang = 0;
            }
        }

        private void boundrate_cbo_TextChanged(object sender, EventArgs e)
        {
            if (change_flag == 0)
            {
                int i;
                serialPort.Close();
                ToolStripMenuItem[] port = new ToolStripMenuItem[chooseport.DropDownItems.Count];
                chooseport.DropDownItems.CopyTo(port, 0);

                for (i = 0; i < port.Length; i++)
                {
                    port[i].Checked = false;
                }

                choosedevices.Checked = false;

                MessageBox.Show("请重新打开串口！");
            }

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (pictureBox1.Left >= -(pictureBox1.Width / 2) + pic_lct_x + (Cross_width - 1) / 2
               && pictureBox1.Left <= pic_lct_x + pictureBox1.Width / 2 - (Cross_width + 1) / 2
               && pictureBox1.Top >= -(pictureBox1.Height / 2) + pic_lct_y + (Cross_height - 1) / 2
               && pictureBox1.Top <= pic_lct_y + pictureBox1.Height / 2 - (Cross_height + 1) / 2)
            {
                switch (move)
                {
                    case 1:
                        pictureBox1.Left -= 1;
                        break;
                    case 2:
                        pictureBox1.Left += 1;
                        break;
                    case 3:
                        pictureBox1.Top -= 1;
                        break;
                    case 4:
                        pictureBox1.Top += 1;
                        break;
                }
            }
            else if (pictureBox1.Left == -(pictureBox1.Width / 2) + pic_lct_x + (Cross_width - 1) / 2 - 1)
            {
                pictureBox1.Left++;
            }
            else if (pictureBox1.Left == pic_lct_x + pictureBox1.Width / 2 - (Cross_width + 1) / 2 + 1)
            {
                pictureBox1.Left--;
            }
            else if (pictureBox1.Top == -(pictureBox1.Height / 2) + pic_lct_y + (Cross_height - 1) / 2 - 1)
            {
                pictureBox1.Top++;
            }
            else if (pictureBox1.Top <= pic_lct_y + pictureBox1.Height / 2 - (Cross_height + 1) / 2 + 1)
            {
                pictureBox1.Top--;
            }


        }

        /*串口接收*/
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
         float jiguang, daogui, qingjiao;
          
                //System.Threading.Thread.Sleep(100);
            Delay(50);
            if (!serialPort.IsOpen)
            {
                ack_timer3.Enabled = false;
                founctioncode = 0;
                return;
            }
            byte[] readBytes = new byte[serialPort.BytesToRead];
         
                serialPort.Read(readBytes, 0, readBytes.Length);
            
          
            /*开机检测串口*/
            if (start_flag == 0)
            {
                if (readBytes[2] == founctioncode)
                {
                    if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X0A && readBytes[3] == 0XFC && readBytes[4] == 0XFC)
                    {
                        start_flag = 1;
                        founctioncode = 0;
                        return;
                    }
                }
                founctioncode = 0;
                return;
            }

            if (readBytes.Length == 0)
            {
                return;
            }
            /*实体按键测量数据反馈*/


            //if (readBytes.Length < 3)
            //{
            //    //overtime_flag = 0;
            //    ack_timer3.Enabled = false;
            //    //MessageBox.Show("数据长度出错！" + readBytes.Length.ToString());
            //    Console.WriteLine("数据长度出错！" + readBytes.Length.ToString());


            //    //标志位复位
            //    parameter.celiang_flag = 0;
            //    parameter1.celiang_flag = 0;
            //    parameter2.celiang_flag = 0;
            //    parameter1.clEnd_flag = 0;
            //    parameter2.clEnd_flag = 0;
            //    parameter1.customAngel_flag = 0;
            //    left_flag = 1;
            //    right_flag = 1;
            //    founctioncode = 0;
            //    jbVerify_celiang = 0;
            //    cgVerify_celiang = 0;
            //    return;
            //}
            /*实时测量数据*/
            if (readBytes[2] == 0x0c)
            {
                if (readBytes.Length == 22)
                {
                    if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X0C && readBytes[20] == 0XFC && readBytes[21] == 0XFC)
                    {
                        parameter.guiju = BitConverter.ToSingle(readBytes, 3);
                        parameter.guiju = (float)Math.Round((double)parameter.guiju, 2);//保留2位小数

                        parameter.zengliang = (BitConverter.ToDouble(readBytes, 7));
                        parameter.zengliang = Math.Round(parameter.zengliang / 10, 2);//保留2位小数

                        parameter.chaogao = BitConverter.ToSingle(readBytes, 15);
                        parameter.chaogao = (float)Math.Round((double)parameter.chaogao, 2);//保留2位小数

                        /*数据填充*/
                        jbcl_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;
                        cssz_gjz_lab.Text = parameter.guiju.ToString();
                        jbcl_kj_lab.Text = parameter.zengliang.ToString() + "cm";
                        jbcl_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;
                        cssz_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;

                        if (readBytes[19] == 0X00)
                        {
                            sjts_lab.Text = "测量失败";
                        }
                        else
                        {
                            sjts_lab.Text = "数据有效";
                        }
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
             /*实体按键开关激光返回*/
             if(readBytes[2] == 0x0E)
            {
                if(readBytes.Length == 6)
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Write(command15, 0, command15.Length);
                    }

                    if(readBytes[3] == 0x00)
                    {
                        jg_bto.BackColor = Color.Gray;//关激光
                    }
                    else if(readBytes[3] == 0x01)
                    {
                        jg_bto.BackColor = Color.Red;//开激光
                    }
                }
            }
             /*实体按键测量返回*/
            if(readBytes[2] == 0x11)
            {
                if(readBytes.Length == 25)
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Write(command16, 0, command16.Length);
                    }

                    if (readBytes[3] == 0xff && readBytes[4] == 0xff && readBytes[5] == 0xff && readBytes[6] == 0xff)
                    {
                        //标志位复位
                        parameter1.celiang_flag = 0;
                        parameter2.celiang_flag = 0;
                        parameter.celiang_flag = 0;
                        parameter1.clEnd_flag = 0;
                        parameter1.clEnd_flag = 0;
                        parameter1.customAngel_flag = 0;
                        left_flag = 1;
                        right_flag = 1;
                        founctioncode = 0;
                        jbVerify_celiang = 0;
                        cgVerify_celiang = 0;
                        MessageBox.Show("激光打到无穷远处！");
                        return;
                    }

                    parameter.lachuzhi = BitConverter.ToSingle(readBytes, 3); /*计算拉出值*/
                    parameter.lachuzhi = (float)Math.Round((double)parameter.lachuzhi, 2);//保留2位小数

                    parameter.daogao = BitConverter.ToSingle(readBytes, 7);   /*计算导高*/
                    parameter.daogao = (float)Math.Round((double)parameter.daogao, 2);//保留2位小数

                    parameter.guiju = BitConverter.ToSingle(readBytes, 11);   /*计算轨距*/
                    parameter.guiju = (float)Math.Round((double)parameter.guiju, 2);//保留2位小数

                    parameter.chaogao = BitConverter.ToSingle(readBytes, 15); /*计算超高*/
                    parameter.chaogao = (float)Math.Round((double)parameter.chaogao, 2);//保留2位小数

                    parameter.cemianxianjie = BitConverter.ToSingle(readBytes, 19);/*计算侧面限界*/
                    parameter.cemianxianjie = (float)Math.Round((double)parameter.cemianxianjie, 2);//保留2位小数

                    /*数据填充*/
                    gc_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;//500高差参数
                    gc_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;


                    cxcl_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;//岔心测量参数
                    cxcl_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;
                    cssz_cg_lab.Text = parameter.chaogao.ToString();
                    if (cxcl_srz_tbo.Text != "")
                    {
                        try
                        {
                            parameter.ngj = Convert.ToSingle(cxcl_gj_lab.Text) - Convert.ToSingle(cxcl_srz_tbo.Text);
                            cxcl_ngj_lab.Text = ((float)Math.Round((double)parameter.ngj, 1)).ToString() + parameter.danwei;//保留一位小数
                            parameter.tyz = (Convert.ToSingle(cxcl_ngj_lab.Text) - parameter.lachuzhi) - (parameter.guiju / 2 - Convert.ToSingle(cxcl_srz_tbo.Text));
                            cxcl_tyz_lab.Text = ((float)Math.Round((double)parameter.tyz, 1)).ToString() + parameter.danwei;//保留一位小数
                            parameter.pyz = Convert.ToSingle(cxcl_ngj_lab.Text) / 2 - Convert.ToSingle(cxcl_srz_tbo.Text);
                            cxcl_pyz_lab.Text = ((float)Math.Round((double)parameter.pyz, 1)).ToString() + parameter.danwei;//保留一位小数
                        }
                        catch
                        {
                            MessageBox.Show("输入值有误！");
                        }
                    }

                    jggd_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;//结构高度参数
                    jggd_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;

                    dwpd_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;//定位坡度参数
                    dwpd_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;

                    kz_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;//跨中参数
                    kz_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;
                    kz_kzgd_lab.Text = parameter.daogao.ToString() + parameter.danwei;
                    kz_kzpy_lab.Text = parameter.lachuzhi.ToString() + parameter.danwei;

                    zycl_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;//自由测量参数
                    zycl_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;

                    jbcl_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;//基本测量参数
                    jbcl_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;
                    jbcl_lcz_lab.Text = parameter.lachuzhi.ToString() + parameter.danwei;
                    jbcl_dg_lab.Text = parameter.daogao.ToString() + parameter.danwei;

                    /*如果处于保存模式*/
                    if (save_flag == 1)
                    {
                        if (jqcl_qjzc_txt.Text != "" || jqcl_qjzc_txt.Text != "" || jqcl_zz_txt.Text != "")
                        {
                            databaseDetails = "'" + jqcl_qjzc_txt.Text + "','','" + jqcl_md_txt.Text + "','','','" + jqcl_zz_txt.Text + "','','" + parameter.daogao.ToString()
                                                  + "','" + parameter.lachuzhi.ToString() + "','','','" + parameter.cemianxianjie.ToString() + "','','','','','" + parameter.chaogao.ToString()
                                                  + "','" + parameter.guiju.ToString() + "'";
                            database_operate.Add(databaseDetails);
                        }
                        else
                        {
                            MessageBox.Show("请输入区间站场、锚段、支柱号！");
                        }
                    }

                    //parameter.celiang_flag = 0;
                    //founctioncode = 0;
                    // MessageBox.Show("2");
                    return;
                }
            }
            if (readBytes[2] == founctioncode)
            {

                //overtime_flag = 0;
                ack_timer3.Enabled = false;
                /*绝对编码器角度置零反馈*/
                if (readBytes.Length == 5)
                {
                    if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X01 && readBytes[3] == 0XFC && readBytes[4] == 0XFC)
                    {
                        //button13.Enabled = true;
                        jqcl_jd_lab.Text = "0°";
                        cssz_jd_lab.Text = "0";
                        MessageBox.Show("角度置零成功！");
                        founctioncode = 0;
                    }
                    else if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X0A && readBytes[3] == 0XFC && readBytes[4] == 0XFC)
                    {
                        if (jg_bto.BackColor == Color.Red)
                        {
                            //jg_bto.Enabled = true;
                            jg_bto.BackColor = Color.Gray;
                            founctioncode = 0;
                        }
                        else
                        {
                            //jg_bto.Enabled = true;
                            jg_bto.BackColor = Color.Red;
                            founctioncode = 0;
                        }
                    }

                    /*增量编码器距离数据置零反馈*/
                    else if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X06 && readBytes[3] == 0XFC && readBytes[4] == 0XFC)
                    {
                        //jbcl_kjql_bto.Enabled = true;
                        jbcl_kj_lab.Text = "0.000" + parameter.danwei;
                        MessageBox.Show("增量编码器角度置零成功！");
                        founctioncode = 0;
                    }
                    /*参数设置反馈*/
                    else if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X07 && readBytes[3] == 0XFC && readBytes[4] == 0XFC)
                    {
                        MessageBox.Show("参数设置成功！");
                        founctioncode = 0;
                    }
                    else if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X0D && readBytes[3] == 0XFC && readBytes[4] == 0XFC)
                    {
                        cssz_cg_lab.Text = "0";
                        MessageBox.Show("超高置零成功！");
                        founctioncode = 0;
                    }
                    else
                    {
                        //标志位复位
                        parameter1.celiang_flag = 0;
                        parameter2.celiang_flag = 0;
                        parameter.celiang_flag = 0;
                        parameter1.clEnd_flag = 0;
                        parameter1.clEnd_flag = 0;
                        parameter1.customAngel_flag = 0;
                        left_flag = 1;
                        right_flag = 1;
                        founctioncode = 0;
                        jbVerify_celiang = 0;
                        cgVerify_celiang = 0;
                        return;
                    }
                }
                else if (readBytes.Length == 9)
                {
                    if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X02 && readBytes[7] == 0XFC && readBytes[8] == 0XFC)//电机粗调反馈
                    {
                        parameter.jueduizhi = BitConverter.ToSingle(readBytes, 3);
                        parameter.jueduizhi = (float)Math.Round((double)parameter.jueduizhi, 2);//保留两位小数

                        jqcl_jd_lab.Text = parameter.jueduizhi.ToString() + "°";

                        founctioncode = 0;
                    }
                    else if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X03 && readBytes[7] == 0XFC && readBytes[8] == 0XFC)//电机微调反馈
                    {
                        parameter.jueduizhi = BitConverter.ToSingle(readBytes, 3);
                        parameter.jueduizhi = (float)Math.Round((double)parameter.jueduizhi, 2);//保留两位小数

                        jqcl_jd_lab.Text = parameter.jueduizhi.ToString() + "°";

                        founctioncode = 0;
                    }
                    else if(readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X0E && readBytes[7] == 0XFC && readBytes[8] == 0XFC)
                    {
                        cssz_jd_lab.Text = (Math.Round((double)BitConverter.ToSingle(readBytes, 3), 2) ).ToString() + "°";

                        founctioncode = 0;
                    }
                    else
                    {
                        //标志位复位
                        parameter1.celiang_flag = 0;
                        parameter2.celiang_flag = 0;
                        parameter.celiang_flag = 0;
                        parameter1.clEnd_flag = 0;
                        parameter1.clEnd_flag = 0;
                        parameter1.customAngel_flag = 0;
                        left_flag = 1;
                        right_flag = 1;
                        founctioncode = 0;
                        jbVerify_celiang = 0;
                        cgVerify_celiang = 0;
                        return;
                    }

                }
                else if (readBytes.Length == 25)
                {
                    if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X04 && readBytes[23] == 0XFC && readBytes[24] == 0XFC)//获取数据指令1反馈
                    {
                        //MessageBox.Show("1");
                        if (parameter.celiang_flag == 1)
                        {
                            if (readBytes[3] == 0xff && readBytes[4] == 0xff && readBytes[5] == 0xff && readBytes[6] == 0xff)
                            {
                                //标志位复位
                                parameter1.celiang_flag = 0;
                                parameter2.celiang_flag = 0;
                                parameter.celiang_flag = 0;
                                parameter1.clEnd_flag = 0;
                                parameter1.clEnd_flag = 0;
                                parameter1.customAngel_flag = 0;
                                left_flag = 1;
                                right_flag = 1;
                                founctioncode = 0;
                                jbVerify_celiang = 0;
                                cgVerify_celiang = 0;
                                MessageBox.Show("激光打到无穷远处！");
                                return;
                            }

                            parameter.lachuzhi = BitConverter.ToSingle(readBytes, 3); /*计算拉出值*/
                            parameter.lachuzhi = (float)Math.Round((double)parameter.lachuzhi, 2);//保留2位小数

                            parameter.daogao = BitConverter.ToSingle(readBytes, 7);   /*计算导高*/
                            parameter.daogao = (float)Math.Round((double)parameter.daogao, 2);//保留2位小数

                            parameter.guiju = BitConverter.ToSingle(readBytes, 11);   /*计算轨距*/
                            parameter.guiju = (float)Math.Round((double)parameter.guiju, 2);//保留2位小数

                            parameter.chaogao = BitConverter.ToSingle(readBytes, 15); /*计算超高*/
                            parameter.chaogao = (float)Math.Round((double)parameter.chaogao, 2);//保留2位小数

                            parameter.cemianxianjie = BitConverter.ToSingle(readBytes, 19);/*计算侧面限界*/
                            parameter.cemianxianjie = (float)Math.Round((double)parameter.cemianxianjie, 2);//保留2位小数

                            /*数据填充*/
                            gc_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;//500高差参数
                            gc_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;


                            cxcl_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;//岔心测量参数
                            cxcl_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;
                            cssz_cg_lab.Text = parameter.chaogao.ToString();
                            if (cxcl_srz_tbo.Text != "")
                            {
                                try
                                {
                                    parameter.ngj = Convert.ToSingle(cxcl_gj_lab.Text) - Convert.ToSingle(cxcl_srz_tbo.Text);
                                    cxcl_ngj_lab.Text = ((float)Math.Round((double)parameter.ngj, 1)).ToString() + parameter.danwei;//保留一位小数
                                    parameter.tyz = (Convert.ToSingle(cxcl_ngj_lab.Text) - parameter.lachuzhi) - (parameter.guiju / 2 - Convert.ToSingle(cxcl_srz_tbo.Text));
                                    cxcl_tyz_lab.Text = ((float)Math.Round((double)parameter.tyz, 1)).ToString() + parameter.danwei;//保留一位小数
                                    parameter.pyz = Convert.ToSingle(cxcl_ngj_lab.Text) / 2 - Convert.ToSingle(cxcl_srz_tbo.Text);
                                    cxcl_pyz_lab.Text = ((float)Math.Round((double)parameter.pyz, 1)).ToString() + parameter.danwei;//保留一位小数
                                }
                                catch
                                {
                                    MessageBox.Show("输入值有误！");
                                }
                            }

                            jggd_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;//结构高度参数
                            jggd_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;

                            dwpd_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;//定位坡度参数
                            dwpd_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;

                            kz_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;//跨中参数
                            kz_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;
                            kz_kzgd_lab.Text = parameter.daogao.ToString() + parameter.danwei;
                            kz_kzpy_lab.Text = parameter.lachuzhi.ToString() + parameter.danwei;

                            zycl_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;//自由测量参数
                            zycl_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;

                            jbcl_gj_lab.Text = parameter.guiju.ToString() + parameter.danwei;//基本测量参数
                            jbcl_cg_lab.Text = parameter.chaogao.ToString() + parameter.danwei;
                            jbcl_lcz_lab.Text = parameter.lachuzhi.ToString() + parameter.danwei;
                            jbcl_dg_lab.Text = parameter.daogao.ToString() + parameter.danwei;

                            /*如果处于保存模式*/
                            if(save_flag == 1)
                            {
                                if (jqcl_qjzc_txt.Text != "" || jqcl_qjzc_txt.Text != "" || jqcl_zz_txt.Text != "")
                                {
                                    databaseDetails = "'" + jqcl_qjzc_txt.Text + "','','" + jqcl_md_txt.Text + "','','','" + jqcl_zz_txt.Text + "','','" + parameter.daogao.ToString()
                                                          + "','" + parameter.lachuzhi.ToString() + "','','','" + parameter.cemianxianjie.ToString() + "','','','','','" + parameter.chaogao.ToString()
                                                          + "','" + parameter.guiju.ToString() + "'";
                                    database_operate.Add(databaseDetails);
                                }
                                else
                                {
                                    MessageBox.Show("请输入区间站场、锚段、支柱号！");
                                }
                            }

                            parameter.celiang_flag = 0;
                            founctioncode = 0;
                            // MessageBox.Show("2");
                        }
                        else if (parameter2.celiang_flag == 1)
                        {
                            if (readBytes[3] == 0xff && readBytes[4] == 0xff && readBytes[5] == 0xff && readBytes[6] == 0xff)
                            {
                                //标志位复位
                                parameter1.celiang_flag = 0;
                                parameter2.celiang_flag = 0;
                                parameter.celiang_flag = 0;
                                parameter1.clEnd_flag = 0;
                                parameter1.clEnd_flag = 0;
                                parameter1.customAngel_flag = 0;
                                left_flag = 1;
                                right_flag = 1;
                                founctioncode = 0;
                                jbVerify_celiang = 0;
                                cgVerify_celiang = 0;
                                MessageBox.Show("激光打到无穷远处！");
                                return;
                            }

                            parameter2.lachuzhi = BitConverter.ToSingle(readBytes, 3); /*计算拉出值*/
                            parameter2.lachuzhi = (float)Math.Round((double)parameter2.lachuzhi, 2);//保留2位小数

                            parameter2.daogao = BitConverter.ToSingle(readBytes, 7);   /*计算导高*/
                            parameter2.daogao = (float)Math.Round((double)parameter2.daogao, 2);//保留2位小数

                            parameter2.guiju = BitConverter.ToSingle(readBytes, 11);   /*计算轨距*/
                            parameter2.guiju = (float)Math.Round((double)parameter2.guiju, 2);//保留2位小数

                            parameter2.chaogao = BitConverter.ToSingle(readBytes, 15); /*计算超高*/
                            parameter2.chaogao = (float)Math.Round((double)parameter2.chaogao, 2);//保留2位小数

                            parameter2.cemianxianjie = BitConverter.ToSingle(readBytes, 19);/*计算侧面限界*/
                            parameter2.cemianxianjie = (float)Math.Round((double)parameter2.cemianxianjie, 2);//保留2位小数
                            /*数据填充*/
                            gc_gj_lab.Text = parameter2.guiju.ToString() + parameter2.danwei;//500高差参数

                            gc_cg_lab.Text = parameter2.chaogao.ToString() + parameter2.danwei;


                            cxcl_gj_lab.Text = parameter2.guiju.ToString() + parameter2.danwei;//岔心测量参数
                            cxcl_cg_lab.Text = parameter2.chaogao.ToString() + parameter2.danwei;
                            cssz_cg_lab.Text = parameter2.chaogao.ToString();
                            if (cxcl_srz_tbo.Text != "")
                            {
                                try
                                {
                                    parameter2.ngj = Convert.ToSingle(cxcl_gj_lab.Text) - Convert.ToSingle(cxcl_srz_tbo.Text);
                                    cxcl_ngj_lab.Text = ((float)Math.Round((double)parameter2.ngj, 1)).ToString() + parameter2.danwei;//保留一位小数
                                    parameter2.tyz = (Convert.ToSingle(cxcl_ngj_lab.Text) - parameter2.lachuzhi) - (parameter2.guiju / 2 - Convert.ToSingle(cxcl_srz_tbo.Text));
                                    cxcl_tyz_lab.Text = ((float)Math.Round((double)parameter2.tyz, 1)).ToString() + parameter2.danwei;//保留一位小数
                                    parameter2.pyz = Convert.ToSingle(cxcl_ngj_lab.Text) / 2 - Convert.ToSingle(cxcl_srz_tbo.Text);
                                    cxcl_pyz_lab.Text = ((float)Math.Round((double)parameter2.pyz, 1)).ToString() + parameter2.danwei;//保留一位小数
                                }
                                catch
                                {
                                    MessageBox.Show("输入值有误！");
                                }
                            }

                            jggd_gj_lab.Text = parameter2.guiju.ToString() + parameter2.danwei;//结构高度参数
                            jggd_cg_lab.Text = parameter2.chaogao.ToString() + parameter2.danwei;

                            dwpd_gj_lab.Text = parameter2.guiju.ToString() + parameter2.danwei;//定位坡度参数
                            dwpd_cg_lab.Text = parameter2.chaogao.ToString() + parameter2.danwei;

                            kz_gj_lab.Text = parameter2.guiju.ToString() + parameter2.danwei;//跨中参数
                            kz_cg_lab.Text = parameter2.chaogao.ToString() + parameter2.danwei;
                            kz_kzgd_lab.Text = parameter2.daogao.ToString() + parameter2.danwei;
                            kz_kzpy_lab.Text = parameter2.lachuzhi.ToString() + parameter2.danwei;

                            zycl_gj_lab.Text = parameter2.guiju.ToString() + parameter2.danwei;//自由测量参数
                            zycl_cg_lab.Text = parameter2.chaogao.ToString() + parameter2.danwei;

                            jbcl_gj_lab.Text = parameter2.guiju.ToString() + parameter2.danwei;//基本测量参数
                            jbcl_cg_lab.Text = parameter2.chaogao.ToString() + parameter2.danwei;
                            jbcl_lcz_lab.Text = parameter2.lachuzhi.ToString() + parameter2.danwei;
                            jbcl_dg_lab.Text = parameter2.daogao.ToString() + parameter2.danwei;

                            clts_lab.Text = "请开始第一次测量！";

                            founctioncode = 0;
                            parameter2.celiang_flag = 0;
                            parameter1.celiang_flag = 0;
                            parameter2.clEnd_flag = 1;

                        }
                        else if (parameter1.celiang_flag == 1)
                        {
                            if (readBytes[3] == 0xff && readBytes[4] == 0xff && readBytes[5] == 0xff && readBytes[6] == 0xff)
                            {
                                //标志位复位
                                parameter1.celiang_flag = 0;
                                parameter2.celiang_flag = 0;
                                parameter.celiang_flag = 0;
                                parameter1.clEnd_flag = 0;
                                parameter1.clEnd_flag = 0;
                                parameter1.customAngel_flag = 0;
                                left_flag = 1;
                                right_flag = 1;
                                founctioncode = 0;
                                jbVerify_celiang = 0;
                                cgVerify_celiang = 0;
                                MessageBox.Show("激光打到无穷远处！");
                                return;
                            }

                            parameter1.lachuzhi = BitConverter.ToSingle(readBytes, 3); /*计算拉出值*/
                            parameter1.lachuzhi = (float)Math.Round((double)parameter1.lachuzhi, 2);//保留2位小数

                            parameter1.daogao = BitConverter.ToSingle(readBytes, 7);   /*计算导高*/
                            parameter1.daogao = (float)Math.Round((double)parameter1.daogao, 2);//保留2位小数

                            parameter1.guiju = BitConverter.ToSingle(readBytes, 11);   /*计算轨距*/
                            parameter1.guiju = (float)Math.Round((double)parameter1.guiju, 2);//保留2位小数

                            parameter1.chaogao = BitConverter.ToSingle(readBytes, 15); /*计算超高*/
                            parameter1.chaogao = (float)Math.Round((double)parameter1.chaogao, 2);//保留2位小数

                            parameter1.cemianxianjie = BitConverter.ToSingle(readBytes, 19);/*计算侧面限界*/
                            parameter1.cemianxianjie = (float)Math.Round((double)parameter1.cemianxianjie, 2);//保留2位小数

                            /*数据填充*/
                            gc_gj_lab.Text = parameter1.guiju.ToString() + parameter1.danwei;//500高差参数

                            gc_cg_lab.Text = parameter1.chaogao.ToString() + parameter1.danwei;


                            cxcl_gj_lab.Text = parameter1.guiju.ToString() + parameter1.danwei;//岔心测量参数
                            cxcl_cg_lab.Text = parameter1.chaogao.ToString() + parameter1.danwei;
                            cssz_cg_lab.Text = parameter1.chaogao.ToString();
                            if (cxcl_srz_tbo.Text != "")
                            {
                                try
                                {
                                    parameter1.ngj = Convert.ToSingle(cxcl_gj_lab.Text) - Convert.ToSingle(cxcl_srz_tbo.Text);
                                    cxcl_ngj_lab.Text = ((float)Math.Round((double)parameter1.ngj, 1)).ToString() + parameter1.danwei;//保留一位小数
                                    parameter1.tyz = (Convert.ToSingle(cxcl_ngj_lab.Text) - parameter1.lachuzhi) - (parameter1.guiju / 2 - Convert.ToSingle(cxcl_srz_tbo.Text));
                                    cxcl_tyz_lab.Text = ((float)Math.Round((double)parameter1.tyz, 1)).ToString() + parameter1.danwei;//保留一位小数
                                    parameter1.pyz = Convert.ToSingle(cxcl_ngj_lab.Text) / 2 - Convert.ToSingle(cxcl_srz_tbo.Text);
                                    cxcl_pyz_lab.Text = ((float)Math.Round((double)parameter1.pyz, 1)).ToString() + parameter1.danwei;//保留一位小数
                                }
                                catch
                                {
                                    MessageBox.Show("输入值有误！");
                                }
                            }

                            jggd_gj_lab.Text = parameter1.guiju.ToString() + parameter1.danwei;//结构高度参数
                            jggd_cg_lab.Text = parameter1.chaogao.ToString() + parameter1.danwei;

                            dwpd_gj_lab.Text = parameter1.guiju.ToString() + parameter1.danwei;//定位坡度参数
                            dwpd_cg_lab.Text = parameter1.chaogao.ToString() + parameter1.danwei;

                            kz_gj_lab.Text = parameter1.guiju.ToString() + parameter1.danwei;//跨中参数
                            kz_cg_lab.Text = parameter1.chaogao.ToString() + parameter1.danwei;
                            kz_kzgd_lab.Text = parameter1.daogao.ToString() + parameter1.danwei;
                            kz_kzpy_lab.Text = parameter1.lachuzhi.ToString() + parameter1.danwei;

                            zycl_gj_lab.Text = parameter1.guiju.ToString() + parameter1.danwei;//自由测量参数
                            zycl_cg_lab.Text = parameter1.chaogao.ToString() + parameter1.danwei;

                            jbcl_gj_lab.Text = parameter1.guiju.ToString() + parameter1.danwei;//基本测量参数
                            jbcl_cg_lab.Text = parameter1.chaogao.ToString() + parameter1.danwei;
                            jbcl_lcz_lab.Text = parameter1.lachuzhi.ToString() + parameter1.danwei;
                            jbcl_dg_lab.Text = parameter1.daogao.ToString() + parameter1.danwei;

                            clts_lab.Text = "请开始第二次测量！";

                            founctioncode = 0;
                            parameter1.clEnd_flag = 1;

                        }
                        /*基本校验、超高校验*/
                        else if (jbVerify_celiang == 1 || jbVerify_celiang == 2 || cgVerify_celiang == 1 || cgVerify_celiang == 2)
                        {
                            if (readBytes[3] == 0xff && readBytes[4] == 0xff && readBytes[5] == 0xff && readBytes[6] == 0xff)
                            {
                                //标志位复位
                                parameter1.celiang_flag = 0;
                                parameter2.celiang_flag = 0;
                                parameter.celiang_flag = 0;
                                parameter1.clEnd_flag = 0;
                                parameter1.clEnd_flag = 0;
                                parameter1.customAngel_flag = 0;
                                left_flag = 1;
                                right_flag = 1;
                                founctioncode = 0;
                                jbVerify_celiang = 0;
                                cgVerify_celiang = 0;
                                MessageBox.Show("激光打到无穷远处！");
                                return;
                            }
                            if (jbVerify_celiang == 1)
                            {
                                parameter1.lachuzhi = BitConverter.ToSingle(readBytes, 3); /*计算拉出值*/
                                parameter1.lachuzhi = (float)Math.Round((double)parameter1.lachuzhi, 2);//保留2位小数

                                clts_lab.Text = "请开始第二次测量!";
                                jbVerify_celiang = 2;
                                founctioncode = 0;
                            }
                            else if (jbVerify_celiang == 2)
                            {
                                parameter2.lachuzhi = BitConverter.ToSingle(readBytes, 3); /*计算拉出值*/
                                parameter2.lachuzhi = (float)Math.Round((double)parameter2.lachuzhi, 2);//保留2位小数

                                founctioncode = 0;
                                clts_lab.Text = "";
                                jbVerify_celiang = 0;
                                //MessageBox.Show("拉出值差为：" + Math.Abs(parameter1.lachuzhi + parameter2.lachuzhi).ToString() + ",请旋转角度：" + ((parameter1.lachuzhi + parameter2.lachuzhi) / 200).ToString());
                                cssz_jdsz_txt.Text = (Math.Round((double)((parameter1.lachuzhi + parameter2.lachuzhi) / 200), 2) ).ToString() + "°";
                            }
                            else if (cgVerify_celiang == 1)
                            {
                                parameter1.chaogao = BitConverter.ToSingle(readBytes, 15); /*计算超高*/
                                parameter1.chaogao = (float)Math.Round((double)parameter1.chaogao, 2);//保留2位小数

                                founctioncode = 0;
                                clts_lab.Text = "请开始第二次测量!";
                                cgVerify_celiang = 2;
                            }
                            else if (cgVerify_celiang == 2)
                            {
                                parameter2.chaogao = BitConverter.ToSingle(readBytes, 15); /*计算超高*/
                                parameter2.chaogao = (float)Math.Round((double)parameter2.chaogao, 2);//保留2位小数

                                founctioncode = 0;
                                clts_lab.Text = "";
                                cgVerify_celiang = 0;
                                MessageBox.Show("超高真实值为：" + Math.Abs((Math.Abs(parameter1.chaogao) + Math.Abs(parameter2.chaogao)) / 2).ToString());
                            }
                        }
                    }
                    else
                    {
                        //标志位复位
                        parameter1.celiang_flag = 0;
                        parameter2.celiang_flag = 0;
                        parameter.celiang_flag = 0;
                        parameter1.clEnd_flag = 0;
                        parameter1.clEnd_flag = 0;
                        parameter1.customAngel_flag = 0;
                        left_flag = 1;
                        right_flag = 1;
                        founctioncode = 0;
                        return;
                    }
                }
                else if (readBytes.Length == 29)
                {
                    if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X05 && readBytes[27] == 0XFC && readBytes[28] == 0XFC)//获取数据指令2反馈
                    {
                        /*计算增量编码器距离值*//*推行距离值*/
                        parameter.zengliang = (float)BitConverter.ToDouble(readBytes, 3);
                        parameter.zengliang = (float)Math.Round((double)parameter.zengliang, 2);//保留两位小数


                        /*计算绝对值编码器角度值*//*电机旋转角度*/
                        parameter.jueduizhi = (float)BitConverter.ToSingle(readBytes, 11);
                        if (parameter.celiang_flag != 1)
                        {
                            parameter.jueduizhi = (float)Math.Round((double)parameter.jueduizhi, 2);//保留两位小数

                            //

                            parameter.customAngel_flag = 0;
                            left_flag = 1;
                            right_flag = 1;
                        }


                        /*计算激光测距距离值*/
                        jiguang = BitConverter.ToSingle(readBytes, 15);

                        /*计算导轨距离值*/
                        daogui = BitConverter.ToSingle(readBytes, 19);

                        /*计算倾角角度值*/
                        qingjiao = BitConverter.ToSingle(readBytes, 23);

                        founctioncode = 0;

                    }
                    else if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X0B && readBytes[27] == 0XFC && readBytes[28] == 0XFC)//获取固定参数指令反馈
                    {
                        /*轨道尺固定测量端到测量仪旋转中心的距离X1(单位mm)*/
                        parameter.Y2 = BitConverter.ToSingle(readBytes, 3);
                        parameter.Y2 = (float)Math.Round((double)parameter.Y2, 2);//保留2位小数

                        /*激光束起始点到测量器旋转中心的水平距离X2(单位mm)*/
                        parameter.X2 = BitConverter.ToSingle(readBytes, 7);
                        parameter.X2 = (float)Math.Round((double)parameter.X2, 2);//保留2位小数

                        /*轨道平面到测量仪旋转中心的距离Y1(单位mm)*/
                        parameter.X1 = BitConverter.ToSingle(readBytes, 11);
                        parameter.X1 = (float)Math.Round((double)parameter.X1, 2);//保留2位小数

                        /*激光束起始点到测量器旋转中心的垂直距离Y2(单位mm)*/
                        parameter.Y1 = BitConverter.ToSingle(readBytes, 15);
                        parameter.Y1 = (float)Math.Round((double)parameter.Y1, 2);//保留2位小数

                        /*轨道尺固定测量端到直线传感器零点位置C1(单位mm)*/
                        parameter.C1 = BitConverter.ToSingle(readBytes, 19);
                        parameter.C1 = (float)Math.Round((double)parameter.C1, 2);//保留2位小数

                        /*推行轮直径*/
                        parameter.D1 = BitConverter.ToSingle(readBytes, 23);
                        parameter.D1 = (float)Math.Round((double)parameter.D1, 2);//保留2位小数

                        /*数据填充*/
                        jqcl_x1_txt.Text = parameter.X1.ToString();
                        jqcl_x2_txt.Text = parameter.X2.ToString();
                        jqcl_y1_txt.Text = parameter.Y1.ToString();
                        jqcl_y2_txt.Text = parameter.Y2.ToString();
                        jqcl_gjsz_txt.Text = parameter.C1.ToString();
                        jqcl_txlzj_txt.Text = parameter.D1.ToString();

                        founctioncode = 0;
                    }
                    else
                    {
                        //标志位复位
                        parameter1.celiang_flag = 0;
                        parameter2.celiang_flag = 0;
                        parameter.celiang_flag = 0;
                        parameter1.clEnd_flag = 0;
                        parameter1.clEnd_flag = 0;
                        parameter1.customAngel_flag = 0;
                        left_flag = 1;
                        right_flag = 1;
                        founctioncode = 0;
                        return;
                    }
                }
              
                else
                {
                    //标志位复位
                    parameter1.celiang_flag = 0;
                    parameter2.celiang_flag = 0;
                    parameter.celiang_flag = 0;
                    parameter1.clEnd_flag = 0;
                    parameter1.clEnd_flag = 0;
                    parameter1.customAngel_flag = 0;
                    left_flag = 1;
                    right_flag = 1;
                    founctioncode = 0;
                    jbVerify_celiang = 0;
                    cgVerify_celiang = 0;
                    return;
                }

                if (parameter2.clEnd_flag == 1 && parameter1.clEnd_flag == 1)
                {
                    parameter1.gaocha = parameter1.daogao - parameter2.daogao;
                    parameter1.gaocha = (float)Math.Round((double)parameter1.gaocha, 2);//保留2位小数
                    gc_gc_lab.Text = parameter1.gaocha.ToString() + parameter1.danwei;//高差测量参数
                    gc_jj_lab.Text = Math.Round((double)(parameter1.lachuzhi - parameter2.lachuzhi),2).ToString() + parameter1.danwei;

                    jggd_dg1_lab.Text = parameter1.daogao.ToString() + parameter1.danwei;//结构高度测量参数
                    jggd_dg2_lab.Text = parameter2.daogao.ToString() + parameter1.danwei;
                    jggd_gc_lab.Text = (parameter1.daogao - parameter2.daogao).ToString() + parameter1.danwei;

                    parameter1.pdz = (parameter1.daogao - parameter2.daogao) / (parameter1.lachuzhi - parameter2.lachuzhi);
                    dwpd_pdz_lab.Text = parameter1.pdz.ToString() + parameter1.danwei;//定位坡度测量参数
                    dwpd_jdz_lab.Text = (Math.Atan((parameter1.daogao - parameter2.daogao) / (parameter1.lachuzhi - parameter2.lachuzhi))).ToString() + " 度";

                    zycl_spjj_lab.Text = Math.Round((double)(parameter2.lachuzhi - parameter1.lachuzhi),2).ToString() + parameter1.danwei;//自由测量参数
                    zycl_czjj_lab.Text = (parameter2.daogao - parameter1.daogao).ToString() + parameter1.danwei;
                    zycl_czjd_lab.Text = Math.Atan((parameter2.daogao - parameter1.daogao) / (parameter2.lachuzhi - parameter1.lachuzhi)).ToString() + " 度";
                    zycl_zxjj_lab.Text = ((parameter2.daogao - parameter1.daogao) / Math.Sin(Math.Atan((parameter2.daogao - parameter1.daogao) / (parameter2.lachuzhi - parameter1.lachuzhi)))).ToString() + parameter1.danwei;

                    mdgj_gzgd_lab.Text = parameter1.daogao.ToString() + parameter1.danwei;//锚段关节测量参数
                    mdgj_gzpy_lab.Text = parameter1.lachuzhi.ToString() + parameter1.danwei;
                    mdgj_fzpy_lab.Text = parameter2.lachuzhi.ToString() + parameter1.danwei;
                    mdgj_fzgd_lab.Text = parameter2.daogao.ToString() + parameter1.danwei;
                    mdgj_gc_lab.Text = (parameter1.daogao - parameter2.daogao).ToString() + parameter1.danwei;
                    mdgj_py_lab.Text = (parameter1.lachuzhi - parameter2.lachuzhi).ToString() + parameter1.danwei;

                    /*如果处于保存模式*/
                    if (save_flag == 1)
                    {
                        if (jqcl_qjzc_txt.Text != "" || jqcl_qjzc_txt.Text != "" || jqcl_zz_txt.Text != "")
                        {
                            databaseDetails = "'" + jqcl_qjzc_txt.Text + "','','" + jqcl_md_txt.Text + "','','','" + jqcl_zz_txt.Text + "','','" + parameter1.daogao.ToString()
                                                  + "','" + parameter1.lachuzhi.ToString() + "','" +parameter2.daogao.ToString() + "','" + parameter2.lachuzhi.ToString() + "','"
                                                  + parameter.cemianxianjie.ToString() + "','" + parameter1.pdz.ToString() + "','','" + parameter1.gaocha + "','','" + parameter.chaogao.ToString()
                                                  + "','" + parameter.guiju.ToString() + "'";
                            database_operate.Add(databaseDetails);
                        }
                        else
                        {
                            MessageBox.Show("请输入区间站场、锚段、支柱号！");
                        }
                    }

                    parameter1.clEnd_flag = 0;
                    parameter2.clEnd_flag = 0;
                    founctioncode = 0;
                }
            }
            else
            {
                ack_timer3.Enabled = false;
                //MessageBox.Show(readBytes[0].ToString() + " " + readBytes[1].ToString() + " " + readBytes[2].ToString() + " " + readBytes[3].ToString() + " " + readBytes[4].ToString() + " " + readBytes[5].ToString() + " " + readBytes[6].ToString() + " " + readBytes[7].ToString() + " " + readBytes[8].ToString() + " " + readBytes[9].ToString() + " " + readBytes[10].ToString() + " " + readBytes[11].ToString() + " " + readBytes[12].ToString() + " " + readBytes[13].ToString() + " " + readBytes[14].ToString() + " " + readBytes[15].ToString() + " " + readBytes[16].ToString() + " " + readBytes[17].ToString() + " " + readBytes[18].ToString() + " " + readBytes[19].ToString() + " " + readBytes[20].ToString() + " " + readBytes[21].ToString() + " " + readBytes[22].ToString() + " " + readBytes[23].ToString() + " " + readBytes[24].ToString());

                // MessageBox.Show(founctioncode.ToString());
                Console.WriteLine(readBytes.Length.ToString());
                Console.WriteLine("传输数据出错！");

                //button13.Enabled = true;
                //jqcl_cl_2.Enabled = true;
                //jqcl_cl_1.Enabled = true;
                //jqcl_cl_bto.Enabled = true;
                //jbcl_kjql_bto.Enabled = true;
                //l_bto.Enabled = true;
                //r_bto.Enabled = true;
                //button10.Enabled = true;
                //jg_bto.Enabled = true;

                //标志位复位
                parameter1.celiang_flag = 0;
                parameter2.celiang_flag = 0;
                parameter.celiang_flag = 0;
                parameter1.clEnd_flag = 0;
                parameter1.clEnd_flag = 0;
                parameter1.customAngel_flag = 0;
                left_flag = 1;
                right_flag = 1;
                founctioncode = 0;
                jbVerify_celiang = 0;
                cgVerify_celiang = 0;

            }

        }

        private void button9_Click(object sender, EventArgs e)
        {
            clts_lab.Text = "请开始第一次测量！";
            cgVerify_celiang = 1;
            jbVerify_celiang = 0;
        }

        private void button3_MouseDown(object sender, MouseEventArgs e)
        {
            timer2.Enabled = true;
            timer2.Interval = 10;
            move = 2;
        }

        private void button3_MouseUp(object sender, MouseEventArgs e)
        {
            timer2.Enabled = false;
        }

        public static void Delay(int milliSecond)
        {
            int start = Environment.TickCount;
            while (Math.Abs(Environment.TickCount - start) < milliSecond)
            {
                Application.DoEvents();
            }
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


