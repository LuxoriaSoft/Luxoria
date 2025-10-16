namespace LuxImport.Logic.Services;

public class SourceService
{
    public static readonly string[] SpecialFolders =
    {
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
    };
}
