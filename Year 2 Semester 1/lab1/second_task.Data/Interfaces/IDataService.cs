namespace second_task.Data.Interfaces
{
    public interface IDataService<T>
    {
        IEnumerable<T> LoadData(string filePath, object settings);
        void SaveData(string filePath, IEnumerable<T> data, object settings);
    }
}
