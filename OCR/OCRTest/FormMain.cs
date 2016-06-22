using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OCRTest
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }
        List<string> placas = new List<string>();
        TcpListener t1;
        TcpListener t2;
        Socket skt;
        Socket skt2;
        NetworkStream ns;
        //StreamReader sr;
        Thread th;
        Thread th2;
       TcpClient cliente;
        String placa;

        NetworkStream ns2;
        NetworkStream ns3;
        TcpClient cliente2;
        string cabecalho;
        string cabecalho2;
        private void aceito()
        {
            try
            {
                t2 = new TcpListener(4350);
                t2.Start();
                skt2 = t2.AcceptSocket();
                ns3 = new NetworkStream(skt2);
                byte[] buffer = new byte[10000];
                ns3.Read(buffer, 0, 1000);
                //Recebe o cabeçalho
                cabecalho2 = Encoding.UTF8.GetString(buffer);
                cabecalho2 = cabecalho2.Substring(0, cabecalho2.IndexOf("$"));
                
                //Identifica a ação e verifica o que fazer
                if ((cabecalho2.Substring(0, 1).Equals("2")) && (cabecalho2.Substring(1, 1).Equals("1")) )
                {
                    Console.WriteLine("Ação: 2-Placa reconhecida");
                }
                else if ((cabecalho2.Substring(0, 1).Equals("2")) && (cabecalho2.Substring(1, 1).Equals("0")))
                {
                    Console.WriteLine("Ação: 2-Placa não reconhecida");
                   
                }
                t2.Stop();
                if (skt.Connected == true)
                {
                    while (true)
                    {
                        aceito();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void processo2()
        {
            var img = (Bitmap)pictureBox1.Image.Clone();
            var showImg = (Bitmap)pictureBox1.Image.Clone();
            //img = Grayscale.CommonAlgorithms.Y.Apply(img);
            img = new OtsuThreshold().Apply(img);
            img = new Erosion().Apply(img);
            img = new Invert().Apply(img);

            BlobCounter bc = new BlobCounter();
            bc.BackgroundThreshold = Color.Black;
            bc.ProcessImage(img);
            
            Rectangle rect = new Rectangle(0, 0, showImg.Width, showImg.Height);
            BitmapData bmpData = showImg.LockBits(rect, ImageLockMode.ReadWrite, showImg.PixelFormat);

            bc.GetObjectsRectangles().ToList().ForEach(i =>
            {
                Crop filter = new Crop(new Rectangle(i.X, i.Y, 230, 75));
                var img2 = (Bitmap)filter.Apply(img);
                img2 = new Invert().Apply(img2);
                var ocr = new tessnet2.Tesseract();
                ocr.SetVariable("tesseract_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVXWYZ-1234567890");
                ocr.Init(@"tessdata", "eng", false);
                var result = ocr.DoOCR(img2, Rectangle.Empty);
                StringBuilder sb = new StringBuilder();
                foreach (tessnet2.Word word in result)
                    sb.Append(word.Text + " ");
                //cliente para servidor
                string aux;
                aux = sb.ToString();
                if (aux.Length >= 6)
                {
                    placas.Add(aux);

                }
                pictureBox1.Image = img2;

            });

            showImg.UnlockBits(bmpData);

            pictureBox1.Image = img;
        }
        private void processo(){
            List<string> placas = new List<string>();
            var img = (Bitmap)pictureBox1.Image.Clone();
            var showImg = (Bitmap)pictureBox1.Image.Clone();
            img = Grayscale.CommonAlgorithms.Y.Apply(img);
            img = new OtsuThreshold().Apply(img);
            img = new Erosion().Apply(img);
            img = new Invert().Apply(img);

            BlobCounter bc = new BlobCounter();
            bc.BackgroundThreshold = Color.Black;
            bc.ProcessImage(img);
            Rectangle rect = new Rectangle(0, 0, showImg.Width, showImg.Height);
            BitmapData bmpData = showImg.LockBits(rect, ImageLockMode.ReadWrite, showImg.PixelFormat);

            bc.GetObjectsRectangles().ToList().ForEach(i =>
            {
                Crop filter = new Crop(new Rectangle(i.X, i.Y, 230, 75));
                var img2 = (Bitmap)filter.Apply(img);
                img2 = new Invert().Apply(img2);
                var ocr = new tessnet2.Tesseract();
                ocr.SetVariable("tesseract_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVXWYZ-1234567890");
                ocr.Init(@"tessdata", "eng", false);
                var result = ocr.DoOCR(img2, Rectangle.Empty);
                StringBuilder sb = new StringBuilder();
                foreach (tessnet2.Word word in result)
                    sb.Append(word.Text + " ");
                //cliente para servidor
                string aux;
                aux = sb.ToString();
                if (aux.Length >= 6)
                {
                    placas.Add(aux);

                }
                foreach (string aux2 in placas)
                {
                    string cabecalho;
                    try
                    {
                        if (aux2.Length > 6)
                        {
                            cabecalho = "3";
                            cabecalho += aux2 + "$";
                        }
                        else
                        {
                            cabecalho = "4";
                        }
                        byte[] buffer = Encoding.ASCII.GetBytes(cabecalho + "$");
                        cliente2 = new TcpClient("172.16.102.113", 4250);
                        ns2 = cliente2.GetStream();
                        ns2.Write(buffer, 0, buffer.Length);
                        ns2.Flush();
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
                
                pictureBox1.Image = img2;

            });

            showImg.UnlockBits(bmpData);

            pictureBox1.Image = img;
        }
        
        void ReceiveImage()
        {
            try
            {
                t1 = new TcpListener(4242);
                t1.Start();
                skt = t1.AcceptSocket();
                ns = new NetworkStream(skt);
                pictureBox1.Image = System.Drawing.Image.FromStream(ns);
                processo();
                t1.Stop();
                if (skt.Connected == true)
                {
                    while (true)
                    {
                        ReceiveImage();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        public Bitmap currentImage { get; set; }

        public Bitmap process(Bitmap bmp)
        {
            Bitmap newImage = (Bitmap)bmp.Clone();
            newImage = new AForge.Imaging.Filters.GrayscaleY().Apply(newImage);
            newImage = new AForge.Imaging.Filters.Threshold(50).Apply(newImage);
            newImage = new AForge.Imaging.Filters.Median().Apply(newImage);
            newImage = new AForge.Imaging.Filters.Median().Apply(newImage);
            newImage = new AForge.Imaging.Filters.Dilatation().Apply(newImage);
            newImage = new AForge.Imaging.Filters.Median().Apply(newImage);
            newImage = new AForge.Imaging.Filters.Erosion().Apply(newImage);
            newImage = new AForge.Imaging.Filters.Erosion().Apply(newImage);

            return newImage;
        }
        

        private void cortar(int x, int y, int w, int z)
        {
            var img = (Bitmap)pictureBox1.Image.Clone();
            int a, b;
            a = (img.Width) / x;
            b = (img.Height) / y;
            Crop filter = new Crop(new Rectangle(a, b, w, z));
            var newImage = (Bitmap)filter.Apply(img);
            currentImage = newImage;
            pictureBox1.Image = newImage;
        }
        private void cortar2(int y, int w, int z)
        {
            var img = (Bitmap)pictureBox1.Image.Clone();
            int a, b;
            a = 0;
            b = (img.Height) / y;
            Crop filter = new Crop(new Rectangle(a, b, w, z));
            var newImage = (Bitmap)filter.Apply(img);
            currentImage = newImage;
            pictureBox1.Image = newImage;
        }
        private void cortar3(int x, int w, int z)
        {
            var img = (Bitmap)pictureBox1.Image.Clone();
            Int32 a, b;
            a = (img.Width) / x;
            b = 0;
            Crop filter = new Crop(new Rectangle(a, b, w, z));
            var newImage = (Bitmap)filter.Apply(img);
            currentImage = newImage;
            pictureBox1.Image = newImage;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                var img = System.Drawing.Image.FromFile(openFileDialog.FileName);
                pictureBox1.Image = img;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var image = (Bitmap) pictureBox1.Image;
            var ocr = new tessnet2.Tesseract();
            ocr.Init(@"tessdata", "eng", true);
            ocr.SetVariable("tesseract_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVXWYZ-1234567890");
            var result = ocr.DoOCR(image, Rectangle.Empty);
            StringBuilder sb = new StringBuilder();
            foreach (tessnet2.Word word in result)
                sb.Append(word.Text + " ");
            textBox1.Text = sb.ToString();
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            th = new Thread(new ThreadStart(ReceiveImage));
            th.Start();
            th2 = new Thread(new ThreadStart(aceito));
            th2.Start();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            List<string> placas = new List<string>();
            var img = (Bitmap)pictureBox1.Image.Clone();
            var showImg = (Bitmap)pictureBox1.Image.Clone();
           // img = Grayscale.CommonAlgorithms.Y.Apply(img);
            img = new OtsuThreshold().Apply(img);
            img = new Erosion().Apply(img);
            img = new Invert().Apply(img);

            BlobCounter bc = new BlobCounter();
            bc.BackgroundThreshold = Color.Black;
            bc.ProcessImage(img);
            MessageBox.Show(String.Format("The image contains {0} objects.", bc.ObjectsCount));

            Rectangle rect = new Rectangle(0, 0, showImg.Width, showImg.Height);
            BitmapData bmpData = showImg.LockBits(rect, ImageLockMode.ReadWrite, showImg.PixelFormat);
            
            bc.GetObjectsRectangles().ToList().ForEach(i =>
            {
                Crop filter = new Crop(new Rectangle(i.X, i.Y, 230,75));
                var img2 = (Bitmap)filter.Apply(img);
                img2 = new Invert().Apply(img2);
                var ocr = new tessnet2.Tesseract();
                ocr.SetVariable("tesseract_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVXWYZ-1234567890");
                ocr.Init(@"tessdata", "eng", false);
                var result = ocr.DoOCR(img2, Rectangle.Empty);
                StringBuilder sb = new StringBuilder();
                foreach (tessnet2.Word word in result)
                    sb.Append(word.Text + " ");
                //cliente para servidor
                string aux;
                aux = sb.ToString();
                if (aux.Length >=6)
                {
                    placas.Add(aux);
              
                }
                   
                MessageBox.Show("?"/*String.Format(sb.ToString())*/);
                textBox1.Text = sb.ToString();
                pictureBox1.Image = img2;
               
            });

            foreach (string aux2 in placas)
            {
                try
                {

                    string cabecalho = "3";
                    cabecalho += aux2 + "$";
                    byte[] buffer = Encoding.ASCII.GetBytes(cabecalho + "$");
                    cliente2 = new TcpClient("172.16.102.113", 4250);
                    ns2 = cliente2.GetStream();
                    ns2.Write(buffer, 0, buffer.Length);
                    ns2.Flush();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            } 
            showImg.UnlockBits(bmpData);

            pictureBox1.Image = img;
        }

        private void crop1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cortar(3, 3, 600, 400);
            cortar(3, 3, 350, 250);
            cortar2(2, 350, 125);
            cortar2(3, 350, 70);
            cortar3(7, 220, 70);
        }

        private void crop2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cortar(3, 3, 600, 400);
            cortar(3, 3, 350, 250);
          
        }

        private void processarFiltrosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var img = (Bitmap)pictureBox1.Image.Clone();
           // var showImg = (Bitmap)currentImage.Clone();
            img = process(img);
            currentImage = img;
            pictureBox1.Image = currentImage;
        }

        private void cinzaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var img = (Bitmap)pictureBox1.Image.Clone();
            img = Grayscale.CommonAlgorithms.Y.Apply(img);
            pictureBox1.Image = img;
        }
    }
}
