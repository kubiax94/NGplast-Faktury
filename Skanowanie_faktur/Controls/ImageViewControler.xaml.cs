using iTextSharp.text.html.simpleparser;
using Org.BouncyCastle.Asn1.Crmf;
using Services.OCRService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Skanowanie_faktur.Controls
{
    /// <summary>
    /// Logika interakcji dla klasy ImageViewControler.xaml
    /// </summary>
    public partial class ImageViewControler : UserControl
    {
        private static int  _mainWidht = 1654,
                            _mainHeight = 2334;

        private List<ScanSize> _scanSizes { get; set; } = new();

        public static readonly DependencyProperty ResultProperty = DependencyProperty.Register(
            "Result", 
            typeof(InnvoiceOCRResult), 
            typeof(ImageViewControler), 
            new PropertyMetadata(new PropertyChangedCallback(ResultChanged))
            );

        private static void ResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            

            var imgcnt = (ImageViewControler)d;
            InnvoiceOCRResult newData = (InnvoiceOCRResult)e.NewValue;

            

            var image = new BitmapImage();
            
            using (MemoryStream ms = new MemoryStream(newData.ImageData))
            {
                ms.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = ms;
                image.EndInit();

            }
            Console.WriteLine(image.Width+ "x" + image.Height + "y");
            Console.WriteLine(image.PixelWidth + "px" + image.PixelHeight + "py" + " DPI: " + image.DpiX + " " + image.DpiY);
            image.Freeze();
            imgcnt.imgCont.Source = image;

            imgcnt.main.Children.Clear();

            double scaleW = ((imgcnt.ActualWidth * 100f / image.PixelWidth)) / 100f;
            double scaleH = (imgcnt.ActualHeight * 100f / image.PixelHeight) / 100f;

            foreach (var r in newData.GetScanAreas)
            {
                var newSize = new ScanSize(new Tesseract.Rect((int)((r.X1 * scaleW)- r.Width * scaleW), (int)((r.Y1 * scaleH)-r.Height * scaleH), r.Width, r.Height ));

                imgcnt.main.Children.Add(newSize);

                newSize.InvalidateVisual();
                var a = (BitmapImage)imgcnt.imgCont.Source;

                Console.WriteLine("DpiX: " + a.DpiX + " DpiY: " + a.DpiY);
            }
        }

        private static void ReloadScanSizes(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var controler = (ImageViewControler)d;
            controler.main.Children.Clear();


        }

        public InnvoiceOCRResult Result {
            get => (InnvoiceOCRResult)GetValue(ResultProperty);
            set => SetValue(ResultProperty, value);
        }


        public ImageViewControler()
        {
            InitializeComponent();
        }
    }
}
