using Microsoft.Win32;
using Skanowanie_faktur.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using WPFFolderBrowser;

namespace Skanowanie_faktur.Command
{
    public class SelecDictionaryCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            var obj = parameter as MainViewModel;
                Console.WriteLine(obj.FilePath);
                var test =  new WPFFolderBrowserDialog(); 

                if (test.ShowDialog() == true)
                {
                    obj.FilePath = test.FileName;
                }
        }
    }
}
