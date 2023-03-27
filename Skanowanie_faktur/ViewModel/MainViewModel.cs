using Microsoft.Win32;
using Services.FilesService;
using Services.OCRService;
using Skanowanie_faktur.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Skanowanie_faktur.ViewModel
{

    public class MainViewModel : BaseViewModel
    {
        FilesReaderService filesReader;
        WebBrowser pdfViewer;

        public ICommand openDictionarySelection { get; }
        
        public string FilePath { 
            get {
                return filesReader.SearchLocation;
            }
            set {
                filesReader.SearchLocation = value;
                NotifyPropertyChanged();
            }
        }

        public string FilePrefix {
            get {
                return filesReader.SearchPrefix;
            }

            set {
                filesReader.SearchPrefix = value;
                NotifyPropertyChanged();
            }
        }

        public Uri CurrentPDFPath {
            get {
                return new Uri(@$"{FilePath}\{SelectedPDF.OrginalFileName}");
            }
        }

        InnvoiceOCRResult _selectedPDF = new InnvoiceOCRResult();
        public InnvoiceOCRResult SelectedPDF { get {
                return _selectedPDF;
            }
            set {
                _selectedPDF = value;
                pdfViewer.Source = CurrentPDFPath;
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

        public MainViewModel(WebBrowser pdfview)
        {
            pdfViewer = pdfview;

            FileList = new ObservableCollection<InnvoiceOCRResult>();
            filesReader = new FilesReaderService();
            openDictionarySelection = new SelecDictionaryCommand();



            FilePrefix = @"scan";
            FilePath = @"C:\Users\Kubiaxx\Documents\Programowanie\PRACA\Koszty Listopad";

            var test = filesReader.CreateFileList();
            var input = new InnvoiceOCRInput(test);
            var b = new InnvoiceOCR();
            filesReader.ImportContractors(@"C:\Users\Kubiaxx\Documents\Programowanie\PRACA\Koszty Listopad\test.xlsx");

            b.SearchForInnvoicesDetails(input);

            FileList = new ObservableCollection<InnvoiceOCRResult>(input.InnvoiceOCRResults);
            

        }

        public void OpenFileBrowser()
        {
            var a = new OpenFileDialog();

        }

    }
}
