using System.Text.Json;
using CommanderD.Filesystem;

namespace CommanderD.Services;

public class PersistenceService
{
    public const string DefaultFileName = "commanderd.json";

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public string FilePath { get; }

    public PersistenceService()
        : this(Path.Combine(AppContext.BaseDirectory, DefaultFileName))
    {
    }

    public PersistenceService(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        FilePath = filePath;
    }

    public void SaveDrives(IEnumerable<Drive> drives)
    {
        Save(drives.Select(PersistenceMapper.ToData).ToList());
    }

    public List<Drive> LoadDrives()
    {
        List<DriveData>? data = Load<List<DriveData>>();

        return data?.Select(PersistenceMapper.ToDrive).ToList() ?? new List<Drive>();
    }

    public void Save<T>(T data)
    {
        string? directory = Path.GetDirectoryName(Path.GetFullPath(FilePath));

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string temporaryPath = FilePath + ".tmp";
        System.IO.File.WriteAllText(temporaryPath, JsonSerializer.Serialize(data, Options));
        System.IO.File.Move(temporaryPath, FilePath, overwrite: true);
    }

    public T? Load<T>()
    {
        if (!System.IO.File.Exists(FilePath))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(System.IO.File.ReadAllText(FilePath), Options);
    }
}