using System.Text;

namespace CommanderD.Filesystem;

public class File(string name, string path) : FileSystemItem(name, path)
{
    public string? Content { get; set; }

    public long Size => Content == null ? 0 : Encoding.UTF8.GetByteCount(Content);
}
