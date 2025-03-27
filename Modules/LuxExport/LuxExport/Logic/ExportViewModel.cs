using System;
using System.ComponentModel;
using System.IO;
using SkiaSharp;

public class ExportViewModel : INotifyPropertyChanged
{
    private string _selectedExportLocation = "Desktop";
    private string _selectedFileConflictResolution = "Overwrite";
    private bool _createSubfolder;
    private string _subfolderName = "Luxoria";
    private string _baseExportPath = "";
    private string _exportFilePath = "";
    private string _filePath;

    public ExportViewModel()
    {
        _baseExportPath = GetDefaultPath(_selectedExportLocation);
        UpdateExportPath();
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
