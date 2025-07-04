//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using LuxExport.Logic;
//using LuxExport.Interfaces;
//using System;

//namespace LuxExport.Tests
//{
//    [TestClass]
//    public class ExporterFactoryTests
//    {
//        [TestMethod]
//        public void CreateExporter_WithJpegFormat_ShouldReturnJpegExporter()
//        {
//            var exporter = ExporterFactory.CreateExporter(ExportFormat.JPEG);
//            Assert.IsInstanceOfType(exporter, typeof(JpegExporter));
//        }

//        [TestMethod]
//        public void CreateExporter_WithPngFormat_ShouldReturnPngExporter()
//        {
//            var exporter = ExporterFactory.CreateExporter(ExportFormat.PNG);
//            Assert.IsInstanceOfType(exporter, typeof(PngExporter));
//        }

//        [TestMethod]
//        public void CreateExporter_WithWebpFormat_ShouldReturnWebpExporter()
//        {
//            var exporter = ExporterFactory.CreateExporter(ExportFormat.WEBP);
//            Assert.IsInstanceOfType(exporter, typeof(WebpExporter));
//        }

//        [TestMethod]
//        public void CreateExporter_WithUnsupportedFormat_ShouldThrow()
//        {
//            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
//                ExporterFactory.CreateExporter(ExportFormat.GIF));
//        }
//    }
//}