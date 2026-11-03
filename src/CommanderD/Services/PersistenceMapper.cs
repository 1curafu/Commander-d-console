using System.Reflection;
using CommanderD.Filesystem;
using FsFile = CommanderD.Filesystem.File;

namespace CommanderD.Services;

public static class PersistenceMapper
{
    private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    public static DriveData ToData(Drive drive)
    {
        return new DriveData
        {
            Name = drive.Name,
            Label = drive.Label,
            RootCreatedOn = GetCreatedOn(drive.RootFolder),
            Items = drive.RootFolder.GetChildItems().Select(ToItemData).ToList(),
        };
    }

    public static Drive ToDrive(DriveData data)
    {
        var drive = new Drive(data.Name, data.Label);

        SetCreatedOn(drive.RootFolder, data.RootCreatedOn);

        foreach (ItemData item in data.Items)
        {
            drive.RootFolder.AddChildItem(ToItem(item));
        }

        return drive;
    }

    private static ItemData ToItemData(FileSystemItem item)
    {
        switch (item)
        {
            case Folder folder:
                return new FolderData
                {
                    Name = GetName(folder),
                    Path = GetPath(folder),
                    CreatedOn = GetCreatedOn(folder),
                    Items = folder.GetChildItems().Select(ToItemData).ToList(),
                };

            case FsFile file:
                return new FileData
                {
                    Name = GetName(file),
                    Path = GetPath(file),
                    CreatedOn = GetCreatedOn(file),
                    Content = file.Content,
                };

            default:
                throw new NotSupportedException($"Cannot save items of type {item.GetType().Name}.");
        }
    }

    private static FileSystemItem ToItem(ItemData data)
    {
        switch (data)
        {
            case FolderData folderData:
                var folder = new Folder(folderData.Name, folderData.Path);
                SetCreatedOn(folder, folderData.CreatedOn);

                foreach (ItemData child in folderData.Items)
                {
                    folder.AddChildItem(ToItem(child));
                }

                return folder;

            case FileData fileData:
                var file = new FsFile(fileData.Name, fileData.Path);
                SetCreatedOn(file, fileData.CreatedOn);
                file.Content = fileData.Content;
                return file;

            default:
                throw new NotSupportedException($"Cannot load items of type {data.GetType().Name}.");
        }
    }

    private static string GetName(FileSystemItem item) => (string)Read(typeof(FileSystemItem), "Name", item)!;

    private static string GetPath(FileSystemItem item) => (string)Read(typeof(FileSystemItem), "Path", item)!;

    private static DateTime GetCreatedOn(FileSystemItem item) => (DateTime)Read(typeof(FileSystemItem), "CreatedOn", item)!;

    private static void SetCreatedOn(FileSystemItem item, DateTime value) => Write(typeof(FileSystemItem), "CreatedOn", item, value);

    private static object? Read(Type type, string propertyName, object target)
    {
        return FindProperty(type, propertyName).GetValue(target);
    }

    private static void Write(Type type, string propertyName, object target, object? value)
    {
        FindProperty(type, propertyName).SetValue(target, value);
    }

    private static PropertyInfo FindProperty(Type type, string propertyName)
    {
        return type.GetProperty(propertyName, AnyInstance)
            ?? throw new InvalidOperationException(
                $"Property '{propertyName}' was not found on {type.Name}. Was it renamed? Update PersistenceMapper.");
    }
}