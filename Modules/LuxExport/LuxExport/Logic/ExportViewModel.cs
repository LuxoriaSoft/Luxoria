using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using LuxExport.Logic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using SkiaSharp;
using Windows.Storage.Streams;

public class ExportViewModel : INotifyPropertyChanged
{
    private readonly TagResolverManager _tagManager = new();

    // Export Settings
    private ExportFormat _selectedFormat = ExportFormat.JPEG;
    private int _quality = 100;
    private string _selectedColorSpace = "sRGB";
    private bool _limitFileSize = false;
    private int _maxFileSizeKB = 0;
    public string SelectedFormatText => SelectedFormat.ToString(); // Exposes the format name as a string

    // File Naming
    private bool _renameFile = true;
    private string _fileNamingMode = "Filename";
    private string _customFileFormat = "{name}";
    private string _extensionCase = "A..Z";
    private string _exampleFileName = "Example.JPEG";
    private int counter = 0;

    // Presets
    public ObservableCollection<FileNamingPreset> Presets { get; } = new();

    // Path
    private string _selectedExportLocation = "Desktop";
    private string _selectedFileConflictResolution = "Overwrite";
    private bool _createSubfolder;
    private string _subfolderName = "Luxoria";
    private string _baseExportPath = "";
    private string _exportFilePath = "";
    private string _filePath;

    // Watermark
    private WatermarkSettings _watermark = new();
    public WatermarkSettings Watermark
    {
        get => _watermark;
        set
        {
            _watermark = value;
            OnPropertyChanged(nameof(Watermark));
            OnPropertyChanged(nameof(IsText));
            OnPropertyChanged(nameof(IsLogo));
            OnPropertyChanged(nameof(TextVisibility));
            OnPropertyChanged(nameof(LogoVisibility));
            RefreshLogoPreview();
        }
    }

    public bool WatermarkEnabled
    {
        get => Watermark.Enabled;
        set { Watermark.Enabled = value; OnPropertyChanged(nameof(WatermarkEnabled)); }
    }

    public string WatermarkText
    {
        get => Watermark.Text;
        set { Watermark.Text = value; OnPropertyChanged(nameof(WatermarkText)); }
    }
    public int WatermarkTypeToInt
    {
        get => (int)Watermark.Type;
        set
        {
            if ((int)Watermark.Type == value) return;
            Watermark.Type = (WatermarkType)value;

            OnPropertyChanged(nameof(IsText));
            OnPropertyChanged(nameof(IsLogo));
            OnPropertyChanged(nameof(TextVisibility));
            OnPropertyChanged(nameof(LogoVisibility));
        }
    }
    private ImageSource? _logoPreview;
    public ImageSource? LogoPreview
    {
        get => _logoPreview;
        private set { _logoPreview = value; OnPropertyChanged(nameof(LogoPreview)); }
    }


    public bool IsText => Watermark.Type == WatermarkType.Text;
    public bool IsLogo => Watermark.Type == WatermarkType.Logo;

    public Visibility TextVisibility => IsText ? Visibility.Visible : Visibility.Collapsed;
    public Visibility LogoVisibility => IsLogo ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// Initializes the ExportViewModel with default export path.
    /// </summary>
    public ExportViewModel()
    {
        _baseExportPath = GetDefaultPath(_selectedExportLocation);
        UpdateExportPath();
    }

    private static WriteableBitmap ToWriteable(SKBitmap bmp)
    {
        var wb = new WriteableBitmap(bmp.Width, bmp.Height);

        using var src = bmp.PeekPixels();
        using var dst = wb.PixelBuffer.AsStream();
        dst.Write(src.GetPixelSpan());

        wb.Invalidate();
        return wb;
    }


    public void RefreshLogoPreview()
    {
        LogoPreview = Watermark.Logo is null ? null : ToWriteable(Watermark.Logo);
    }

    // Format & Export Settings
    public ExportFormat SelectedFormat
    {
        get => _selectedFormat;
        set
        {
            if (_selectedFormat != value)
            {
                _selectedFormat = value;
                OnPropertyChanged(nameof(SelectedFormat));
                OnPropertyChanged(nameof(SelectedFormatText));
                OnPropertyChanged(nameof(IsQualitySupported));

                if (!IsQualitySupported)
                {
                    Quality = 100;
                }

                UpdateExample();
            }
        }
    }

    public int Quality
    {
        get => _quality;
        set
        {
            if (_quality != value)
            {
                _quality = value;
                OnPropertyChanged(nameof(Quality));
            }
        }
    }

    public string SelectedColorSpace
    {
        get => _selectedColorSpace;
        set
        {
            if (_selectedColorSpace != value)
            {
                _selectedColorSpace = value;
                OnPropertyChanged(nameof(SelectedColorSpace));
            }
        }
    }

    public bool LimitFileSize
    {
        get => _limitFileSize;
        set
        {
            if (_limitFileSize != value)
            {
                _limitFileSize = value;
                OnPropertyChanged(nameof(LimitFileSize));
            }
        }
    }

    public int MaxFileSizeKB
    {
        get => _maxFileSizeKB;
        set
        {
            if (_maxFileSizeKB != value)
            {
                _maxFileSizeKB = value;
                OnPropertyChanged(nameof(MaxFileSizeKB));
            }
        }
    }

