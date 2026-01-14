namespace second_task.Data.Providers
{
    public interface IDataProvider<T>
    {
        IEnumerable<T> ReadData(string filePath, object importSettings);
        void WriteData(string filePath, IEnumerable<T> data, object exportSettings);
    }
}
