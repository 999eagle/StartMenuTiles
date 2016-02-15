using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using System.Windows.Input;

namespace StartMenuTiles.Common
{
    enum CropType
    {
        UseScaleFactor,
        GetLargestRect
    }

    class CropHelper
    {
        public static async Task CropImageAsync(string sourceImageFile, string destinationImageFile, double scaleFactor, Rect clipRect, CropType cropType = CropType.UseScaleFactor)
        {
            byte[] sourcePixels;
            BitmapTransform transform;
            BitmapDecoder bmpDecoder;
            using (var srcStream = await (await StorageFile.GetFileFromPathAsync(sourceImageFile)).OpenAsync(FileAccessMode.Read))
            {
                bmpDecoder = await BitmapDecoder.CreateAsync(srcStream);
                if (cropType == CropType.GetLargestRect)
                {
                    // calculate scale factor to be minimal while still fitting the rect
                    double sx, sy;
                    sx = clipRect.Width / bmpDecoder.PixelWidth;
                    sy = clipRect.Height / bmpDecoder.PixelHeight;
                    scaleFactor = Math.Max(sx, sy);
                }
                transform = new BitmapTransform() // scale source bitmap to size of drawn image
                {
                    ScaledHeight = (uint)(bmpDecoder.PixelHeight * scaleFactor),
                    ScaledWidth = (uint)(bmpDecoder.PixelWidth * scaleFactor),
                    InterpolationMode = BitmapInterpolationMode.Fant
                };
                // decode data and get binary data
                var pixelData = await bmpDecoder.GetPixelDataAsync(BitmapPixelFormat.Rgba16, BitmapAlphaMode.Straight, transform, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);
                sourcePixels = pixelData.DetachPixelData();
            }
            using (var dstStream = await (await CreateOrOpenFileAsync(destinationImageFile)).OpenAsync(FileAccessMode.ReadWrite))
            {
                // create encoder to save data
                dstStream.Size = 0;
                var ext = destinationImageFile.Substring(destinationImageFile.LastIndexOf('.') + 1);
                Guid encId;
                switch (ext.ToLowerInvariant())
                {
                    case "jpg":
                    case "jpeg":
                        encId = BitmapEncoder.JpegEncoderId;
                        break;
                    case "gif":
                        encId = BitmapEncoder.GifEncoderId;
                        break;
                    case "png":
                        encId = BitmapEncoder.PngEncoderId;
                        break;
                    case "bmp":
                        encId = BitmapEncoder.BmpEncoderId;
                        break;
                    case "tif":
                    case "tiff":
                        encId = BitmapEncoder.TiffEncoderId;
                        break;
                }
                var bmpEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, dstStream);
                // set data
                bmpEncoder.SetPixelData(BitmapPixelFormat.Rgba16, BitmapAlphaMode.Straight, transform.ScaledWidth, transform.ScaledHeight, bmpDecoder.DpiX, bmpDecoder.DpiY, sourcePixels);
                // apply crop
                bmpEncoder.BitmapTransform.Bounds = new BitmapBounds()
                {
                    X = (uint)clipRect.X,
                    Y = (uint)clipRect.Y,
                    Width = (uint)clipRect.Width,
                    Height = (uint)clipRect.Height
                };
                bmpEncoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                // save
                await bmpEncoder.FlushAsync();
            }
        }

        static async Task<StorageFile> CreateOrOpenFileAsync(string path)
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(path.Substring(0, path.LastIndexOf('\\')));
            return await folder.CreateFileAsync(path.Substring(path.LastIndexOf('\\') + 1), CreationCollisionOption.OpenIfExists);
        }
    }
}
