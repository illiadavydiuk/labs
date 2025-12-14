namespace DuckNet.Data.Models
{
    // Цей клас живе в Data, бо це спільна модель даних для всіх шарів
    public class NetworkAdapterInfo
    {
        public int DeviceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NetConnectionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }
}