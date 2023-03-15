using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using Services.FilesService;

namespace Skanowanie_faktur
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> FileList { get; set; } 

        public MainWindow()
        {
            InitializeComponent();

            FileList = new ObservableCollection<string>();

            var a = new FilesReaderService();

            a.SearchLocation = @"c:\games";
            a.SearchPrefix = @"scan";

            


            var test = a.CreateFileList();

            foreach(var file in test)
            {
                FileList.Add(file);
                Console.WriteLine(file);
            }

            DataContext = this;
        }
    }
}
