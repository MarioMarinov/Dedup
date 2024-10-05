namespace Services
{
    public class FileWrapper : IFileWrapper
    {
        public void Delete(string path)
        {
            File.Delete(path);
        }

        public void Move(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }
    }
}
