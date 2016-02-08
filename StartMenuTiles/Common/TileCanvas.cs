using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace StartMenuTiles.Common
{
    public class TileCanvas : Canvas
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(TileCanvas), new PropertyMetadata(null, ImageSourceChanged));

        private Size lastActualSize;

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public TileCanvas()
        {
            LayoutUpdated += OnLayoutUpdated;
        }

        private void OnLayoutUpdated(object sender, object o)
        {
            var newSize = new Size(ActualWidth, ActualHeight);
            if (lastActualSize != newSize)
            {
                lastActualSize = newSize;
                Rebuild();
            }
        }

        private static void ImageSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TileCanvas self = (TileCanvas)o;
            var src = self.ImageSource;
            if (src != null)
            {
                var img = new Image { Source = src };
                img.ImageOpened += self.Image_OnImageOpened;
                img.ImageFailed += self.Image_OnImageFailed;

                self.Children.Add(img);
            }
        }

        private void Image_OnImageFailed(object sender, ExceptionRoutedEventArgs args)
        {
            var img = (Image)sender;
            img.ImageFailed -= Image_OnImageFailed;
            img.ImageOpened -= Image_OnImageOpened;
            Children.Add(new TextBlock { Text = args.ErrorMessage, Foreground = new SolidColorBrush(Colors.Red) });
        }

        private void Image_OnImageOpened(object sender, RoutedEventArgs args)
        {
            var img = (Image)sender;
            img.ImageFailed -= Image_OnImageFailed;
            img.ImageOpened -= Image_OnImageOpened;
            Rebuild();
        }

        private void Rebuild()
        {
            var bmp = ImageSource as BitmapSource;
            if (bmp == null) return;

            var w = bmp.PixelWidth;
            var h = bmp.PixelHeight;

            if (w == 0 || h == 0) return;

            Children.Clear();
            for (int x = 0; x < ActualWidth; x += w)
            {
                for (int y = 0; y < ActualHeight; y += h)
                {
                    var img = new Image { Source = ImageSource };
                    Canvas.SetLeft(img, x);
                    Canvas.SetTop(img, y);
                    Children.Add(img);
                }
            }

            Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
        }
    }
}
