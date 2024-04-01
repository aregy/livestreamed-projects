namespace devexBlazor.Data;

public class FileUrlStorageService
{
    private Dictionary<Guid, string> _FileUrlStorageStorage = new Dictionary<Guid, string>();
    public void Add(Guid key, string val)
    {
        if (!_FileUrlStorageStorage.ContainsKey(key))
            _FileUrlStorageStorage.Add(key, val);
    }
    public string? Get(Guid key)
    {
        return _FileUrlStorageStorage.GetValueOrDefault(key);
    }
}

