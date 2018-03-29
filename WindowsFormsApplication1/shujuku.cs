using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    class shujuku
    {
        public Boolean Add(int id_num, string data)//数据库添加
        {

            Boolean tempvalue = false; //定义返回值，并设置初值
                                       //下面把note中的数据添加到数据库中！


            OleDbConnection conn = getConn(); //getConn():得到连接对象
            conn.Open();

            //设置SQL语句
            //string insertstr = "INSERT INTO A (姓名, 年龄, 班级, 性别 ) VALUES ('";
            //insertstr += "周军" + "', '";
            //insertstr +=  "19"+ "','";
            //insertstr +=  "2班"+ "','";
            //insertstr += "男" + "')";


            string insertstr = "INSERT INTO 表1 (序号,1 ) VALUES ('" + id_num.ToString() + "', '" + data + "')";


            OleDbCommand insertcmd = new OleDbCommand(insertstr, conn);
            insertcmd.ExecuteNonQuery();

            conn.Close();
            tempvalue = true;

            return (tempvalue);
        }

        public Boolean Add(string data)//数据库添加
        {

            Boolean tempvalue = false; //定义返回值，并设置初值
                                       //下面把note中的数据添加到数据库中！


            OleDbConnection conn = getConn(); //getConn():得到连接对象
            conn.Open();

            //设置SQL语句
            //string insertstr = "INSERT INTO A (姓名, 年龄, 班级, 性别 ) VALUES ('";
            //insertstr += "周军" + "', '";
            //insertstr +=  "19"+ "','";
            //insertstr +=  "2班"+ "','";
            //insertstr += "男" + "')";


            string insertstr = "INSERT INTO S (区间站场, 车间, 锚段, 工区, 行别, 支柱号, 定位点距离, 导高1, 拉出值1, 导高2, 拉出值2, 限界, 坡度1, 坡度2, 高差1, 高差2, 超高, 轨距, 时间) VALUES (" + data + ",'" + DateTime.Now.ToLocalTime().ToString() + "')";


            OleDbCommand insertcmd = new OleDbCommand(insertstr, conn);
            insertcmd.ExecuteNonQuery();

            conn.Close();
            tempvalue = true;

            return (tempvalue);
        }
        /*返回数据库连接对象*/
        public OleDbConnection getConn()//返回一个connection对象
        {
            string connstr = "Provider=Microsoft.Jet.OLEDB.4.0 ;Data Source = " + Application.StartupPath + @"\\shujuku.mdb";
            OleDbConnection tempconn = new OleDbConnection(connstr);
            return (tempconn);
        }
        /*返回一个数据表*//*输入参数为sqlstr查询语句*/
        public DataTable getNoteList(string sqlstr)//返回一个数据表
        {

            System.Data.DataSet mydataset; //定义DataSet

            try
            {
                OleDbConnection conn = getConn(); //getConn():得到连接对象
                conn.Open();

                OleDbDataAdapter adapter = new OleDbDataAdapter();
               
                mydataset = new System.Data.DataSet();
                adapter.SelectCommand = new OleDbCommand(sqlstr, conn);
                adapter.Fill(mydataset, "S");//这里是在mydataset里新建了一个表，叫Student,S，注意是新建，多次执行会报错，实际使用时，可以用contain来判断是否存在同名的表
                // mydataset.Tables[0]
                //sqlstr = "select Student.id  from Student,S where S.姓名 = Student.姓名";//
                //adapter.SelectCommand = new OleDbCommand(sqlstr, conn);
                //adapter.Fill(mydataset, "1");// mydataset.Tables[1]

                conn.Close();
            }
            catch (Exception e)
            {
                throw (new Exception("数据库出错:" + e.Message));
            }
            return mydataset.Tables[0];
        }


    }
}
