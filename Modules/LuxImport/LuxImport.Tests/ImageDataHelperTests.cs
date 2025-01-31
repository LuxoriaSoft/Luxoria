using LuxImport.Utils;
using Luxoria.Modules.Models;
using SkiaSharp;

namespace LuxImport.Tests
{
    /// <summary>
    /// Unit tests for the ImageDataHelper class.
    /// </summary>
    public class ImageDataHelperTests : IDisposable
    {
        private readonly string _testImagePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageDataHelperTests"/> class.
        /// Sets up a temporary test image for use in tests.
        /// </summary>
        public ImageDataHelperTests()
        {
            _testImagePath = Path.Combine(Path.GetTempPath(), "test_image.png");
            GenerateTestImage(_testImagePath);
        }

        /// <summary>
        /// Tests that LoadFromPath throws an ArgumentException when given an empty path.
        /// </summary>
        [Fact]
        public void LoadFromPath_ShouldThrowArgumentException_ForEmptyPath()
        {
            Assert.Throws<ArgumentException>(() => ImageDataHelper.LoadFromPath(""));
        }

        /// <summary>
        /// Tests that LoadFromPath throws a FileNotFoundException when given a nonexistent file path.
        /// </summary>
        [Fact]
        public void LoadFromPath_ShouldThrowFileNotFoundException_ForInvalidPath()
        {
            string invalidPath = Path.Combine(Path.GetTempPath(), "non_existent_image.png");
            Assert.Throws<FileNotFoundException>(() => ImageDataHelper.LoadFromPath(invalidPath));
        }

        /// <summary>
        /// Tests that LoadFromPath throws a NotSupportedException when given an unsupported file format.
        /// </summary>
        [Fact]
        public void LoadFromPath_ShouldThrowNotSupportedException_ForUnsupportedFormat()
        {
            string unsupportedFile = Path.Combine(Path.GetTempPath(), "unsupported.txt");
            File.WriteAllText(unsupportedFile, "This is not an image.");
            Assert.Throws<NotSupportedException>(() => ImageDataHelper.LoadFromPath(unsupportedFile));
            File.Delete(unsupportedFile);
        }

        /// <summary>
        /// Tests that LoadFromPath throws an InvalidOperationException when given an empty file.
        /// </summary>
        [Fact]
        public void LoadFromPath_ShouldThrowInvalidOperationException_ForEmptyFile()
        {
            string emptyFile = Path.Combine(Path.GetTempPath(), "empty.png");
            File.WriteAllBytes(emptyFile, new byte[0]);
            Assert.Throws<InvalidOperationException>(() => ImageDataHelper.LoadFromPath(emptyFile));
            File.Delete(emptyFile);
        }

        /// <summary>
        /// Tests that LoadFromPath successfully returns valid image data for a properly formatted image file.
        /// </summary>
        [Fact]
        public void LoadFromPath_ShouldReturnValidImageData_ForValidImage()
        {
            var imageData = ImageDataHelper.LoadFromPath(_testImagePath);
            Assert.NotNull(imageData);
            Assert.IsType<ImageData>(imageData);
            Assert.NotNull(imageData.Bitmap);
        }

        /// <summary>
        /// Generates a test image file for use in test cases.
        /// </summary>
        /// <param name="path">The path where the test image should be saved.</param>
        private void GenerateTestImage(string path)
        {
            using (var bitmap = new SKBitmap(100, 100))
            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite(path))
            {
                data.SaveTo(stream);
            }
        }

        /// <summary>
        /// Cleans up test resources by deleting the test image file.
        /// </summary>
        public void Dispose()
        {
            if (File.Exists(_testImagePath))
            {
                File.Delete(_testImagePath);
            }
        }
    }
}
