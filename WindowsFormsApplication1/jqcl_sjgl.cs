using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;

namespace WindowsFormsApplication1
{
    public partial class jqcl_sjgl : Form
    {
        byte close_flag = 1;
        string sqlstr = "";
        shujuku database_operate;
        public jqcl_sjgl()
        {
            InitializeComponent();
        }

        private void jqcl_sjgl_FormClosing(object sender, FormClosingEventArgs e)
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

        private void jqcl_sjgl_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (close_flag == 1)
            {
                Application.Exit();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            close_flag = 0;
            this.Close();
            new jqcl().Show();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void jqcl_sjgl_Load(object sender, EventArgs e)
        {
            sjcx_dtp.Value = DateTime.Now;   //获取当前日期       
            sjcx_dtp1.Value = DateTime.Now;
            sjcx_pnel.Visible = false;
            database_operate = new shujuku();    
        }

        private void sjcx_sjdc_bto_Click(object sender, EventArgs e)
        {
            OleDbConnection con = new OleDbConnection();

            DialogResult result = MessageBox.Show("是否建立新表？", "提示信息", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)//建立一个新表
            {
                try
                {
                    SaveFileDialog saveFile = new SaveFileDialog();
                    saveFile.Filter = ("Excel 文件(*.xls)|*.xls");//指定文件后缀名为Excel 文件。  
                    if (saveFile.ShowDialog() == DialogResult.OK)
                    {
                        string filename = saveFile.FileName;

                        if (System.IO.File.Exists(filename))
                        {
                            System.IO.File.Delete(filename);//如果文件存在删除文件。  
                        }

                        int index = filename.LastIndexOf("//");//获取最后一个/的索引  
                        filename = filename.Substring(index + 1);//获取excel名称(新建表的路径相对于SaveFileDialog的路径)  

                        //select * into 建立 新的表。  
                        //[[Excel 8.0;database= excel名].[sheet名] 如果是新建sheet表不能加$,如果向sheet里插入数据要加$.　  
                        //sheet最多存储65535条数据。  
                        //                    select into语句同时具备两个功能： 
                        //根据select后跟的字段以及into后面跟的表名建立空表（如果select后是 *, 空表的结构和from所指的表的结构相同）； 
                        //将select查出的数据插入到这个空表中。在使用select into语句时，into后跟的表必须在数据库不存在，否则出错，下面是一个使用select into的例子。   
                        //假设有一个表table1，字段为f1(int)、f2(varchar(50))。 select* intotable2 fromtable1 这条sql语的在建立table2表后，将table1的数据全部插入到table1中的，还可以将* 改为f1或f2以便向适当的字段中插入数据。  
                        //select into不仅可以在同一个数据中建立表，也可以在不同的sql server数据库中建立表
                        try
                        {
                            string sql = "select top 65535 *  into   [Excel 8.0;database=" + filename + "].[姓名] from S where 序号 = 1";//导入一个空表

                            // string sql = "insert into   [Excel 8.0;database=C:\\Users\\y\\Desktop\\工作簿1.xls].[姓名] select * from Student";//插入表
                            con.ConnectionString = "Provider=Microsoft.Jet.Oledb.4.0;Data Source=" + Application.StartupPath + "//shujuku.mdb";//将数据库放到debug目录下。  
                            OleDbCommand com = new OleDbCommand(sql, con);
                            con.Open();
                            com.ExecuteNonQuery();

                            MessageBox.Show("导出数据成功，保存路径为：" + filename, "导出数据", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch
                        {
                            MessageBox.Show("导出数据失败！");
                            return;
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    con.Close();
                }
            }
            /*导出数据插入到一个表中*/
            else
            {
                try
                {
                    OpenFileDialog fileDialog = new OpenFileDialog();
                    fileDialog.Multiselect = true;
                    fileDialog.Title = "请选择文件";
                    fileDialog.Filter = "所有文件(*.*)|*.*";
                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string[] names = fileDialog.FileNames;

                        foreach (string file in names)
                        {
                            string sql = "insert into   [Excel 8.0;database=" + file + "].[姓名] select * from Student";//插入表
                            con.ConnectionString = "Provider=Microsoft.Jet.Oledb.4.0;Data Source=" + Application.StartupPath + "//shujuku.mdb";//将数据库放到debug目录下。  
                            OleDbCommand com = new OleDbCommand(sql, con);
                            con.Open();
                            com.ExecuteNonQuery();
                            con.Close();
                        }
                        MessageBox.Show("导出数据成功", "导出数据", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("导出数据失败！该表中可能不存在子表！");
                    return;
                }
            }
        }

        private void sjcx_sjcx_bto_Click(object sender, EventArgs e)
        {
            sqlstr = "";
            //shujuchaxun f2 = new shujuchaxun();
           
            if (sjcx_qj_cbo.Text != "")//区间站场
            {
                sqlstr = "select *  from S where 区间站场 = '" + sjcx_qj_cbo.Text + "'";//SQL语句
            }

            if (sjcx_md_cbo.Text != "")//锚段
            {
                if (sqlstr != "")
                {
                    if (sqlstr.Contains("select"))
                    {
                        sqlstr = sqlstr + "and 锚段 = '" + sjcx_md_cbo.Text + "'";//SQL语句
                    }
                    else
                    {
                        sqlstr = "select * from S where 锚段 = '" + sjcx_md_cbo.Text + "'";//SQL语句
                    }
                }
                else
                {
                    sqlstr = "select * from S where 锚段 = '" + sjcx_md_cbo.Text + "'";//SQL语句
                }

            }
            //if(data2.Text != "")//测量员
            //{
            //    sqlstr = sqlstr + "and 测量员 ="
            //}
            //if(data3.Text != "")//起始杆号
            //{
            //    sqlstr = sqlstr + "and "
            //}
            //if (data4.Text != "")//结束杆号
            //{
            //    sqlstr = sqlstr + "and "
            //}
            if (sjcx_dtp.Text != "" && sjcx_dtp1.Text != "")
            {
                if (sqlstr != "")
                {
                    if (sqlstr.Contains("select"))
                    {
                        sqlstr = sqlstr + " and 时间 >= #" + sjcx_dtp.Value + "# and 时间 <= #" + sjcx_dtp1.Value + "#";
                        //sqlstr = sqlstr + " and 时间 between #" + sjcx_dtp.Value.ToString("yyy/MM/dd") + "# and # " + sjcx_dtp1.Value.ToString("yyy/MM/dd") + "#";

                    }
                    else
                    {
                        sqlstr = "select *  from S where 时间 >= #" + sjcx_dtp.Value.ToString("yyy/MM/dd") + "# and 时间 <= #" + sjcx_dtp1.Value.ToString("yyy/MM/dd") + "#";//SQL语句
                    }
                }
                else
                {
                    sqlstr = "select *  from S where 时间 >= #" + sjcx_dtp.Value.ToString("yyy/MM/dd") + "# and 时间 <= #" + sjcx_dtp1.Value.ToString("yyy/MM/dd") + "#";//SQL语句
                }
            }

            sqlstr = sqlstr + "order by 序号 asc";
            sjcx_datatable.DataSource = database_operate.getNoteList(sqlstr);
            sjcx_pnel.Visible = true;
         
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void sjcx_sjdr_bto_Click(object sender, EventArgs e)
        {

        }

        private void sjcx_fh_bto_Click(object sender, EventArgs e)
        {
            sjcx_pnel.Visible = false;
        }

        private void sjcx_dc_bto_Click(object sender, EventArgs e)
        {
            OleDbConnection con = new OleDbConnection();

            DialogResult result = MessageBox.Show("是否建立新表？", "提示信息", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)//建立一个新表
            {
                try
                {
                    SaveFileDialog saveFile = new SaveFileDialog();
                    saveFile.Filter = ("Excel 文件(*.xls)|*.xls");//指定文件后缀名为Excel 文件。  
                    if (saveFile.ShowDialog() == DialogResult.OK)
                    {
                        string filename = saveFile.FileName;

                        if (System.IO.File.Exists(filename))
                        {
                            System.IO.File.Delete(filename);//如果文件存在删除文件。  
                        }

                        int index = filename.LastIndexOf("//");//获取最后一个/的索引  
                        filename = filename.Substring(index + 1);//获取excel名称(新建表的路径相对于SaveFileDialog的路径)  

                        //select * into 建立 新的表。  
                        //[[Excel 8.0;database= excel名].[sheet名] 如果是新建sheet表不能加$,如果向sheet里插入数据要加$.　  
                        //sheet最多存储65535条数据。  
                        //                    select into语句同时具备两个功能： 
                        //根据select后跟的字段以及into后面跟的表名建立空表（如果select后是 *, 空表的结构和from所指的表的结构相同）； 
                        //将select查出的数据插入到这个空表中。在使用select into语句时，into后跟的表必须在数据库不存在，否则出错，下面是一个使用select into的例子。   
                        //假设有一个表table1，字段为f1(int)、f2(varchar(50))。 select* intotable2 fromtable1 这条sql语的在建立table2表后，将table1的数据全部插入到table1中的，还可以将* 改为f1或f2以便向适当的字段中插入数据。  
                        //select into不仅可以在同一个数据中建立表，也可以在不同的sql server数据库中建立表
                        try
                        {
                            string sql = "select top 65535 *  into   [Excel 8.0;database=" + filename + "].[表1] " + sqlstr.Remove(0, 8);//导入一个空表

                            // string sql = "insert into   [Excel 8.0;database=C:\\Users\\y\\Desktop\\工作簿1.xls].[姓名] select * from Student";//插入表
                            con.ConnectionString = "Provider=Microsoft.Jet.Oledb.4.0;Data Source=" + Application.StartupPath + "//shujuku.mdb";//将数据库放到debug目录下。  
                            OleDbCommand com = new OleDbCommand(sql, con);
                            con.Open();
                            com.ExecuteNonQuery();

                            MessageBox.Show("导出数据成功，保存路径为：" + filename, "导出数据", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch
                        {
                            MessageBox.Show("导出数据失败！");
                            return;
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    con.Close();
                }
            }
            /*导出数据插入到一个表中*/
            else
            {
                try
                {
                    OpenFileDialog fileDialog = new OpenFileDialog();
                    fileDialog.Multiselect = true;
                    fileDialog.Title = "请选择文件";
                    fileDialog.Filter = "所有文件(*.*)|*.*";
                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string[] names = fileDialog.FileNames;

                        foreach (string file in names)
                        {
                            string sql = "insert into   [Excel 8.0;database=" + file + "].[表1]" + sqlstr;//插入表
                            con.ConnectionString = "Provider=Microsoft.Jet.Oledb.4.0;Data Source=" + Application.StartupPath + "//shujuku.mdb";//将数据库放到debug目录下。  
                            OleDbCommand com = new OleDbCommand(sql, con);
                            con.Open();
                            com.ExecuteNonQuery();
                            con.Close();
                        }
                        MessageBox.Show("导出数据成功", "导出数据", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("导出数据失败！该表中可能不存在子表！");
                    return;
                }
            }
        }
    }
}
