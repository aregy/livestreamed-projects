namespace devexBlazor.Data;

public class FileUrlStorageService
{
    private Dictionary<Guid, FileTag> _FileUrlStorageStorage = new Dictionary<Guid, FileTag>();
    public void Add(FileTag fileTag)
    {
        if (!_FileUrlStorageStorage.ContainsKey(fileTag.Id))
            _FileUrlStorageStorage.Add(fileTag.Id, fileTag);
    }
    public FileTag? Get(Guid key)
    {
        return _FileUrlStorageStorage.GetValueOrDefault(key);
    }
    public List<FileTag> List(string match = "")
    {
        var results = new List<FileTag>();
        foreach (var guid in _FileUrlStorageStorage.Keys)
        {
            if (_FileUrlStorageStorage[guid].Name.Contains(match))
                results.Add(_FileUrlStorageStorage[guid]);
        }
        return results;
    }
}

