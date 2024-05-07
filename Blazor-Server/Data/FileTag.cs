namespace devexBlazor.Data;

public class FileTag {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime LastWriteTime { get; set; }
    public SignStatus SignStatus { get; set; }
    public int FileSize { get; set; } // in KB, matching Windows Explorer size format
}
public enum SignStatus { Requested, Declined, Signed }