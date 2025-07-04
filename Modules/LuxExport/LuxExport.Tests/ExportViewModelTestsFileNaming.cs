//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using LuxExport.Logic;
//using System.IO;
//using System.Text.Json;
//using System.Collections.Generic;

//namespace LuxExport.Tests;

//[TestClass]
//public class ExportViewModelTestsFileNaming
//{
//    private ExportViewModel _viewModel;

//    [TestInitialize]
//    public void Setup()
//    {
//        _viewModel = new ExportViewModel();
//    }

//    [TestMethod]
//    public void GenerateFileName_WithDefaultFormat_ShouldReturnBaseName()
//    {
//        var metadata = new Dictionary<string, string> { { "ISO", "100" } };
//        var name = _viewModel.GenerateFileName("sunset.jpg", metadata, 1);
//        Assert.AreEqual("sunset", name);
//    }

//    [TestMethod]
//    public void GetExtensionFromFormat_JPEG_ShouldReturnLowercase()
//    {
//        _viewModel.SelectedFormat = ExportFormat.JPEG;
//        var ext = _viewModel.GetExtensionFromFormat();
//        Assert.AreEqual("jpeg", ext);
//    }

//    [TestMethod]
//    public void GetExtensionFromFormat_Original_ShouldReturnOriginal()
//    {
//        _viewModel.SelectedFormat = ExportFormat.Original;
//        var ext = _viewModel.GetExtensionFromFormat();
//        Assert.AreEqual("original", ext);
//    }

//    [TestMethod]
//    public void UpdateExportPath_WithSubfolder_ShouldUpdateFullPath()
//    {
//        _viewModel.SetBasePath("C:\\Exports");
//        _viewModel.CreateSubfolder = true;
//        _viewModel.SubfolderName = "MyFolder";

//        _viewModel.UpdateExportPath();

//        Assert.AreEqual("C:\\Exports\\MyFolder", _viewModel.ExportFilePath);
//    }

//    [TestMethod]
//    public void UpdateExportPath_WithoutSubfolder_ShouldUseBasePath()
//    {
//        _viewModel.SetBasePath("D:\\Photos");
//        _viewModel.CreateSubfolder = false;

//        _viewModel.UpdateExportPath();

//        Assert.AreEqual("D:\\Photos", _viewModel.ExportFilePath);
//    }
//}


//[TestClass]
//public class ExportViewModelQualityAndFormatTests
//{
//    private ExportViewModel _viewModel;

//    [TestInitialize]
//    public void Setup()
//    {
//        _viewModel = new ExportViewModel();
//    }

//    [TestMethod]
//    public void Quality_SetValue_ShouldUpdateProperty()
//    {
//        _viewModel.Quality = 85;
//        Assert.AreEqual(85, _viewModel.Quality);
//    }

//    [TestMethod]
//    public void LimitFileSize_Enable_ShouldSetToTrue()
//    {
//        _viewModel.LimitFileSize = true;
//        Assert.IsTrue(_viewModel.LimitFileSize);
//    }

//    [TestMethod]
//    public void MaxFileSizeKB_SetValue_ShouldUpdateProperty()
//    {
//        _viewModel.MaxFileSizeKB = 2048;
//        Assert.AreEqual(2048, _viewModel.MaxFileSizeKB);
//    }

//    [TestMethod]
//    public void SelectedFormat_SetToHEIF_ShouldSupportQuality()
//    {
//        _viewModel.SelectedFormat = ExportFormat.HEIF;
//        Assert.IsTrue(_viewModel.IsQualitySupported);
//    }

//    [TestMethod]
//    public void SelectedFormat_SetToPNG_ShouldDisableQuality()
//    {
//        _viewModel.SelectedFormat = ExportFormat.PNG;
//        Assert.IsFalse(_viewModel.IsQualitySupported);
//    }

//    [TestMethod]
//    public void SelectedColorSpace_SetValue_ShouldUpdate()
//    {
//        _viewModel.SelectedColorSpace = "AdobeRGB";
//        Assert.AreEqual("AdobeRGB", _viewModel.SelectedColorSpace);
//    }

//    [TestMethod]
//    public void SelectedFormat_Change_ShouldTriggerExtensionUpdate()
//    {
//        _viewModel.ExtensionCase = "a..z";
//        _viewModel.CustomFileFormat = "{name}";
//        _viewModel.SelectedFormat = ExportFormat.JPEG;

//        var ext = _viewModel.GetExtensionFromFormat();
//        Assert.AreEqual("jpeg", ext);
//    }
//}

//[TestClass]
//public class ExportViewModelNamingTests
//{
//    private ExportViewModel _viewModel;

//    [TestInitialize]
//    public void Setup()
//    {
//        _viewModel = new ExportViewModel();
//    }

//    [TestMethod]
//    public void RenameFile_SetToFalse_ShouldUpdateProperty()
//    {
//        _viewModel.RenameFile = false;
//        Assert.IsFalse(_viewModel.RenameFile);
//    }

//    [TestMethod]
//    public void FileNamingMode_Change_ShouldTriggerUpdate()
//    {
//        _viewModel.FileNamingMode = "Metadata";
//        Assert.AreEqual("Metadata", _viewModel.FileNamingMode);
//    }

//    [TestMethod]
//    public void CustomFileFormat_SetPattern_ShouldUpdate()
//    {
//        _viewModel.CustomFileFormat = "{date}-{name}";
//        Assert.AreEqual("{date}-{name}", _viewModel.CustomFileFormat);
//    }