    public bool IsQualitySupported
    {
        get
        {
            return SelectedFormat == ExportFormat.JPEG
                || SelectedFormat == ExportFormat.HEIF
                || SelectedFormat == ExportFormat.AVIF
                || SelectedFormat == ExportFormat.JPEGXL;
        }
    }

    // File Naming
    public bool RenameFile
    {
        get => _renameFile;
        set
        {
            if (_renameFile != value)
            {
                _renameFile = value;
                OnPropertyChanged(nameof(RenameFile));
                UpdateExample();
            }
        }
    }

    public string FileNamingMode
    {
        get => _fileNamingMode;
        set
        {
            if (_fileNamingMode != value)
            {
                _fileNamingMode = value;
                OnPropertyChanged(nameof(FileNamingMode));
                UpdateExample();
            }
        }
    }

    public string CustomFileFormat
    {
        get => _customFileFormat;
        set
        {
            if (_customFileFormat != value)
            {
                _customFileFormat = value;
                OnPropertyChanged(nameof(CustomFileFormat));
                UpdateExample();
            }
        }
    }

    public string ExtensionCase
    {
        get => _extensionCase;
        set
        {
            if (_extensionCase != value)
            {
                _extensionCase = value;
                OnPropertyChanged(nameof(ExtensionCase));
                UpdateExample();
            }
        }
    }

    public string ExampleFileName
    {
        get => _exampleFileName;
        set
        {
            if (_exampleFileName != value)
            {
                _exampleFileName = value;
                OnPropertyChanged(nameof(ExampleFileName));
            }
        }
    }

    /// <summary>
    /// Updates the example file name based on the current settings.
    /// </summary>
    public void UpdateExample()
    {
        var fakeMetadata = new Dictionary<string, string> {
            { "Make", "Sony" },
            { "Model", "ILCE-7M2" },
            { "Focal Length", "85 mm" },
            { "ISO Speed Ratings", "100" },
            { "F-Number", "f/1.8" },
            { "Exposure time", "1/320 sec" }
        };

        string ext = ExtensionCase == "a..z"
            ? GetExtensionFromFormat().ToLowerInvariant()
            : GetExtensionFromFormat().ToUpperInvariant();

        string baseName = GenerateFileName("Example.jpeg", fakeMetadata, counter++);
        ExampleFileName = $"{baseName}.{ext}";
    }

    /// <summary>
    /// Generates a custom file name using the provided format and metadata.
    /// </summary>
    public string GenerateFileName(string originalName, IReadOnlyDictionary<string, string> metadata, int counter)
    {
        return _tagManager.ResolveAll(CustomFileFormat, originalName, metadata, counter);
    }

    /// <summary>
    /// Gets the file extension based on the selected export format.
    /// </summary>
    public string GetExtensionFromFormat()
    {
        return SelectedFormat == ExportFormat.Original
            ? "original"
            : SelectedFormat.ToString().ToLowerInvariant();
    }

    // Export Path

    public string SelectedExportLocation
    {
        get => _selectedExportLocation;
        set
        {
            if (_selectedExportLocation != value)
            {
                _selectedExportLocation = value;
                _baseExportPath = GetDefaultPath(value);
                OnPropertyChanged(nameof(SelectedExportLocation));
                UpdateExportPath();
            }
        }
    }

    public string ExportFilePath
    {
        get => _exportFilePath;
        set
        {
            if (_exportFilePath != value)
            {
                _exportFilePath = value;
                OnPropertyChanged(nameof(ExportFilePath));
            }
        }
    }

    public string SelectedFileConflictResolution
    {
        get => _selectedFileConflictResolution;
        set
        {
            if (_selectedFileConflictResolution != value)
            {
                _selectedFileConflictResolution = value;
                OnPropertyChanged(nameof(SelectedFileConflictResolution));
            }
        }
    }

    public bool CreateSubfolder
    {
        get => _createSubfolder;
        set
        {
            if (_createSubfolder != value)
            {
                _createSubfolder = value;
                OnPropertyChanged(nameof(CreateSubfolder));
                UpdateExportPath();
            }
        }
    }

    public string SubfolderName
    {
        get => _subfolderName;
        set
        {
            if (_subfolderName != value)
            {
                _subfolderName = value;
                OnPropertyChanged(nameof(SubfolderName));
                UpdateExportPath();
            }
        }
    }

    public string FilePath
    {
        get => _filePath;
        set
        {
            if (_filePath != value)
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }
    }

    /// <summary>
    /// Updates the export path based on the current folder and subfolder settings.
    /// </summary>
    public void UpdateExportPath()
    {
        ExportFilePath = _createSubfolder && !string.IsNullOrWhiteSpace(_subfolderName)
            ? Path.Combine(_baseExportPath, _subfolderName)
            : _baseExportPath;
    }

    /// <summary>
    /// Returns the default path based on the selected export location.
    /// </summary>
    private string GetDefaultPath(string location)
    {
        return location switch
        {
            "Desktop" => Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "Documents" => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Pictures" => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "Same path as original file" => "Unknown",
            _ => "Select a path..."
        };
    }

    /// <summary>
    /// Sets the base export path manually.
    /// </summary>
    public void SetBasePath(string path)
    {
        _baseExportPath = path;
        UpdateExportPath();
    }

    /// <summary>
    /// Loads file naming presets from a JSON file.
    /// </summary>
    public void LoadPresets(string path)
    {
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        var list = JsonSerializer.Deserialize<List<FileNamingPreset>>(json);
        Presets.Clear();
        foreach (var preset in list)
            Presets.Add(preset);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
