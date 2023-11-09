using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Configuration;

namespace Synchronization
{
    public partial class Synchronization : Form
    {
        private DataTable Source = new DataTable();//源
        private DataTable SourceDirectory = new DataTable();
        private DataTable Target = new DataTable();//目标
        private DataTable TargetDirectory = new DataTable();
        public Synchronization()
        {
            InitializeComponent();
            if (Properties.Settings.Default.Yuan!="")
            {
                    Yuan.Text = Properties.Settings.Default.Yuan;
            }
            if (Properties.Settings.Default.Mubiao != "")
            {
                    Mubiao.Text = Properties.Settings.Default.Mubiao;
            }
            if (Properties.Settings.Default.time != "")
            {
                label3.Text = Properties.Settings.Default.time;
            }
            Cpublic.log.Info("程序启动");
            TargetDirectory.Columns.Add("name");
            SourceDirectory.Columns.Add("name");
            Source.Columns.Add("all");
            Source.Columns.Add("wname");
            Source.Columns.Add("fname");
            Source.Columns.Add("time");
            Target.Columns.Add("all");
            Target.Columns.Add("wname");
            Target.Columns.Add("fname");
            Target.Columns.Add("time");
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = @"Istrue.config";
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            checkBox1.Checked=bool.Parse(config.AppSettings.Settings["Istrue"].Value);
            if (checkBox1.Checked==true)
            {
                bool d=Sc();
                if (d)
                {
                    Cpublic.log.Info("程序关闭");
                    System.Environment.Exit(0);
                }
            }
        }

