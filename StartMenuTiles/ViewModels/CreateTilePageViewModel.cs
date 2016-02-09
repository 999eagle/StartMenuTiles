using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartMenuTiles.Mvvm;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace StartMenuTiles.ViewModels
{
    class CreateTilePageViewModel : ViewModelBase
    {
        string m_pageHeader = "Create a tile";
        public string PageHeader
        {
            get { return m_pageHeader; }
            set { Set(ref m_pageHeader, value); }
        }

        string m_tileTitle = "";
        public string TileTitle
        {
            get { return m_tileTitle; }
            set { Set(ref m_tileTitle, value); }
        }

        bool m_useDarkText;
        public bool UseDarkText
        {
            get { return m_useDarkText; }
            set
            {
                Set(ref m_useDarkText, value);
                if (m_useDarkText)
                    TileTextBrush = new SolidColorBrush(Colors.Black);
                else
                    TileTextBrush = new SolidColorBrush(Colors.White);
            }
        }

        SolidColorBrush m_tileTextBrush;
        public SolidColorBrush TileTextBrush
        {
            get { return m_tileTextBrush; }
            private set { Set(ref m_tileTextBrush, value); }
        }

        CreateTilePage_ImageViewModel m_logoImage;
        public CreateTilePage_ImageViewModel LogoImage
        {
            get { return m_logoImage; }
            set { Set(ref m_logoImage, value); }
        }

        CreateTilePage_ImageViewModel m_wideImage;
        public CreateTilePage_ImageViewModel WideImage
        {
            get { return m_wideImage; }
            set { Set(ref m_wideImage, value); }
        }

        CreateTilePage_ImageViewModel m_largeImage;
        public CreateTilePage_ImageViewModel LargeImage
        {
            get { return m_largeImage; }
            set { Set(ref m_largeImage, value); }
        }

        CreateTilePage_ImageViewModel m_smallImage;
        public CreateTilePage_ImageViewModel SmallImage
        {
            get { return m_smallImage; }
            set { Set(ref m_smallImage, value); }
        }

        public CreateTilePageViewModel()
        {
            m_logoImage = new CreateTilePage_ImageViewModel();
            m_logoImage.ParentPage = this;
            m_logoImage.Header = "Logo";
            m_logoImage.Desc = "The logo image should be 150x150";
            m_logoImage.ImageWidth = 150;
            m_logoImage.ImageHeight = 150;
            m_logoImage.UseDarkTextCheckVisibility = Visibility.Visible;

            m_wideImage = new CreateTilePage_ImageViewModel();
            m_wideImage.ParentPage = this;
            m_wideImage.Header = "Wide logo";
            m_wideImage.Desc = "The wide logo image should be 310x150";
            m_wideImage.ImageHeight = 150;
            m_wideImage.ImageWidth = 310;

            m_smallImage = new CreateTilePage_ImageViewModel();
            m_smallImage.ParentPage = this;
            m_smallImage.Header = "Small logo";
            m_smallImage.Desc = "The small logo image should be 70x70";
            m_smallImage.ImageHeight = 70;
            m_smallImage.ImageWidth = 70;

            m_largeImage = new CreateTilePage_ImageViewModel();
            m_largeImage.ParentPage = this;
            m_largeImage.Header = "Large logo";
            m_largeImage.Desc = "The large logo should be 310x310";
            m_largeImage.ImageHeight = 310;
            m_largeImage.ImageWidth = 310;

            UseDarkText = true;

            if (IsInDesignMode)
            {
                m_tileTitle = "Fallout 4";
            }
        }

        public override void OnNavigatedTo(string parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            JsonObject data = JsonObject.Parse(parameter);
            TileTitle = data.GetNamedString("tile_title");
            PageHeader = data.GetNamedString("header");
            if (data.ContainsKey("image_wide_url"))
            {
                WideImage.ImageSource = data.GetNamedString("image_wide_url");
            }
        }
    }

    class CreateTilePage_ImageViewModel : ViewModelBase
    {
        CreateTilePageViewModel m_parentPage;
        public CreateTilePageViewModel ParentPage
        {
            get { return m_parentPage; }
            set { Set(ref m_parentPage, value); }
        }

        string m_header;
        public string Header
        {
            get { return m_header; }
            set { Set(ref m_header, value); }
        }

        string m_desc;
        public string Desc
        {
            get { return m_desc; }
            set { Set(ref m_desc, value); }
        }

        Visibility m_useDarkTextCheckVisibility = Visibility.Collapsed;
        public Visibility UseDarkTextCheckVisibility
        {
            get { return m_useDarkTextCheckVisibility; }
            set { Set(ref m_useDarkTextCheckVisibility, value); }
        }

        int m_imageWidth;
        public int ImageWidth
        {
            get { return m_imageWidth; }
            set { Set(ref m_imageWidth, value); }
        }

        int m_imageHeight;
        public int ImageHeight
        {
            get { return m_imageHeight; }
            set { Set(ref m_imageHeight, value); }
        }

        bool m_showTitle;
        public bool ShowTitle
        {
            get { return m_showTitle; }
            set { Set(ref m_showTitle, value); }
        }

        string m_imageSource;
        public string ImageSource
        {
            get { return m_imageSource; }
            set { Set(ref m_imageSource, value); }
        }

        private Command m_openImageCommand;
        public Command OpenImageCommand { get { return m_openImageCommand ?? (m_openImageCommand = new Command(ExecuteOpenImage)); } }
        private async void ExecuteOpenImage()
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            var file = await picker.PickSingleFileAsync();
        }
    }
}
