namespace CommanderD.Services;

public interface IFileSystemService
{
    string CurrentDirectory { get; }

    string ResolvePath(string path);

    bool DirectoryExists(string path);

    void ChangeDirectory(string path);
}