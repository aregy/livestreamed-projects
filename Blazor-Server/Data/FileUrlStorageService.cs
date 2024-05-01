namespace devexBlazor.Data;

public class FileUrlStorageService {
    private Dictionary<Guid, FileTag> _FileUrlStorageStorage = new Dictionary<Guid, FileTag>();
    private string _rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

    public FileUrlStorageService() {
        Initialize();
    }

    public string FullName(FileTag fileTag) {
        return Path.Combine(_rootPath, fileTag.Name);
    }

    private void Initialize() {
        var directoryInfo = new DirectoryInfo(_rootPath);
        if (!directoryInfo.Exists) throw new FileNotFoundException();
        foreach (var file in directoryInfo.GetFiles("*.pdf")) {
            Add(new FileTag() { Id = Guid.NewGuid(), LastWriteTime = file.LastWriteTime, Name = file.Name });
        }
    }

    public void Add(FileTag fileTag) {
        if (!_FileUrlStorageStorage.ContainsKey(fileTag.Id))
            _FileUrlStorageStorage.Add(fileTag.Id, fileTag);
    }
    public FileTag? Get(Guid key) {
        return _FileUrlStorageStorage.GetValueOrDefault(key);
    }
    public List<FileTag> Files(string match = "") {
        var results = new List<FileTag>();
        foreach (var guid in _FileUrlStorageStorage.Keys) {
            if (_FileUrlStorageStorage[guid].Name.Contains(match))
                results.Add(_FileUrlStorageStorage[guid]);
        }
        return results;
    }
}

