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
using log4net;// 使用第三方封装的日志类记录日志
using gzdemo.code;
using System.Windows.Forms;

namespace gzdemo
{
    public partial class FormMain : Form
    {

        private OraDb oraData;
        private SqlDataLayer sqlConn;
        private string strTB;
        private int icount = 0;// 计数器计数要插入的数据
        const char SOH = (char)0x01;
        const string zero4dec = "0.0000";
        const string zero2dec = "0.00";
        const string zero1dec = "0.0";
        const string zero = "0";

        //声明读写INI文件的API函数
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string filePath);
        
        private static ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private delegate void AddMessage(String value, int type);// 委托函数指针
        public FormMain()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;// 注意现在此阶段需要将线程保护标志 标记成false---重要参数
            strTB = "gztb100";            
        }

        // 更新查询组合框显示
        private void UpdateGroupCtrl()
        {
            labelID.Enabled = radioButtonQuery.Checked;
            textBoxID.Enabled = radioButtonQuery.Checked;
        }

        public void AddFrontLog(String log, int type)
        {
            AddMessage objAddMessage = new AddMessage(AddFrontLog);

            if (this.lvwMessage.InvokeRequired)
            {
                try
                {
                    Invoke(objAddMessage, new object[] { log, type });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                while (this.lvwMessage.Items.Count > 1000)
                {
                    this.lvwMessage.Items.Clear();
                }

                ListViewItem lvItem = new ListViewItem();
                lvItem.SubItems[0].Text = DateTime.Now.ToLocalTime().ToString();
                switch (type)
                {
                    case 0:
                        lvItem.SubItems.Add(new ListViewItem.ListViewSubItem(lvItem, "Infor"));
                        break;
                    case 1:
                        lvItem.SubItems.Add(new ListViewItem.ListViewSubItem(lvItem, "Error"));
                        lvItem.ForeColor = Color.Red;
                        break;
                }
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem(lvItem, log));
                this.lvwMessage.Items.Add(lvItem);
            }
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
                MessageBox.Show(ex.Message, "连接数据库失败", MessageBoxButtons.OK,MessageBoxIcon.Error);
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
            GfLogManager.LogRecoder = this;
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
            Thread.Sleep(2000);  // 线程睡眠2秒，防止重复的后台线程开启
            CheckButton("START");
            try
            {
                this.bgWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                //this.bgWorker.CancelAsync();
                MessageBox.Show(ex.Message, "后台线程启动失败", MessageBoxButtons.OK,MessageBoxIcon.Error);
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
        // 核心处理函数
        private bool MktToSql(IniFiles inifile)
        {
            var file = File.Open(inifile.File, FileMode.Open);
            List<string> txt = new List<string>();
            List<Dictionary<int, string>> map_txt = new List<Dictionary<int, string>>();
            string mdtext,stime_get,tmpsqldcmd,sqlcmd,sqlcmd_foreach,sqlcmd_last ;
            string[] sheader;
            SqlTransaction sqlTrans = (SqlTransaction)null;
            int i = 0;
            stime_get = "";
            sqlcmd_foreach = "";
            sqlcmd_last = "";
            using (var stream = new StreamReader(file))
            {
                while (!stream.EndOfStream)
                {
                    string stmp = stream.ReadLine();
                    if (stmp.Contains("HEADER")  )// 文件头和尾不需要插入链表
                    {
                        sheader = stmp.Split('|');
                        stime_get = sheader[6];// 分割后的第7个字符串为时间戳
                        continue;
                    }
                    if(stmp.Contains("TRAILER"))
                    {
                        continue;
                    }
                    txt.Add(stmp);
                }
                stream.Close();// 关闭文件流
            }
            file.Close();// 关闭文件
            tmpsqldcmd = "insert into " + inifile.Table + " (pubnum,bcasttype,setid,seqnum,recordtimestamp,mdtext) " + "(" ;
            sqlcmd = tmpsqldcmd;
            foreach (string sstmp in txt)
            {
                string[] sArray = sstmp.Split('|');
  
                icount++;// 计数器加1
                i++;
                mdtext = "35=W" + SOH + "963= " + SOH + "1187=0" + SOH + "48=" + sArray[1].Trim() + SOH
                    + "8506=" + sArray[2].Trim() + SOH + "8538=" + sArray[33] + SOH + "8504=" + ((sArray[4].Trim() == "")?zero2dec:sArray[4].Trim())
                          + SOH + "387=" + sArray[3].Trim() + SOH + "268=16" + SOH + "269=4" + SOH + "270=" + ((sArray[6].Trim() == "") ? zero4dec : sArray[6].Trim())
                          + SOH + "269=6" + SOH + "270=" + ((sArray[32].Trim() == "") ? zero4dec : sArray[32].Trim()) + SOH +
                          "269=7" + SOH + "270=" + ((sArray[9].Trim() == "") ? zero4dec : sArray[9].Trim()) + SOH + "269=8" + SOH + "270=" + ((sArray[10].Trim() == "") ? zero4dec : sArray[10].Trim())
                          + SOH + "269=x" + SOH + "270=" + ((sArray[7].Trim() == "") ? zero4dec : sArray[7].Trim()) + SOH + "269=2" + SOH + "270=" + ((sArray[11].Trim() == "") ? zero4dec : sArray[11].Trim()) + SOH + "269=0" + SOH + "270=" + ((sArray[12].Trim() == "") ? "0.0000" : sArray[12].Trim())
                          + SOH + "271=" + sArray[13].Trim() + SOH + "290=1" + SOH + "269=0" + SOH + "270=" + ((sArray[16].Trim() == "") ? "0.0000" : sArray[16].Trim())
                          + SOH + "271=" + sArray[17].Trim() + SOH + "290=2" + SOH + "269=0" + SOH + "270=" + ((sArray[20].Trim() == "") ? "0.0000" : sArray[20].Trim())
                          + SOH + "271=" + sArray[21].Trim() + SOH + "290=3" + SOH + "269=0" + SOH + "270=" + ((sArray[24].Trim() == "") ? "0.0000" : sArray[24].Trim())
                          + SOH + "271=" + sArray[25].Trim() + SOH + "290=4" + SOH + "269=0" + SOH + "270=" + ((sArray[28].Trim() == "") ? "0.0000" : sArray[28].Trim())
                          + SOH + "271=" + sArray[29].Trim() + SOH + "290=5" + SOH + "269=1" + SOH + "270=" + ((sArray[14].Trim() == "") ? "0.0000" : sArray[14].Trim())
                          + SOH + "271=" + sArray[15].Trim() + SOH + "290=1" + SOH + "269=1" + SOH + "270=" + ((sArray[18].Trim() == "") ? "0.0000" : sArray[18].Trim())
                          + SOH + "271=" + sArray[19].Trim() + SOH + "290=2" + SOH + "269=1" + SOH + "270=" + ((sArray[22].Trim() == "") ? "0.0000" : sArray[22].Trim())
                          + SOH + "271=" + sArray[23].Trim() + SOH + "290=3" + SOH + "269=1" + SOH + "270=" + ((sArray[26].Trim() == "") ? "0.0000" : sArray[26].Trim())
                          + SOH + "271=" + sArray[27].Trim() + SOH + "290=4" + SOH + "269=1" + SOH + "270=" + ((sArray[30].Trim() == "") ? "0.0000" : sArray[30].Trim())
                          + SOH + "271=" + sArray[31].Trim() + SOH + "290=5" + SOH;
                mdtext = "9=" + (mdtext.Length) + SOH + mdtext;// 一条数据的信息成功获得
                sqlcmd_foreach = "SELECT " + icount + "," + "'" + "7H" + "'" + "," + "'" + "300" + "'" + "," + icount + "," + "'" + stime_get + "'" + "," + "'" + mdtext + "'";
                
                if (i == inifile.CommitCount)
                {
                    sqlcmd += (sqlcmd_foreach + ")");

                    try
                    {
                        // Todo执行插入操作
                        sqlTrans = sqlConn.GetTrans();
                        sqlConn.command.Transaction = sqlTrans;
                        if (sqlConn.RunNonQuery(sqlcmd) != 0)
                        {
                            sqlTrans.Commit();
                        }
                        else
                        {
                            GfLogManager.WriteLog(sqlcmd, 1);
                            sqlTrans.Rollback();
                        }
                    }
                    catch (Exception sqlex)
                    {
                        GfLogManager.WriteLog(sqlcmd+sqlex.Message, 1);
                        if (sqlTrans != null)
                        {
                            sqlTrans.Rollback();
                        } 
                    }
                    sqlcmd = tmpsqldcmd;
                    i = 0;
                }
                else
                {
                    sqlcmd_last = sqlcmd;
                    sqlcmd += (sqlcmd_foreach + " UNION ALL ");
                }
            }
            if (i != 0) // 如果 i不等于0说明还有余下的数据需要插入到数据库 
            {
                sqlcmd_last += (sqlcmd_foreach + ")");
                try
                {
                    // Todo执行插入操作
                    sqlTrans = sqlConn.GetTrans();
                    sqlConn.command.Transaction = sqlTrans;
                    if (sqlConn.RunNonQuery(sqlcmd_last) != 0)
                    {
                        sqlTrans.Commit();
                    }
                    else
                    {
                        sqlTrans.Rollback();
                    }
                }
                catch (Exception sexlast)
                {
                    GfLogManager.WriteLog(sqlcmd_last+sexlast.Message, 1);
                    if (sqlTrans != null)
                    {
                        sqlTrans.Rollback();
                    }
                }

            }

            return true;
        }

        private object getmaxpubnum(IniFiles inifile)
        {
            object result = null;
            SqlTransaction sqlTrans = null;
            string getmaxpubnum;
            getmaxpubnum = "select isnull(max(pubnum),0) from " +inifile.Table;
            try
            {
                sqlTrans = sqlConn.GetTrans();
                sqlConn.command.Transaction = sqlTrans;
                result = sqlConn.RunOneReCordQuery(getmaxpubnum);
                if (result != null)
                {
                    sqlTrans.Commit();
                }
                else
                {
                    sqlTrans.Rollback();
                }
            }
            catch (Exception exmes)
            {
                if (sqlTrans != null)
                {
                    sqlTrans.Rollback();
                }
            }
            return result;
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IniFiles inifile = new IniFiles(Properties.Resources.cfgname);// 获取配置文件
            // GfLogManager.WriteLog("test", 1);
            string   sFile;
            string dataFile;
            int imax = 0;
            object iresult = null;
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
                    this.bgWorker.CancelAsync();
                    e.Cancel = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                GfLogManager.WriteLog(ex.Message, 1);
                MessageBox.Show(ex.Message, "连接数据库失败", MessageBoxButtons.OK,MessageBoxIcon.Error);
                sqlConn.CloseConnection();
                this.bgWorker.CancelAsync();
                return;
            }

            // 查询表中最大流水号
            iresult = this.getmaxpubnum(inifile);
            if (iresult != null)
            {
                imax = (int)iresult;
            }
            else
            {
                imax = 0;
            }
            // 取表中的最大流水号完毕
            icount = imax;
            //  CheckButton("START");// 启动按钮线程不安全
            if (this.MktToSql(inifile) == false)//启动的时候先转文件
            {
                MessageBox.Show("启动转换文件失败");
                return;
            }
            GfLogManager.WriteLog("初次转换文件到数据库成功", 0);

            while (true)
            {
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
                    //break;
                }


                DateCurr = DateComp;
                if (this.MktToSql(inifile) == false)// 当文件时间戳更改的时候，再转文件
                {
                    GfLogManager.WriteLog("转换文件到数据库失败，详细请见日志", 1);
                    return;
                }
                GfLogManager.WriteLog("转换文件到数据库成功", 0);


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

        private void lvwMessage_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ExittoolStripButton_Click(object sender, EventArgs e)
        {
            if (this.bgWorker.IsBusy)
            {
                MessageBox.Show("请先停止发送再退出！");
                return;
            }

            this.Close();
        }

    }
}
