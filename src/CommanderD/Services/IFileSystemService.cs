namespace CommanderD.Services;

public interface IFileSystemService
{
    string CurrentDirectory { get; }

    string ResolvePath(string path);

    bool DirectoryExists(string path);
    
    bool FileExists(string path);

    void ChangeDirectory(string path);

    void CreateDirectory(string path);

    void CreateFile(string path, string content);

    void DeleteFile(string path);

    void Rename(string path, string newName);

    bool IsProtectedFile(string path);

    IReadOnlyList<DirectoryEntry> GetEntries(string path);
}