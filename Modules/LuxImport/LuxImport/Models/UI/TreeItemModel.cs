using Microsoft.UI.Xaml.Media.Imaging;

namespace LuxImport.Models.UI;

public class TreeItem
{
    public BitmapImage? BitmapImage { get; set; } = null;
    public string? DisplayText { get; set; } = null;
    public string? Path { get; set; } = null;
}
