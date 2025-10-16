namespace second_task.Data.Utils
{
    public static class FileUtil
    {
        public static bool IsFileLockedForRead(string path)
        {
            try
            {
                using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
            catch
            {
                return true;
            }
        }

        public static bool IsFileLockedForWrite(string path)
        {
            try
            {
                using var stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
            catch
            {
                return true;
            }
        }
    }
}
