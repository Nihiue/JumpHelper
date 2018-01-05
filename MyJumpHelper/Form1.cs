using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;
using System.Timers;
using System.Drawing.Imaging;
using System.Threading;

namespace MyJumpHelper
{
    public partial class JumpHelper : Form
    {
        private string adbPath = "adb.exe";
        private bool isRunning = false;
        private int jumpInterval = 3000;
        private int convertRatio = 1480;       

        public JumpHelper()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(loopThread));
            thread.IsBackground = true;
            thread.Start();
        }


        private void loopThread()
        {
            while(true) {
                if (this.isRunning)
                {
                    this.getScreenshot();                    
                    Thread.Sleep(500);                    
                    this.processScreenshot();
                    Thread.Sleep(this.jumpInterval - 500);
                }
                else {
                    Thread.Sleep(1000);
                }              
                
            }
        }

        private void start_Click(object sender, EventArgs e)
        {
            this.startAutoJump();
        }

        private void stop_Click(object sender, EventArgs e)
        {
            this.stopAutoJump();
        }

        private void stopAutoJump() {
            this.isRunning = false;
            this.stop.Enabled = false;
            this.start.Enabled = true;
        }
        private void startAutoJump() {
            if (!this.test_ADB())
            {
                return;
            }
            this.isRunning = true;
            this.stop.Enabled = true;
            this.start.Enabled = false;            
        }

        private void getScreenshot()
        {
            string[] cmds = { "shell /system/bin/screencap -p /sdcard/screenshot_jp.png", "pull /sdcard/screenshot_jp.png" };
            this.sendADBCmd(cmds);
        }

        private int calColorErr(Color c1, Color c2)
        {
            return (c1.R - c2.R) * (c1.R - c2.R) + (c1.G - c2.G) * (c1.G - c2.G) + (c1.B - c2.B) * (c1.B - c2.B);
        }

        private int[] findTargetEdge(int direction,int[,] cache, int []start, LockBitmap lockbmp) {
            int[] ret = { 0, 0 };
            int curX = start[0];
            int curY = start[1];
            while (curX > 0 && curX < lockbmp.Width - 1)
            {
                curX = curX + direction;
                int newY = -1;
                for (int y = 0; y < 7; y++)
                {
                    if (cache[curX, curY + y] != 1 && cache[curX, curY + y - 1] == 1)
                    {
                        newY = curY + y;                       
                        //lockbmp.SetPixel(curX, newY, Color.FromArgb(0, 0, 255));
                        //lockbmp.SetPixel(curX, newY + 1, Color.FromArgb(0, 0, 255));
                        //lockbmp.SetPixel(curX, newY - 1, Color.FromArgb(0, 0, 255));                       
                        break;
                    }
                }
                if (newY == -1)
                {
                    break;
                }
                curY = newY;
            }
            ret[0] = curX;
            ret[1] = curY;
            return ret;
        }
        private bool isBgColor(Color c1, Color c2, Color d)
        {
            
            if ((d.R - c1.R) * (c2.R - c1.R) < 0 || (d.R - c2.R) * (c1.R - c2.R) < 0)
            {
                return false;
            }
            if ((d.G - c1.G) * (c2.G - c1.G) < 0 || (d.G - c2.G) * (c1.G - c2.G) < 0)
            {
                return false;
            }
            if ((d.B - c1.B) * (c2.B - c1.B) < 0 || (d.B - c2.B) * (c1.B - c2.B) < 0)
            {
                return false;
            }
            return true;
        }
        private void processScreenshot()
        {
            System.Drawing.Image img;
            Bitmap bmp;
            LockBitmap lockbmp;
            try
            {
                img = System.Drawing.Image.FromFile("screenshot_jp.png");
                bmp = new System.Drawing.Bitmap(img);
                lockbmp = new LockBitmap(bmp);
                img.Dispose();
                lockbmp.LockBits();
            }
            catch
            {
                this.stopAutoJump();
                MessageBox.Show("打开图片时出错，请确认ADB已连接并工作正常");                
                return;
            }       
             
            

            Color curColor;
            Color curBgColor;
            Color curBgShadow;
            Color figureColor = Color.FromArgb(54, 59, 99);            
          
            int[,] cache = new int[lockbmp.Width, lockbmp.Height];

                      
            bool isTargetFound = false;
            bool isFigureFound = false;

            int[] targetStart = { 0, 0 };
            int[] figure = { 0, 0 };     
            
            int figureWidth = (int)(lockbmp.Width * 76 / 1080);
            int figureHeight = (int)(lockbmp.Width * 210 / 1080);

            for (int j = (int)(lockbmp.Height * 0.7); j > (int)(lockbmp.Height * 0.25); j--)
            {
                for (int i = (int)(lockbmp.Width - 1); i > 0; i--)
                {
                    curColor = lockbmp.GetPixel(i, j);
                    if (this.calColorErr(curColor, figureColor) < 5)
                    {
                        isFigureFound = true;
                        figure[0] = i;
                        figure[1] = j - (int)(lockbmp.Width * 18 / 1080);
                        break;
                    }

                }
                if (isFigureFound)
                {
                    break;
                }
            }
            

            for (int j = (int)(lockbmp.Height * 0.25); j < (int)(lockbmp.Height * 0.7); j++)
            {
                curBgColor = lockbmp.GetPixel(0, j);
                curBgShadow = Color.FromArgb((int)(curBgColor.R * 0.69), (int)(curBgColor.G * 0.69), (int)(curBgColor.B * 0.69));
                for (int i = (int)(1); i < (int)(lockbmp.Width - 1); i++)
                {
                    curColor = lockbmp.GetPixel(i, j);
                    if (i >= (figure[0] - figureWidth / 2) && i <= (figure[0] + figureWidth / 2) && j >= figure[1] - figureHeight && j <= figure[1])
                    {
                        cache[i, j] = 1;
                    }
                    else if (this.isBgColor(curBgColor, curBgShadow, curColor))
                    {
                        // 需要识别背景色和阴影颜色
                        //lockbmp.SetPixel(i,j,Color.FromArgb(255, 0, 0));
                        cache[i, j] = 1;
                    }
                    else if (!isTargetFound)
                    {
                        if (cache[i - 1, j] == 1 || cache[i, j - 1] == 1 || cache[i - 1, j - 1] == 1)
                        {
                            isTargetFound = true;
                            targetStart[0] = i;
                            targetStart[1] = j;
                        }

                    }
                }
            }

            if (!isFigureFound || !isTargetFound)
            {
                lockbmp.UnlockBits();
                this.pictureBox1.Image = (System.Drawing.Image)bmp;
                return;
            }

            int[] targetLeft = this.findTargetEdge(-1, cache, targetStart, lockbmp);
            int[] targetRight = this.findTargetEdge(1, cache, targetStart, lockbmp);

            int[] targetMid = { 0, 0 };

            targetMid[0] = (int)((targetLeft[0] + targetRight[0]) / 2);
            targetMid[1] = (int)((targetLeft[1] + targetRight[1]) / 2);
            
            for (int i = -15; i < 16; i++)
            {
                for (int j = -15; j < 16; j++)
                {
                    lockbmp.SetPixel(targetMid[0] + i, targetMid[1] + j, Color.FromArgb(0, 255, 0));
                    lockbmp.SetPixel(figure[0] + i, figure[1] + j, Color.FromArgb(255, 0, 0));
                }
            }

            double distance = Math.Sqrt((targetMid[0] - figure[0]) * (targetMid[0] - figure[0]) + (targetMid[1] - figure[1]) * (targetMid[1] - figure[1]));

            int duration = (int)((distance * this.convertRatio) / lockbmp.Width);
           
            lockbmp.UnlockBits();
            this.pictureBox1.Image = (System.Drawing.Image)bmp;

            string[] cmd = { "shell input swipe 300 800 300 800 " + duration };
            this.sendADBCmd(cmd);

        }

        private string sendADBCmd(string[] cmdlines)
        {
            try
            {
                using (Process myPro = new Process())
                {
                    myPro.StartInfo.FileName = "cmd.exe";
                    myPro.StartInfo.UseShellExecute = false;
                    myPro.StartInfo.RedirectStandardInput = true;
                    myPro.StartInfo.RedirectStandardOutput = true;
                    myPro.StartInfo.RedirectStandardError = true;
                    myPro.StartInfo.CreateNoWindow = true;
                    myPro.Start();
                    myPro.StandardInput.AutoFlush = true;

                    string cmdstr = "";
                    for (int i = 0; i < cmdlines.Length; i++)
                    {
                        cmdstr += (string.Format(@"""{0}"" {1}", this.adbPath, cmdlines[i]) + "&");
                    }
                    cmdstr += "exit";
                    myPro.StandardInput.WriteLine(cmdstr);
                    myPro.WaitForExit();
                    return myPro.StandardOutput.ReadToEnd();
                }
            }
            catch
            {
                return "";
            }

        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Nihiue/JumpHelper/blob/master/README.md");
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "Choose ADB";
            dialog.Filter = "ADB file|adb.exe";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.adbPath = dialog.FileName;
            }
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private bool test_ADB()
        {
            string[] cmd = { "devices" };
            string output = this.sendADBCmd(cmd);

            bool isADBOK = output.Contains("List of devices attached");           
           
            if (isADBOK)
            {
                output = output.Substring(output.LastIndexOf("List of devices attached") + 24);
                if (output.Contains("device"))
                {
                    return true;
                }
                else if (output.Contains("unauthorized"))
                {
                    MessageBox.Show("ADB: 设备未授权，请点击接受调试\n" + output);
                }
                else
                {
                    MessageBox.Show("ADB: 未连接到设备\n" + output);
                }                
            }
            else
            {
                MessageBox.Show("未找到 ADB 路径,请手动选择");
            }
            return false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                double value = double.Parse(this.textBox1.Text);
                if (value >= 500 && value <= 5000)
                {
                    this.convertRatio = (int)value;
                }
                else
                {
                    MessageBox.Show("有效范围 500 - 5000");
                }
            }
            catch
            {
                MessageBox.Show("请输入一个数字");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                double value = double.Parse(this.textBox2.Text);
                if (value >= 3000 && value <= 10000)
                {
                    this.jumpInterval = (int)value;                    
                }
                else
                {
                    MessageBox.Show("有效范围 3000 - 10000");
                }
            }
            catch
            {
                MessageBox.Show("请输入一个数字");
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
      
    }
