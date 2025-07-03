//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using LuxExport.Interfaces;
//using LuxExport.Logic;
//using SkiaSharp;
//using System.IO;

//namespace LuxExport.Tests
//{
//    [TestClass]
//    public class ExportersTests
//    {
//        private SKBitmap _testBitmap;
//        private ExportSettings _settings;

//        [TestInitialize]
//        public void Setup()
//        {
//            _testBitmap = new SKBitmap(10, 10);
//            using var canvas = new SKCanvas(_testBitmap);
//            canvas.Clear(SKColors.Red);

//            _settings = new ExportSettings
//            {
//                Quality = 90,
//                ColorSpace = "sRGB"
//            };
//        }

//        [TestMethod]
//        public void JpegExporter_ShouldWriteValidFile()
//        {
//            string path = Path.GetTempFileName().Replace(".tmp", ".jpeg");
//            var exporter = new JpegExporter();

//            exporter.Export(_testBitmap, path, ExportFormat.JPEG, _settings);

//            Assert.IsTrue(File.Exists(path));
//            Assert.IsTrue(new FileInfo(path).Length > 0);

//            File.Delete(path);
//        }

//        [TestMethod]
//        public void PngExporter_ShouldWriteValidFile()
//        {
//            string path = Path.GetTempFileName().Replace(".tmp", ".png");
//            var exporter = new PngExporter();

//            exporter.Export(_testBitmap, path, ExportFormat.PNG, _settings);

//            Assert.IsTrue(File.Exists(path));
//            Assert.IsTrue(new FileInfo(path).Length > 0);

//            File.Delete(path);
//        }

//        [TestMethod]
//        public void WebpExporter_ShouldWriteValidFile()
//        {
//            string path = Path.GetTempFileName().Replace(".tmp", ".webp");
//            var exporter = new WebpExporter();

//            exporter.Export(_testBitmap, path, ExportFormat.WEBP, _settings);

//            Assert.IsTrue(File.Exists(path));
//            Assert.IsTrue(new FileInfo(path).Length > 0);

//            File.Delete(path);
//        }
//    }
//}