using System;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace gzdemo
{
    public partial class FormMain : Form
    {
        private OraDb oraData;
        private SqlDataLayer sqlConn;
        private string strTB;

        //声明读写INI文件的API函数
        [DllImport("kernel32")]
        private static extern long CompareFileTime(FILETIME lpFileTime1, FILETIME lpFileTime2);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string filePath);

        public FormMain()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;// 注意现在此阶段需要将线程保护标志 标记成false
            strTB = "gztb100";            
        }

        // 更新查询组合框显示
        private void UpdateGroupCtrl()
        {
            labelID.Enabled = radioButtonQuery.Checked;
            textBoxID.Enabled = radioButtonQuery.Checked;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {        

            Application.Exit();
        }

        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormLogin logFrm = new FormLogin(this);
            logFrm.Show();
        }

        // login
        public bool loginOracle(string strDB, string strUsername, string strPassword)
        {
            bool bOpened  = false;
            bool bCreated = false;
            string strstatus = "连接失败";
            try
            {
                string connStr = "Data Source=" + strDB + "; User=" + strUsername + ";Password=" + strPassword + 
                    ";Max Pool Size=500;";

// 连接远程数据库
//                string connStr = "Data Source=LOCAL_TEST; User=ctais2;Password=oracle;Max Pool Size=500;";
//                 string connStr = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.4.105)" +
//                                 "(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=CTAIS2)));Persist Security Info=True;User Id=ctais2; Password=oracle"; 
                if (oraData == null)
                {
                    oraData = new OraDb(connStr);
                    bCreated = true;
                }
                if (oraData.connection.State == ConnectionState.Closed)
                {
                    oraData.connection.Open();
                }                    

                if (oraData.connection.State == ConnectionState.Open)
                {
                    bOpened = true;
                    strstatus = "用户" + strUsername + "已连接" + strDB + "数据库";
                    if (bCreated == false)
                    {
                        strstatus = "用户" + "第二次连接";
                    }
                }
            }
            catch (System.Exception ex)
            {
            	
            }
            loginStatusToolStripStatusLabel.Text = strstatus;
            return bOpened;
        }

        // loginSqlServer
        public bool loginSqlServer(string strDB)
        {
            bool bOpened = false;
            bool bCreated = false;
            string strstatus = "连接失败";
            try
            {
                string connStr = strDB;

                // 连接远程数据库
                //                string connStr = "Data Source=LOCAL_TEST; User=ctais2;Password=oracle;Max Pool Size=500;";
                //                 string connStr = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.4.105)" +
                //                                 "(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=CTAIS2)));Persist Security Info=True;User Id=ctais2; Password=oracle"; 
                if (sqlConn == null  
                    ||
                    sqlConn.GetConnection().State ==ConnectionState.Closed
                    ||
                    connStr != sqlConn.ConnectionString)// 三种判断条件
                {
                    try
                    {
                        sqlConn = new SqlDataLayer(connStr);
                    }
                    catch
                    {
                        throw new Exception("错误信息");
                    }
                    bCreated = true;
                }
                if (sqlConn == (SqlDataLayer)null)
                {
                     throw new Exception("错误信息");
                }
                try
                {
                    if (sqlConn.connection.State == ConnectionState.Closed)
                    {
                        sqlConn.connection.Open();
                    }
                }
                catch (Exception ex_sqlstate)
                {
                    throw new Exception(ex_sqlstate.Message);
                }

                if (sqlConn.connection.State == ConnectionState.Open)
                {
                    bOpened = true;
                    strstatus = "已连接数据库";
                    if (bCreated == false)
                    {
                        strstatus = "用户" + "第二次连接";
                    }
                }
            }
            catch (System.Exception ex)
            {
                strstatus = ex.Message;
                MessageBox.Show(ex.Message);
                //throw new Exception(ex.Message);
            }
            finally
            {
                loginStatusToolStripStatusLabel.Text = strstatus;
            }
            return bOpened;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dataSet1.GZTB11' table. You can move, or remove it, as needed.
            //this.gZTB11TableAdapter.Fill(this.dataSet1.GZTB11);

            textBoxTb.Text = strTB;
            radioButtonQueryAll.Checked = true;
            UpdateGroupCtrl();
        }

        private void disconnectToolStripButton_Click(object sender, EventArgs e)
        {
            if (oraData.GetConnection().State == ConnectionState.Open)
            {
                oraData.CloseConnection();
            }
            loginStatusToolStripStatusLabel.Text = "关闭数据库连接成功"; 
        }

        private void createTbToolStripButton_Click(object sender, EventArgs e)
        {
            string strstatus = "创建数据库表失败";
            try
            {
                string cmmdStr = "create table " + strTB + "(id integer, name char(50), sex char(10), age integer, banji char(50))";
                oraData.RunNonQuery(cmmdStr);
                strstatus = "创建数据库表成功"; 
            }
            catch (System.Exception ex)
            {
                
            }
            loginStatusToolStripStatusLabel.Text = strstatus;        
        }

        private void insertToolStripButton_Click(object sender, EventArgs e)
        {
            string strstatus = "插入数据失败";
            try
            {
                string cmmdStr;
                for (int i = 1; i <= 30; i++)
                {
                    cmmdStr = "insert into " + strTB + "(id, name, sex, age, banji) values("
                    + "'" + i.ToString() + "',"
                    + "'name" + i.ToString() + "',"
                    + "'female',"
                    + "'" + (20 + i).ToString() + "',"
                    + "'class" + i.ToString() + "')";
                    oraData.RunNonQuery(cmmdStr);
                }

                strstatus = "插入数据成功"; 
            }
            catch (System.Exception ex)
            {
                
            }
            loginStatusToolStripStatusLabel.Text = strstatus;
        }

        private void queryToolStripButton_Click(object sender, EventArgs e)
        {
            string strstatus = "查询显示失败";
            try
            {
                string cmmdStr;
                if (radioButtonQueryAll.Checked)
                {
                    cmmdStr = "select * from " + strTB;                        
                }
                else
                {
                    cmmdStr = "select * from " + strTB + " where rownum <= " + textBoxID.Text;
                }
                dataGridView1.DataSource = oraData.FillTable(cmmdStr);                    

                // 设置第一列为只读属性
                dataGridView1.Columns[0].ReadOnly = true;
                dataGridView1.Columns[0].DefaultCellStyle.BackColor = Color.Turquoise;
                strstatus = "查询显示成功";
            }
            catch (System.Exception ex)
            {

            }
            loginStatusToolStripStatusLabel.Text = strstatus;
        }

        private void delCurSelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string strstatus = "删除失败";
            try
            {
                string cmmdStr;
                cmmdStr = "select * from " + strTB;
                if (dataGridView1.SelectedCells == null)
                {
                }
                else
                {
                    if (dataGridView1.CurrentCell.ColumnIndex == 0)
                    {
                        string strId = this.dataGridView1[0, this.dataGridView1.CurrentCell.RowIndex].Value.ToString();
                        cmmdStr = "delete from " + strTB + " where id = " + strId;
                        oraData.RunNonQuery(cmmdStr);

                        //更新界面显示
                        cmmdStr = "select * from " + strTB;
                        dataGridView1.DataSource = oraData.FillTable(cmmdStr);

                        strstatus = "删除成功";
                    }
                    else
                    {
                        MessageBox.Show("请选择第一列再执行删除操作");
                    }                        
                }
            }
            catch (System.Exception ex)
            {

            }
            loginStatusToolStripStatusLabel.Text = strstatus;
        }

        private void delAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string strstatus = "删除失败";
            try
            {
                string cmmdStr = "truncate table " + strTB;
                oraData.RunNonQuery(cmmdStr);

                //更新界面显示
                cmmdStr = "select * from " + strTB;
                dataGridView1.DataSource = oraData.FillTable(cmmdStr);

                strstatus = "删除成功";
            }
            catch (System.Exception ex)
            {

            }
            loginStatusToolStripStatusLabel.Text = strstatus;
        }

        private void updateToolStripButton_Click(object sender, EventArgs e)
        {
            string strstatus = "更新失败";
            try
            {
                string cmmdStr;
                if (dataGridView1.SelectedCells == null)
                {
                }
                else
                {
                    int nCurRowIndex = this.dataGridView1.CurrentCell.RowIndex;
                    string strId = this.dataGridView1[0, nCurRowIndex].Value.ToString().Trim();
                    string strName = this.dataGridView1[1, nCurRowIndex].Value.ToString().Trim();
                    string strSex = this.dataGridView1[2, nCurRowIndex].Value.ToString().Trim();
                    string strAge = this.dataGridView1[3, nCurRowIndex].Value.ToString().Trim();
                    string strClass = this.dataGridView1[4, nCurRowIndex].Value.ToString().Trim();

                    cmmdStr = "update " + strTB + " set name='" + strName +
                                            "', sex='" + strSex +
                                            "', age='" + strAge +
                                            "', banji='" + strClass +
                                            "' where id=" + strId;
                    oraData.RunNonQuery(cmmdStr);

                    strstatus = "更新成功";
                }
            }
            catch (System.Exception ex)
            {

            }
            loginStatusToolStripStatusLabel.Text = strstatus;
        }

        private void radioButtonQuery_CheckedChanged(object sender, EventArgs e)
        {
            UpdateGroupCtrl();
        }

        private void textBoxID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < 48 || e.KeyChar > 57) && e.KeyChar != 8)
            {
                e.Handled = true;
            }
        }

        private void addToolStripButton_Click(object sender, EventArgs e)
        {
            string strstatus = "添加失败";
            try
            {
                string cmmdStr;
                if (dataGridView1.SelectedCells == null)
                {
                }
                else
                {
                    int nCurRowIndex = this.dataGridView1.CurrentCell.RowIndex;
                    //string strId = this.dataGridView1[0, nCurRowIndex].Value.ToString().Trim();
                    string strId = "100";
                    string strName = this.dataGridView1[1, nCurRowIndex].Value.ToString().Trim();
                    string strSex = this.dataGridView1[2, nCurRowIndex].Value.ToString().Trim();
                    string strAge = this.dataGridView1[3, nCurRowIndex].Value.ToString().Trim();
                    string strClass = this.dataGridView1[4, nCurRowIndex].Value.ToString().Trim();

                    cmmdStr = "insert into " + strTB + "(id, name, sex, age, banji) values("
                    + "'" + strId + "',"
                    + "'" + strName + "',"
                    + "'" + strSex + "',"
                    + "'" + strAge + "',"
                    + "'" + strClass + "')";
                    oraData.RunNonQuery(cmmdStr);

                    //更新界面显示
                    cmmdStr = "select * from " + strTB;
                    dataGridView1.DataSource = oraData.FillTable(cmmdStr);

                    strstatus = "更新成功";
                }
            }
            catch (System.Exception ex)
            {

            }
            loginStatusToolStripStatusLabel.Text = strstatus;
        }

        private void buttonTB_Click(object sender, EventArgs e)
        {
            strTB = textBoxTb.Text;
        }

        private void delTBToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string strstatus = "删除当前表失败";
            try
            {
                //string cmmdStr = "drop table " + strTB;
                //oraData.RunNonQuery(cmmdStr);

                DataTable dt = new DataTable();
                this.dataGridView1.DataSource = dt;                
                //strstatus = "删除当前表成功";
            }
            catch (System.Exception ex)
            {

            }
            loginStatusToolStripStatusLabel.Text = strstatus;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBoxID_TextChanged(object sender, EventArgs e)
        {

        }

        private void labelID_Click(object sender, EventArgs e)
        {

        }

        private void textBoxTb_TextChanged(object sender, EventArgs e)
        {

        }


        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void CfgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormCfg frmCfg= new FormCfg(this);
            frmCfg.initForm();
            frmCfg.Show();

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
            
        }

        private void CheckButton(string status)
        {
            if (status.ToUpper() == "START")
            {
                this.StarttoolStripButton.Enabled = false;
                this.PausetoolStripButton.Enabled = true;
                this.StoptoolStripButton.Enabled = true;
            }

            if (status.ToUpper() == "PAUSE")
            {
                this.StarttoolStripButton.Enabled = true;
                this.PausetoolStripButton.Enabled = false;
                this.StoptoolStripButton.Enabled = true;
            }

            if (status.ToUpper() == "STOP")
            {
                this.StarttoolStripButton.Enabled = true;
                this.PausetoolStripButton.Enabled = false;
                this.StoptoolStripButton.Enabled = false;
            }

            if (status.ToUpper() == "STOPING")
            {
                this.StarttoolStripButton.Enabled = false;
                this.PausetoolStripButton.Enabled = false;
                this.StoptoolStripButton.Enabled = false;
            }
        }
        private void StarttoolStripButton_Click(object sender, EventArgs e)
        {
            /*do
            {

            } while (this.bgWorker.IsBusy);*/
            Thread.Sleep(2000);  // 线程睡眠2秒，防止重复的后台线程开启
            CheckButton("START");// 启动按钮
            try
            {
                this.bgWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                //this.bgWorker.CancelAsync();
                MessageBox.Show(ex.Message);
                return;
            }
            loginStatusToolStripStatusLabel.Text = "运行中...";
            this.tmrSender.Enabled = true;
            return;

        }


        private void PausetoolStripButton_Click(object sender, EventArgs e)
        {
            CheckButton("PAUSE");// 暂停按钮--(暂时可能没用)
        }

        private void StoptoolStripButton_Click(object sender, EventArgs e)
        {
            CheckButton("STOP");// 停止按钮
            if (this.bgWorker.IsBusy)
            {
                this.bgWorker.CancelAsync();
            }
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IniFiles inifile = new IniFiles(Properties.Resources.cfgname);// 获取配置文件
            string   sFile;
            //object locker = new object();
            string dataFile;
            DateTime DateCurr,DateComp;

            dataFile = inifile.File;// 配置文件名称
            DateCurr = File.GetLastWriteTime(dataFile);
            
            sFile = inifile.File;
            try
            {
                // 登录SqlServer
                if (this.loginSqlServer(inifile.DbConn) == false)
                {
                    sqlConn.CloseConnection();
                    MessageBox.Show("连接数据库失败");
                    // this.bgWorker.CancelAsync();
                    e.Cancel = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                sqlConn.CloseConnection();
                MessageBox.Show(ex.Message);
                this.bgWorker.CancelAsync();
                return;
            }
            //  CheckButton("START");// 启动按钮
            //  dataString = File.GetLastWriteTime(inifile.File).ToLongDateString();// 线程不安全
            //  dataString_tmp = dataString;
            while (true)
            {
              //  dataString = File.GetLastWriteTime(inifile.File).ToLongDateString(); ;

                if (this.bgWorker.CancellationPending)// 需要自己判断状态并退出
                {
                    e.Cancel = true;
                    return;
                }
                DateComp = File.GetLastWriteTime(dataFile);
                if (DateCurr.CompareTo(DateComp) == 0)
                {
                    continue;
                }
                else
                {
                    MessageBox.Show("文件已经更改");
                    break;
                }
                DateCurr = DateComp;

               /* if (dataString == dataString_tmp)
                {
                    continue;
                }
                dataString_tmp = dataString;*/
            }
        }
        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
           // sqlConn.CloseConnection();---这里不需要释放连接
            string strstatus;
            strstatus = "结束";
            CheckButton("STOP");
            loginStatusToolStripStatusLabel.Text = strstatus;
            this.tsProgressBar.Value = 0;
            this.tmrSender.Enabled = false;

        }

        // 定时器
        private void tmrSender_Tick(object sender, EventArgs e)
        {
            if (this.tsProgressBar.Value == 100)
            {
                this.tsProgressBar.Value = 0;
            }
            else
            {
                this.tsProgressBar.Value++;
            }
        }

    }
}
