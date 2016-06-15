using System;

namespace OCRTest
{
    using System.Drawing;
    using System.Windows.Forms;
    using tessnet2;

    class Program
    {
        //static void Main(string[] args)
        //{
        //    var image = new Bitmap(@"C:\tess\gabriel.png");
        //    var ocr = new Tesseract();
        //    ocr.Init(@"tessdata", "eng", false);
        //    var result = ocr.DoOCR(image, Rectangle.Empty);
        //    foreach (Word word in result)
        //        Console.WriteLine(word.Text);
        //    Console.ReadLine();
        //}

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
