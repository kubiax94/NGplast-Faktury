﻿using System;
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
using Microsoft.Win32;
using Services.FilesService;
using Services.OCRService;
using Skanowanie_faktur.ViewModel;

namespace Skanowanie_faktur
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       

        public MainWindow(object mainView)
        {
            InitializeComponent();

            DataContext = mainView;


        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = new TextBox();
            
        }

        private void ComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            var a = sender as DataGrid;
            Console.WriteLine(sender);
        }
    }
}
