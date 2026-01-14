using CommunityToolkit.Mvvm.ComponentModel;
using System.Text;

namespace second_task.WPF.ViewModels
{
    public partial class ExportSettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _selectedEncoding = "UTF-8";

        [ObservableProperty]
        private string _selectedDelimiter = ",";

        [ObservableProperty]
        private string _sheetName = "Data";

        [ObservableProperty]
        private bool _includeHeaders = true;

        [ObservableProperty]
        private string _dateFormat = "yyyy-MM-dd";

        [ObservableProperty]
        private string _numberFormat = "0.##";

        public List<string> EncodingOptions { get; } = new()
        {
            "UTF-8",
            "UTF-16",
            "ASCII",
            "Windows-1251"
        };

        public List<string> DelimiterOptions { get; } = new()
        {
            ",",
            ";", 
            "\t",
            "|"
        };

        public List<string> DelimiterDisplayNames { get; } = new()
        {
            "Comma (,)",
            "Semicolon (;)",
            "Tab (\\t)",
            "Pipe (|)"
        };

        public Encoding GetEncoding()
        {
            return SelectedEncoding switch
            {
                "UTF-8" => Encoding.UTF8,
                "UTF-16" => Encoding.Unicode,
                "ASCII" => Encoding.ASCII,
                "Windows-1251" => Encoding.GetEncoding("windows-1251"),
                _ => Encoding.UTF8
            };
        }

        public char GetDelimiterChar()
        {
            return SelectedDelimiter switch
            {
                "," => ',',
                ";" => ';',
                "\t" => '\t',
                "|" => '|',
                _ => ','
            };
        }
    }
}
