using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class ChooseDriver : Form
    {
        FilterInfo info;
        FilterInfoCollection videoDevices;

        byte close_flag = 1;//关闭标志：值为1时，关闭窗口会弹出关闭提示框；为0时不弹提示框

        public ChooseDriver()
        {
            InitializeComponent();
        }

        private void ChooseDriver_Load(object sender, EventArgs e)
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);//作为摄像头驱动的收集

                int i = 0;

                foreach (FilterInfo device in videoDevices)
                {
                    ChooseDriver_cbx.Items.Add(videoDevices[i].Name.ToString());//将视频驱动依次放入ChooseDriver_cbx
                    i++;
                }

                ChooseDriver_cbx.SelectedIndex = 1;//默认第二个驱动
            }
            catch
            {
                ChooseDriver_cbx.SelectedIndex = 0;
            }
        }

        private void queding_btn_Click(object sender, EventArgs e)
        {
            close_flag = 0;
            info = videoDevices[ChooseDriver_cbx.SelectedIndex];
            jqcl g = new jqcl();
            g.Show();//打开jqcl.cs
            this.Close();//关闭本文件
            
        }

        private void ChooseDriver_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (close_flag == 1)
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

        private void ChooseDriver_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (close_flag == 1)
            {
                Application.Exit();
            }
        }
    }
}
