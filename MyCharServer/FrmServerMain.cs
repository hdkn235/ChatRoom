using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace MyCharServer
{
    public partial class FrmServerMain : Form
    {
        Thread th = null;
        Thread thGetMsg = null;
        Dictionary<string, Socket> dic = new Dictionary<string, Socket>();

        public FrmServerMain()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {

            IPAddress ip = IPAddress.Parse(txtServer.Text);
            IPEndPoint ep = new IPEndPoint(ip, 50000);
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                s.Bind(ep);
                s.Listen(1);
                btnStart.Enabled = false;
                SetText("开始监听");

                th = new Thread(Listen);
                th.IsBackground = true;
                th.Start(s);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        int n = 0;
        private void Listen(object s)
        {
            Socket socket = s as Socket;
            while (n < 100)
            {
                n++;
                Socket sConn = socket.Accept();
                string remoteIp = sConn.RemoteEndPoint.ToString();
                SetText(remoteIp + ":连接成功！");

                cboUsers.Items.Add(remoteIp);
                dic.Add(remoteIp, sConn);

                thGetMsg = new Thread(RecMsg);
                thGetMsg.IsBackground = true;
                thGetMsg.Start(sConn);
            }
        }

        private void RecMsg(object o)
        {
            Socket socket = o as Socket;
            string ip = socket.RemoteEndPoint.ToString();
            while (true)
            {
                byte[] b = new byte[1024 * 1024];
                try
                {
                    int count = socket.Receive(b);
                    if (count == 0)
                    {
                        SetText(ip + ":离开了");
                        break;
                    }
                    string recMsg = Encoding.UTF8.GetString(b, 0, count);
                    SetText(ip + ":" + recMsg);
                }
                catch
                {
                    socket.Close();
                    SetText(ip + ":断开连接");
                    break;
                }
            }
        }

        private void SetText(string msg)
        {
            txtLog.AppendText(msg + " \r\n");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (th != null)
            {
                th.Abort();
            }
            if (thGetMsg != null)
            {
                thGetMsg.Abort();
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (cboUsers.SelectedIndex >= 0)
            {
                Socket ss = dic[cboUsers.Text];
                byte[] buffer = Encoding.UTF8.GetBytes(txtMsg.Text) ;

                List<byte> list = new List<byte>();
                list.Add(0);
                list.AddRange(buffer);

                try
                {
                    ss.Send(list.ToArray());
                    SetText(ss.LocalEndPoint + ":" + txtMsg.Text);
                }
                catch (Exception ex)
                {
                    ss.Close();
                    SetText(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("请选择客户端！");
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtPath.Text = ofd.FileName;
            }
        }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            if (cboUsers.SelectedIndex >= 0)
            {
                if (string.IsNullOrEmpty(txtPath.Text))
                {
                    MessageBox.Show("请选择文件的路径");
                }
                else
                {
                    using (FileStream fs = new FileStream(txtPath.Text, FileMode.Open))
                    {
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0,buffer.Length);

                        List<byte> list = new List<byte>();
                        list.Add(1);
                        list.AddRange(buffer);

                        dic[cboUsers.Text].Send(list.ToArray());
                    }
                }

            }
            else
            {
                MessageBox.Show("请选择发送的客户端！");
            }
        }

        private void btnZD_Click(object sender, EventArgs e)
        {
            if (cboUsers.SelectedIndex >= 0)
            {
                byte[] buffer = new byte[1];
                buffer[0] = 2;
                dic[cboUsers.Text].Send(buffer);
            }
            else
            {
                MessageBox.Show("请选择发送的客户端！");
            }
        }
    }
}
