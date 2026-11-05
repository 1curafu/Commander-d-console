namespace CommanderD.Services;

public record DirectoryEntry(string Name, bool IsDirectory, long Size, DateTime CreatedOn, string? Content);
