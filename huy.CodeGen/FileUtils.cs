using System.Text;

namespace huy.CodeGen
{
    public static class FileUtils
    {
        public static void WriteAllTextInUTF8(string path, string content)
        {
            System.IO.File.WriteAllText(path, content, Encoding.UTF8);
        }
    }
}
