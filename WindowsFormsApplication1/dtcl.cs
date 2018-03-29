using SpeechLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApplication1
{
    public partial class dtcl : Form
    {
        Series m;
        Series n;
        Series o;
        shujuku database_perate = new shujuku();
        private IPAddress connectServer;
        private string ipAddr = "192.168.0.1", portNum = "2111";
        byte close_flag = 1, change_flag=1, start_flag = 0;//start_flag:刚开始运行标志
        private IPEndPoint iep;
        private Socket server;
        private ManualResetEvent timeoutObject;
        private bool isConn;
        private Thread thread_creat_server; byte[] revBytes = new byte[1024 * 10];
        byte[] revByte = new byte[1024];
        private AllParameter parameter = new AllParameter();
        private AllParameter parameter2 = new AllParameter();
        private float angel;
        private static byte[] singleData = { 0x02, 0x73, 0x52, 0x4E, 0x20, 0x4C, 0x4D, 0x44, 0x73, 0x63, 0x61, 0x6E, 0x64, 0x61, 0x74, 0x61, 0x03 };//单次取数据指令

        //精测板指令
        private byte[] command1 = { 0XEE, 0XEE, 0X01, 0XFC, 0XFC };//绝对编码器角度置零指令
        private byte[] command2 = { 0XEE, 0XEE, 0X02, 0X00, 0X05, 0XFC, 0XFC };//电机正转5度
        private byte[] command3 = { 0XEE, 0XEE, 0X02, 0X01, 0X05, 0XFC, 0XFC };//电机反转5度
        private byte[] command4 = { 0XEE, 0XEE, 0X03, 0X00, 0XFC, 0XFC };//电机正转0.1度
        private byte[] command5 = { 0XEE, 0XEE, 0X03, 0X01, 0XFC, 0XFC };//电机反转0.1度
        private byte[] command6 = { 0XEE, 0XEE, 0X04, 0XFC, 0XFC };//获取拉出值、导高、轨距、超高、侧面限界5个数据
        private byte[] command7 = { 0XEE, 0XEE, 0X05, 0XFC, 0XFC };//获取增量编码器距离值、绝对值编码器角度值、激光测距距离值、导轨距离值、倾角角度值
        private byte[] command8 = { 0XEE, 0XEE, 0X06, 0XFC, 0XFC };//增量编码器距离数据置零指令
        private byte[] command9 = { 0XEE, 0XEE, 0X08, 0XFC, 0XFC };//开始测量指令
        private byte[] command10 = { 0XEE, 0XEE, 0X09, 0XFC, 0XFC };//停止测量指令

        private byte[] command_readRealV = { 0XEE, 0XEE, 0X0C, 0X01, 0XFC, 0XFC };//读取实时值
        private byte[] command_closeRealV = { 0XEE, 0XEE, 0X0C, 0X00, 0XFC, 0XFC };//关闭读取实时值


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
        ///////////////////////
        private Queue<double> dataQueue1 = new Queue<double>(20);

        private Queue<double> dataQueue2 = new Queue<double>(20);

        private Queue<double> dataQueue3 = new Queue<double>(20);

        private Queue<double> dataQueue4 = new Queue<double>(20);

        private int curValue = 0, pointNum;

        private int num = 1, founctioncode, ack = 0, overtime_flag;
        ///////////////////////数据测试

        private static int id_num = 0;

        private static double last_licheng_value = 0;

        public dtcl()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        int t_i = 0;
        int cmxj = 0;
        void Thread_Server_Listen(object msgg)
        {
            /*lineCount:线的根数；  daogao_index：导高1与导高2平均索引； daogao1_index：daogao1索引； daogao2_index：daogao2索引*/
            int i, minPoint, angel_flag=55555, maxPoint, lineCount = 0, daogao_index=0, daogao1_index=0, daogao2_index=0, time = 0;
            float daogao_min;
            if (set_Wrap_Dg_U_text.Text != "")
            {
                minPoint = Convert.ToInt32(set_Wrap_Dg_U_text.Text);//导高下限
            }
            else minPoint = 5100;
            if (set_Wrap_Dg_D_text.Text != "")
            {
                maxPoint = Convert.ToInt32(set_Wrap_Dg_D_text.Text);//导高上限
            }
            else
                maxPoint = 6500;

            kscl_bto.Enabled = false;
            tzcl_bto.Enabled = true;
            bctp_bto.Enabled = true;
            bcbx_bto.Enabled = true;

            while (true)
            {
                if(last_licheng_value <= 0)
                {
                    last_licheng_value = parameter.zengliang + 1000;
                }

                //if(last_licheng_value <= parameter.zengliang) { 
                if (true) { 
                

                    // Thread.Sleep(3000);
                    try
                    {
                        server.Send(singleData);
                    }
                    catch
                    {
                        if (serialPort1.IsOpen)
                        {
                            founctioncode = 9;
                            ack_timer.Enabled = true;
                            ack_timer.Interval = 5000;
                            ack = 1;
                            serialPort1.Write(command10, 0, command10.Length);//停止测量
                        }
                        tzcl_bto.Enabled = false;
                        kscl_bto.Enabled = true;
                        MessageBox.Show("请检查服务器是否连接！");
                        return;
                    }

                    minPoint = 6500;
                    int rlen = server.Receive(revBytes, SocketFlags.None);
                    string msg = Encoding.ASCII.GetString(revBytes, 1, rlen - 2);
                    string[] sArray = Regex.Split(msg, " ", RegexOptions.IgnoreCase);
                    float angle_resolution = 0f;


                    /*数据描点*/         /*sArray[23]扫描开始角度；sArray[24]为角度分辨率；sArray[25]为扫描点个数*/
                    if (sArray[0] == "sRA")
                    {
                        if (rlen != 0)
                        {
                            //if (sArray[24] == "1388")
                            //{
                            //    eAngel = (double)Convert.ToInt32(sArray[23], 16) / 10000 + (Convert.ToInt32(sArray[25], 16) - 1) / 2;
                            //    //drawP.draw5_185_base(myPen, sAngel, eAngel);
                            //    //for (int i = 26; i < 26 + Convert.ToInt32(sArray[25], 16); i++)
                            //    //{
                            //    //    drawP.drawAPoint((int)(scaling * Convert.ToInt32(sArray[i], 16)), sAngel + 0.5 * (i - 26));
                            //    //}
                            //}
                            //if (sArray[24] == "9C4")
                            //{
                            //    eAngel = (double)Convert.ToInt32(sArray[23], 16) / 10000 + (Convert.ToInt32(sArray[25], 16) - 1) / 4;
                            //    //drawP.draw5_185_base(myPen, sAngel, eAngel);
                            //    //for (int i = 26; i < 26 + Convert.ToInt32(sArray[25], 16); i++)
                            //    //{
                            //    //    drawP.drawAPoint((int)(scaling * Convert.ToInt32(sArray[i], 16)), sAngel + 0.25 * (i - 26));
                            //    //}
                            //}

                            ///* 向数据库中保存的数据 */
                            //string waitSaveStr = "";

                            //for (int ii = 26; ii < 26 + Convert.ToInt32(sArray[25], 16); ii++)
                            //{
                            //    waitSaveStr += Convert.ToInt32(sArray[ii], 16).ToString() + " ";
                            //}
                            ////database_perate.Add(++id_num, waitSaveStr);



                            angel = (float)Convert.ToInt32(sArray[23], 16) / 10000;
                            float sAngel = (float)Convert.ToInt32(sArray[23], 16) / 10000;
                            if (sArray[24] == "1388")
                            {
                                angel = angel + angel_flag / 2;
                                angle_resolution = 0.5f;
                            }
                            else if (sArray[24] == "9C4")
                            {
                                angel = angel + angel_flag / 4;
                                angle_resolution = 0.25f;
                            }
                            else if (sArray[24] == "683")
                            {
                                angel = angel + angel_flag / 6;
                                angle_resolution = 0.166667f;
                            }
                            else if (sArray[24] == "D05")
                            {
                                angel = angel + angel_flag / 3;
                                angle_resolution = 0.333333f;
                            }


                            /* processing data */
                            filterAlgForRadarData.filterRadarData(sArray, angle_resolution);


                            Console.WriteLine("一共有" + filterAlgForRadarData.list_final_points_end.Count.ToString() + "个点。。");

                            for (int kk = 0; kk < filterAlgForRadarData.list_final_points_end.Count; kk++)
                            {
                                Console.WriteLine("第" + kk + "个点为：" + filterAlgForRadarData.list_final_points_end[kk].ToString() + ", " + (sAngel + angle_resolution * (filterAlgForRadarData.list_final_points_angle_end[kk])));

                                //filterAlgForRadarData.list_final_points_angle_end_last[kk] = filterAlgForRadarData.list_final_points_angle_end[kk] * angle_resolution;
                                //filterAlgForRadarData.list_final_pionts_curlcz_end[kk] = parameter.LMSxx_X1 + parameter.LMSxx_X2 + filterAlgForRadarData.list_final_points_end[kk] * (float)Math.Cos(angel * parameter.OneDegree) - 1435 / 2;
                            }
                            Console.WriteLine("");

                            cmxj = filterAlgForRadarData.getCMXJ_DATA(sArray, 0); //侧面限界

                            Scrollbar_right(t_i, 200);     //最大显示200个轴
                            dtcl_Gxsj(t_i, filterAlgForRadarData.list_final_points_end, filterAlgForRadarData.list_final_pionts_curlcz_end, cmxj);
                            t_i++;
                            Thread.Sleep(1000);




                            if (filterAlgForRadarData.list_final_points_end.Count == 1)
                            {
                                dg1_lab.Text = filterAlgForRadarData.list_final_points_end[0].ToString() + parameter.danwei;
                                //float lcz1 = filterAlgForRadarData.list_final_pionts_curlcz_end[0];
                                lcz1_lab.Text = filterAlgForRadarData.list_final_pionts_curlcz_end[0].ToString() + parameter.danwei;
                                lcz2_lab.Text = "0.00 mm";
                            }
                            else if (filterAlgForRadarData.list_final_points_end.Count == 2)
                            {
                                dg1_lab.Text = filterAlgForRadarData.list_final_points_end[0].ToString() + parameter.danwei;
                                dg2_lab.Text = filterAlgForRadarData.list_final_points_end[1].ToString() + parameter.danwei;
                                lcz1_lab.Text = filterAlgForRadarData.list_final_pionts_curlcz_end[0].ToString() + parameter.danwei;
                                lcz2_lab.Text = filterAlgForRadarData.list_final_pionts_curlcz_end[1].ToString() + parameter.danwei;
                            }

                            //lcz1_lab.Text = parameter.lachuzhi.ToString() + parameter.danwei;
                            //xj_lab.Text = parameter.cemianxianjie.ToString();
                            ///*语音播报*/
                            //if(yuyinbobao_kai.Checked == true)
                            //{
                            //    SpeechVoiceSpeakFlags flag = SpeechVoiceSpeakFlags.SVSFlagsAsync;
                            //    SpVoice voice = new SpVoice();
                            //    voice.Voice = voice.GetVoices(string.Empty, string.Empty).Item(0);
                            //    string speak = gc_jj_lab.Text + "毫米";
                            //    voice.Speak(speak, flag);
                            //}

                        }
                        else
                        {
                            if (serialPort1.IsOpen)
                            {
                                founctioncode = 9;
                                ack_timer.Enabled = true;
                                ack_timer.Interval = 5000;
                                ack = 1;
                                serialPort1.Write(command10, 0, command10.Length);//停止测量
                            }
                            tzcl_bto.Enabled = false;
                            kscl_bto.Enabled = true;
                            this.timer1.Stop();

                            if (thread_creat_server != null)
                            {
                                thread_creat_server.Abort();
                                thread_creat_server.Join();
                            }
                            MessageBox.Show("雷达异常！");
                        }

                    }

                    last_licheng_value = parameter.zengliang + 1000;
                }

                gj_lab.Text = parameter.guiju + parameter.danwei;
                cg_lab.Text = (Math.Round((parameter.guiju + 70) * (float)Math.Sin(parameter.qingjiao), 1)).ToString() + parameter.danwei;
                parameter.zengliang = Math.Round(parameter.zengliang, 2);//保留2位小数

                /* 实时里程显示 */
                if (qsglb_txt1.Text != "" && qsglb_txt2.Text != "")
                {
                    double km, m;

                    km = Convert.ToInt64(qsglb_txt1.Text);
                    km += (int)parameter.zengliang / 1000000;

                    m = Convert.ToInt64(qsglb_txt2.Text);
                    m += (parameter.zengliang - (int)(parameter.zengliang / 1000000) * 1000000) / 1000;
                    km += m / 1000;
                    m = m % 1000;
                    km = Convert.ToInt32(km - 0.5); /* 这里不能四舍五入 */
                    m = Math.Round(m, 2);
                    lc_lab.Text = "K " + km.ToString() + " + " + m.ToString() + " m";
                }


                //    parameter.command6Ack = 0;//返回标志位置零
                //    parameter.command7Ack = 0;
                //}
            }
        }
    /*从文件读出数据*/
        //private static string fileToString()
        //{
        //    string str = "";

        //    //获取文件内容  
        //    if (System.IO.File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"ipAddr\ipAddr(重要资料请勿修改).txt"))
        //    {
        //        System.IO.StreamReader file1 = new System.IO.StreamReader(System.AppDomain.CurrentDomain.BaseDirectory + @"ipAddr\ipAddr(重要资料请勿修改).txt");//读取文件中的数据  
        //        str = file1.ReadToEnd();                                            //读取文件中的全部数据  

        //        file1.Close();
        //        file1.Dispose();
        //    }
        //    return str;
        //}

        private void dtcl_Load(object sender, EventArgs e)
        {
            tzcl_bto.Enabled = false;
            kscl_bto.Enabled = true;
            bctp_bto.Enabled = false;
            bcbx_bto.Enabled = false;

            UpdateQueueValue_chart1();
            UpdateQueueValue_chart2();
            UpdateQueueValue_chart3();
            jingjiexian_Init();

            /*配置动态测量的数据设置*/
            dtcl_cssz(0, set_Display_Dg_U_text.Text, set_Display_Dg_D_text.Text);
            dtcl_cssz(1, set_Display_Lcz_U_text.Text, set_Display_Lcz_D_text.Text);
            dtcl_cssz(2, set_Display_Gc_U_text.Text, set_Display_Gc_D_text.Text);

            if (!Directory.Exists("c:\\动态测量图片"))//创建照片文件夹
            {
                Directory.CreateDirectory("c:\\动态测量图片");
            }
            if (!Directory.Exists("c:\\波形图片"))//创建照片文件夹
            {
                Directory.CreateDirectory("c:\\波形图片");
            }

            /*读取机械参数*/
            try
            {
                string[] parameters = parameter.fileToString("LMSxxparameter(重要资料，请勿删改！！！).txt").Split(' ');

                parameter.LMSxx_X1 = Convert.ToSingle(parameters[0]);
                parameter.LMSxx_X2 = Convert.ToSingle(parameters[1]);
                parameter.LMSxx_Y1 = Convert.ToSingle(parameters[2]);
                parameter.LMSxx_Y2 = Convert.ToSingle(parameters[3]);

                filterAlgForRadarData.parameter.LMSxx_X1 = parameter.LMSxx_X1;
                filterAlgForRadarData.parameter.LMSxx_X2 = parameter.LMSxx_X2;
                filterAlgForRadarData.parameter.LMSxx_Y1 = parameter.LMSxx_Y1;
                filterAlgForRadarData.parameter.LMSxx_Y2 = parameter.LMSxx_Y2;

                /* 雷达滤波角度范围值读取 */
                filterAlgForRadarData.startAngle_int = Convert.ToInt32(parameters[4]);
                filterAlgForRadarData.allAngle_size = Convert.ToInt32(parameters[5]) - filterAlgForRadarData.startAngle_int;

                string[] dtcl_cssz_str = parameter.fileToString("dtcl_cssz.txt").Split(' ');

                if (dtcl_cssz_str.Count() == 16)
                {
                    ParamDTCL.dtcl_lczzb_L = Convert.ToInt32(dtcl_cssz_str[0]);
                    ParamDTCL.dtcl_lczzb_H = Convert.ToInt32(dtcl_cssz_str[1]);

                    ParamDTCL.dtcl_dgzb_L = Convert.ToInt32(dtcl_cssz_str[2]);
                    ParamDTCL.dtcl_dgzzb_H = Convert.ToInt32(dtcl_cssz_str[3]);

                    ParamDTCL.dtcl_cmxjzb_L = Convert.ToInt32(dtcl_cssz_str[4]);
                    ParamDTCL.dtcl_cmxjzb_H = Convert.ToInt32(dtcl_cssz_str[5]);

                    ParamDTCL.dtcl_lczb_L = Convert.ToInt32(dtcl_cssz_str[6]);
                    ParamDTCL.dtcl_lczb_H = Convert.ToInt32(dtcl_cssz_str[7]);

                    /* 拉出值超限参数范围 */
                    ParamDTCL.dtcl_lczcx_L = Convert.ToInt32(dtcl_cssz_str[8]);
                    ParamDTCL.dtcl_lczcx_H = Convert.ToInt32(dtcl_cssz_str[9]);

                    /* 导高超限参数上下限 */
                    ParamDTCL.dtcl_dgcx_L = Convert.ToInt32(dtcl_cssz_str[10]);
                    ParamDTCL.dtcl_dgcxb_H = Convert.ToInt32(dtcl_cssz_str[11]);

                    /* 侧面界线参数上下限 */
                    ParamDTCL.dtcl_cmxjcx_L = Convert.ToInt32(dtcl_cssz_str[12]);
                    ParamDTCL.dtcl_cmxjcx_H = Convert.ToInt32(dtcl_cssz_str[13]);

                    /* 坡度上下限 */
                    ParamDTCL.dtcl_pdcx_L = Convert.ToInt32(dtcl_cssz_str[14]);
                    ParamDTCL.dtcl_pdcx_H = Convert.ToInt32(dtcl_cssz_str[15]);
                }
            }
            catch
            {
                MessageBox.Show("参数读取失败！");
            }

            /*串口*/
            try
            {
                string[] names = SerialPort.GetPortNames();//查询串口名字
                                                           //MessageBox.Show(names.Length.ToString());
                string parameters = parameter.fileToString("serialPortName(重要资料，请勿删改！！！).txt");

                btl_cbo.SelectedIndex = 4;

                if (names.Length > 0)
                {
                    if (parameters != "")//如果已保存串口号
                    {
                        foreach (string device in names)
                        {
                            if (device == parameters)
                            {
                                serialPort1.PortName = device;
                                // serialPort1.BaudRate = Convert.ToInt32(btl_cbo.Text);
                                serialPort1.BaudRate = 115200;
                                serialPort1.DataBits = 8;
                                serialPort1.Parity = Parity.None;
                                serialPort1.StopBits = StopBits.One;

                                serialPort1.Open();
                                founctioncode = 9;
                                serialPort1.Write(command10, 0, command10.Length);//增量编码器置零
                                Thread.Sleep(1000);
                                break;
                            }
                        }
                    }

                    foreach (string device in names)
                    {
                        if (start_flag == 0)
                        {
                            serialPort1.Close();

                            serialPort1.PortName = device;
                            // serialPort1.BaudRate = Convert.ToInt32(btl_cbo.Text);
                            serialPort1.BaudRate = 115200;
                            serialPort1.DataBits = 8;
                            serialPort1.Parity = Parity.None;
                            serialPort1.StopBits = StopBits.One;

                            serialPort1.Open();

                            founctioncode = 9;
                            serialPort1.Write(command10, 0, command10.Length);//增量编码器置零
                            Thread.Sleep(1000);

                        }
                        port_cbo.Items.Add(device);
                    }

                    if (start_flag == 0)
                    {
                        Application.Exit();
                    }

                    if (parameters == "" || parameters != serialPort1.PortName)//如果串口号未保存或已更改
                    {
                        parameter.SaveProcess("serialPortName(重要资料，请勿删改！！！).txt", serialPort1.PortName);
                    }
                    port_cbo.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("没有搜索到串口设备！！");
                    //Application.Exit();
                }

            }
            catch
            {
                MessageBox.Show("打开串口失败！！");
                Application.Exit();
            }

            change_flag = 0;


            /* delete the lists */
            label26.Visible = false;
            port_cbo.Visible = false;
            label27.Visible = false;
            btl_cbo.Visible = false;
        }

        private void dtcl_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (close_flag == 1)
            {
                if (serialPort1.IsOpen)
                {
                    founctioncode = 9;
                    ack_timer.Enabled = true;
                    ack_timer.Interval = 5000;
                    ack = 1;
                    serialPort1.Write(command10, 0, command10.Length);//停止测量
                    serialPort1.Close();
                }
              
                if (thread_creat_server != null)
                {
                    thread_creat_server.Abort();
                    thread_creat_server.Join();
                }
                Application.Exit();
            }
            if (thread_creat_server != null && thread_creat_server.IsAlive)
            {
                thread_creat_server.Abort();
                thread_creat_server.Join();
            }
        }

        private void dtcl_FormClosing(object sender, FormClosingEventArgs e)
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
                        e.Cancel = false;  //点击OK 
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        private void fh_bto_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                //founctioncode = 9;
                //ack_timer.Enabled = true;
                //ack_timer.Interval = 5000;
                //ack = 1;
                serialPort1.Write(command10, 0, command10.Length);//停止测量
                serialPort1.Close();
            }
            close_flag = 0;
            this.Close();
            new Form1().Show();  
        }

        private void bctp_bto_Click(object sender, EventArgs e)
        {
            Save_dtcl_pictures();
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

        private void CallBackMethod(IAsyncResult asyncresult)
        {
            try
            {
                server = asyncresult.AsyncState as Socket;

                if (server != null)
                {
                    server.EndConnect(asyncresult);
                    isConn = true;
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.StackTrace.ToString());
                isConn = false;
            }
            finally
            {
                timeoutObject.Set();
            }
        }
        private void kscl_bto_Click(object sender, EventArgs e)
        {
            //return_jqcl.Enabled = false;
            if(qsglb_txt1.Text == "" || qsglb_txt2.Text == "")
            {
                MessageBox.Show("请先输入起始公里标！");
                return;
            }

            qsglb_txt1.Enabled = false;
            qsglb_txt2.Enabled = false;

            connectServer = IPAddress.Parse(ipAddr);
            iep = new IPEndPoint(connectServer, Convert.ToInt32(portNum));
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            timeoutObject = new ManualResetEvent(false);

            server.BeginConnect(connectServer, Convert.ToInt32(portNum), new AsyncCallback(CallBackMethod), server);
            if (!timeoutObject.WaitOne(1000, false))
            {
                server.Close();
                MessageBox.Show("雷达连接超时！");
                return;
            }

            //if (serialPort1.IsOpen)
            //{
            //    //founctioncode = 8;
            //    //ack_timer.Enabled = true;
            //    //ack_timer.Interval = 5000;
            //    //ack = 1;
            //    serialPort1.Write(command9, 0, command9.Length);//开始测量
            //}
            //else
            //{
            //    if (server != null)
            //        server.Close();
            //    MessageBox.Show("请先打开串口！");
            //    return;
            //    }

                // xj_lab.Text = parameter.cemianxianjie.ToString(); //侧面限界
                this.timer1.Start();
            thread_creat_server = new Thread(Thread_Server_Listen);
            //lc_lab.Text = parameter.zengliang.ToString();
            //dg1_lab.Text = parameter.qingjiao.ToString();
            thread_creat_server.Start();
        }

        ////////////////////////////////////////////
        //测试数据
        private void UpdateQueueValue_chart2()
        {
            Random r = new Random();
            for (int i = 0; i < num; i++)
            {
                dataQueue2.Enqueue(r.Next(-5, 5));
            }


        }

        private void UpdateQueueValue_chart1()
        {
            Random r = new Random();
            for (int i = 0; i < num; i++)
            {
                //对curValue只取[0,360]之间的值
                curValue = curValue % 360;
                //对得到的正玄值，放大50倍，并上移50
                dataQueue1.Enqueue((5 * Math.Sin(curValue * Math.PI / 180)) + 5);
                dataQueue4.Enqueue(r.Next(0, 10));
                curValue = curValue + 10;
            }
        }

        private void UpdateQueueValue_chart3()//更新队列值
        {
            Random r = new Random();
            for (int i = 0; i < num; i++)
            {
                dataQueue3.Enqueue(r.Next(0, 10));
            }
        }
        /////////////////////////////////////////////// 

       
        private void timer1_Tick(object sender, EventArgs e)
        {
            //float[] data = new float[2];
            //data[1] = 4000 + t_i * 50;
            //data[0] = 4000 + t_i * 100;
            //Scrollbar_right(t_i, 200);     //最大显示200个轴
            //dtcl_Gxsj(t_i, data);
            //if (t_i % 5 == 0)
            //{
            //    chart1.Series[0].Points[t_i].IsValueShownAsLabel = true;
            //    chart1.Series[1].Points[t_i].IsValueShownAsLabel = true;
            //    chart1.Series[2].Points[t_i].IsValueShownAsLabel = true;
            //    chart1.Series[3].Points[t_i].IsValueShownAsLabel = true;

            //    chart1.Series[0].Points[t_i].MarkerStyle = MarkerStyle.Circle;
            //    chart1.Series[1].Points[t_i].MarkerStyle = MarkerStyle.Circle;
            //    chart1.Series[2].Points[t_i].MarkerStyle = MarkerStyle.Circle;

            //    //chart1.ChartAreas[0].AxisX.CustomLabels[t_i].Text = (t_i / 5).ToString();
            //}
            //else
            //{
            //    chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = true;
            //}
            //t_i++;
        }

        //滑动块跟随点实时更新
        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="m">最大显示轴数</param>
        private void Scrollbar_right(int n, int m)
        {
            if (n > (m - 2))
            {
                this.chart1.ChartAreas[0].AxisX.ScaleView.Position = n - (m - 2);
                this.chart1.ChartAreas[1].AxisX.ScaleView.Position = n - (m - 2);
                this.chart1.ChartAreas[2].AxisX.ScaleView.Position = n - (m - 2);
            }
            if (n == m)
            {
                this.chart1.ChartAreas[0].AxisX.ScaleView.Size = m - 1;
                this.chart1.ChartAreas[1].AxisX.ScaleView.Size = m - 1;
                this.chart1.ChartAreas[2].AxisX.ScaleView.Size = m - 1;
            }
            
        }

        private void tzcl_bto_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                founctioncode = 9;
                ack_timer.Enabled = true;
                ack_timer.Interval = 5000;
                ack = 1;
                serialPort1.Write(command10, 0, command10.Length);//停止测量
            }

            if (thread_creat_server != null)
            {
                thread_creat_server.Abort();
                thread_creat_server.Join();
            }
            this.timer1.Stop();
            tzcl_bto.Enabled = false;
            kscl_bto.Enabled = true;

            qsglb_txt1.Enabled = true;
            qsglb_txt2.Enabled = true;
            //bctp_bto.Enabled = false;
            //bcbx_bto.Enabled = false;
        }

        /// <summary>
        /// 保存dtcl图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_dtcl_pictures()
        {
            Graphics g1 = dtcl_Cssz_pne.CreateGraphics();
            Image myImage = new Bitmap(this.dtcl_Cssz_pne.Width, this.dtcl_Cssz_pne.Height, g1);
            Graphics g2 = Graphics.FromImage(myImage);
            IntPtr dc1 = g1.GetHdc();
            IntPtr dc2 = g2.GetHdc();
            BitBlt(dc2, 0, 0, this.dtcl_Cssz_pne.Width, this.dtcl_Cssz_pne.Height, dc1, 0, 0, 13369376);
            g1.ReleaseHdc(dc1);
            g2.ReleaseHdc(dc2);
            // myImage.Save(@"d:\1.bmp", ImageFormat.Bmp);
            myImage.Save("C:\\动态测量图片\\" + $"{System.DateTime.Now.ToString("yyyy - MM - dd HH：m：ss")}.jpg");
        }

        private void Save_chart_pictures()
        {
            Graphics g1 = chart1.CreateGraphics();
            Image myImage = new Bitmap(this.chart1.Width, this.chart1.Height, g1);
            Graphics g2 = Graphics.FromImage(myImage);
            IntPtr dc1 = g1.GetHdc();
            IntPtr dc2 = g2.GetHdc();
            BitBlt(dc2, 0, 0, this.chart1.Width, this.chart1.Height, dc1, 0, 0, 13369376);
            g1.ReleaseHdc(dc1);
            g2.ReleaseHdc(dc2);
            // myImage.Save(@"d:\1.bmp", ImageFormat.Bmp);
            myImage.Save("C:\\波形图片\\" + $"{System.DateTime.Now.ToString("yyyy - MM - dd HH：m：ss")}.jpg");
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

        private void port_cbo_TextChanged(object sender, EventArgs e)
        {
            if(change_flag == 0)
            {

                try
                {
                    serialPort1.Close();

                    serialPort1.PortName = port_cbo.Text;//串口参数设置

                    serialPort1.BaudRate = Convert.ToInt32(btl_cbo.Text);
                    serialPort1.DataBits = 8;
                    serialPort1.Parity = Parity.None;
                    serialPort1.StopBits = StopBits.One;

                    serialPort1.Open();

                }
                catch
                {
                    MessageBox.Show("串口分身失败，请关掉占用该串口的软件", "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }
            }
        }

        private void btl_cbo_TextChanged(object sender, EventArgs e)
        {
            if (change_flag == 0)
            {
                try
                {
                    serialPort1.Close();

                    serialPort1.PortName = port_cbo.Text;//串口参数设置

                    serialPort1.BaudRate = Convert.ToInt32(btl_cbo.Text);
                    serialPort1.DataBits = 8;
                    serialPort1.Parity = Parity.None;
                    serialPort1.StopBits = StopBits.One;
                    serialPort1.Open();
                }
                catch
                {
                    MessageBox.Show("串口分身失败，请关掉占用该串口的软件", "ERROR", MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);                   
                    return;
                }
            }
        }

        private void ack_timer_Tick(object sender, EventArgs e)
        {
            if (ack == 1)
            {
                ack_timer.Enabled = false;
                if(server != null)
                server.Close();
                MessageBox.Show("消息接收超时！");
                founctioncode = 0;
                ack = 0;
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
                            change_flag = 1;
                            // MessageBox.Show(names.Length.ToString());
                            try
                            {
                                if (names.Length > 0)
                                {
                                    port_cbo.Items.Clear();
                                    foreach (string device in names)
                                    {
                                        port_cbo.Items.Add(device);
                                    }

                                    for (i = 0; i < names.Length; i++)
                                    {
                                        if (serialPort1.PortName == names[i])
                                        {
                                        
                                            if (serialPort1.IsOpen)//如果串口未打开则打开串口
                                            {
                                                port_cbo.Text = serialPort1.PortName;
                                            }
                                            else
                                            {
                                                serialPort1.Open();
                                                port_cbo.Text = serialPort1.PortName;

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
                                    serialPort1.Close();
                                    port_cbo.Items.Clear();
                                }
                            }
                            catch//没有发现串口
                            {
                                if (thread_creat_server != null)
                                {
                                    thread_creat_server.Abort();
                                    thread_creat_server.Join();

                                    this.timer1.Stop();
                                    tzcl_bto.Enabled = false;
                                    kscl_bto.Enabled = true;
                                }
                                serialPort1.Close();
                                //chooseport.DropDownItems.Clear();
                                MessageBox.Show("有可能是因为拔出正在使用的串口导致程序出错，请重新打开软件！！");
                            }
                            change_flag = 0;
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

        private void set_Bcsz_bto_Click(object sender, EventArgs e)
        {
            dtcl_cssz(0, set_Display_Dg_U_text.Text, set_Display_Dg_D_text.Text);
            dtcl_cssz(1, set_Display_Lcz_U_text.Text, set_Display_Lcz_D_text.Text);
            dtcl_cssz(2, set_Display_Gc_U_text.Text, set_Display_Gc_D_text.Text);


            try
            {
                ParamDTCL.dtcl_lczzb_L = Convert.ToInt32(set_Display_Lcz_U_text.Text);
                ParamDTCL.dtcl_lczzb_H = Convert.ToInt32(set_Display_Lcz_D_text.Text);

                ParamDTCL.dtcl_dgzb_L = Convert.ToInt32(set_Display_Dg_U_text.Text);
                ParamDTCL.dtcl_dgzzb_H = Convert.ToInt32(set_Display_Dg_D_text.Text);

                ParamDTCL.dtcl_cmxjzb_L = Convert.ToInt32(set_Display_Gc_U_text.Text);
                ParamDTCL.dtcl_cmxjzb_H = Convert.ToInt32(set_Display_Gc_D_text.Text);

                ParamDTCL.dtcl_lczb_L = Convert.ToInt32(set_Display_Lc_U_text.Text);
                ParamDTCL.dtcl_lczb_H = Convert.ToInt32(set_Display_Lc_D_text.Text);

                /* 拉出值超限参数范围 */
                ParamDTCL.dtcl_lczcx_L = Convert.ToInt32(set_Wrap_Lcz_U_text.Text);
                ParamDTCL.dtcl_lczcx_H = Convert.ToInt32(set_Wrap_Lcz_D_text.Text);

                /* 导高超限参数上下限 */
                ParamDTCL.dtcl_dgcx_L = Convert.ToInt32(set_Wrap_Dg_U_text.Text);
                ParamDTCL.dtcl_dgcxb_H = Convert.ToInt32(set_Wrap_Dg_D_text.Text);

                /* 侧面界线参数上下限 */
                ParamDTCL.dtcl_cmxjcx_L = Convert.ToInt32(set_Wrap_xj_U_text.Text);
                ParamDTCL.dtcl_cmxjcx_H = Convert.ToInt32(set_Wrap_xj_D_text.Text);

                /* 坡度上下限 */
                ParamDTCL.dtcl_pdcx_L = Convert.ToInt32(set_Wrap_Pd_U_text.Text);
                ParamDTCL.dtcl_pdcx_H = Convert.ToInt32(set_Wrap_Pd_D_text.Text);
            }
            catch
            {
                MessageBox.Show("文本框中不得输入除数字之外的其他字符！");
                return;
            }


            try
            {
                string saveStringParam = ParamDTCL.dtcl_lczzb_L.ToString() + " " +
                                        ParamDTCL.dtcl_lczzb_H.ToString() + " " +
                                        ParamDTCL.dtcl_dgzb_L.ToString() + " " +
                                        ParamDTCL.dtcl_dgzzb_H.ToString() + " " +
                                        ParamDTCL.dtcl_cmxjzb_L.ToString() + " " +
                                        ParamDTCL.dtcl_cmxjzb_H.ToString() + " " +

                                        ParamDTCL.dtcl_lczb_L.ToString() + " " +
                                        ParamDTCL.dtcl_lczb_H.ToString() + " " +

                                        /* 拉出值超限参数范围 */
                                        ParamDTCL.dtcl_lczcx_L.ToString() + " " +
                                        ParamDTCL.dtcl_lczcx_H.ToString() + " " +

                                        /* 导高超限参数上下限 */
                                        ParamDTCL.dtcl_dgcx_L.ToString() + " " +
                                        ParamDTCL.dtcl_dgcxb_H.ToString() + " " +

                                        /* 侧面界线参数上下限 */
                                        ParamDTCL.dtcl_cmxjcx_L.ToString() + " " +
                                        ParamDTCL.dtcl_cmxjcx_H.ToString() + " " +

                                        /* 坡度上下限 */
                                        ParamDTCL.dtcl_pdcx_L.ToString() + " " +
                                        ParamDTCL.dtcl_pdcx_H.ToString();

                parameter.SaveProcess("dtcl_cssz.txt", saveStringParam);
                //MessageBox.Show("保存成功！");
            }
            catch
            {
                MessageBox.Show("保存参数错误！");
            }

            MessageBox.Show("参数保存成功！");
        }

        private void dtcl_Cssz_Bto_Click(object sender, EventArgs e)
        {
            dtcl_Cssz_pne.Visible = true;
        }

        private void set_Return_bto_Click(object sender, EventArgs e)
        {
            dtcl_Cssz_pne.Visible = false;
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
                     try {
                System.Threading.Thread.Sleep(80);
                byte[] readBytes = new byte[serialPort1.BytesToRead];
                serialPort1.Read(readBytes, 0, readBytes.Length);
                if(start_flag == 0)
                {
                    if(readBytes[2] == founctioncode)
                    {
                        if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X09 && readBytes[3] == 0XFC && readBytes[4] == 0XFC)
                        {
                            start_flag = 1;
                            founctioncode = 0;
                            return;
                        }
                    }
                    founctioncode = 0;
                    return;
                }
                //MessageBox.Show("数据长度出错！");
                //if (readBytes.Length < 3)
                //{
                //    ack_timer.Enabled = false;
                //    MessageBox.Show("数据长度出错！" + readBytes.Length.ToString());
                //    return;
                //}


                if (readBytes.Length == 21)
                {
                    if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X08 && readBytes[19] == 0XFC && readBytes[20] == 0XFC)//获取数据指令1反馈
                    {
                        Console.WriteLine("__---_______------");
                        /*计算轨距*/
                        parameter.guiju = BitConverter.ToSingle(readBytes, 3);

                        /*计算倾角角度值*/
                        parameter.qingjiao = BitConverter.ToSingle(readBytes, 7);
                        cg_lab.Text = (Math.Round((parameter.guiju + 70) * (float)Math.Sin(parameter.qingjiao), 1)).ToString();

                        /*计算增量编码器距离值*/
                        parameter.zengliang = BitConverter.ToDouble(readBytes, 11);

                        //lc_lab.Text = parameter.zengliang.ToString();
                        //parameter.command6Ack = 1;

                        ///*计算拉出值*/
                        //lachuzhi = BitConverter.ToSingle(readBytes, 3);

                        ///*计算导高*/
                        //daogao = BitConverter.ToSingle(readBytes, 7);

                        ///*计算轨距*/
                        //parameter.guiju = BitConverter.ToSingle(readBytes, 11);


                        ///*计算超高*/
                        //parameter.chaogao = BitConverter.ToSingle(readBytes, 15);


                        ///*计算侧面限界*/
                        //parameter.cemianxianjie = BitConverter.ToSingle(readBytes, 19);

                    }

                    //else if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X05 && readBytes[23] == 0XFC && readBytes[24] == 0XFC)//获取数据指令2反馈
                    //{
                    //    parameter.command7Ack = 1;
                    //    /*计算增量编码器距离值*/
                    //    parameter.zengliang = BitConverter.ToSingle(readBytes, 3);

                    //    /*计算绝对值编码器角度值*/
                    //    jueduizhi = BitConverter.ToSingle(readBytes, 7);

                    //    /*计算激光测距距离值*/
                    //    jiguang = BitConverter.ToSingle(readBytes, 11);

                    //    /*计算导轨距离值*/
                    //    guiju = BitConverter.ToSingle(readBytes, 15);

                    //    /*计算倾角角度值*/
                    //    parameter.qingjiao = BitConverter.ToSingle(readBytes, 19);

                    //}
                    return;
                }
                if (readBytes[2] == founctioncode)
                {                   
                    //overtime_flag = 0;
                    ack_timer.Enabled = false;
                  
                    if (readBytes.Length == 5)
                    {
                       
                        if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X06 && readBytes[3] == 0XFC && readBytes[4] == 0XFC)
                        {
                            if (qsglb_txt1.Text != "" || qsglb_txt2.Text != "")
                            {
                                lc_lab.Text = "K " + qsglb_txt1.Text + " + " + qsglb_txt2.Text + " m";
                            }
                            else
                            {
                                MessageBox.Show("请输入起始公里标！");
                            }
                            founctioncode = 0;
                            return ;
                        }

                        if (readBytes[0] == 0XEE && readBytes[1] == 0XEE && readBytes[2] == 0X09 && readBytes[3] == 0XFC && readBytes[4] == 0XFC)
                        {
                            founctioncode = 0;
                        }
                        else
                        {
                            MessageBox.Show("停止测量失败！");
                            founctioncode = 0;
                        }
                    }
                } else
                {
                    ack_timer.Enabled = false;
                    serialPort1.Close();
                    //if (readBytes.Length == 5 || readBytes.Length == 29 || readBytes.Length == 25)
                    MessageBox.Show("传输数据出错！");
                    ////overtime_flag = 0;
                   
                }

            }
            catch
            {

            }
       }

        private void set_Display_Lcz_U_text_TextChanged(object sender, EventArgs e)
        {
            if(set_Display_Lcz_U_text.Text != ""| set_Display_Lcz_U_text.Text == "-")
            {
                try {
                        Convert.ToInt32(set_Display_Lcz_U_text.Text);
                    }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Display_Lcz_U_text.Text = "";
                }
            }
        }

        private void set_Display_Lcz_D_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Display_Lcz_D_text.Text != "" && set_Display_Lcz_D_text.Text != "-")
            {
                try
                {
                    Convert.ToInt32(set_Display_Lcz_D_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Display_Lcz_D_text.Text = "";
                }
            }
            else if (set_Display_Lcz_D_text.Text == "-") {
                try
                {
                    Convert.ToInt32(set_Display_Lcz_D_text.Text);
                }
                catch { }                
            }
        }

        private void set_Display_Gc_U_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Display_Gc_U_text.Text != "" && set_Display_Gc_U_text.Text != "-")
            {
                try
                {
                    Convert.ToInt32(set_Display_Gc_U_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Display_Gc_U_text.Text = "";
                }
            }
            else if (set_Display_Gc_U_text.Text == "-") {
                try
                {
                    Convert.ToInt32(set_Display_Gc_U_text.Text);
                }
                catch { }
            }
        }

        private void set_Display_Gc_D_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Display_Gc_D_text.Text != "")
            {
                try
                {
                    Convert.ToInt32(set_Display_Gc_D_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Display_Gc_D_text.Text = "";
                }
            }
        }

        private void set_Display_Dg_U_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Display_Dg_U_text.Text != "")
            {
                try
                {
                    Convert.ToInt32(set_Display_Dg_U_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Display_Dg_U_text.Text = "";
                }
            }
        }

        private void set_Display_Dg_D_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Display_Dg_D_text.Text != "")
            {
                try
                {
                    Convert.ToInt32(set_Display_Dg_D_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Display_Dg_D_text.Text = "";
                }
            }
        }

        private void set_Display_Lc_U_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Display_Lc_U_text.Text != "")
            {
                try
                {
                    Convert.ToInt32(set_Display_Lc_U_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Display_Lc_U_text.Text = "";
                }
            }
        }

        private void set_Display_Lc_D_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Display_Lc_D_text.Text != "")
            {
                try
                {
                    Convert.ToInt32(set_Display_Lc_D_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Display_Lc_D_text.Text = "";
                }
            }
        }

        private void set_Wrap_Lcz_U_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Wrap_Lcz_U_text.Text != "")
            {
                try
                {
                    Convert.ToInt32(set_Wrap_Lcz_U_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Wrap_Lcz_U_text.Text = "";
                }
            }
        }

        private void set_Wrap_Lcz_D_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Wrap_Lcz_D_text.Text != "")
            {
                try
                {
                    Convert.ToInt32(set_Wrap_Lcz_D_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Wrap_Lcz_D_text.Text = "";
                }
            }
        }

        private void set_Wrap_xj_U_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Wrap_xj_U_text.Text != "")
            {
                try
                {
                    Convert.ToInt32(set_Wrap_xj_U_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Wrap_xj_U_text.Text = "";
                }
            }
        }

        private void set_Wrap_xj_D_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Wrap_xj_D_text.Text != "")
            {
                try
                {
                    Convert.ToInt32(set_Wrap_xj_D_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Wrap_xj_D_text.Text = "";
                }
            }
        }

        private void set_Wrap_Dg_U_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Wrap_Dg_U_text.Text != "")
            {
                try
                {
                    Convert.ToInt32(set_Wrap_Dg_U_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Wrap_Dg_U_text.Text = "";
                }
            }
        }

        private void set_Wrap_Dg_D_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Wrap_Dg_D_text.Text != "")
            {
                try
                {
                    Convert.ToInt32(set_Wrap_Dg_D_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Wrap_Dg_D_text.Text = "";
                }
            }
        }

        private void set_Wrap_Pd_U_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Wrap_Pd_U_text.Text != "")
            {
                try
                {
                    Convert.ToInt32(set_Wrap_Pd_U_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Wrap_Pd_U_text.Text = "";
                }
            }
        }

        private void set_Wrap_Pd_D_text_TextChanged(object sender, EventArgs e)
        {
            if (set_Wrap_Pd_D_text.Text != "")
            {
                try
                {
                    Convert.ToInt32(set_Wrap_Pd_D_text.Text);
                }
                catch
                {
                    MessageBox.Show("输入有误！");
                    set_Wrap_Pd_D_text.Text = "";
                }
            }
        }

        private void chart1_Click(object sender, EventArgs e)
        {
                  
        }

        private void qsglb_txt1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Convert.ToInt64(qsglb_txt1.Text);
            }
            catch
            {
                qsglb_txt1.Text = "";
                MessageBox.Show("输入有误！");
            }
        }

        private void panel16_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void qsglb_txt2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Convert.ToSingle(qsglb_txt2.Text);
            }
            catch
            {
                qsglb_txt2.Text = "";
                MessageBox.Show("输入有误！");
            }
        }

        private void bcbx_bto_Click(object sender, EventArgs e)
        {
            Save_chart_pictures();
        }

        private void dtcl_lcql_bto_Click(object sender, EventArgs e)
        {
            //if (serialPort1.IsOpen && founctioncode == 8)
            //{
            //    founctioncode = 9;
            //    ack_timer.Enabled = true;
            //    ack_timer.Interval = 5000;
            //    ack = 1;
            //    serialPort1.Write(command10, 0, command10.Length);//停止测量

            //    if (thread_creat_server != null)
            //    {
            //        thread_creat_server.Abort();
            //        thread_creat_server.Join();
            //    }
            //    this.timer1.Stop();
            //    tzcl_bto.Enabled = false;
            //    kscl_bto.Enabled = true;
            //}

            if (serialPort1.IsOpen)
            {
                founctioncode = 6;
                ack_timer.Enabled = true;
                ack_timer.Interval = 5000;
                ack = 1;
                serialPort1.Write(command8, 0, command8.Length);//增量编码器置零
            }
        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        private void return_jqcl_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                //founctioncode = 9;
                //ack_timer.Enabled = true;
                //ack_timer.Interval = 5000;
                //ack = 1;
                serialPort1.Write(command10, 0, command10.Length);//停止测量
                try
                {
                    serialPort1.Close();
                }
                catch { }
            }
           // serialPort1.Close();
            close_flag = 0;
            jqcl m = new jqcl();
            m.Show();
            this.Close();          
        }

        ///初始化ChartArea2警戒线和零位线
        private void jingjiexian_Init()
        {
            m = new Series("");
            n = new Series("");
            o = new Series("");
            m.ChartArea = "ChartArea2";
            n.ChartArea = "ChartArea2";
            o.ChartArea = "ChartArea2";
            m.Legend = "Legend2";
            n.Legend = "Legend2";
            o.Legend = "Legend2";
            m.Color = Color.Red;
            n.Color = Color.Red;
            o.Color = Color.Red;

            m.ChartType = SeriesChartType.Line;
            n.ChartType = SeriesChartType.Line;
            o.ChartType = SeriesChartType.Line;
            m.IsVisibleInLegend = false;
            n.IsVisibleInLegend = false;
            o.IsVisibleInLegend = false;
            chart1.Series.Add(m);
            chart1.Series.Add(n);
            chart1.Series.Add(o);
        }

        private void sz_bto_Click(object sender, EventArgs e)
        {
            close_flag = 0;
            if (serialPort1.IsOpen)
            {
                //founctioncode = 9;
                //ack_timer.Enabled = true;
                //ack_timer.Interval = 5000;
                //ack = 1;
                serialPort1.Write(command10, 0, command10.Length);//停止测量
                serialPort1.Close();
            }
            if (thread_creat_server != null)
            {
                thread_creat_server.Abort();
                thread_creat_server.Join();
            }
       
            deviceChoose f2 = new deviceChoose();
            f2.StartPosition = FormStartPosition.CenterScreen;
            f2.Show();
            this.Close();


        }

        /// <summary>
        /// 动态测量图标显示的参数设置函数
        /// </summary>
        /// <param name="n">ChartAreas的组号</param>
        /// <param name="o">ChartAreas[n]的Y轴最大坐标</param>
        /// <param name="p">ChartAreas[n]的Y轴最小坐标</param>
        private void dtcl_cssz(int n,string o,string p)
        {
            double i, j;
            if (Convert.ToInt32(o) >= Convert.ToInt32(p))
            {
                j = Convert.ToInt32(o) - Convert.ToInt32(p);
                i = j / 5;
                chart1.ChartAreas[n].AxisY.Maximum = Convert.ToInt32(o);
                chart1.ChartAreas[n].AxisY.Minimum = Convert.ToInt32(p);
                chart1.ChartAreas[n].AxisY.Interval = i;
                chart1.ChartAreas[n].AxisY.ScaleView.Size = j;
                chart1.ChartAreas[n].AxisY.ScaleView.Position = Convert.ToInt32(p);
            }
            else {
                j = Convert.ToInt32(p) - Convert.ToInt32(o);
                i = j / 5;
                chart1.ChartAreas[n].AxisY.Maximum = Convert.ToInt32(p);
                chart1.ChartAreas[n].AxisY.Minimum = Convert.ToInt32(o);
                chart1.ChartAreas[n].AxisY.Interval = i;
                chart1.ChartAreas[n].AxisY.ScaleView.Size = j;
                chart1.ChartAreas[n].AxisY.ScaleView.Position = Convert.ToInt32(o);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="queue1">导高列表</param>
        /// <param name="queue2">拉出值列表</param>
        /// <param name="param">侧面限界</param>
        private void dtcl_Gxsj(int i, List<float> queue1, List<float> queue2, int param)
        {
            //serier[0] 导高1
            //serier[1] 导高2
            //serier[2] 拉出值1
            //serier[3] 拉出值2
            //serier[4] 侧面限界
            if (queue1.Count == 1)
            {
                //描绘点


                try
                {
                    this.chart1.Series[1].Points[i].IsEmpty = true;
                    this.chart1.Series[3].Points[i].IsEmpty = true;
                }
                catch { }



                this.chart1.Series[0].Points.AddXY((i + 1), queue1[0].ToString("f2"));   //tostring("f2") 为保留两位小数
                //this.chart1.Series[1].Points[i].IsEmpty = true;
                this.chart1.Series[2].Points.AddXY((i + 1), queue2[0].ToString("f2"));
                //this.chart1.Series[3].Points[i].IsEmpty = true;
                this.chart1.Series[4].Points.AddXY((i + 1), param.ToString("f2"));                         
            }
            else if (queue1.Count == 2)
            {
                //描绘点
                this.chart1.Series[0].Points.AddXY((i + 1), queue1[0].ToString("f2"));
                this.chart1.Series[1].Points.AddXY((i + 1), queue1[1].ToString("f2"));
                this.chart1.Series[2].Points.AddXY((i + 1), queue2[0].ToString("f2"));
                this.chart1.Series[3].Points.AddXY((i + 1), queue2[1].ToString("f2"));
                this.chart1.Series[4].Points.AddXY((i + 1), param.ToString("f2"));                         

            }
            else {
                this.chart1.Series[0].Points.AddXY((i + 1), 0);
                this.chart1.Series[2].Points.AddXY((i + 1), 0);
                this.chart1.Series[4].Points.AddXY((i + 1), 0);
            }

            if (i % 5 == 0 && i != 0)
            {
                try { chart1.Series[0].Points[i].IsValueShownAsLabel = true; chart1.Series[0].Points[i].MarkerStyle = MarkerStyle.Circle; } catch { }
                try { chart1.Series[1].Points[i].IsValueShownAsLabel = true; chart1.Series[1].Points[i].MarkerStyle = MarkerStyle.Circle; } catch { }
                try { chart1.Series[2].Points[i].IsValueShownAsLabel = true; chart1.Series[2].Points[i].MarkerStyle = MarkerStyle.Circle; } catch { }
                try { chart1.Series[3].Points[i].IsValueShownAsLabel = true; chart1.Series[3].Points[i].MarkerStyle = MarkerStyle.Circle; } catch { }
                if(param != 0) { 
                    try { chart1.Series[4].Points[i].IsValueShownAsLabel = true; } catch { }
                }
            }

            //绘制ChartArea2的警戒线和零位线
            //this.chart1.Series[4].Points.AddXY(i, -3);
            //this.chart1.Series[4].Points.AddXY((i + 1), -3);
            //this.chart1.Series[5].Points.AddXY(i, 3);
            //this.chart1.Series[5].Points.AddXY((i + 1), 3);
            //this.chart1.Series[6].Points.AddXY(i, 0);
            //this.chart1.Series[6].Points.AddXY((i + 1), 0);

        }
    }
}
