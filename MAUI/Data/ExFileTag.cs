﻿namespace SignPDF.Data;

public class FileTag
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime LastWriteTime { get; set; }
    public SignStatus SignStatus { get; set; }
}
public enum SignStatus { Requested, Declined, Signed }