        private void Folder(TextBox textBox,string s)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择根目录文件夹";
            try
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (string.IsNullOrEmpty(dialog.SelectedPath))
                    {
                        MessageBox.Show("文件夹路径不能为空！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Cpublic.log.Error("选择的文件夹路径为空！");
                        return;
                    }
                    textBox.Text = dialog.SelectedPath;
                    Cpublic.log.Info("选择" + s + "目录:" + dialog.SelectedPath);
                }
            }
            catch (Exception ex)
            {
                Cpublic.log.Error(ex.Message);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Folder(Yuan,"映射文件夹");
            Properties.Settings.Default.Yuan = Yuan.Text;
            Properties.Settings.Default.Save();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Folder(Mubiao, "目标文件夹");
            Properties.Settings.Default.Mubiao = Mubiao.Text;
            Properties.Settings.Default.Save();
        }
        private  void Ergodic(string dirs,DataTable dataTable,string gtext,DataTable table)
        {
            try
            {
                if (Directory.Exists(dirs))
                {       
                    //文件路径
                    string[] dir = Directory.GetDirectories(dirs);
                    //文件名
                    for (int i = 0; i <dir.Length; i++)
                    {
                        Cpublic.log.Info(dir[i] + "，开始遍历");
                        if ((new FileInfo(dir[i]).Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)//去除隐藏文件夹
                        {
                            continue;
                        };

                        DirectoryInfo Files = new DirectoryInfo(dir[i]);
                        FileInfo[] files = Files.GetFiles();
                        var filtered = files.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));//去除隐藏文件
                        foreach (FileInfo Filename in filtered)
                        {
                            DataRow dr = dataTable.NewRow();
                            dr["all"] = Filename.FullName;
                            dr["wname"] = Filename.FullName.Replace(gtext + "\\", "");
                            dr["fname"] = Filename.FullName.Replace(gtext + "\\", "").Replace(Filename.Name, "").Remove(Filename.FullName.Replace(gtext + "\\", "").Replace(Filename.Name, "").Length-1,1);
                            dr["time"] = Filename.LastWriteTime;
                            dataTable.Rows.Add(dr);
                        }
                        DataRow dc = table.NewRow();
                        dc["name"] = dir[i].Replace(gtext + "\\", "");
                        table.Rows.Add(dc);
                        Ergodic(dir[i],dataTable,gtext,table);
                    }
                }
                else
                {
                    MessageBox.Show("遍历文件失败！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Cpublic.log.Error("遍历文件失败，路径：" + dirs);
                }
               // return list;
            }
            catch (Exception ex) 
            {
                Cpublic.log.Error(ex.Message);
            }
        }
        private void Start_Click(object sender, EventArgs e)
        {
            Sc();
        }
        private bool Sc()
        {
            if (Yuan.Text == "" || Mubiao.Text == ""||(Yuan.Text == Mubiao.Text))
            {
                Cpublic.log.Error("文件夹路径未设置正确！");
                label3.Text = "文件夹路径未设置正确！";
                return false;
            }
            Source.Clear();
            DirectoryInfo Files = new DirectoryInfo(Yuan.Text);
            FileInfo[] files = Files.GetFiles();
            var filtered = files.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));//去除隐藏文件
            Cpublic.log.Info(Yuan.Text + "，开始遍历");
            foreach (FileInfo Filename in filtered)
            {
                DataRow dr = Source.NewRow();
                dr["all"] = Filename.FullName;
                dr["wname"] = Filename.FullName.Replace(Yuan.Text + "\\", "");
                dr["fname"] = Filename.FullName.Replace(Yuan.Text + "\\", "").Replace(Filename.Name, "");
                dr["time"] = Filename.LastWriteTime;
                Source.Rows.Add(dr);
            }
            Ergodic(Yuan.Text, Source, Yuan.Text, SourceDirectory);
            Cpublic.log.Info("映射文件夹共:" + Source.Rows.Count + "个文件");

            Target.Clear();
            Files = new DirectoryInfo(Mubiao.Text);
            files = Files.GetFiles();
            filtered = files.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));//去除隐藏文件
            Cpublic.log.Info(Mubiao.Text + "，开始遍历");
            foreach (FileInfo Filename in filtered)
            {
                DataRow dr = Target.NewRow();
                dr["all"] = Filename.FullName;
                dr["wname"] = Filename.FullName.Replace(Mubiao.Text + "\\", "");
                dr["fname"] = Filename.FullName.Replace(Mubiao.Text + "\\", "").Replace(Filename.Name, "");
                dr["time"] = Filename.LastWriteTime;
                Target.Rows.Add(dr);
            }
            Ergodic(Mubiao.Text, Target, Mubiao.Text, TargetDirectory);
            Cpublic.log.Info("目标文件夹共:" + Target.Rows.Count + "个文件");
            //Target = Ergodic(Mubiao.Text);
            Cpublic.log.Info("开始同步删除!");
            for (int i = 0; i < TargetDirectory.Rows.Count; i++)
            {
                DataRow[] fc;
                fc = SourceDirectory.Select("name = '" + TargetDirectory.Rows[i]["name"] + "'");
                if (fc.Length < 1)
                {
                    if (Directory.Exists(Mubiao.Text + "\\" + TargetDirectory.Rows[i]["name"].ToString()))
                        Directory.Delete(Mubiao.Text + "\\" + TargetDirectory.Rows[i]["name"].ToString(), true);
                    DataRow[] f = Target.Select("fname='" + TargetDirectory.Rows[i]["name"] + "'");
                    for (int t = 0; t < f.Length; t++)
                    {
                        Target.Rows.Remove(f[t]);
                    }
                    Cpublic.log.Info("删除文件夹：" + TargetDirectory.Rows[i]["name"]);
                }
            }
            for (int i = 0; i < Target.Rows.Count; i++)
            {
                DataRow[] fc;
                fc = Source.Select("wname = '" + Target.Rows[i]["wname"] + "'");
                if (fc.Length < 1)
                {
                    File.Delete(Target.Rows[i]["all"].ToString());
                    Cpublic.log.Info("删除文件：" + Target.Rows[i]["all"]);
                }
            }
            Cpublic.log.Info("开始移动文件!");
            for (int i = 0; i < Source.Rows.Count; i++)
            {
                DataRow[] fs;
                string s = Source.Rows[i]["all"].ToString().Replace(Yuan.Text, Mubiao.Text);
                s = s.Remove(s.LastIndexOf("\\"));
                fs = Target.Select("wname = '" + Source.Rows[i]["wname"] + "' and time = '" + Source.Rows[i]["time"] + "'");
                if (fs.Length < 1)
                {
                    //Cpublic.log.Info("file:"+Source.Rows[i]["all"].ToString());
                    if (!File.Exists(Source.Rows[i]["all"].ToString().Replace(Yuan.Text, Mubiao.Text)))
                    {
                        Directory.CreateDirectory(@"" + s);
                    }
                    File.Copy(Source.Rows[i]["all"].ToString(), Source.Rows[i]["all"].ToString().Replace(Yuan.Text, Mubiao.Text), true);
                    Cpublic.log.Info("移动文件:" + Source.Rows[i]["all"].ToString());
                }
            }
            label3.Text = "上次映射时间：" + DateTime.Now;
            Properties.Settings.Default.time = label3.Text;
            Properties.Settings.Default.Save();
            Cpublic.log.Info("映射完成！");
            return true;
        }
        private void Synchronization_FormClosed(object sender, FormClosedEventArgs e)
        {
            Cpublic.log.Info("程序关闭");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = @"Istrue.config";
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            config.AppSettings.Settings["Istrue"].Value = checkBox1.Checked.ToString();
            config.Save();
        }
    }
}
