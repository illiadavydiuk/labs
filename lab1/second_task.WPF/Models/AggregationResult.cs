namespace second_task.WPF.Models
{
    public class AggregationResult
    {
        public string GroupName { get; set; } = string.Empty;
        public string AggregationType { get; set; } = string.Empty;
        public double Value { get; set; }
        public int Count { get; set; }
        public string FormattedValue => AggregationType == "Count" ? Count.ToString() : Value.ToString("F2");
    }
}
