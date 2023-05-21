using iTextSharp.text.pdf.codec;
using Microsoft.Win32;
using Services.FilesService;
using Services.OCRService;
using Skanowanie_faktur.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Skanowanie_faktur.ViewModel
{

    public class MainViewModel : BaseViewModel
    {
        IFilesReaderService filesReader;
        System.Windows.Controls.Image pdfViewer = new System.Windows.Controls.Image();

        public ICommand openDictionarySelection { get; }

        public IEnumerable<object> ConractorList => filesReader.ContractorList.Values;
        public IEnumerable<string> GetContractorsName => filesReader.GetContractorNames;

        public string FilePath { 
            get {
                return filesReader.Configuration.SearchDir;
            }
            set {
                filesReader.Configuration.SearchDir = value;
                NotifyPropertyChanged();
            }
        }

        public string FilePrefix {
            get {
                return filesReader.Configuration.SearchPrefix;
            }

            set {
                filesReader.Configuration.SearchPrefix = value;
                NotifyPropertyChanged();
            }
        }

        public byte[] CurrentPDFPath {
            get {
                return SelectedPDF.ImageData;
            }
        }

        IEnumerable<Tesseract.Rect> _currentScans;
        public IEnumerable<Tesseract.Rect> CurrentScanAreas {
            get => _currentScans;
            set {
                _currentScans = value;
                NotifyPropertyChanged();
            }
        }

        byte[] _currentImageData;
        public byte[] CurrentImageData {
            get {
                return _currentImageData;
            }

             set {
                _currentImageData = value;
                NotifyPropertyChanged();
            }
        }

        InnvoiceOCRResult? _selectedPDF = null;
        public InnvoiceOCRResult? SelectedPDF { get {
                return _selectedPDF;
            }
            set {
                _selectedPDF = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<InnvoiceOCRResult> FileList { get; set; }
        

        string _searchCrit;
        public string SearchCriteria {

            get {
                return _searchCrit;
            }

            set {
                _searchCrit = value;
                NotifyPropertyChanged();
            } 
        }

        public MainViewModel(IFilesReaderService pdfview)
        {
            filesReader = pdfview;

            FileList = new ObservableCollection<InnvoiceOCRResult>();
            filesReader = new FilesReaderService();
            openDictionarySelection = new SelecDictionaryCommand();



            FilePrefix = @"scan";
            FilePath = @"C:\Users\Kubiaxx\Documents\Programowanie\PRACA";

            var test = new InnvoiceOCRInputList(filesReader.CreateFileList());
            //var input = new InnvoiceOCRInput(test);
            var b = new InnvoiceOCR(filesReader);
            //filesReader.ImportContractors(@"C:\Users\Kubiaxx\Documents\Programowanie\PRACA\Koszty Listopad\test.xlsx");

            b.test(test);

            var path = @"C:\Users\Kubiaxx\Documents\Programowanie\PRACA\Koszty Listopad\Scan_15-03-2023_1140_0040.pdf";
            FileList = new ObservableCollection<InnvoiceOCRResult>(test.Values);

            filesReader.SaveContractors();
        }

        public void OpenFileBrowser()
        {
            var a = new OpenFileDialog();

        }

    }
}
