using StartMenuTiles.Mvvm;
using StartMenuTiles.Services.NavigationService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

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

        string m_action = "";
        public string Action
        {
            get { return m_action; }
            set { Set(ref m_action, value); }
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

        Command m_createTileCommand;
        public Command CreateTileCommand { get { return m_createTileCommand ?? (m_createTileCommand = new Command(ExecuteCreateTile, CanCreateTile)); } }
        private async void ExecuteCreateTile()
        {
            var tile = new SecondaryTile();
            var ja = new JsonObject();
            ja.SetNamedValue("url", JsonValue.CreateStringValue(m_action));
            tile.Arguments = ja.Stringify();
            tile.DisplayName = TileTitle;
            tile.RoamingEnabled = true;
            tile.TileOptions = TileOptions.CopyOnDeployment;
            tile.TileId = "Run_" + m_id.Replace("\\", "_");

            tile.VisualElements.BackgroundColor = Colors.Black;
            tile.VisualElements.ForegroundText = (m_useDarkText ? ForegroundText.Dark : ForegroundText.Light);
            tile.VisualElements.ShowNameOnSquare150x150Logo = m_logoImage.ShowTitle;
            tile.VisualElements.ShowNameOnSquare310x310Logo = m_largeImage.ShowTitle;
            tile.VisualElements.ShowNameOnWide310x150Logo = m_wideImage.ShowTitle;
            
            var id = m_id.Substring(m_id.IndexOf('\\') + 1);
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(m_id.Substring(0, m_id.IndexOf('\\')), CreationCollisionOption.OpenIfExists);

            var uri = await GetLocalUriForTileImage(m_largeImage.ImageSource, folder, id + "_l");
            if (uri != null) tile.VisualElements.Square310x310Logo = uri;
            uri = await GetLocalUriForTileImage(m_logoImage.ImageSource, folder, id + "_m");
            if (uri != null) tile.VisualElements.Square150x150Logo = uri;
            uri = await GetLocalUriForTileImage(m_wideImage.ImageSource, folder, id + "_w");
            if (uri != null) tile.VisualElements.Wide310x150Logo = uri;
            uri = await GetLocalUriForTileImage(m_smallImage.ImageSource, folder, id + "_s");
            if (uri != null) tile.VisualElements.Square70x70Logo = uri;

            await tile.RequestCreateAsync();
        }
        private async Task<Uri> GetLocalUriForTileImage(ImageSource imageSource, StorageFolder folder, string id)
        {
            if (imageSource == null) return null;
            var src = await StorageFile.GetFileFromPathAsync((imageSource as BitmapImage).UriSource.AbsolutePath.Replace('/', '\\'));
            var ext = src.Name.Substring(src.Name.LastIndexOf('.'));
            src = await src.CopyAsync(folder, id + ext, NameCollisionOption.ReplaceExisting);
            return new Uri("ms-appdata:///local/" + folder.Name + "/" + id + ext);
        }
        private bool CanCreateTile()
        {
            return m_logoImage.ImageSource != null;
        }

        string m_id;

        public CreateTilePageViewModel()
        {
            m_logoImage = new CreateTilePage_ImageViewModel(CreateTilePage_ImageViewModelType.Medium, this);
            m_wideImage = new CreateTilePage_ImageViewModel(CreateTilePage_ImageViewModelType.Wide, this);
            m_smallImage = new CreateTilePage_ImageViewModel(CreateTilePage_ImageViewModelType.Small, this);
            m_largeImage = new CreateTilePage_ImageViewModel(CreateTilePage_ImageViewModelType.Large, this);
            m_logoImage.PropertyChanged += Image_PropertyChanged;
            m_wideImage.PropertyChanged += Image_PropertyChanged;

            UseDarkText = true;

            if (IsInDesignMode)
            {
                m_tileTitle = "Fallout 4";
            }
        }

        private void Image_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ImageSource")
                CreateTileCommand.RaiseCanExecuteChanged();
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            JsonObject data = new JsonObject();
            data.Add("logo", GetImageData(m_logoImage));
            data.Add("small", GetImageData(m_smallImage));
            data.Add("wide", GetImageData(m_wideImage));
            data.Add("large", GetImageData(m_largeImage));
            data.Add("use_dark", JsonValue.CreateBooleanValue(m_useDarkText));
            var key = TempDataStore.GetInstance().StoreObject(data);
            state["key"] = key;
            return Task.FromResult<object>(null);
        }

        private JsonObject GetImageData(CreateTilePage_ImageViewModel m)
        {
            JsonObject data = new JsonObject();
            if (m.ImageSource != null)
            {
                data.Add("src", JsonValue.CreateStringValue((m.ImageSource as BitmapImage).UriSource.AbsolutePath));
            }
            data.Add("title", JsonValue.CreateBooleanValue(m.ShowTitle));
            return data;
        }

        private async void SetImageData(JsonObject data, CreateTilePage_ImageViewModel m)
        {
            if (data.ContainsKey("src"))
            {
                await m.SetImageSource(data.GetNamedString("src"));
                //m.RaisePropertyChanged("ImageSource");
            }
            m.ShowTitle = data.GetNamedBoolean("title");
        }

        public override async void OnNavigatedTo(string parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            JsonObject data = JsonObject.Parse(parameter);
            TileTitle = data.GetNamedString("tile_title");
            PageHeader = data.GetNamedString("header");
            Action = data.GetNamedString("action");
            m_id = data.GetNamedString("id");
            if (mode == NavigationMode.Back)
            {
                int key = (int)state["key"];
                var d = (JsonObject)TempDataStore.GetInstance().GetObject(key);
                SetImageData(d.GetNamedObject("logo"), m_logoImage);
                SetImageData(d.GetNamedObject("small"), m_smallImage);
                SetImageData(d.GetNamedObject("wide"), m_wideImage);
                SetImageData(d.GetNamedObject("large"), m_largeImage);
                UseDarkText = d.GetNamedBoolean("use_dark");
            }
            else
            {
                await WideImage.SetImageSource("");
                await SmallImage.SetImageSource("");
                await LogoImage.SetImageSource("");
                await LargeImage.SetImageSource("");
                string uri = "";
                if (data.ContainsKey("image_wide_url"))
                {
                    uri = data.GetNamedString("image_wide_url");
                    await WideImage.SetImageSource(uri);
                }
                if (uri != "")
                {
                    if (WideImage.ImageSource == null)
                        await WideImage.SetImageSource(uri);
                    if (LogoImage.ImageSource == null)
                        await LogoImage.SetImageSource(uri);
                    //if (String.IsNullOrEmpty(SmallImage.ImageSource))
                    //    await SmallImage.SetImageSource(uri);
                    //if (String.IsNullOrEmpty(LargeImage.ImageSource))
                    //    await LargeImage.SetImageSource(uri);
                }
            }
        }
    }

    enum CreateTilePage_ImageViewModelType
    {
        Small,
        Medium,
        Wide,
        Large
    }

    class CreateTilePage_ImageViewModel : ViewModelBase
    {
        #region Properties for View
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

        ImageSource m_imageSource;
        public ImageSource ImageSource
        {
            get { return m_imageSource; }
            private set
            {
                Set(ref m_imageSource, value);
                OpenImageCommand.RaiseCanExecuteChanged();
                RemoveImageCommand.RaiseCanExecuteChanged();
                CropImageCommand.RaiseCanExecuteChanged();
            }
        }

        private Command m_openImageCommand;
        public Command OpenImageCommand { get { return m_openImageCommand ?? (m_openImageCommand = new Command(ExecuteOpenImage, CanOpenImage)); } }

        private Command m_removeImageCommand;
        public Command RemoveImageCommand { get { return m_removeImageCommand ?? (m_removeImageCommand = new Command(ExecuteRemoveImage, CanRemoveImage)); } }

        private Command m_cropImageCommand;
        public Command CropImageCommand { get { return m_cropImageCommand ?? (m_cropImageCommand = new Command(ExecuteCropImage, CanCropImage)); } }
        #endregion

        #region Command Handlers
        private async void ExecuteOpenImage()
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                await SetImageSource(file);
            }
        }
        private bool CanOpenImage()
        {
            return m_imageSource == null;
        }

        private void ExecuteRemoveImage()
        {
            ImageSource = null;
        }
        private bool CanRemoveImage()
        {
            return m_imageSource != null;
        }

        private void ExecuteCropImage()
        {
            JsonObject ja = new JsonObject();
            ja.Add("original", JsonValue.CreateStringValue(m_filename + "a" + m_ext));
            ja.Add("crop", JsonValue.CreateStringValue(m_filename + m_ext));
            JsonArray s = new JsonArray();
            s.Add(JsonValue.CreateNumberValue(ImageWidth));
            s.Add(JsonValue.CreateNumberValue(ImageHeight));
            ja.Add("size", s);
            NavigationService.Navigate(typeof(Views.CropImagePage), ja.Stringify());
        }
        private bool CanCropImage()
        {
            return m_imageSource != null;
        }
        #endregion

        CreateTilePage_ImageViewModelType m_type;
        string m_filename, m_ext;

        public CreateTilePage_ImageViewModel() : this(CreateTilePage_ImageViewModelType.Medium, null) { }
        public CreateTilePage_ImageViewModel(CreateTilePage_ImageViewModelType type, CreateTilePageViewModel parentPage)
        {
            m_type = type;
            m_parentPage = parentPage;
            SetDefaults();
        }

        private void SetDefaults()
        {
            switch (m_type)
            {
                case CreateTilePage_ImageViewModelType.Large:
                    Header = "Large logo";
                    Desc = "The large logo should be 310x310";
                    ImageHeight = 310;
                    ImageWidth = 310;
                    m_filename = "l";
                    break;
                case CreateTilePage_ImageViewModelType.Medium:
                    Header = "Logo";
                    Desc = "The logo image should be 150x150";
                    ImageWidth = 150;
                    ImageHeight = 150;
                    UseDarkTextCheckVisibility = Visibility.Visible;
                    m_filename = "m";
                    break;
                case CreateTilePage_ImageViewModelType.Small:
                    Header = "Small logo";
                    Desc = "The small logo image should be 70x70";
                    ImageHeight = 70;
                    ImageWidth = 70;
                    m_filename = "s";
                    break;
                case CreateTilePage_ImageViewModelType.Wide:
                    Header = "Wide logo";
                    Desc = "The wide logo image should be 310x150";
                    ImageHeight = 150;
                    ImageWidth = 310;
                    m_filename = "w";
                    break;
            }
        }

        ImageSource CreateSource(string source)
        {
            var src = new BitmapImage(new Uri(source));
            src.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            return src;
        }

        public async Task SetImageSource(string sourceUri)
        {
            if (String.IsNullOrEmpty(sourceUri))
            {
                ImageSource = null;
                return;
            }
            sourceUri = sourceUri.Replace('\\', '/');
            if (sourceUri.StartsWith(ApplicationData.Current.TemporaryFolder.Path.Replace('\\', '/')))
            {
                ImageSource = CreateSource(sourceUri);
                m_ext = sourceUri.Substring(sourceUri.LastIndexOf('.'));
            }
            else
                await SetImageSource(await StorageFile.CreateStreamedFileFromUriAsync(sourceUri.Substring(sourceUri.LastIndexOfAny(new[] { '\\', '/' })), new Uri(sourceUri), null));
        }

        public async Task SetImageSource(StorageFile sourceFile)
        {
            var tempFolder = ApplicationData.Current.TemporaryFolder;
            m_ext = sourceFile.Name.Substring(sourceFile.Name.LastIndexOf('.'));
            // original full size image
            //var img = await sourceFile.CopyAsync(tempFolder, m_filename + "a" + m_ext, NameCollisionOption.ReplaceExisting);
            // cropped image will be stored here
            //img = await sourceFile.CopyAsync(tempFolder, m_filename + m_ext, NameCollisionOption.ReplaceExisting);
            //ImageSource = img.Path;
            var f = tempFolder.Path + "\\";
            var i1 = m_filename + "a" + m_ext;
            var i2 = m_filename + m_ext;
            
            // copy new source to temp folder
            await sourceFile.CopyAsync(tempFolder, i1, NameCollisionOption.ReplaceExisting);
            // crop image to target size
            await Common.CropHelper.CropImageAsync(f + i1, f + i2, 1, new Rect(0, 0, ImageWidth, ImageHeight), Common.CropType.GetLargestRect);
            ImageSource = CreateSource(f + i2);
        }
    }
}
