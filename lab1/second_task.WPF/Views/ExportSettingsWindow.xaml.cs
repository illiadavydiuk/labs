using System.Windows;
using System.Windows.Controls;

namespace second_task.WPF.Views
{
    public partial class ExportSettingsWindow : Window
    {
        public bool IsConfirmed { get; private set; }

        public ExportSettingsWindow()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }

        public char GetDelimiterChar()
        {
            var selectedItem = DelimiterComboBox.SelectedItem as ComboBoxItem;
            var delimiter = selectedItem?.Tag?.ToString() ?? ",";
            
            return delimiter switch
            {
                "," => ',',
                ";" => ';',
                "\t" => '\t',
                "|" => '|',
                _ => ','
            };
        }

        public bool IncludeHeaders => IncludeHeadersCheckBox.IsChecked ?? true;
    }
}
