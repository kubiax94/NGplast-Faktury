using Microsoft.Win32;
using Services.FilesService;
using Services.OCRService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Skanowanie_faktur.ViewModel
{

    public class MainViewModel : BaseViewModel
    {
        FilesReaderService filesReader;
        
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

        public ObservableCollection<string> FileList { get; set; }

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

        public MainViewModel()
        {

            FileList = new ObservableCollection<string>();
            filesReader = new FilesReaderService();

            FilePrefix = @"scan";
            FilePath = @"C:\Users\Kubiaxx\Documents\Programowanie\PRACA\Koszty Listopad";

            var test = filesReader.CreateFileList();

            foreach (var file in test)
            {
                FileList.Add(file);
                Console.WriteLine(file);
            }

            var b = new InnvoiceOCR();

            b.SearchForInnvoicesDetails(new InnvoiceOCRInput(test));
        }

        public void OpenFileBrowser()
        {
            var a = new OpenFileDialog();

        }

    }
}
