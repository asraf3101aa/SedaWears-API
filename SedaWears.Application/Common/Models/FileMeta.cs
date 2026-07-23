namespace SedaWears.Application.Common.Models;

public record FileMeta(string FileName, string ContentType)
{
    public string FileName { get; init; } = FileName.Trim();
    public string ContentType { get; init; } = ContentType.Trim();
}
