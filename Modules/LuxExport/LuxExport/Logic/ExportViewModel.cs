using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using LuxExport.Logic;
using SkiaSharp;

namespace LuxExport.Logic;

public class ExportViewModel : INotifyPropertyChanged
{
    private string _selectedExportLocation = "Desktop";
    private string _selectedFileConflictResolution = "Overwrite";
    private bool _createSubfolder;
    private string _subfolderName = "Luxoria";
    private string _baseExportPath = "";
    private string _exportFilePath = "";
    private string _filePath;
    private bool _renameFile = true;
    private string _fileNamingMode = "Filename";
    private string _customFileName = "";
    private string _extensionCase = "A..Z";
    private string _customFileFormat = "{name}";
    private string _exampleFileName = "Example.JPEG";
    public ObservableCollection<FileNamingPreset> Presets { get; } = new();
    private readonly TagResolverManager _tagManager = new();



    public ExportViewModel()
    {
        _baseExportPath = GetDefaultPath(_selectedExportLocation);
        UpdateExportPath();
    }

    public void LoadPresets(string path)
    {

        Debug.WriteLine("Loading presets!");

        if (!File.Exists(path)) return;


        Debug.WriteLine("Path of presets was found!");

        var json = File.ReadAllText(path);
        var list = System.Text.Json.JsonSerializer.Deserialize<List<FileNamingPreset>>(json);

        Presets.Clear();
        foreach (var preset in list)
            Presets.Add(preset);
    }

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
        private set
        {
            if (_exampleFileName != value)
            {
                _exampleFileName = value;
                OnPropertyChanged(nameof(ExampleFileName));
            }
        }
    }


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

    private void UpdateExample()
    {
        ExampleFileName = GenerateFileName("Exemple.JPEG", new Dictionary<string, string> {
            { "Camera Model", "Sony A7 II" },
            { "ISO", "100" }
        });
    }



    public string GenerateFileName(string originalName, IReadOnlyDictionary<string, string> metadata, int counter = 1)
    {
        string ext = ExtensionCase == "a..z" ? "jpeg" : "JPEG";
        string resolved = _tagManager.ResolveAll(CustomFileFormat, originalName, metadata, counter);
        return $"{resolved}.{ext}";
    }



    public void UpdateExportPath()
    {
        if (string.IsNullOrWhiteSpace(_baseExportPath))
        {
            ExportFilePath = string.Empty;
            return;
        }

        ExportFilePath = _baseExportPath;
    }

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

    public void SetBasePath(string path)
    {
        _baseExportPath = path;
        UpdateExportPath();
    }


    public void ExportImage(SKBitmap image)
    {
        if (image == null || string.IsNullOrWhiteSpace(FilePath))
        {
            Console.WriteLine("Invalid image or export path.");
            return;
        }

        using var imageStream = File.OpenWrite(FilePath);
        image.Encode(imageStream, SKEncodedImageFormat.Png, 100);
        Console.WriteLine($"Image exported to: {FilePath}");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
