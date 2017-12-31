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
        private string adbPath = "C:\\Users\\Nihiue\\Desktop\\platform-tools-latest-windows\\platform-tools\\adb.exe";
        private System.Timers.Timer jpTimer  = new System.Timers.Timer();
        public JumpHelper()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.jpTimer.Elapsed += new ElapsedEventHandler(timeTrigger);
            this.jpTimer.Interval = 4500;
            this.jpTimer.AutoReset = true;//执行一次 false，一直执行true  

        }
     

        private void forward() {              
            this.getScreenshot();           
            this.processScreenshot();
        }

        private void timeTrigger(object source, ElapsedEventArgs e) {
            this.forward();
        }
        private void start_Click(object sender, EventArgs e)
        {
           // MessageBox.Show("a" + Math.Sin(Math.Atan(-1)));
            if (!this.test_ADB()) {
                return;
            }            
            this.jpTimer.Enabled = true;
            this.stop.Enabled = true;
            this.start.Enabled = false;
            this.forward();
            //this.processScreenshot();
        }

        private void stop_Click(object sender, EventArgs e)
        {            
            this.jpTimer.Enabled = false;
            this.stop.Enabled = false;
            this.start.Enabled = true;
        }


        private void getScreenshot() {
            string[] cmds = { "shell /system/bin/screencap -p /sdcard/screenshot_jp.png", "pull /sdcard/screenshot_jp.png" };
            this.sendADBCmd(cmds);
        }

        private bool isBgColor(Color c1, Color c2, Color d, int bgError) { 
            if ((d.R - c1.R) * (c2.R - c1.R) < 0 || (d.R - c2.R) * (c1.R - c2.R) < 0) {
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
            return this.calColorErr(d, c1) <= bgError;
        }
        private int calColorErr(Color c1, Color c2) {
            return (c1.R - c2.R) * (c1.R - c2.R) + (c1.G - c2.G) * (c1.G - c2.G) + (c1.B - c2.B) * (c1.B - c2.B);
        }
        private void processScreenshot() {
            System.Drawing.Image img = System.Drawing.Image.FromFile("screenshot_jp.png");
            // System.Drawing.Image bmp = new System.Drawing.Bitmap(img);
            Bitmap bmp = new System.Drawing.Bitmap(img);
            LockBitmap lockbmp = new LockBitmap(bmp);
            img.Dispose();

            Color curColor;
            lockbmp.LockBits();
            Color topCorlor = lockbmp.GetPixel(0, 0);
            Color bottomColor = lockbmp.GetPixel((int)(lockbmp.Width  * 0.1), lockbmp.Height - 1);
            Color bottomColorLeft = lockbmp.GetPixel(0, lockbmp.Height - 1);
            Color bottomColorRight = lockbmp.GetPixel(lockbmp.Width -1, lockbmp.Height - 1);
            Color bottomColorCenter = lockbmp.GetPixel((int)(lockbmp.Width * 0.5), lockbmp.Height - 1);
            for (int i = 1; i < 7; i++) {
                Color sample = lockbmp.GetPixel((int)(lockbmp.Width * i * 0.15), lockbmp.Height - 1);
                int count = 0;
                if (this.calColorErr(sample, bottomColorLeft) < 100) {
                    count++;                  
                }

                if (this.calColorErr(sample, bottomColorRight) < 100)
                {
                    count++;                   
                }

                if (this.calColorErr(sample, bottomColorCenter) < 100)
                {
                    count++;                    
                }
                if (this.calColorErr(sample, bottomColor) < 100)
                {
                    count++;
                }
                if (count > 1) {
                    bottomColor = sample;
                    break;
                }
            }

            int bgError = this.calColorErr(topCorlor, bottomColor);
            int[,] cache = new int[lockbmp.Width, lockbmp.Height];

            Color figureColor = Color.FromArgb(54, 59, 99);
            Color figureHeadColor = Color.FromArgb(67, 58, 89);

            bool isTargetFound = false;
            bool isFigureFound = false;

            int targetStartX = 0;
            int targetStartY = 0;

            int figureX = 0;
            int figureY = 0;
            

            for (int j = (int)(lockbmp.Height * 0.25); j < (int)(lockbmp.Height * 0.7); j++)
            {
                for (int i = (int)(1); i < (int)(lockbmp.Width - 1); i++)
                {
                    curColor = lockbmp.GetPixel(i, j);
                    if (this.isBgColor(topCorlor, bottomColor, curColor, bgError))
                    {
                        // lockbmp.SetPixel(i,j,Color.FromArgb(255, 0, 0));
                        cache[i, j] = 1;
                    }
                    else if (!isTargetFound)
                    {
                        if (this.calColorErr(curColor, figureHeadColor) < 1200) {
                            continue;
                        }

                        if (cache[i - 1, j] == 1 || cache[i, j - 1] == 1 || cache[i - 1, j - 1] == 1)
                        {
                            isTargetFound = true;
                            targetStartX = i;
                            targetStartY = j;
                        }
                        
                    }
                    else if (!isFigureFound)
                    {
                        if (this.calColorErr(curColor, figureColor) < 8) {
                            isFigureFound = true;
                            figureX = i + 15;
                            figureY = j;
                        }
                    }
                }
            }
            
            if (!isFigureFound || !isTargetFound) {
                lockbmp.UnlockBits();
                this.pictureBox1.Image = (System.Drawing.Image)bmp;
                return;
            }
            
            int curX = targetStartX;
            int curY = targetStartY;

            while (true) {
                curX = curX + 1;
                int newY = -1;
                for (int y = -4; y < 5; y++) {
                    if (cache[curX, curY + y] != 1 && cache[curX, curY + y - 1] == 1) {
                        newY = curY + y;                       
                        break;
                    }
                }
                if (newY == -1) {
                    break;
                }
                curY = newY;
            }


            int midX = (curX + targetStartX) / 2;
            int midY = (curY + targetStartY) / 2;

            double angleOfLine = Math.Atan((double)(curY - targetStartY) / (curX - targetStartX));
            double length = Math.Sqrt((curX - targetStartX) * (curX - targetStartX) + (curY - targetStartY) * (curY - targetStartY)) / 2;
            midX = (int)(midX - length * Math.Cos(angleOfLine));
            midY = (int)(midY + length * Math.Sin(angleOfLine));
           
            for (int i = -10; i < 11; i++) {
                for (int j = -10; j < 11; j++) {
                    lockbmp.SetPixel(midX + i, midY + j, Color.FromArgb(0, 255, 0));
                    lockbmp.SetPixel(figureX + i, figureY + j, Color.FromArgb(255, 0, 0));
                }
            }
         
            lockbmp.UnlockBits();
           
            double distance = Math.Sqrt((midX - figureX) * (midX - figureX) + (midY - figureY) * (midY - figureY)) / lockbmp.Width;
           
            this.pictureBox1.Image = (System.Drawing.Image)bmp;
           
           string[] cmd = { "shell input swipe 300 800 300 800 " + (int) (distance * 1490) };
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
                    for (int i = 0; i < cmdlines.Length; i++) {
                        cmdstr += (string.Format(@"""{0}"" {1}", this.adbPath, cmdlines[i])+"&");
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
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
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
                    string str = string.Format(@"""{0}"" {1} {2}", this.adbPath, "devices", "&exit");

                    myPro.StandardInput.WriteLine(str);
                    myPro.StandardInput.AutoFlush = true;                    
                    myPro.WaitForExit();
                    string output = myPro.StandardOutput.ReadToEnd();
                    
                    if (!output.Contains("List of devices attached")) {
                        MessageBox.Show("未找到ADB ，请手动选择");
                        return false;
                    }

                    bool result = false;
                    output = output.Substring(output.LastIndexOf("List of devices attached") + 24);
                    if (output.Contains("device")) {
                        result = true;
                    } else if (output.Contains("unauthorized")) {
                        MessageBox.Show("ADB: 设备未授权，请点击接受调试\n" + output);
                    }
                    else
                    {
                        MessageBox.Show("ADB: 未连接到设备\n" + output);
                    }

                    return result;
                }
            }
            catch
            {
                MessageBox.Show("未知 ADB 错误");
                return false;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
