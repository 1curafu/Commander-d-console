using System.Text.Json.Serialization;

namespace CommanderD.Services;



public class DriveData
{
    public string Name { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public DateTime RootCreatedOn { get; set; }

    public List<ItemData> Items { get; set; } = new();
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(FolderData), "folder")]
[JsonDerivedType(typeof(FileData), "file")]
public abstract class ItemData
{
    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }
}

public class FolderData : ItemData
{
    public List<ItemData> Items { get; set; } = new();
}

public class FileData : ItemData
{
    public string? Content { get; set; }
}