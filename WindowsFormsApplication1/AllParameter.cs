using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class AllParameter
    {
        public float daogao;//导高
        public float lachuzhi;//拉出值
        public float cemianxianjie;//侧面限界
        public float guiju;//轨距
        public float chaogao;//超高
        public double zengliang;//增量编码器
        public float jueduizhi;//绝对值编码器
        public float qingjiao;//倾角
        public float ngj;//内轨距
        public float tyz;//投影值
        public float pyz;//偏移值 
        public float pdz;//坡度值
        public float gaocha;//高差

        /*单位*/
        public string danwei = " mm";
        public double OneDegree = 0.0174533;//度转换为弧度
        /*指令响应标志位*/
        public byte command6Ack;
        public byte command7Ack;
        /*LMSxx机械参数*/
        public float LMSxx_X1;
        public float LMSxx_X2;
        public float LMSxx_Y1;
        public float LMSxx_Y2;
        /*精测固定机械参数*/
        public float X1;//轨道尺固定测量端到测量仪旋转中心的距离X1(单位mm)
        public float X2;//激光束起始点到测量器旋转中心的水平距离X2(单位mm)
        public float Y1;//轨道平面到测量仪旋转中心的距离Y1(单位mm)
        public float Y2;//激光束起始点到测量器旋转中心的垂直距离Y2(单位mm)
        public float C1;//轨道尺固定测量端到直线传感器零点位置C1(单位mm)
        public float D1;//推行轮直径


        public float zycl_spjj;
        public float zycl_zxjj;
        public float zycl_czjj;
        public float zycl_czjd;

        public byte celiang_flag;
        public byte clEnd_flag;

        /*自定义旋转角度标志*/
        public byte customAngel_flag;
        /*写入数据到文件*/
        public void SaveProcess(string filename, String data)
        {
            string CurDir = System.AppDomain.CurrentDomain.BaseDirectory + @"Parameter\";    //设置当前目录  
            if (!System.IO.Directory.Exists(CurDir)) System.IO.Directory.CreateDirectory(CurDir);   //该路径不存在时，在当前文件目录下创建文件夹

            //不存在该文件时先创建  
            String filePath = CurDir + filename;
            System.IO.StreamWriter file1 = new System.IO.StreamWriter(filePath, false);     //文件已覆盖方式添加内容  

            file1.Write(data);                                                              //保存数据到文件  

            file1.Close();                                                                  //关闭文件  
            file1.Dispose();                                                                //释放对象  
        }


        /*从文件读出数据*/
        public string fileToString(string filename)
        {
            string str = "";

            //获取文件内容  
            if (System.IO.File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"Parameter\" +  filename))
            {
                System.IO.StreamReader file1 = new System.IO.StreamReader(System.AppDomain.CurrentDomain.BaseDirectory + @"Parameter\" + filename);//读取文件中的数据  
                str = file1.ReadToEnd();                                            //读取文件中的全部数据  

                file1.Close();
                file1.Dispose();
            }
            return str;
        }
    }
}

