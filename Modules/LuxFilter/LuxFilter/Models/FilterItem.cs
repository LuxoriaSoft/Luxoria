using System.ComponentModel;

namespace LuxFilter.Models;

public class FilterItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public string Name { get; }
    public string Description { get; }
    public string Version { get; }

    public FilterItem(string name, string description, string version)
    {
        Name = name;
        Description = description;
        Version = version;
    }

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
