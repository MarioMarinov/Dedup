namespace Services
{
    public interface IDirectoryWrapper
    {
        IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
    }
}
