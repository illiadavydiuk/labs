using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace first_task_file_encryptor.Domain.Models
{
    public partial class FileItem : ObservableObject
    {
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public long FileSize { get; set; }

        [ObservableProperty]
        private string _status;

        public string FormattedSize
        {
            get
            {
                if (FileSize < 1024) return $"{FileSize} B";
                if (FileSize < 1024 * 1024) return $"{FileSize / 1024.0:F2} KB";
                return $"{FileSize / (1024.0 * 1024.0):F2} MB";
            }
        }
    }
}