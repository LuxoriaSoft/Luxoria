using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace LuxEditor.Components
{

    public class KeyValueStringPair
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public sealed partial class Infos : Page
    {
        public ObservableCollection<KeyValueStringPair> ExifData { get; } = new();

        public Infos()
        {
            this.InitializeComponent();
            ExifListView.ItemsSource = ExifData;
        }

        /// <summary>
        /// Sets the EXIF data into the ListView for viewing.
        /// </summary>
        public void DisplayExifData(
            System.Collections.ObjectModel.ReadOnlyDictionary<string, string> metadata)
        {
            ExifData.Clear();

            foreach (var entry in metadata)
            {
                if (entry.Key != null && !entry.Key.ToLower().StartsWith("unknown"))
                {
                    ExifData.Add(new KeyValueStringPair
                    {
                        Key = entry.Key,
                        Value = entry.Value
                    });
                }
            }
        }
    }
}
