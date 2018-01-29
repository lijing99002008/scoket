using SocketClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        SocketClient socketClient = new SocketClient();
        //SocketClient socketClient;
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //connButton_Click(null, null);
            socketClient.OnReceive += Class1_OnReceive;
            socketClient.OnConnected += SocketClient_OnConnected;
        }

        private void SocketClient_OnConnected(object sender, EventArgs e)
        {
            //连接成功
        }

        private void Class1_OnReceive(object sender, MsgEventArgs e)
        {
            textBox1.Text += e.Message + "\r\n";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(userNameTextBox.Text.Trim()) && !String.IsNullOrWhiteSpace(studioTextBox.Text.Trim()))
                socketClient.Login(userNameTextBox.Text.Trim(), studioTextBox.Text.Trim());
            else
                MessageBox.Show("请填写用户名和站点号!");

        }

        private void connButton_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(urlTextBox.Text.Trim()))
            {
                socketClient.Abort();
                socketClient.Connect(urlTextBox.Text.Trim());
            }
            else
                MessageBox.Show("请填写服务器地址!");
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrWhiteSpace(messageTextBox.Text.Trim()))
            {
                MessageBox.Show("请填写发送内容");
                return;
            }
            if (String.IsNullOrWhiteSpace(receiveStudioTextBox.Text.Trim()) && String.IsNullOrWhiteSpace(receiveUserNameTextBox.Text.Trim()))
            {
                MessageBox.Show("请填写接收站点或用户名");
                return;
            }
            socketClient.SendMsg(receiveUserNameTextBox.Text.Trim(), receiveStudioTextBox.Text.Trim(), messageTextBox.Text.Trim());
        }
    }
}
