namespace Services
{
    public interface IFileWrapper
    {
        void Delete(string path);
        void Move(string sourceFileName, string destFileName);
    }
}
