using System.Text;

namespace second_task.Data.Providers
{
    public class ImportSettings
    {
        public string EncodingName { get; set; } = Encoding.UTF8.WebName;
        public string Delimiter { get; set; } = ",";
        public bool HasHeader { get; set; } = true;
        public bool ForceEncoding { get; set; } = false;
        
        // Additional properties for export settings
        public Encoding? Encoding { get; set; }
        public bool HasHeaders 
        { 
            get => HasHeader; 
            set => HasHeader = value; 
        }
        public char DelimiterChar 
        { 
            get => string.IsNullOrEmpty(Delimiter) ? ',' : Delimiter[0]; 
            set => Delimiter = value.ToString(); 
        }
    }
}
