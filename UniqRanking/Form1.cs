using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace UniqRanking
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        SqlConnection sqlConnection;
        SqlCommand sqlCommand;
        SqlDataReader dataReader;
        List<UniqPoint> uniqPoints;
        string connectionString;
        DateTime lastReadTime;
        private void Form1_Load(object sender, EventArgs e)
        {
            connectionString = "Server=" + Properties.Settings.Default.ServerName + ";Database=" + Properties.Settings.Default.DbName + ";User Id=" + Properties.Settings.Default.UserName + ";Password=" + Properties.Settings.Default.Password + ";MultipleActiveResultSets = True;";
            sqlConnection = new SqlConnection(connectionString);
            try
            {
                sqlConnection.Open();
                using (sqlConnection)
                {
                    sqlCommand = new SqlCommand("IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='UniqPoints') SELECT 1 AS res ELSE SELECT 0 AS res;", sqlConnection);
                    dataReader = sqlCommand.ExecuteReader();
                    dataReader.Read();
                    if (!Convert.ToBoolean(dataReader[0]))
                    {
                        MessageBox.Show("Sorgu No1 i çalıştırın");
                        Application.Exit();
                    }
                    dataReader.Close();


                    sqlCommand = new SqlCommand("IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='UniqKillList') SELECT 1 AS res ELSE SELECT 0 AS res;", sqlConnection);
                    dataReader = sqlCommand.ExecuteReader();
                    dataReader.Read();
                    if (!Convert.ToBoolean(dataReader[0]))
                    {
                        MessageBox.Show("Sorgu No2 i çalıştırın");
                        Application.Exit();
                    }
                    dataReader.Close();


                    sqlCommand = new SqlCommand("IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='UniqKillPoint') SELECT 1 AS res ELSE SELECT 0 AS res;", sqlConnection);
                    dataReader = sqlCommand.ExecuteReader();
                    dataReader.Read();
                    if (!Convert.ToBoolean(dataReader[0]))
                    {
                        MessageBox.Show("Sorgu No3 i çalıştırın");
                        Application.Exit();
                    }
                    dataReader.Close();


                    sqlCommand = new SqlCommand("select COUNT(*) from UniqPoints", sqlConnection);
                    dataReader = sqlCommand.ExecuteReader();
                    dataReader.Read();
                    if (Convert.ToInt32(dataReader[0]) == 0)
                    {
                        dataReader.Close();
                        sqlCommand = new SqlCommand("select CodeName128 , ID from _RefObjCommon where Rarity = 3 and CodeName128 like 'MOB_%' and Service = 1", sqlConnection);
                        dataReader = sqlCommand.ExecuteReader();
                        while (dataReader.Read())
                        {
                            SqlCommand sqlCommand1 = new SqlCommand("INSERT INTO UniqPoints (UniqName,UniqID, Point) VALUES (@CodeName,@UniqID,1)", sqlConnection);
                            sqlCommand1.Parameters.Add("@CodeName", System.Data.SqlDbType.NVarChar, 100).Value = dataReader[0].ToString();
                            sqlCommand1.Parameters.Add("@UniqID", System.Data.SqlDbType.Int).Value = dataReader[1].ToString();
                            sqlCommand1.ExecuteNonQuery();
                        }
                    }
                    dataReader.Close();

                    sqlConnection.Close();
                    FillList();
                }
                lastReadTime = DateTime.Now;
                timer1.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        public void FillList()
        {
            sqlConnection.Open();
            comboBox1.Items.Clear();
            uniqPoints = new List<UniqPoint>();
            sqlCommand = new SqlCommand("select * from UniqPoints", sqlConnection);
            dataReader = sqlCommand.ExecuteReader();
            while (dataReader.Read())
            {
                uniqPoints.Add(new UniqPoint(Convert.ToInt32(dataReader[0]), dataReader[2].ToString(), Convert.ToInt32(dataReader[3]), Convert.ToInt32(dataReader[1])));
            }
            dataReader.Close();
            sqlConnection.Close();
            comboBox1.Items.AddRange(uniqPoints.ToArray());
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UniqPoint uniq = (UniqPoint)(((ComboBox)sender).SelectedItem);
            label2.Text = uniq.UniqName;
            textBox1.Text = uniq.Point.ToString();
            button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                sqlConnection = new SqlConnection(connectionString);
                UniqPoint uniq = (UniqPoint)(comboBox1.SelectedItem);
                uniq.Point = Convert.ToInt32(textBox1.Text);
                sqlCommand = new SqlCommand("update  UniqPoints set point = @point where id = @id", sqlConnection);
                sqlCommand.Parameters.Add("@id", System.Data.SqlDbType.Int).Value = uniq.Id;
                sqlCommand.Parameters.Add("@point", System.Data.SqlDbType.Int).Value = uniq.Point;
                sqlConnection.Open();
                button1.Enabled = false;
                sqlCommand.ExecuteNonQuery();
                MessageBox.Show("Tamamlandı", "Sorgu Işlemi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Biraz bekleyip tekrar deneyin", "Sorgu Işlemi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                sqlConnection.Close();
                FillList();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            using (StreamReader file = new StreamReader(DateTime.Now.ToString("yyyy-MM-dd") + "_FatalLog.txt"))
            {
                string ln;

                while ((ln = file.ReadLine()) != null)
                {

                    int year = Convert.ToInt32(ln.Substring(0, 4));
                    int month = Convert.ToInt32(ln.Substring(5, 2));
                    int day = Convert.ToInt32(ln.Substring(8, 2));
                    int hour = Convert.ToInt32(ln.Substring(11, 2));
                    int min = Convert.ToInt32(ln.Substring(14, 2));
                    int sec = Convert.ToInt32(ln.Substring(17, 2));
                    DateTime lnDate = new DateTime(year, month, day, hour, min, sec);
                    if (lnDate > lastReadTime)
                    {
                        lastReadTime = lnDate;
                        string mobCode;
                        if (ln.Contains("Unique Monster Entered"))
                        {
                            mobCode = (ln.Substring(ln.IndexOf("UNIQUE[") + 7, 40)).Split(']')[0];
                            listBox1.Items.Insert(0,lnDate.ToString("yyyy-MM-dd hh-mm-ss") +"  Spawn  " + mobCode);
                        }
                        else if (ln.Contains("Unique Monster Killed!"))
                        {
                            mobCode = (ln.Substring(ln.IndexOf("UNIQUE[") + 7, 20)).Split(']')[0];
                            string charName = ln.Split('[')[ln.Split('[').Length - 1].Split(']')[0];
                            listBox1.Items.Insert(0, lnDate.ToString("yyyy-MM-dd hh-mm-ss") + "  Killed  " + mobCode + "  By  " + charName);
                            AddKill(charName, mobCode);
                        }
                    }
                }
                file.Close();
            }
            timer1.Start();
        }
        public void AddKill(string charName, string mobCode) {
            int uniqID = uniqPoints.Where(x => x.UniqName == mobCode).Select(x => x.UniqID).FirstOrDefault();
            sqlConnection = new SqlConnection(connectionString);
            sqlCommand = new SqlCommand("UniqRankingCore", sqlConnection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            SqlParameter param;
            param = sqlCommand.Parameters.Add("@CharName", SqlDbType.NVarChar, 70);
            param.Value = charName;
            param = sqlCommand.Parameters.Add("@UniqID", SqlDbType.Int);
            param.Value = uniqID;
            MessageBox.Show(uniqID.ToString());
            sqlConnection.Open();
            sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();
        }
    }
}
