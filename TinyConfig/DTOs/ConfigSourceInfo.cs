namespace TinyConfig
{
    public class ConfigSourceInfo
    {
        public bool IsFile { get; }
        public string FilePath { get; }

        internal ConfigSourceInfo(bool isFile, string filePath)
        {
            IsFile = isFile;
            FilePath = filePath;
        }
    }
}
