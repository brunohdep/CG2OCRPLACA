using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using client.Classes;

namespace client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //cria o servidor

        MemoryStream ms;
        TcpClient client;
        NetworkStream ns;
        BinaryWriter br;
        //server
        Socket skt;
        TcpListener t1;
        NetworkStream ns2;
        StreamReader sr;
        Thread th;
        TcpClient cliente;

        string ip;
        void ReceivedText()
        {
            try
            {
                t1 = new TcpListener(4243);
                t1.Start();
                skt = t1.AcceptSocket();
                ns2 = new NetworkStream(skt);
                byte[] recebido = new byte[10000];
                ns2.Read(recebido, 0, (int)cliente.ReceiveBufferSize);
                string line;
                line = Encoding.ASCII.GetString(recebido);
                line = line.Substring(0, line.IndexOf("$"));
                textBox3.Text = line;
                t1.Stop();
                if (skt.Connected == true)
                {
                    while (true)
                    {
                        ReceivedText();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        //obtem endereço local
        //string GetIpAdress()
        //{
        //    //IPHostEntry host;
        //    string localip = "172.16.103.16";
        //    //host = Dns.GetHostEntry(Dns.GetHostName());
        //    //foreach (IPAddress ip in host.AddressList)
        //    //{
        //    //    if(ip.AddressFamily.ToString()== "InterNetwork")
        //    //    {
        //    //        localip = ip.ToString();
        //    //    }
        ////}
        //    return localip;
        //}
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string caminho = openFileDialog1.FileName;
            pictureBox1.Image = Image.FromFile(caminho);
            textBox2.Text = caminho;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                ms = new MemoryStream();
                pictureBox1.Image.Save(ms, pictureBox1.Image.RawFormat);
                byte[] buffer = ms.GetBuffer();
                ms.Close();
                client = new TcpClient(textBox1.Text, 4242);
                ns = client.GetStream();
                br = new BinaryWriter(ns);
                br.Write(buffer);
                br.Close();
                ns.Close();
                client.Close();

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            th = new Thread(new ThreadStart(ReceivedText));
            th.Start();
            //textBox1.Text = GetIpAdress();

        }
    }
}
