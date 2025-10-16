namespace second_task.Data.Interfaces
{
    public interface IReportGenerator<T>
    {
        void Generate(string filePath, IEnumerable<T> data);
    }
}
