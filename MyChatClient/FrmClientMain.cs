using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace MyChatClient
{
    public partial class FrmClientMain : Form
    {
        public FrmClientMain()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        Socket socket = null;
        Thread th = null;
        private void btnStart_Click(object sender, EventArgs e)
        {
            IPAddress ip = IPAddress.Parse(txtServer.Text);
            IPEndPoint ipep = new IPEndPoint(ip, int.Parse(txtPort.Text));
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(ipep);
                SetText("连接成功！");
                btnStart.Enabled = false;

                th = new Thread(GetMsg);
                th.IsBackground = true;
                th.Start();
            }
            catch (Exception ex)
            {
                SetText(ex.Message);
            }
        }

        private void GetMsg()
        {
            string ip = socket.RemoteEndPoint.ToString();
            while (true)
            {
                byte[] b = new byte[1024 * 1024];
                try
                {
                    int count = socket.Receive(b);
                    if (count == 0)
                    {
                        SetText("与服务器断开连接了~~");
                        socket.Close();
                        break;
                    }
                    byte flag = b[0];
                    if (flag == 0)
                    {
                        string recMsg = Encoding.UTF8.GetString(b, 1, count - 1);
                        SetText(ip + ":" + recMsg);
                    }
                    else if (flag == 1)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        if (sfd.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                        {
                            using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create))
                            {
                                fs.Write(b, 1, count - 1);
                            }
                        }
                    }
                    else if (flag == 2)
                    {
                        ZhenDong();
                    }

                }
                catch (Exception ex)
                {
                    Thread.CurrentThread.Abort();
                    SetText(ex.Message);
                    socket.Close();
                    break;
                }
            }
        }

        private void ZhenDong()
        {
            this.Show();
            this.TopMost = true;

            int x = this.Location.X;
            int y = this.Location.Y;
            for (int i = 0; i < 100; i++)
            {
                if (i % 2 == 0)
                {
                    this.Location = new Point(x - 10, y - 10);
                }
                else
                {
                    this.Location = new Point(x + 10, y + 10);
                }
                Thread.Sleep(10);

            }
            this.Location = new Point(x, y);
            this.TopMost = false;
        }

        private void SetText(string str)
        {
            txtLog.AppendText(str + "\r\n");
        }

        private void btnSend_Click(object sender, EventArgs e)
        {

            if (socket != null)
            {
                if (socket.Connected)
                {
                    try
                    {
                        byte[] b = Encoding.UTF8.GetBytes(txtMsg.Text);
                        socket.Send(b);
                        SetText(socket.LocalEndPoint + ":" + txtMsg.Text);
                    }
                    catch (Exception ex)
                    {
                        socket.Close();
                        SetText(ex.Message);
                    }
                }
            }

        }

        private void FrmClientMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (socket != null)
            {
                socket.Close();
            }
            if (th != null)
            {
                th.Abort();
            }
        }
    }
}
