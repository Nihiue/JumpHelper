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
                    string str = string.Format(@"{0} {1} {2}", "adb shell input swipe 300 300 300 300", duration, "&exit");

                    myPro.StandardInput.WriteLine(str);
                    myPro.StandardInput.AutoFlush = true;
                    myPro.WaitForExit();

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
    }
}
