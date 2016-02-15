using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartMenuTiles.Mvvm;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Foundation;

namespace StartMenuTiles.ViewModels
{
    class CropImagePageViewModel : ViewModelBase
    {
        string m_imageSource;
        public string ImageSource
        {
            get { return m_imageSource; }
            set { Set(ref m_imageSource, value); }
        }

        Rect m_clipRect;
        public Rect ClipRect
        {
            get { return m_clipRect; }
            set { Set(ref m_clipRect, value); }
        }

        string m_imageDestination;
        public string ImageDestination
        {
            get { return m_imageDestination; }
            set { Set(ref m_imageDestination, value); }
        }

        Command m_imageCroppedCommand;
        public Command ImageCroppedCommand { get { return m_imageCroppedCommand ?? (m_imageCroppedCommand = new Command(ExecuteImageCropped)); } }

        public CropImagePageViewModel()
        {
            if (IsInDesignMode)
            {
                ImageSource = "http://cdn.akamai.steamstatic.com/steam/apps/377160/header.jpg";
                ClipRect = new Rect(0, 0, 310, 150);
            }
        }

        public override void OnNavigatedTo(string parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            JsonObject p = JsonObject.Parse(parameter);
            string folder = ApplicationData.Current.TemporaryFolder.Path;
            ImageSource = folder + "\\" + p.GetNamedString("original");
            ImageDestination = folder + "\\" + p.GetNamedString("crop");
            JsonArray s = p.GetNamedArray("size");
            ClipRect = new Rect(0, 0, s[0].GetNumber(), s[1].GetNumber());
        }

        void ExecuteImageCropped()
        {
            NavigationService.GoBack();
        }
    }
}