//    [TestMethod]
//    public void ExtensionCase_SetUppercase_ShouldApplyToExampleFileName()
//    {
//        _viewModel.SelectedFormat = ExportFormat.PNG;
//        _viewModel.ExtensionCase = "A..Z";
//        _viewModel.CustomFileFormat = "{name}";
//        _viewModel.RenameFile = true;

//        _viewModel.UpdateExample();

//        StringAssert.EndsWith(_viewModel.ExampleFileName, ".PNG");
//    }

//    [TestMethod]
//    public void ExtensionCase_SetLowercase_ShouldApplyToExampleFileName()
//    {
//        _viewModel.SelectedFormat = ExportFormat.JPEG;
//        _viewModel.ExtensionCase = "a..z";
//        _viewModel.CustomFileFormat = "{name}";
//        _viewModel.RenameFile = true;

//        _viewModel.UpdateExample();

//        StringAssert.EndsWith(_viewModel.ExampleFileName, ".jpeg");
//    }

//    [TestMethod]
//    public void ExampleFileName_ShouldUpdateOnFormatChange()
//    {
//        _viewModel.ExtensionCase = "a..z";
//        _viewModel.SelectedFormat = ExportFormat.WEBP;
//        var example = _viewModel.ExampleFileName;

//        StringAssert.EndsWith(example, ".webp");
//    }
//}

//[TestClass]
//public class ExportViewModel_LoadPresetsTests
//{
//    private ExportViewModel _viewModel;

//    [TestInitialize]
//    public void Setup()
//    {
//        _viewModel = new ExportViewModel();
//    }

//    [TestMethod]
//    public void LoadPresets_WithValidFile_ShouldLoadPresets()
//    {
//        // Arrange
//        var presets = new List<FileNamingPreset>
//        {
//            new FileNamingPreset { Name = "Test1", Pattern = "{name}-{date}" },
//            new FileNamingPreset { Name = "Test2", Pattern = "{counter}" }
//        };

//        string tempPath = Path.GetTempFileName();
//        File.WriteAllText(tempPath, JsonSerializer.Serialize(presets));

//        // Act
//        _viewModel.LoadPresets(tempPath);

//        // Assert
//        Assert.AreEqual(2, _viewModel.Presets.Count);
//        Assert.AreEqual("Test1", _viewModel.Presets[0].Name);
//        Assert.AreEqual("{name}-{date}", _viewModel.Presets[0].Pattern);

//        // Clean up
//        File.Delete(tempPath);
//    }

//    [TestMethod]
//    public void LoadPresets_WithMissingFile_ShouldDoNothing()
//    {
//        // Act
//        _viewModel.LoadPresets("Z:\\ThisFileShouldNotExist.json");

//        // Assert
//        Assert.AreEqual(0, _viewModel.Presets.Count);
//    }
//}

//[TestClass]
//    public class ExportViewModel_ExportPathTests
//    {
//        private ExportViewModel _viewModel;

//        [TestInitialize]
//        public void Setup()
//        {
//            _viewModel = new ExportViewModel();
//        }

//        [TestMethod]
//        public void SelectedExportLocation_SetToDesktop_ShouldUpdateExportPath()
//        {
//            _viewModel.SelectedExportLocation = "Desktop";
//            string expected = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
//            Assert.AreEqual(expected, _viewModel.ExportFilePath);
//        }

//        [TestMethod]
//        public void SelectedExportLocation_SetToDocuments_ShouldUpdateExportPath()
//        {
//            _viewModel.SelectedExportLocation = "Documents";
//            string expected = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//            Assert.AreEqual(expected, _viewModel.ExportFilePath);
//        }

//        [TestMethod]
//        public void SelectedExportLocation_SetToPictures_ShouldUpdateExportPath()
//        {
//            _viewModel.SelectedExportLocation = "Pictures";
//            string expected = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
//            Assert.AreEqual(expected, _viewModel.ExportFilePath);
//        }

//        [TestMethod]
//        public void SelectedExportLocation_SetToUnknown_ShouldReturnPlaceholder()
//        {
//            _viewModel.SelectedExportLocation = "Same path as original file";
//            Assert.AreEqual("Unknown", _viewModel.ExportFilePath);
//        }

//        [TestMethod]
//        public void SelectedExportLocation_SetToCustom_ShouldReturnFallbackText()
//        {
//            _viewModel.SelectedExportLocation = "Banana Island";
//            Assert.AreEqual("Select a path...", _viewModel.ExportFilePath);
//        }

//        [TestMethod]
//        public void CreateSubfolder_WithValidName_ShouldUpdatePath()
//        {
//            _viewModel.SetBasePath("C:\\Temp");
//            _viewModel.CreateSubfolder = true;
//            _viewModel.SubfolderName = "Luxoria";
//            _viewModel.UpdateExportPath();

//            Assert.AreEqual("C:\\Temp\\Luxoria", _viewModel.ExportFilePath);
//        }

//        [TestMethod]
//        public void CreateSubfolder_Disabled_ShouldUseBasePathOnly()
//        {
//            _viewModel.SetBasePath("D:\\Exports");
//            _viewModel.CreateSubfolder = false;
//            _viewModel.UpdateExportPath();

//            Assert.AreEqual("D:\\Exports", _viewModel.ExportFilePath);
//        }
//    }