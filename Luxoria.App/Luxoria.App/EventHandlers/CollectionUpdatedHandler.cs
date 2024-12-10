using Luxoria.App.EventHandlers;
using Luxoria.Modules.Models;
using Luxoria.Modules.Models.Events;
using Luxoria.Modules.Utils;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace Luxoria.App.EventHandlers
{
    public class CollectionUpdatedHandler
    {
        private readonly ILoggerService _loggerService;

        public CollectionUpdatedHandler(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        // Handles the CollectionUpdatedEvent
        public async Task OnCollectionUpdated(CollectionUpdatedEvent body, Image imageControl)
        {
            _loggerService.Log($"Collection updated: {body.CollectionName}");
            _loggerService.Log($"Collection path: {body.CollectionPath}");

            // Iterate over the assets and process each image
            for (int i = 0; i < body.Assets.Count; i++)
            {
                var asset = body.Assets.ElementAt(i);
                _loggerService.Log($"Asset {i}: {asset.MetaData.Id}");

                // Get the pixel data from the asset
                ReadOnlyMemory<byte> bitmap = asset.Data.PixelData;

                // Show basic info about the bitmap for debugging
                Debug.WriteLine($"Bitmap data length: {bitmap.Length} bytes");
                Debug.WriteLine($"Bitmap width: {asset.Data.Width}");
                Debug.WriteLine($"Bitmap height: {asset.Data.Height}");

                try
                {
                    // Convert the byte array to SoftwareBitmap
                    var softwareBitmap = ConvertToSoftwareBitmap(bitmap, asset.Data.Width, asset.Data.Height);

                    // Convert SoftwareBitmap to BitmapImage (ImageSource compatible)
                    BitmapImage bitmapImage = await ConvertToBitmapImage(softwareBitmap);

                    // Set the BitmapImage as the source of the Image control
                    imageControl.Source = bitmapImage;
                }
                catch (Exception ex)
                {
                    // Log the exception details for debugging
                    _loggerService.Log($"Error processing asset {asset.MetaData.Id}: {ex.Message}");
                    Debug.WriteLine($"Error processing asset {asset.MetaData.Id}: {ex.StackTrace}");
                }
            }
        }

        // Convert byte array (ReadOnlyMemory<byte>) to SoftwareBitmap
        private SoftwareBitmap ConvertToSoftwareBitmap(ReadOnlyMemory<byte> bitmap, int width, int height)
        {
            try
            {
                // Log first few bytes of the bitmap to verify if it contains valid image data (e.g., PNG or JPEG header)
                string bytePreview = BitConverter.ToString(bitmap.Span.Slice(0, 10).ToArray());
                Debug.WriteLine($"First 10 bytes of image data: {bytePreview}");

                // Create a stream to hold the pixel data
                var stream = new InMemoryRandomAccessStream();
                var writer = new DataWriter(stream.GetOutputStreamAt(0));

                // Write the byte data to the stream
                writer.WriteBytes(bitmap.Span.ToArray());
                writer.StoreAsync().AsTask().Wait();

                // Seek back to the beginning of the stream
                stream.Seek(0);

                // Create a BitmapDecoder to decode the byte data
                var decoder = BitmapDecoder.CreateAsync(stream).AsTask().Result;

                // Retrieve the SoftwareBitmap from the decoder
                return decoder.GetSoftwareBitmapAsync().AsTask().Result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ConvertToSoftwareBitmap: {ex.Message}");
                throw new InvalidOperationException("Failed to convert byte array to SoftwareBitmap.", ex);
            }
        }


        // Convert SoftwareBitmap to BitmapImage (which is an ImageSource)
        private async Task<BitmapImage> ConvertToBitmapImage(SoftwareBitmap softwareBitmap)
        {
            try
            {
                var bitmapImage = new BitmapImage();
                var stream = new InMemoryRandomAccessStream();
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                // Encode the SoftwareBitmap into the stream
                encoder.SetSoftwareBitmap(softwareBitmap);
                await encoder.FlushAsync();

                // Set the stream as the source for the BitmapImage
                stream.Seek(0);
                await bitmapImage.SetSourceAsync(stream);

                return bitmapImage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ConvertToBitmapImage: {ex.Message}");
                throw new InvalidOperationException("Failed to convert SoftwareBitmap to BitmapImage.", ex);
            }
        }
    }
}
