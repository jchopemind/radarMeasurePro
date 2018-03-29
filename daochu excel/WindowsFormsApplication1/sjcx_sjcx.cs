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

    public partial class sjcx_sjcx : Form
    {
        byte close_flag = 1;
        public sjcx_sjcx()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void sjcx_sjcx_Load(object sender, EventArgs e)
        {

        }

        private void sjcx_sjcx_FormClosing(object sender, FormClosingEventArgs e)
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

        private void sjcx_sjcx_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (close_flag == 1)
            {
                Application.Exit();
            }
        }
    }
}
