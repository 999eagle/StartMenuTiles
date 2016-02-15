using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using System.Windows.Input;

namespace StartMenuTiles.Controls
{
    class CropControl : Grid
    {
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(CropControl), new PropertyMetadata(null, OnColorChanged));
        public static readonly DependencyProperty ClipRectProperty = DependencyProperty.Register("ClipRect", typeof(Rect), typeof(CropControl), new PropertyMetadata(null, OnClipRectChanged));
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(CropControl), new PropertyMetadata(null, OnImageSourceChanged));
        public static readonly DependencyProperty CroppedImageDestinationProperty = DependencyProperty.Register("CroppedImageDestination", typeof(string), typeof(CropControl), new PropertyMetadata(null));
        public static readonly DependencyProperty ImageCroppedCommandProperty = DependencyProperty.Register("ImageCroppedCommand", typeof(ICommand), typeof(CropControl), new PropertyMetadata(null));

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public Rect ClipRect
        {
            get { return (Rect)GetValue(ClipRectProperty); }
            set { SetValue(ClipRectProperty, value); }
        }

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public string CroppedImageDestination
        {
            get { return (string)GetValue(CroppedImageDestinationProperty); }
            set { SetValue(CroppedImageDestinationProperty, value); }
        }

        public ICommand ImageCroppedCommand
        {
            get { return (ICommand)GetValue(ImageCroppedCommandProperty); }
            set { SetValue(ImageCroppedCommandProperty, value); }
        }

        static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            CropControl self = (CropControl)d;
            var b = new SolidColorBrush(self.Color);
            self.m_bottomBorder.Background = b;
            self.m_leftBorder.Background = b;
            self.m_rightBorder.Background = b;
            self.m_topBorder.Background = b;
        }

        static void OnClipRectChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            CropControl self = (CropControl)d;
            self.UpdateBorderSizes();
            var size = new Size(self.ClipRect.Width, self.ClipRect.Height);
            self.ClampClipRect();
            if (self.m_lastClipRectSize != size)
            {
                self.UpdateZoomFactors();
                self.m_lastClipRectSize = size;
            }
        }

        static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            CropControl self = (CropControl)d;
            self.m_image.ImageOpened += self.OnImageOpened;
            self.m_image.Source = self.ImageSource;
        }

        private void OnImageOpened(object sender, RoutedEventArgs e)
        {
            m_image.ImageOpened -= OnImageOpened;
            var src = m_image.Source as BitmapSource;
            m_image.Width = src.PixelWidth;
            m_image.Height = src.PixelHeight;
            m_imgSourceSize = new Size(src.PixelWidth, src.PixelHeight);

            m_zoomSlider.ValueChanged -= OnSliderValueChanged;
            UpdateZoomFactors();
            m_zoomSlider.ValueChanged += OnSliderValueChanged;
            m_zoomSlider.Value = 1;
        }

        Border m_leftBorder, m_rightBorder, m_topBorder, m_bottomBorder, m_imageBorder;
        Image m_image;
        Slider m_zoomSlider;
        bool m_dragging;
        Size m_lastClipRectSize = new Size(), m_imgSourceSize;
        Point m_lastPoint;
        CircleButton m_cropButton;

        public CropControl()
        {
            m_image = new Image();
            m_image.Stretch = Stretch.Uniform;
            m_image.SizeChanged += Image_SizeChanged;
            Children.Add(m_image);

            m_leftBorder = new Border();
            m_leftBorder.Clip = new RectangleGeometry();
            Children.Add(m_leftBorder);

            m_rightBorder = new Border();
            m_rightBorder.Clip = new RectangleGeometry();
            Children.Add(m_rightBorder);

            m_topBorder = new Border();
            m_topBorder.Clip = new RectangleGeometry();
            Children.Add(m_topBorder);

            m_bottomBorder = new Border();
            m_bottomBorder.Clip = new RectangleGeometry();
            Children.Add(m_bottomBorder);

            m_imageBorder = new Border();
            m_imageBorder.BorderThickness = new Thickness(2);
            m_imageBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0x33, 0xff, 0xff, 0xff));
            Children.Add(m_imageBorder);

            var stackPanel = new StackPanel();
            stackPanel.Margin = new Thickness(12);
            stackPanel.VerticalAlignment = VerticalAlignment.Bottom;
            stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
            stackPanel.Orientation = Orientation.Horizontal;
            Children.Add(stackPanel);

            var textBlock = new TextBlock();
            textBlock.FontFamily = new FontFamily("Segoe MDL2 Assets");
            textBlock.FontSize = 34;
            textBlock.Margin = new Thickness(0, 0, 10, 0);
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Text = "\ue1a4";
            stackPanel.Children.Add(textBlock);

            m_zoomSlider = new Slider();
            m_zoomSlider.Maximum = 1;
            //m_zoomSlider.Margin = new Thickness(12);
            m_zoomSlider.VerticalAlignment = VerticalAlignment.Center;
            //m_zoomSlider.HorizontalAlignment = HorizontalAlignment.Left;
            m_zoomSlider.Width = 300;
            m_zoomSlider.Value = 0;
            m_zoomSlider.ValueChanged += OnSliderValueChanged;
            m_zoomSlider.StepFrequency = 0.0001;
            stackPanel.Children.Add(m_zoomSlider);

            textBlock = new TextBlock();
            textBlock.FontFamily = new FontFamily("Segoe MDL2 Assets");
            textBlock.FontSize = 34;
            textBlock.Margin = new Thickness(10, 0, 0, 0);
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Text = "\ue12e";
            stackPanel.Children.Add(textBlock);

            m_cropButton = new CircleButton();
            m_cropButton.Icon = "\ue123";
            m_cropButton.FontSize = 34;
            m_cropButton.Command = new Mvvm.Command(ExecuteCrop);
            m_cropButton.VerticalAlignment = VerticalAlignment.Bottom;
            m_cropButton.HorizontalAlignment = HorizontalAlignment.Right;
            m_cropButton.Margin = new Thickness(12);
            Children.Add(m_cropButton);

            SizeChanged += OnSizeChanged;
            PointerPressed += OnPointerPressed;
            PointerMoved += OnPointerMoved;
            PointerExited += OnPointerExited;
            PointerReleased += OnPointerReleased;

            Background = new SolidColorBrush(Colors.Black);
            Color = Color.FromArgb(0x33, 0xff, 0xff, 0xff);
        }

        private void OnPointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            m_dragging = false;
        }

        private void OnPointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            m_dragging = false;
        }

        private void OnPointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (m_dragging)
            {
                var newPoint = e.GetCurrentPoint(this).Position;
                var pos = new Point(ClipRect.X + newPoint.X - m_lastPoint.X, ClipRect.Y + newPoint.Y - m_lastPoint.Y);
                ClipRect = new Rect(pos, new Size(ClipRect.Width, ClipRect.Height));
                m_lastPoint = newPoint;
            }
        }

        private void OnPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (m_imgSourceSize != null && m_imgSourceSize.Height > 0 && m_imgSourceSize.Width > 0)
            {
                m_dragging = true;
                m_lastPoint = e.GetCurrentPoint(this).Position;
            }
        }

        private void OnSliderValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            double z = m_zoomSlider.Value;
            var src = m_image.Source as BitmapSource;
            if (src != null && m_imgSourceSize != null)
            {
                m_image.Width = m_imgSourceSize.Width * z;
                m_image.Height = m_imgSourceSize.Height * z;
                if (e.OldValue > 0 && e.NewValue > 0)
                {
                    var rect = ClipRect;
                    rect.X = (rect.X + rect.Width / 2) / e.OldValue * e.NewValue - rect.Width / 2;
                    rect.Y = (rect.Y + rect.Height / 2) / e.OldValue * e.NewValue - rect.Height / 2;
                    ClipRect = rect;
                }
            }
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBorderSizes();
            ClampClipRect();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBorderSizes();
            UpdateZoomFactors();
        }

        void UpdateBorderSizes()
        {
            var rect = ClipRect;
            rect.X += (ActualWidth - m_image.ActualWidth) / 2;
            rect.Y += (ActualHeight - m_image.ActualHeight) / 2;
            m_imageBorder.Margin = new Thickness(rect.Left, rect.Top, Math.Max(ActualWidth - rect.Right, 0), Math.Max(ActualHeight - rect.Bottom, 0));
            m_bottomBorder.Clip.Rect = new Rect(0, rect.Bottom, ActualWidth, Math.Max(ActualHeight - rect.Bottom, 0));
            m_leftBorder.Clip.Rect = new Rect(0, rect.Top, rect.Left, rect.Height);
            m_rightBorder.Clip.Rect = new Rect(rect.Right, rect.Top, Math.Max(ActualWidth - rect.Right, 0), rect.Height);
            m_topBorder.Clip.Rect = new Rect(0, 0, ActualWidth, rect.Top);
        }

        void UpdateZoomFactors()
        {
            var rect = ClipRect;
            var src = m_image.Source as BitmapSource;
            if (src != null && src.PixelHeight > 0 && src.PixelWidth > 0)
            {
                double zx, zy;
                zx = rect.Width / src.PixelWidth;
                zy = rect.Height / src.PixelHeight;
                m_zoomSlider.Minimum = Math.Max(zx, zy);
                zx = ActualWidth / src.PixelWidth;
                zy = ActualWidth / src.PixelHeight;
                m_zoomSlider.Maximum = Math.Min(zx, zy);
            }
        }

        void ClampClipRect()
        {
            var rect = ClipRect;
            rect.X = Math.Max(0, Math.Min(m_image.ActualWidth - rect.Width, rect.X));
            rect.Y = Math.Max(0, Math.Min(m_image.ActualHeight - rect.Height, rect.Y));
            if(rect != ClipRect)
                ClipRect = rect;
        }

        async void ExecuteCrop()
        {
            var src = m_image.Source as BitmapImage;
            var srcUri = src.UriSource;
            await Common.CropHelper.CropImageAsync(srcUri.LocalPath, CroppedImageDestination, m_zoomSlider.Value, ClipRect);
            if (ImageCroppedCommand != null && ImageCroppedCommand.CanExecute(null))
                ImageCroppedCommand.Execute(null);
        }
    }
}
