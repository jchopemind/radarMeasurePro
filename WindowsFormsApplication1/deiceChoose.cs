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
    public partial class deviceChoose : Form
    {
        public deviceChoose()
        {
            InitializeComponent();
        }
        byte close_flag = 1;

        private void deiceChoose_Load(object sender, EventArgs e)
        {
            deviceChoose_cbo.SelectedIndex = 1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(deviceChoose_cbo.Text != "")
            {
                LMSxx_SetUp fm = new LMSxx_SetUp();

                if (deviceChoose_cbo.Text == "LMS1xx")
                {
                    fm.LMS1xx = 1;
                }
                if (deviceChoose_cbo.Text == "LMS5xx")
                {
                    fm.LMS5xx = 1;
                }

                fm.StartPosition = FormStartPosition.CenterScreen;
                fm.Show();
                close_flag = 0;
                this.Close();
            }
            else
            {
                MessageBox.Show("请先选择雷达型号！");
            }
        }

        private void deviceChoose_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (close_flag == 1)
            new dtcl().Show();
        }
    }
}
