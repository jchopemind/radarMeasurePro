using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class LMSxx_SetUp : Form
    {
        /* Global Parameters */
        private Thread thread_creat_server;
        private Socket server;
        private IPAddress connectServer;
        private IPEndPoint iep;
        private bool isConn;
        private ManualResetEvent timeoutObject;
        Graphics g;
        PicBoxDraw drawP;
        private byte changeFlag = 0;
        Pen myPen = new Pen(Color.Blue, 1);
        Pen cx_Pen = new Pen(Color.Red, 1);
        double scaling = 0.1;
        int cx_sAngel = -45, cx_eAngel = 225;
        byte close_flag = 1;
        byte[] revBytes = new byte[1024 * 10];
        byte[] revByte = new byte[1024];
        public byte LMS1xx, LMS5xx;
        private AllParameter parameter = new AllParameter();

        /*雷达常用16进制指令*/
        private static byte[] singleData= { 0x02, 0x73, 0x52, 0x4E, 0x20, 0x4C, 0x4D, 0x44, 0x73, 0x63, 0x61, 0x6E, 0x64, 0x61, 0x74, 0x61, 0x03 };//单次取数据指令
        private static byte[] logIn = { 0x02, 0x73, 0x4D, 0x4E, 0x20, 0x53, 0x65, 0x74, 0x41, 0x63, 0x63, 0x65, 0x73, 0x73, 0x4D, 0x6F,
                                        0x64, 0x65, 0x20, 0x30, 0x33, 0x20, 0x46, 0x34, 0x37, 0x32, 0x34, 0x37, 0x34, 0x34, 0x03 };//登录设备指令
        private static byte[] saveChange = { 0x02, 0x73, 0x4D, 0x4E, 0x20, 0x6D, 0x45, 0x45, 0x77, 0x72, 0x69, 0x74, 0x65, 0x61, 0x6C, 0x6C, 0x03 };//保存参数指令
        private static byte[] startLMS = { 0x02, 0x73, 0x4D, 0x4E, 0x20, 0x52, 0x75, 0x6E, 0x03 };//启动设备指令
        private static byte[] inquire = { 0x02, 0x73, 0x52, 0x4E, 0x20, 0x4C, 0x4D, 0x50, 0x73, 0x63, 0x61, 0x6E, 0x63, 0x66, 0x67, 0x03 };//查询参数指令
        /*写入数据到文件*/
        //private void SaveProcess(String data)
        //{
        //    string CurDir = System.AppDomain.CurrentDomain.BaseDirectory + @"ipAddr\";    //设置当前目录  
        //    if (!System.IO.Directory.Exists(CurDir)) System.IO.Directory.CreateDirectory(CurDir);   //该路径不存在时，在当前文件目录下创建文件夹"导出.."  

        //    //不存在该文件时先创建  
        //    String filePath = CurDir +  "ipAddr(重要资料请勿修改).txt";
        //    System.IO.StreamWriter file1 = new System.IO.StreamWriter(filePath, false);     //文件已覆盖方式添加内容  

        //    file1.Write(data);                                                              //保存数据到文件  

        //    file1.Close();                                                                  //关闭文件  
        //    file1.Dispose();                                                                //释放对象  
        //}
        public LMSxx_SetUp()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        void Thread_Server_Listen(object msgg)
        {
            int sAngel, eAngel;//数据界线的开始角度和结束角度
           
            while (true)
            {
               // Thread.Sleep(3000);
                try
                {
                    server.Send(singleData);
                }
                catch
                {
                    MessageBox.Show("请检查服务器是否连接！");
                    return;
                }

                int rlen = server.Receive(revBytes, SocketFlags.None);
                string msg = Encoding.ASCII.GetString(revBytes, 1, rlen - 2);
                string[] sArray = Regex.Split(msg, " ", RegexOptions.IgnoreCase);

                Bitmap pic_img = new Bitmap(pic_box.Width, pic_box.Height);
                g = Graphics.FromImage(pic_img);
                drawP = new PicBoxDraw(g, pic_box.Width, pic_box.Height);
                drawP.draw5_185_base(cx_Pen, cx_sAngel, cx_eAngel);//画查询角度的界线

/*数据描点*/
                if (sArray[0] == "sRA")
                {
                    float angle_resolution = 0.0f;

                    if (rlen != 0)
                    {
                        //sAngel = (int)Convert.ToInt32(sArray[23], 16) / 10000;
                        sAngel = filterAlgForRadarData.startAngle_int;
                        eAngel = filterAlgForRadarData.startAngle_int + filterAlgForRadarData.allAngle_size;

                        if (changeResolution_cbo.Text == "0.5度")
                        {
                            //eAngel = (int)Convert.ToInt32(sArray[23], 16) / 10000 + (Convert.ToInt32(sArray[25], 16) - 1) / 2;
                            drawP.draw5_185_base(myPen, sAngel, eAngel);
                            angle_resolution = 0.5f;
                        }
                        else if (changeResolution_cbo.Text == "0.25度")
                        {
                            //eAngel = (int)Convert.ToInt32(sArray[23], 16) / 10000 + (Convert.ToInt32(sArray[25], 16) - 1) / 4;
                            drawP.draw5_185_base(myPen, sAngel, eAngel);
                            angle_resolution = 0.25f;
                        }
                        else if (changeResolution_cbo.Text == "0.333度")
                        {
                            //eAngel = (int)Convert.ToInt32(sArray[23], 16) / 10000 + (Convert.ToInt32(sArray[25], 16) - 1) / 3;
                            //eAngel = Convert.ToInt32(changeAngel_tb02.Text);
                            drawP.draw5_185_base(myPen, sAngel, eAngel);
                            angle_resolution = 0.3333f;
                        }
                        else if (changeResolution_cbo.Text == "0.1667度")
                        {
                            //eAngel = (int)Convert.ToInt32(sArray[23], 16) / 10000 + (Convert.ToInt32(sArray[25], 16) - 1) / 6;
                            //eAngel = Convert.ToInt32(changeAngel_tb02.Text);
                            drawP.draw5_185_base(myPen, sAngel, eAngel);
                            angle_resolution = 0.16667f;
                        }

                        try
                        {
                            for (int i = 26 + filterAlgForRadarData.startAngle_int * 6; i < 26 + filterAlgForRadarData.startAngle_int * 6 + filterAlgForRadarData.allAngle_size * 6; i++)
                            {
                                int tep_data = Convert.ToInt32(sArray[i], 16);

                                if (tep_data >= filterAlgForRadarData.lower_limit && tep_data <= filterAlgForRadarData.super_limit)
                                    drawP.drawAPoint((int)(scaling * tep_data), angle_resolution * (i - 26));
                            }
                        }
                        catch
                        {
                            MessageBox.Show("请稍后，雷达还未初始化成功！");
                            continue;
                        }
                        
                        /*
                        for (int i = 26; i < 26 + Convert.ToInt32(sArray[25], 16); i++)
                        {
                            int tep_data = Convert.ToInt32(sArray[i], 16);

                            if (tep_data >= filterAlgForRadarData.lower_limit && tep_data <= filterAlgForRadarData.super_limit)
                                drawP.drawAPoint((int)(scaling * tep_data), sAngel + angle_resolution * (i - 26));
                        }
                        */

                        pic_box.Image = pic_img;
                    }
                    else
                    {
                        MessageBox.Show("未接收到任何数据");
                    }
                }
            }
        }


        void Thread_Server_Listen_process(object msgg)
        {
            int sAngel, eAngel;//数据界线的开始角度和结束角度
            float angle_resolution = 0f;

            while (true)
            {
                try {
                    server.Send(singleData);
                } catch {
                    MessageBox.Show("请检查服务器是否连接！");
                    return;
                }

                int rlen = server.Receive(revBytes, SocketFlags.None);
                string msg = Encoding.ASCII.GetString(revBytes, 1, rlen - 2);
                string[] sArray = Regex.Split(msg, " ", RegexOptions.IgnoreCase);

                Bitmap pic_img = new Bitmap(pic_box.Width, pic_box.Height);
                g = Graphics.FromImage(pic_img);
                drawP = new PicBoxDraw(g, pic_box.Width, pic_box.Height);
                drawP.draw5_185_base(cx_Pen, cx_sAngel, cx_eAngel);//画查询角度的界线

                /*数据描点*/
                if (sArray[0] == "sRA")
                {
                    if (rlen != 0)
                    {
                        //sAngel = (int)Convert.ToInt32(sArray[23], 16) / 10000;
                        sAngel = filterAlgForRadarData.startAngle_int;
                        eAngel = filterAlgForRadarData.startAngle_int + filterAlgForRadarData.allAngle_size;
                        if (changeResolution_cbo.Text == "0.5度")
                        {
                            //eAngel = (int)Convert.ToInt32(sArray[23], 16) / 10000 + (Convert.ToInt32(sArray[25], 16) - 1) / 2;
                            drawP.draw5_185_base(myPen, sAngel, eAngel);
                            angle_resolution = 0.5f;
                        }
                        else if (changeResolution_cbo.Text == "0.25度")
                        {
                            //eAngel = (int)Convert.ToInt32(sArray[23], 16) / 10000 + (Convert.ToInt32(sArray[25], 16) - 1) / 4;
                            drawP.draw5_185_base(myPen, sAngel, eAngel);
                            angle_resolution = 0.25f;
                        }
                        else if (changeResolution_cbo.Text == "0.333度")
                        {
                            //eAngel = (int)Convert.ToInt32(sArray[23], 16) / 10000 + (Convert.ToInt32(sArray[25], 16) - 1) / 3;
                            //eAngel = Convert.ToInt32(changeAngel_tb02.Text);
                            drawP.draw5_185_base(myPen, sAngel, eAngel);
                            angle_resolution = 0.3333f;
                        }
                        else if (changeResolution_cbo.Text == "0.1667度")
                        {
                            //eAngel = (int)Convert.ToInt32(sArray[23], 16) / 10000 + (Convert.ToInt32(sArray[25], 16) - 1) / 6;
                            //eAngel = Convert.ToInt32(changeAngel_tb02.Text);
                            drawP.draw5_185_base(myPen, sAngel, eAngel);
                            angle_resolution = 0.16667f;
                        }
#if true

                        /* processing data */
                        filterAlgForRadarData.filterRadarData(sArray, angle_resolution);
                        float cmxjjjj = filterAlgForRadarData.getCMXJ_DATA(sArray, 0);

                        if (cmxjjjj > 0)
                        {
                            Console.WriteLine("侧面：" + cmxjjjj.ToString());
                        }


                        /*-------------------------------------------------------------------------------------------------------------*/

                        for (int kk = 0; kk < filterAlgForRadarData.list_final_points_end.Count; kk++)
                        {
                            float length = filterAlgForRadarData.list_final_points_end[kk];
                            int angle_int = (filterAlgForRadarData.list_final_points_angle_end[kk]);
                            drawP.drawAPoint((int)(scaling * length), angle_resolution * angle_int);
                        }

                        Console.WriteLine("一共有" + filterAlgForRadarData.list_final_points_end.Count.ToString() + "个点。。");

                        for (int kk = 0; kk < filterAlgForRadarData.list_final_points_end.Count; kk++)
                        {
                            Console.WriteLine("第" + kk + "个点为：" + filterAlgForRadarData.list_final_points_end[kk].ToString() + ", " + (sAngel + angle_resolution * (filterAlgForRadarData.list_final_points_angle_end[kk])));
                        }
                        Console.WriteLine("");


                        /* wait a moment */
                        // Thread.Sleep(1000);

#else
                        /*
                        for (int i = 26; i < 26 + Convert.ToInt32(sArray[25], 16); i++)
                        {
                            int realV = Convert.ToInt32(sArray[i], 16);

                            drawP.drawAPoint((int)(scaling * realV), sAngel + angle_resolution * (i - 26));
                        }*/
                        filterAlgForRadarData.showRadarData(sArray, drawP, sAngel, angle_resolution);
#endif
                        /* show data points */
                        pic_box.Image = pic_img;
                    }
                    else
                    {
                        MessageBox.Show("未接收到任何数据");

                    }
                }
            }
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


        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }



        private void pic_box_Paint(object sender, PaintEventArgs e)
        {
           
        }


        private void LMSxxSetUp_Load_1(object sender, EventArgs e)
        {
            string[] parameters = parameter.fileToString("LMSxxparameter(重要资料，请勿删改！！！).txt").Split(' ');

            if (parameters.Length == 6)
            {
                dtcl_X1_txt.Text = parameters[0];
                dtcl_X2_txt.Text = parameters[1];
                dtcl_Y1_txt.Text = parameters[2];
                dtcl_Y2_txt.Text = parameters[3];
                changeAngel_tb01.Text = parameters[4];
                changeAngel_tb02.Text = parameters[5];

            }
            else
            {
                MessageBox.Show("参数读取失败！");
            }

            button1.Enabled = false;
            lbxs_btn.Enabled = false;
            getPrameter_bto.Enabled = false;
            changeResolution_cbo.Enabled = false;
        }

        private void connect_btn_Click_1(object sender, EventArgs e)
        {
            string ipAddr;
            int sAngel, eAngel;
/*连接到雷达设备*/
            if (connect_btn.Text == "连接设备")
            {
                if (ipAddr_txt1.Text == "" || ipAddr_txt2.Text == "" || ipAddr_txt3.Text == "" || ipAddr_txt4.Text == "")
                {
                    MessageBox.Show("Please Enter IPaddress");
                    return;
                }

                ipAddr = ipAddr_txt1.Text + '.' + ipAddr_txt2.Text + '.' + ipAddr_txt3.Text + '.' + ipAddr_txt4.Text;
                int portNum = Convert.ToInt32(port_box.Text);
                connectServer = IPAddress.Parse(ipAddr);
                iep = new IPEndPoint(connectServer, portNum);
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                label_status.Text = ("Connecting ...");
                Console.WriteLine("Connecting ...");
                timeoutObject = new ManualResetEvent(false);

                server.BeginConnect(connectServer, portNum, new AsyncCallback(CallBackMethod), server);
                if (!timeoutObject.WaitOne(1000, false))
                {
                    server.Close();
                    MessageBox.Show("connect Timeout");
                    return;
                }

                try
                {
                    server.Send(inquire);//查询设备参数
                }
                catch
                {
                    MessageBox.Show("请检查服务器是否连接！");
                    return;
                }

                int rlen1 = server.Receive(revBytes, SocketFlags.None);
                string msg1 = Encoding.ASCII.GetString(revBytes, 1, rlen1 - 2);
                string[] sArray1 = Regex.Split(msg1, " ", RegexOptions.IgnoreCase);
                if(LMS1xx == 1)
                {
                    if(sArray1[5] != "FFF92230")
                    {
                        MessageBox.Show("请检查选择设备型号与连接设备型号是否一致！");
                        return;
                    }
                }else if(LMS5xx == 1)
                {
                    if(sArray1[5] != "FFFF3CB0")
                    {
                        MessageBox.Show("请检查选择设备型号与连接设备型号是否一致！");
                        return;
                    }
                }

                connect_btn.Text = "停止连接";
                button1.Enabled = true;
                lbxs_btn.Enabled = true;
                getPrameter_bto.Enabled = true;
                changeResolution_cbo.Enabled = true;

                try
                {
                    server.Send(singleData);//查询设备参数
                }
                catch
                {
                    MessageBox.Show("请检查服务器是否连接！");
                    connect_btn.Text = "连接设备";
                    button1.Enabled = false;
                    lbxs_btn.Enabled = false;
                    getPrameter_bto.Enabled = false;
                    changeResolution_cbo.Enabled = false;
                    return;
                }

                int rlen = server.Receive(revBytes, SocketFlags.None);
                string msg = Encoding.ASCII.GetString(revBytes, 1, rlen - 2);
                string[] sArray = Regex.Split(msg, " ", RegexOptions.IgnoreCase);

                if (msg != "")
                {
                    // sAngel =(int) Convert.ToInt32(sArray[23], 16) / 10000;
                    sAngel = filterAlgForRadarData.startAngle_int;
                    changeAngel_tb01.Text = sAngel.ToString();
                    eAngel = filterAlgForRadarData.startAngle_int + filterAlgForRadarData.allAngle_size;

                    /*判断扫描角度*/
                    // MessageBox.Show(sArray[24]);
                    if (sArray[24] == "1388")
                    {
                        changeResolution_cbo.Text = "0.5度";
                        changeFlag = 1;
                        //eAngel = (int)Convert.ToInt32(sArray[23], 16) / 10000 + (Convert.ToInt32(sArray[25], 16) - 1) / 2;
                        changeAngel_tb02.Text = eAngel.ToString();
                    }
                   else if (sArray[24] == "9C4")
                    {
                        changeResolution_cbo.Text = "0.25度";
                        changeFlag = 1;
                        //eAngel = (int)Convert.ToInt32(sArray[23], 16) / 10000 + (Convert.ToInt32(sArray[25], 16) - 1) / 4;
                        changeAngel_tb02.Text = eAngel.ToString();
                    }
                    else if (sArray[24] == "D05")
                    {
                        changeResolution_cbo.Text = "0.333度";
                        changeFlag = 1;
                        //eAngel = (int)Convert.ToInt32(sArray[23], 16) / 10000 + (Convert.ToInt32(sArray[25], 16)) / 3;
                        changeAngel_tb02.Text = eAngel.ToString();
                    }
                    else if (sArray[24] == "683")
                    {
                        changeResolution_cbo.Text = "0.1667度";
                        changeFlag = 1;
                        //eAngel = (int)Convert.ToInt32(sArray[23], 16) / 10000 + (Convert.ToInt32(sArray[25], 16)) / 6;
                        changeAngel_tb02.Text = eAngel.ToString();
                    }

                }

            }
            else
            {
                if (thread_creat_server != null)
                {
                    thread_creat_server.Abort();
                    thread_creat_server.Join();
                }
                button1.Text = "显示数据";

                if (thread_creat_server != null)
                {
                    server.Close();
                }
                label_status.Text = (" >> exit");
                if (thread_creat_server != null)
                {
                    thread_creat_server.Abort();
                    thread_creat_server.Join();
                }
                connect_btn.Text = "连接设备";
                button1.Enabled = false;
                lbxs_btn.Enabled = false;
                getPrameter_bto.Enabled = false;
                changeResolution_cbo.Enabled = false;
            }
        }

        private void LMSxxSetUp_FormClosing_1(object sender, FormClosingEventArgs e)
        {
/*关闭窗口提示窗*/
            if (close_flag == 1)
            {
                //DialogResult result = MessageBox.Show("？", "提示信息", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
//                if (result == DialogResult.OK)
 //               {
                    if (thread_creat_server != null)
                    {
                        thread_creat_server.Abort();
                        thread_creat_server.Join();
                    }
                    button1.Text = "显示数据";

                    if (server != null)
                        server.Close();
                    e.Cancel = false;  //点击OK   
                //}
                //else
                //{
                //    e.cancel = true;
                //}
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (button1.Text == "显示数据")
            {
                thread_creat_server = new Thread(Thread_Server_Listen);
                thread_creat_server.Start();
                button1.Text = "停止显示";
                lbxs_btn.Enabled = false;
            }
            else if (button1.Text == "停止显示")
            {
                thread_creat_server.Abort();
                thread_creat_server.Join();
                button1.Text = "显示数据";
                lbxs_btn.Enabled = true;
            }
        }

        private void getPrameter_bto_Click(object sender, EventArgs e)
        {
 /*查询设备参数*/
            try
            {
                server.Send(inquire);//查询设备参数
            }
            catch
            {
                MessageBox.Show("请检查服务器是否连接！");
                return;
            }

            int rlen = server.Receive(revByte, SocketFlags.None);
            string msg = Encoding.ASCII.GetString(revByte, 1, rlen - 2);
            MessageBox.Show(msg);
        }

        private void changeResolution_cbo_TextChanged_1(object sender, EventArgs e)
        { 
/*登录设备*/
            if (changeFlag == 1)
            {
                if (thread_creat_server != null)
                {
                    thread_creat_server.Abort();
                    thread_creat_server.Join();
                }
                button1.Text = "显示数据";

                try
                {
                    server.Send(logIn);//登录设备
                }
                catch
                {
                    MessageBox.Show("请检查服务器是否连接！");
                    return;
                }

                int rlen = server.Receive(revByte, SocketFlags.None);
                string msg = Encoding.ASCII.GetString(revByte, 1, rlen - 2);
              
                // MessageBox.Show(msg);
                //while (msg.Contains("sAN")) ;
/*修改分辨率*/
                if (msg != "")
                {
                    if (msg[18] == '1')//返回1登录成功
                    {
/*进度条显示*/
                        progressBar fm = new progressBar(0, 100);
                        fm.StartPosition = FormStartPosition.CenterScreen;
                        fm.Show(this);
                        fm.setPos(0);//设置进度条位置

                        if (LMS5xx == 1)//如果选择雷达型号为LMS5xx
                        {
                           
                                if (changeResolution_cbo.Text == "0.1667度")
                                {
                                    // MessageBox.Show("设备登录成功！");
                                    byte[] rev1Bytes = new byte[1024];
                                    byte[] changeResolutionBytes = { 0x02, 0x73, 0x4D, 0x4E, 0x20, 0x6D, 0x4C, 0x4D, 0x50, 0x73, 0x65, 0x74, 0x73, 0x63, 0x61,
                                                                     0x6E, 0x63, 0x66, 0x67, 0x20, 0x2B, 0x32, 0x35, 0x30, 0x30, 0x20, 0x2B, 0x31, 0x20, 0x2B,
                                                                     0x31, 0x36, 0x36, 0x37, 0x20, 0x2D, 0x35, 0x30, 0x30, 0x30, 0x30, 0x20, 0x2B, 0x31,
                                                                     0x38, 0x35, 0x30, 0x30, 0x30, 0x30, 0x03 };//LMS5xx

                                    try
                                    {
                                        server.Send(changeResolutionBytes);//修改参数
                                    }
                                    catch
                                    {
                                        MessageBox.Show("请检查服务器是否连接！");
                                        return;
                                    }

                                    int rlen1 = server.Receive(rev1Bytes, SocketFlags.None);
                                    string msg1 = Encoding.ASCII.GetString(rev1Bytes, 1, rlen1 - 2);
                                //MessageBox.Show("修改参数成功" + msg1);
                            }

                                else if (changeResolution_cbo.Text == "0.333度")
                                {
                                    // MessageBox.Show("设备登录成功！");
                                    byte[] rev1Bytes = new byte[1024];
                                    byte[] changeResolutionBytes = { 0x02, 0x73, 0x4D, 0x4E, 0x20, 0x6D, 0x4C, 0x4D, 0x50, 0x73, 0x65, 0x74, 0x73, 0x63, 0x61,
                                                                     0x6E, 0x63, 0x66, 0x67, 0x20, 0x2B, 0x35, 0x30, 0x30, 0x30, 0x20, 0x2B, 0x31, 0x20, 0x2B,
                                                                     0x33, 0x33, 0x33, 0x33, 0x20, 0x2D, 0x35, 0x30, 0x30, 0x30, 0x30, 0x20, 0x2B, 0x31,
                                                                     0x38, 0x35, 0x30, 0x30, 0x30, 0x30, 0x03 };//LMS5xx

                                    try
                                    {
                                        server.Send(changeResolutionBytes);//修改参数
                                    }
                                    catch
                                    {
                                        MessageBox.Show("请检查服务器是否连接！");
                                        return;
                                    }

                                    int rlen1 = server.Receive(rev1Bytes, SocketFlags.None);
                                    string msg1 = Encoding.ASCII.GetString(rev1Bytes, 1, rlen1 - 2);
                                //MessageBox.Show("修改参数成功" + msg1);
                            }

                                else if (changeResolution_cbo.Text == "0.25度")
                                {
                                    // MessageBox.Show("设备登录成功！");
                                    byte[] rev1Bytes = new byte[1024];
                                    byte[] changeResolutionBytes = { 0x02, 0x73, 0x4D, 0x4E, 0x20, 0x6D, 0x4C, 0x4D, 0x50, 0x73, 0x65, 0x74, 0x73, 0x63, 0x61,
                                                                     0x6E, 0x63, 0x66, 0x67, 0x20, 0x2B, 0x32, 0x35, 0x30, 0x30, 0x20, 0x2B, 0x31, 0x20, 0x2B,
                                                                     0x32, 0x35, 0x30, 0x30, 0x20, 0x2D, 0x35, 0x30, 0x30, 0x30, 0x30, 0x20, 0x2B, 0x31,
                                                                     0x38, 0x35, 0x30, 0x30, 0x30, 0x30, 0x03 };//LMS5xx

                                    try
                                    {
                                        server.Send(changeResolutionBytes);//修改参数
                                    }
                                    catch
                                    {
                                        MessageBox.Show("请检查服务器是否连接！");
                                        return;
                                    }

                                    int rlen1 = server.Receive(rev1Bytes, SocketFlags.None);
                                    string msg1 = Encoding.ASCII.GetString(rev1Bytes, 1, rlen1 - 2);
                                //MessageBox.Show("修改参数成功" + msg1);
                              }

                                else if (changeResolution_cbo.Text == "0.5度")
                                {
                                    //MessageBox.Show("设备登录成功！");
                                    byte[] rev1Bytes = new byte[1024];
                                    byte[] changeResolutionBytes = { 0x02, 0x73, 0x4D, 0x4E, 0x20, 0x6D, 0x4C, 0x4D, 0x50, 0x73, 0x65, 0x74, 0x73, 0x63, 0x61,
                                                                        0x6E, 0x63, 0x66, 0x67, 0x20, 0x2B, 0x35, 0x30, 0x30, 0x30, 0x20, 0x2B, 0x31, 0x20, 0x2B,
                                                                        0x35, 0x30, 0x30, 0x30, 0x20, 0x2D, 0x35, 0x30, 0x30, 0x30, 0x30, 0x20, 0x2B, 0x31,
                                                                        0x38, 0x35, 0x30, 0x30, 0x30, 0x30, 0x03 };//LMS5xx
                                    try
                                    {
                                        server.Send(changeResolutionBytes);//修改参数
                                    }
                                    catch
                                    {
                                        MessageBox.Show("请检查服务器是否连接！");
                                        return;
                                    }


                                    int rlen1 = server.Receive(rev1Bytes, SocketFlags.None);
                                    //Console.WriteLine("Server receive rlen {0}", rlen);
                                    string msg1 = Encoding.ASCII.GetString(rev1Bytes, 1, rlen1 - 2);
                                   //MessageBox.Show("修改参数成功" + msg1);
                            }
                        }

                        else //选择雷达设备型号为LMS1xx
                        {
                            if (changeResolution_cbo.Text == "0.1667度")
                            {
                                MessageBox.Show("LMS1xx不支持该分辨率");
                                return;
                            }

                            else if (changeResolution_cbo.Text == "0.333度")
                            {
                                MessageBox.Show("LMS1xx不支持该分辨率");
                                return;
                            }

                            else if (changeResolution_cbo.Text == "0.25度")
                            {
                                // MessageBox.Show("设备登录成功！");
                                byte[] rev1Bytes = new byte[1024];
                                byte[] changeResolutionBytes = { 0x02, 0x73, 0x4d, 0x4e, 0x20, 0x6d, 0x4c, 0x4d, 0x50, 0x73, 0x65, 0x74, 0x73, 0x63, 0x61,
                                                                0x6e, 0x63, 0x66, 0x67, 0x20, 0x2b, 0x32, 0x35, 0x30, 0x30, 0x20, 0x2b, 0x31, 0x20, 0x2b,
                                                                0x32, 0x35, 0x30, 0x30, 0x20, 0x2d, 0x34, 0x35, 0x30, 0x30, 0x30, 0x30, 0x20, 0x2b, 0x32,
                                                                0x32, 0x35, 0x30, 0x30, 0x30, 0x30, 0x03 };//lms1xx

                                try
                                {
                                    server.Send(changeResolutionBytes);//修改参数
                                }
                                catch
                                {
                                    MessageBox.Show("请检查服务器是否连接！");
                                    return;
                                }

                                int rlen1 = server.Receive(rev1Bytes, SocketFlags.None);
                                string msg1 = Encoding.ASCII.GetString(rev1Bytes, 1, rlen1 - 2);
                                //MessageBox.Show("修改参数成功" + msg1);
                            }

                            else if (changeResolution_cbo.Text == "0.5度")
                            {
                                //  MessageBox.Show("设备登录成功！");
                                byte[] rev1Bytes = new byte[1024];
                                byte[] changeResolutionBytes = { 0x02, 0x73, 0x4D, 0x4E, 0x20, 0x6D, 0x4C, 0x4D, 0x50, 0x73, 0x65, 0x74, 0x73, 0x63, 0x61,
                                                                0x6E, 0x63, 0x66, 0x67, 0x20, 0x2B, 0x32, 0x35, 0x30, 0x30, 0x20, 0x2B, 0x31, 0x20, 0x2B,
                                                                0x32, 0x35, 0x30, 0x30, 0x20, 0x2D, 0x34, 0x35, 0x30, 0x30, 0x30, 0x30, 0x20, 0x2B, 0x32,
                                                                0x32, 0x35, 0x30, 0x30, 0x30, 0x30, 0x03 };//LMS1xx

                                try
                                {
                                    server.Send(changeResolutionBytes);//修改参数
                                }
                                catch
                                {
                                    MessageBox.Show("请检查服务器是否连接！");
                                    return;
                                }


                                int rlen1 = server.Receive(rev1Bytes, SocketFlags.None);
                                //Console.WriteLine("Server receive rlen {0}", rlen);
                                string msg1 = Encoding.ASCII.GetString(rev1Bytes, 1, rlen1 - 2);
                                //MessageBox.Show("修改参数成功" + msg1);
                            }
                        }
 /*保存修改参数*/
                        try
                        {
                            server.Send(saveChange);//保存参数
                        }
                        catch
                        {
                            MessageBox.Show("请检查服务器是否连接！");
                            return;
                        }

                        int rlen2 = server.Receive(revByte, SocketFlags.None);
                        string msg2 = Encoding.ASCII.GetString(revByte, 1, rlen2 - 2);
                        // MessageBox.Show(msg2);
                        //while (!msg2.Contains("sAN")) ;
/*重启设备*/
                        try
                        {
                            server.Send(startLMS);//启动设备
                        }
                        catch
                        {
                            MessageBox.Show("请检查服务器是否连接！");
                            return;
                        }


                        int rlen3 = server.Receive(revByte, SocketFlags.None);
                        string msg3 = Encoding.ASCII.GetString(revByte, 1, rlen3 - 2);
                        //  MessageBox.Show(msg3);
                        //while (!msg3.Contains("sMN")) ;
/*修改参数等待时间*/
                        if (LMS1xx == 1)
                        {
                            for (int i = 0; i < 100; i++)
                            {
                                fm.setPos(i);//设置进度条位置
                                Thread.Sleep(500);//睡眠时间为100
                            }
                        }
                        else if(LMS5xx == 1)
                        {
                            for (int i = 0; i < 100; i++)
                            {
                                fm.setPos(i);//设置进度条位置
                                Thread.Sleep(50);//睡眠时间为100
                            }
                        }
                        fm.Close();//关闭窗体
                        this.Enabled = true;
                        this.Show();

                    }
                }
                else
                    MessageBox.Show("登录设备失败！");
            }
        }

        private void affirm_bto_Click(object sender, EventArgs e)
        {
            filterAlgForRadarData.startAngle_int = Convert.ToInt32(changeAngel_tb01.Text);
            filterAlgForRadarData.allAngle_size = Convert.ToInt32(changeAngel_tb02.Text) - filterAlgForRadarData.startAngle_int;
#if false
            int i, j = 0;
            string text1, text2;
            byte[] changeAngel1Bytes = { 0x02, 0x73, 0x57, 0x4E, 0x20, 0x4C, 0x4D, 0x50, 0x6F, 0x75, 0x74, 0x70, 0x75, 0x74, 0x52, 0x61,
                                         0x6E, 0x67, 0x65, 0x20, 0x31, 0x20, 0x31, 0x33, 0x38, 0x38, 0x20};//修改扫描角度指令1

            if (changeAngel_tb01.Text != "" && changeAngel_tb02.Text != "")
            {
                if (Convert.ToInt16(changeAngel_tb02.Text) < Convert.ToInt16(changeAngel_tb01.Text))
                {
                    MessageBox.Show("终止角度应小于起始角度！");
                    return;
                }

                if (!changeAngel_tb01.Text.Contains("-"))
                    text1 = '+' + changeAngel_tb01.Text;
                else
                    text1 = changeAngel_tb01.Text;
                if (!changeAngel_tb02.Text.Contains("-"))
                    text2 = '+' + changeAngel_tb02.Text;
                else
                    text2 = changeAngel_tb02.Text;

                byte[] angelStartBytes = new byte[text1.Length + 5];//扫描起始角度
                byte[] angelEndBytes = new byte[text2.Length + 5];//扫描结束角度
 /*填充起始角度指令*/
                byte[] angelStart1Bytes = System.Text.Encoding.Default.GetBytes(text1.ToString());
                for (i = 0; i < angelStart1Bytes.Length; i++)
                {
                    angelStartBytes[i] = angelStart1Bytes[i];
                }
                for (i = 0; i < 4; i++)
                {
                    angelStartBytes[text1.Length + i] = 0x30;
                }
                angelStartBytes[angelStartBytes.Length - 1] = 0x20;
/*填充结束角度指令*/
                byte[] angelEnd1Bytes = System.Text.Encoding.Default.GetBytes(text2.ToString());
                for (i = 0; i < angelEnd1Bytes.Length; i++)
                {
                    angelEndBytes[i] = angelEnd1Bytes[i];
                }
                for (i = 0; i < 4; i++)
                {
                    angelEndBytes[text2.Length + i] = 0x30;
                }
                angelEndBytes[angelEndBytes.Length - 1] = 0x03;
 /*填充修改角度指令*/
                byte[] changeAngelBytes = new byte[angelStartBytes.Length + angelEndBytes.Length + 27];

                for (i = 0; i < 27; i++)//填充changeAngel1Bytes进去
                {
                    changeAngelBytes[i] = changeAngel1Bytes[i];
                }
                for (i = 27; i < angelStartBytes.Length + 27; i++)//填充angelStartBytes进去
                {
                    changeAngelBytes[i] = angelStartBytes[j];
                    j++;
                }
                j = 0;
                for (i = 27 + angelStartBytes.Length; i < changeAngelBytes.Length; i++)//填充angelEndBytes进去
                {
                    changeAngelBytes[i] = angelEndBytes[j];
                    j++;
                }
/*登录设备*/
                if (thread_creat_server != null)
                {
                    thread_creat_server.Abort();
                    thread_creat_server.Join();
                }//停止线程
                button1.Text = "显示数据";

                try
                {
                    server.Send(logIn);//登录设备
                }
                catch
                {
                    MessageBox.Show("请检查服务器是否连接！");
                    return;
                }

                int rlen = server.Receive(revByte, SocketFlags.None);
                string msg = Encoding.ASCII.GetString(revByte, 1, rlen - 2);
/*修改扫描角度*/
                if (msg != "")
                {
                    if (msg[18] == '1')
                    {

                        try
                        {
                            server.Send(changeAngelBytes);//修改扫描角度
                        }
                        catch
                        {
                            MessageBox.Show("请检查服务器是否连接！");
                            return;
                        }

                        int rlen1 = server.Receive(revByte, SocketFlags.None);
                        string msg1 = Encoding.ASCII.GetString(revByte, 1, rlen1 - 2);
 /*保存修改参数*/
                        try
                        {
                            server.Send(saveChange);//保存参数
                        }
                        catch
                        {
                            MessageBox.Show("请检查服务器是否连接！");
                            return;
                        }

                        int rlen2 = server.Receive(revByte, SocketFlags.None);
                        string msg2 = Encoding.ASCII.GetString(revByte, 1, rlen2 - 2);
/*重启设备*/
                        try
                        {
                            server.Send(startLMS);//启动设备
                        }
                        catch
                        {
                            MessageBox.Show("请检查服务器是否连接！");
                            return;
                        }
                        int rlen3 = server.Receive(revByte, SocketFlags.None);
                        string msg3 = Encoding.ASCII.GetString(revByte, 1, rlen3 - 2);
                    }
                }
                else
                    MessageBox.Show("登录设备失败！");

            }
#endif
        }

        private void changeAngel_tb01_TextChanged_1(object sender, EventArgs e)
        {
            short i;

#if false
            if (thread_creat_server != null)
            {
                thread_creat_server.Abort();
                thread_creat_server.Join();
            }
            button1.Text = "显示数据";
#endif
            if (changeAngel_tb01.Text != "")
            {
                if (changeAngel_tb01.Text != "-")
                {
                    try
                    {
                        i = Convert.ToInt16(changeAngel_tb01.Text);
                    }
                    catch
                    {
                        MessageBox.Show("输入角度范围为：-45°到 225°！");
                        changeAngel_tb01.Text = "";
                        return;
                    }
                    if (LMS1xx == 1)
                    {
                        if (i < -45 | i > 225)
                        {
                            MessageBox.Show("输入角度范围为：-45°到 225°！");
                            changeAngel_tb01.Text = "";
                        }
                    }
                    else if (LMS5xx == 1)
                    {
                        if (i < -5 | i > 185)
                        {
                            MessageBox.Show("输入角度范围为：-5°到 185°！");
                            changeAngel_tb01.Text = "";
                        }
                    }
                }

            }
        }

        private void changeAngel_tb02_TextChanged_1(object sender, EventArgs e)
        {
            short i;

#if false
            if (thread_creat_server != null)
            {
                thread_creat_server.Abort();
                thread_creat_server.Join();
            }
            button1.Text = "显示数据";
#endif

            if (changeAngel_tb02.Text != "")
            {
                if (changeAngel_tb02.Text != "-")
                {
                    try
                    {
                        i = Convert.ToInt16(changeAngel_tb02.Text);
                    }
                    catch
                    {
                        MessageBox.Show("输入角度范围为：-45°到 225°！");
                        changeAngel_tb02.Text = "";
                        return;
                    }

                    if (LMS1xx == 1)
                    {
                        if (i < -45 | i > 225)
                        {
                            MessageBox.Show("输入角度范围为：-5°到 185°！");
                            changeAngel_tb01.Text = "";
                        }
                    }
                    else if (LMS5xx == 1)
                    {
                        if (i < -5 | i > 185)
                        {
                            MessageBox.Show("输入角度范围为：-45°到 225°！");
                            changeAngel_tb01.Text = "";
                        }
                    }
                }
            }
        }

        private void LMSxxSetUp_FormClosed_1(object sender, FormClosedEventArgs e)
        {
            if (thread_creat_server != null)
            {
                thread_creat_server.Abort();
                thread_creat_server.Join();
            }
            close_flag = 0;
            new dtcl().Show();
        }

        private void ipAddr_txt1_TextChanged(object sender, EventArgs e)
        {
            if (ipAddr_txt1.Text != "")
            {
                try
                {
                    Convert.ToInt32(ipAddr_txt1.Text);
                }
                catch
                {
                    MessageBox.Show("ip地址输入有误！");
                    ipAddr_txt1.Text = "";
                    return;
                }
                if (Convert.ToInt32(ipAddr_txt1.Text) > 255 || Convert.ToInt32(ipAddr_txt1.Text) < 0)
                {
                    MessageBox.Show("ip地址输入有误！");
                    ipAddr_txt1.Text = "";
                    return;
                }
            }
        }

        private void ipAddr_txt2_TextChanged(object sender, EventArgs e)
        {

            if (ipAddr_txt2.Text != "")
            {
                try
                {
                    Convert.ToInt32(ipAddr_txt2.Text);
                }
                catch
                {
                    MessageBox.Show("ip地址输入有误！");
                    ipAddr_txt2.Text = "";
                    return;
                }
                if (Convert.ToInt32(ipAddr_txt2.Text) > 255 || Convert.ToInt32(ipAddr_txt2.Text) < 0)
                {
                    MessageBox.Show("ip地址输入有误！");
                    ipAddr_txt2.Text = "";
                    return;
                }
            }
        }

        private void ipAddr_txt3_TextChanged(object sender, EventArgs e)
        {
            if (ipAddr_txt3.Text != "")
            {
                try
                {
                    Convert.ToInt32(ipAddr_txt3.Text);
                }
                catch
                {
                    MessageBox.Show("ip地址输入有误！");
                    ipAddr_txt3.Text = "";
                    return;
                }
                if (Convert.ToInt32(ipAddr_txt3.Text) > 255 || Convert.ToInt32(ipAddr_txt3.Text) < 0)
                {
                    MessageBox.Show("ip地址输入有误！");
                    ipAddr_txt3.Text = "";
                    return;
                }
            }
        }

        private void ipAddr_txt4_TextChanged(object sender, EventArgs e)
        {
            if (ipAddr_txt4.Text != "")
            {
                try
                {
                    Convert.ToInt32(ipAddr_txt4.Text);
                }
                catch
                {
                    MessageBox.Show("ip地址输入有误！");
                    ipAddr_txt4.Text = "";
                    return;
                }
                if (Convert.ToInt32(ipAddr_txt4.Text) > 255 || Convert.ToInt32(ipAddr_txt4.Text) < 0)
                {
                    MessageBox.Show("ip地址输入有误！");
                    ipAddr_txt4.Text = "";
                    return;
                }
            }
        }

        private void scaling_txt_TextChanged(object sender, EventArgs e)
        {
            if (scaling_txt.Text != "")
            {
                try {
                    scaling = Convert.ToDouble(scaling_txt.Text);
                    hScrollBar1.Value = (int)(scaling * 100);
                }
                catch
                {
                    MessageBox.Show("老兄，你在搞怪埋？");
                    scaling_txt.Text = "";
                    return;
                }
            }
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            double scale_p = (double)(hScrollBar1.Value) / 100.0;
            scaling = Convert.ToDouble(scale_p);
            scaling_txt.Text = scale_p.ToString();
        }

        private void qsjd_txt_TextChanged(object sender, EventArgs e)
        {
            int i;
            if(cxjd_txt.Text != "")
            {
                if (qsjd_txt.Text != "")
                {
                    try
                    {
                        i = Convert.ToInt32(qsjd_txt.Text);
                        if (i > 225 || i < -45)
                        {
                            MessageBox.Show("输入起始角度大小有误！");
                            qsjd_txt.Text = "";
                            return;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("输入起始角度大小有误！");
                        cxjd_txt.Text = "";
                        return;
                    }

                    hScrollBar2.Value = i;
                    cx_sAngel = (int)(i);
                    cx_eAngel =(int) (cx_sAngel + Convert.ToInt32(cxjd_txt.Text));
                }
            }
        }

        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            int scaling2;
            scaling2 = Convert.ToInt32(hScrollBar2.Value);
            qsjd_txt.Text = hScrollBar2.Value.ToString();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void lbxs_btn_Click(object sender, EventArgs e)
        {
            if (lbxs_btn.Text == "滤波显示")
            {
                thread_creat_server = new Thread(Thread_Server_Listen_process);
                thread_creat_server.Start();
                lbxs_btn.Text = "停止显示";
                button1.Enabled = false;
            }
            else if (lbxs_btn.Text == "停止显示")
            {
                thread_creat_server.Abort();
                thread_creat_server.Join();
                lbxs_btn.Text = "滤波显示";
                button1.Enabled = true;
            }

        }

        private void saveSet_bto_Click(object sender, EventArgs e)
        {
            string parameters;

            try {
                parameters = dtcl_X1_txt.Text + " " + dtcl_X2_txt.Text + " " + dtcl_Y1_txt.Text + " " + dtcl_Y2_txt.Text + " " 
                            + changeAngel_tb01.Text + " " + changeAngel_tb02.Text;
                parameter.SaveProcess("LMSxxparameter(重要资料，请勿删改！！！).txt", parameters);
                MessageBox.Show("保存成功！");
            }
            catch
            {
                MessageBox.Show("保存失败,请检查对应文件是否存在！");
            }
    }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int i;
            if(qsjd_txt.Text != "")
            {
                if (cxjd_txt.Text != "")
                {
                    try
                    {
                        i = Convert.ToInt32(cxjd_txt.Text);
                        if (i > 225 - cx_sAngel)
                        {
                            MessageBox.Show("输入角度大小有误！");
                            cxjd_txt.Text = "";
                            return;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("输入角度大小有误！");
                        cxjd_txt.Text = "";
                        return;
                    }

                    cx_eAngel = (int)(cx_sAngel + i);
                }
            }
        }
    }


    

    /**
     * New Server Class for service to client
     */
    public class ClientThread
    {
        public static int connections = 0;
        public Socket service;
        int i;

        public ClientThread(Socket clientsocket)
        {
            this.service = clientsocket;
        }

        public void ClientService()
        {
            String data = null;
            byte[] bytes = new byte[2048];

            if (service != null)
            {
                connections++;
                Console.WriteLine("current connect:" + connections);
            }

            try
            {
                while ((i = service.Receive(bytes)) != 0)
                {
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("REV:" + data);
                    data = data.ToUpper();
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                    service.Send(msg);
                    Console.WriteLine("SET:" + data);
                }
            }
            catch
            {
                service.Close();
                connections--;
                Console.WriteLine("current connect:" + connections);
                return;
            }

            service.Close();
            connections--;
            Console.WriteLine("current connect:" + connections);
        }
    }
}
