using Services.FilesService;

using System;
using System.Collections.Generic;
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
using Tesseract;
using FileInfo = Services.FilesService.FileInfo;

namespace Skanowanie_faktur.Controls
{
    enum ScanSizeState
    {
        None,
        Drag,
        Resize
    }
    enum ScanAreaResizeMode
    {
        None,
        N,
        S,
        W,
        E,
        NW,
        NE,
        SW,
        SE
    }

    /// <summary>
    /// Logika interakcji dla klasy ScanSize.xaml
    /// </summary>
    public partial class ScanSize : UserControl
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImagePath",
            typeof(FileInfo), typeof(ScanSize), new PropertyMetadata(new PropertyChangedCallback(ChangeImageCallBack)));
            

        public FileInfo ImagePath { 
            get => (FileInfo)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);           
         }

        private static void ChangeImageCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScanSize scanSize = (ScanSize)d;
            var value = (FileInfo)e.NewValue;
            //scanSize.imgContent.Source = BitmapImage.Create((int)scanSize.Width, (int)scanSize.Height, 96, 96, PixelFormats.Gray8, null, value.FileData, 1);
            
        }
        int _mainWidht = 1654,
            _mainHeight = 2334;

        Point _mousePrevPos = new Point(0, 0);
        Point _mouseCurrrentPos = new Point(0, 0);
        Vector _mouseDir => _mouseCurrrentPos - _mousePrevPos;

        ScanSizeState _state = ScanSizeState.None;
        ScanAreaResizeMode _resizeMode = ScanAreaResizeMode.None;
        Canvas _parent { get; set; }
        Point _scanArePosition => new Point(Canvas.GetLeft(this), Canvas.GetTop(this));

        public ScanSize()
        {
            InitializeComponent();

            Canvas.SetLeft(this, 0);
            Canvas.SetTop(this, 0);

            ScanArea.MouseLeftButtonDown += StartDragScanArea;
            ScanArea.MouseLeftButtonUp += StopDragScanArea;

            var childrens = ScanArea.Children.GetEnumerator();
            while (childrens.MoveNext())
            {
                if(childrens.Current is Border)
                {
                    var child = (Border)childrens.Current;
                    child.MouseLeftButtonDown += StartResize;
                    child.MouseLeftButtonUp += StopResize;
                    child.MouseEnter += EnterResizeArea;
                    child.MouseLeave += ExitResizeArea;
                }
            }
        }

        public ScanSize(Tesseract.Rect rect)
        {
            InitializeComponent();

            SetBoundry(rect);
            
            ScanArea.MouseLeftButtonDown += StartDragScanArea;
            ScanArea.MouseLeftButtonUp += StopDragScanArea;

            var childrens = ScanArea.Children.GetEnumerator();
            while (childrens.MoveNext())
            {
                if (childrens.Current is Border)
                {
                    var child = (Border)childrens.Current;
                    child.MouseLeftButtonDown += StartResize;
                    child.MouseLeftButtonUp += StopResize;
                    child.MouseEnter += EnterResizeArea;
                    child.MouseLeave += ExitResizeArea;
                }
            }
        }
        public void SetBoundry(Tesseract.Rect boundry)
        {

            //float scaleW = ((627f * 100f / _mainWidht)) / 100f;
            //float scaleH = (617f * 100f / _mainHeight) / 100f;

            ScanArea.Width = boundry.Width;
            ScanArea.Height = boundry.Height;
            Canvas.SetLeft(this, boundry.X1);
            Canvas.SetTop(this, boundry.Y1);
        }


        private void StartDragScanArea(object sender, MouseEventArgs eventArgs)
        {
            if (_state != ScanSizeState.None)
                return;

            if (_parent == null)
                _parent = (Canvas)Parent;
            
                

            Mouse.OverrideCursor = Cursors.Hand;

            _state = ScanSizeState.Drag;
            _mousePrevPos = Mouse.GetPosition(_parent);
            App.Current.Windows[0].MouseMove += OnMouseMove;

        }

        private void StopDragScanArea(object sender, MouseEventArgs eventArgs)
        {
            if (_state != ScanSizeState.Drag)
                return;

            ResetStates();

        }

        private void OnMouseMove(object sender, MouseEventArgs eventArgs)
        {
            if (_state != ScanSizeState.None)
                _mouseCurrrentPos = Mouse.GetPosition(_parent);

            Console.WriteLine(eventArgs.GetPosition(_parent));
            switch(_state)
            {
                case ScanSizeState.Drag:

                    var containerPos = _scanArePosition;

                    var mouseDir = _mouseDir;

                    Canvas.SetLeft(this, Math.Clamp(containerPos.X + mouseDir.X, 0, _parent.ActualWidth - ScanArea.ActualWidth));
                    Canvas.SetTop(this, Math.Clamp(containerPos.Y + mouseDir.Y, 0, _parent.ActualHeight - ScanArea.ActualHeight));

                    if (Mouse.LeftButton == MouseButtonState.Released)
                        ResetStates();

                    _mousePrevPos = _mouseCurrrentPos;
                    break;
                        
                case ScanSizeState.Resize:
                    Resize();

                    _mousePrevPos = _mouseCurrrentPos;
                    break;

                default: 

                    break;

            }

        }

        private void StartResize(object sender, MouseEventArgs e)
        {
            if (_parent == null)
                _parent = (Canvas)Parent;

                
            _state = ScanSizeState.Resize;
            _mousePrevPos = Mouse.GetPosition(_parent);
            App.Current.Windows[0].MouseMove += OnMouseMove;
        }

        private void Resize()
        {
            if (_state != ScanSizeState.Resize)
                return;

            Vector mouseDir = _mouseDir;
            Point scanAreaPosition = _scanArePosition;

            double resizeStep = 1.3;


            switch (_resizeMode) {

                case ScanAreaResizeMode.N:


                    if (scanAreaPosition.Y <= 0.1 && mouseDir.Y < 0 ||
                        ScanArea.Height <= 10 && mouseDir.Y > 0)
                        mouseDir = new Vector(0, 0);

                    ScanArea.Height = Math.Clamp(ScanArea.ActualHeight - mouseDir.Y * resizeStep, 10, _parent.ActualHeight);
                    Canvas.SetTop(this, Math.Clamp(scanAreaPosition.Y + mouseDir.Y * resizeStep, 0, _parent.ActualHeight - ScanArea.Height));
                    
                    break;

                case ScanAreaResizeMode.S:

                    if (scanAreaPosition.Y + ScanArea.Height > _parent.ActualHeight && mouseDir.Y > 0)
                        break;

                    ScanArea.Height = Math.Clamp(ScanArea.ActualHeight + mouseDir.Y * resizeStep, 10, _parent.ActualHeight);

                  break;

                case ScanAreaResizeMode.W:

                    if (scanAreaPosition.X <= 0 && mouseDir.X < 0 ||
                        ScanArea.Width <= 10 && mouseDir.X > 0)
                        mouseDir = new Vector(0, 0);

                    ScanArea.Width = Math.Clamp(ScanArea.ActualWidth - mouseDir.X * resizeStep, 10, _parent.ActualWidth);
                    Canvas.SetLeft(this, Math.Clamp(scanAreaPosition.X + mouseDir.X * resizeStep, 0, _parent.ActualWidth - ScanArea.Width));

                    break;

                case ScanAreaResizeMode.E:

                    if (scanAreaPosition.X + ScanArea.Width > _parent.ActualWidth && mouseDir.X > 0)
                        break;

                    ScanArea.Width = Math.Clamp(ScanArea.ActualWidth + mouseDir.X * resizeStep, 10, _parent.ActualWidth);
                    break;

                case ScanAreaResizeMode.NW:

                    if (scanAreaPosition.Y <= 0.1 && mouseDir.Y < 0 ||
                        ScanArea.Height <= 10 && mouseDir.Y > 0)
                        mouseDir = new Vector(mouseDir.X, 0);

                    if (scanAreaPosition.X <= 0 && mouseDir.X < 0 ||
                        ScanArea.Width <= 10 && mouseDir.X > 0)
                        mouseDir = new Vector(0, mouseDir.Y);

                    ScanArea.Width = Math.Clamp(ScanArea.ActualWidth - mouseDir.X * resizeStep, 10, _parent.ActualWidth);
                    ScanArea.Height = Math.Clamp(ScanArea.ActualHeight - mouseDir.Y * resizeStep, 10, _parent.ActualHeight);

                    Canvas.SetLeft(this, Math.Clamp(scanAreaPosition.X + mouseDir.X * resizeStep, 0, _parent.ActualWidth - ScanArea.Width));
                    Canvas.SetTop(this, Math.Clamp(scanAreaPosition.Y + mouseDir.Y * resizeStep, 0, _parent.ActualHeight - ScanArea.Height));
                    break;

                case ScanAreaResizeMode.SE:

                    ScanArea.Width = Math.Clamp(ScanArea.ActualWidth + mouseDir.X * resizeStep, 10, _parent.ActualWidth);
                    ScanArea.Height = Math.Clamp(ScanArea.ActualHeight + mouseDir.Y * resizeStep, 10, _parent.ActualHeight);

                    break;

                case ScanAreaResizeMode.NE:

                    if (scanAreaPosition.Y <= 0.1 && mouseDir.Y < 0 ||
                        ScanArea.Height <= 10 && mouseDir.Y > 0)
                        mouseDir = new Vector(mouseDir.X, 0);

                    ScanArea.Width = Math.Clamp(ScanArea.ActualWidth + mouseDir.X * resizeStep, 10, _parent.ActualWidth);
                    ScanArea.Height = Math.Clamp(ScanArea.ActualHeight - mouseDir.Y * resizeStep, 10, _parent.ActualHeight);

                    Canvas.SetTop(this, Math.Clamp(scanAreaPosition.Y + mouseDir.Y * resizeStep, 0, _parent.ActualHeight - ScanArea.Height));
                    break;

                case ScanAreaResizeMode.SW:

                    if (scanAreaPosition.X <= 0 && mouseDir.X < 0 ||
                        ScanArea.Width <= 10 && mouseDir.X > 0)
                        mouseDir = new Vector(0, mouseDir.Y);


                    ScanArea.Width = Math.Clamp(ScanArea.ActualWidth - mouseDir.X * resizeStep, 10, _parent.ActualWidth);
                    ScanArea.Height = Math.Clamp(ScanArea.ActualHeight + mouseDir.Y * resizeStep, 10, _parent.ActualHeight);

                    Canvas.SetLeft(this, Math.Clamp(scanAreaPosition.X + mouseDir.X * resizeStep, 0, _parent.ActualWidth - ScanArea.Width));
                    break;


            }


            _mousePrevPos = _mouseCurrrentPos;

            if (Mouse.LeftButton == MouseButtonState.Released)
                ResetStates();

        }

        private void ResetStates()
        {
            _state = ScanSizeState.None;
            _resizeMode = ScanAreaResizeMode.None;
            Mouse.OverrideCursor = null;
            App.Current.Windows[0].MouseMove -= OnMouseMove;
        }

        private void StopResize(object sender, MouseEventArgs e)
        {
            if (_state != ScanSizeState.Resize)
                return;

            ResetStates();
        }

        private void EnterResizeArea(object sender, MouseEventArgs e)
        {
            if (_state != ScanSizeState.None)
                return;

            Border border = (Border)sender;

            switch (border.Name)
            {
                case "N":
                    Mouse.OverrideCursor = Cursors.SizeNS;
                    _resizeMode = ScanAreaResizeMode.N;
                    break;

                case "S":
                    Mouse.OverrideCursor = Cursors.SizeNS;
                    _resizeMode = ScanAreaResizeMode.S;
                    break;

                case "W":
                    Mouse.OverrideCursor = Cursors.SizeWE;
                    _resizeMode = ScanAreaResizeMode.W;
                    break;

                case "E":
                    Mouse.OverrideCursor = Cursors.SizeWE;
                    _resizeMode = ScanAreaResizeMode.E;
                    break;

                case "NW":
                    Mouse.OverrideCursor = Cursors.SizeNWSE;
                    _resizeMode = ScanAreaResizeMode.NW;
                    break;

                case "NE":
                    Mouse.OverrideCursor = Cursors.SizeNESW;
                    _resizeMode = ScanAreaResizeMode.NE;
                    break;

                case "SW":
                    Mouse.OverrideCursor = Cursors.SizeNESW;
                    _resizeMode = ScanAreaResizeMode.SW;
                    break;

                case "SE":
                    Mouse.OverrideCursor = Cursors.SizeNWSE;
                    _resizeMode = ScanAreaResizeMode.SE;
                    break;
            }
        }

        private void ExitResizeArea(object sender, MouseEventArgs e)
        {
            if (_state == ScanSizeState.None)
            {
                Mouse.OverrideCursor = null;
                _resizeMode = ScanAreaResizeMode.None;
            }
        }

    }
}
