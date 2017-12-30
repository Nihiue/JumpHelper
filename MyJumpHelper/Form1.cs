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
using CSharpKeyboardHook;
namespace MyJumpHelper
{
    public partial class JumpHelper : Form
    {
        private KeyboardHookLib _keyboardHook = null;

        private int startX=0;
        private int startY=0;
        private int endX=0;
        private int endY = 0;
        private bool isReadyToJump = false;
        private double convRatio = 2.5;
        private string adbPath = "adb.exe";
        public JumpHelper()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
        }



        private void start_Click(object sender, EventArgs e)
        {
            if (_keyboardHook == null)
            {
                _keyboardHook = new KeyboardHookLib();
                _keyboardHook.InstallHook(this.KeyPress);
                label2.Text = "Running";
            }
        }

        private void stop_Click(object sender, EventArgs e)
        {
            if (_keyboardHook != null)
            {
                _keyboardHook.UninstallHook();
                label2.Text = "Ready";
                _keyboardHook = null;
            }
        }


        private bool sendTouchCmd(int duration)
        {
            bool result = false;
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
                    string str = string.Format(@"""{0}"" {1} {2} {3}", this.adbPath, "shell input swipe 300 800 300 800", duration, "&exit");

                    myPro.StandardInput.WriteLine(str);
                    myPro.StandardInput.AutoFlush = true;
                    // myPro.WaitForExit();

                    result = true;
                }
            }
            catch
            {

            }
            return result;
        }

        public void KeyPress(KeyboardHookLib.HookStruct hookStruct, out bool handle)
        {
            handle = false;

            switch (hookStruct.vkCode)
            {
                case 49: // number key 1
                    this.startX = Control.MousePosition.X;
                    this.startY = Control.MousePosition.Y;
                    label1.Text = string.Format(@"Start: ({0},{1})", this.startX, this.startY);
                    this.isReadyToJump = true;
                    break;
                case 50: // number key 2
                    this.endX = Control.MousePosition.X;
                    this.endY = Control.MousePosition.Y;

                    label3.Text = string.Format(@"End: ({0},{1})", this.endX, this.endY);
        
                    if (!this.isReadyToJump)
                    {
                        return;
                    }
                    this.isReadyToJump = false;
                    int delta =(int) Math.Sqrt((this.startX - this.endX) * (this.startX - this.endX) + (this.startY - this.endY) * (this.startY - this.endY));
                    int duration = (int)(delta * this.convRatio);
                    label4.Text = string.Format(@"Delta: {0} px, Ratio: {1}, Duration: {2} ms", delta, this.convRatio, duration);
                    this.sendTouchCmd(duration);
                    break;                
                    
            }           
        }

        private void setRatio_Click(object sender, EventArgs e)
        {
            try
            {
                double value = double.Parse(this.textBox1.Text);
                if (value > 0 && value < 20)
                {
                    this.convRatio = value;
                }
                else
                {
                    
                     MessageBox.Show("Input too large or too small");
                    
                }
            }
            catch
            {
                MessageBox.Show("Not a valid number");
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
                this.label6.Text = this.adbPath;
            }
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool result = false;
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
                        MessageBox.Show("ADB not found");
                        return;
                    }
                    output = output.Substring(output.LastIndexOf("List of devices attached") + 24);
                    if (output.Contains("device")) {
                        MessageBox.Show("Success\n" + output);
                    } else if (output.Contains("unauthorized")) {
                        MessageBox.Show("Please click [accept] on your phone\n" + output);
                    }
                    else
                    {
                        MessageBox.Show("Fail\n" + output);
                    }
                    
                    result = true;
                }
            }
            catch
            {

            }
        }
    }
}
