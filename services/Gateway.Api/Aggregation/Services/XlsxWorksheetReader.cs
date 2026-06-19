using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Timegrip.Gateway.Api.Aggregation.Services;

internal static partial class XlsxWorksheetReader
{
    private static readonly XNamespace SpreadsheetNamespace =
        "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace RelationshipsNamespace =
        "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace PackageRelationshipsNamespace =
        "http://schemas.openxmlformats.org/package/2006/relationships";

    public static List<List<string>> ReadFirstWorksheet(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        var sharedStrings = ReadSharedStrings(archive);
        var worksheetPath = GetFirstWorksheetPath(archive);
        var worksheetEntry = archive.GetEntry(worksheetPath)
            ?? throw new InvalidOperationException("Excel-arket mangler et worksheet.");

        using var worksheetStream = worksheetEntry.Open();
        var document = XDocument.Load(worksheetStream);

        return document
            .Descendants(SpreadsheetNamespace + "row")
            .Select(row => ReadRow(row, sharedStrings))
            .Where(row => row.Any(cell => !string.IsNullOrWhiteSpace(cell)))
            .ToList();
    }

    private static string GetFirstWorksheetPath(ZipArchive archive)
    {
        var workbookEntry = archive.GetEntry("xl/workbook.xml")
            ?? throw new InvalidOperationException("Excel-filen mangler workbook.xml.");
        var relationshipsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels")
            ?? throw new InvalidOperationException("Excel-filen mangler workbook relationships.");

        using var workbookStream = workbookEntry.Open();
        using var relationshipsStream = relationshipsEntry.Open();

        var workbook = XDocument.Load(workbookStream);
        var relationships = XDocument.Load(relationshipsStream);

        var firstSheet = workbook.Descendants(SpreadsheetNamespace + "sheet").FirstOrDefault()
            ?? throw new InvalidOperationException("Excel-filen har ingen sheets.");
        var relationshipId = firstSheet.Attribute(RelationshipsNamespace + "id")?.Value
            ?? throw new InvalidOperationException("Første sheet mangler relationship id.");
        var target = relationships
            .Descendants(PackageRelationshipsNamespace + "Relationship")
            .FirstOrDefault(relationship => relationship.Attribute("Id")?.Value == relationshipId)
            ?.Attribute("Target")
            ?.Value
            ?? throw new InvalidOperationException("Første sheet kunne ikke findes.");

        return target.StartsWith("xl/", StringComparison.OrdinalIgnoreCase)
            ? target
            : $"xl/{target.TrimStart('/')}";
    }

    private static List<string> ReadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry is null)
        {
            return [];
        }

        using var stream = entry.Open();
        var document = XDocument.Load(stream);

        return document
            .Descendants(SpreadsheetNamespace + "si")
            .Select(item => string.Concat(item.Descendants(SpreadsheetNamespace + "t").Select(text => text.Value)))
            .ToList();
    }

    private static List<string> ReadRow(XElement row, IReadOnlyList<string> sharedStrings)
    {
        var cellsByIndex = row
            .Elements(SpreadsheetNamespace + "c")
            .Select(cell => new
            {
                Index = GetColumnIndex(cell.Attribute("r")?.Value),
                Value = ReadCellValue(cell, sharedStrings),
            })
            .ToList();

        if (cellsByIndex.Count == 0)
        {
            return [];
        }

        var result = Enumerable.Repeat(string.Empty, cellsByIndex.Max(cell => cell.Index) + 1).ToList();
        foreach (var cell in cellsByIndex)
        {
            result[cell.Index] = cell.Value;
        }

        return result;
    }

    private static string ReadCellValue(XElement cell, IReadOnlyList<string> sharedStrings)
    {
        var cellType = cell.Attribute("t")?.Value;

        if (cellType == "inlineStr")
        {
            return string.Concat(cell.Descendants(SpreadsheetNamespace + "t").Select(text => text.Value)).Trim();
        }

        var rawValue = cell.Element(SpreadsheetNamespace + "v")?.Value ?? string.Empty;
        if (cellType == "s"
            && int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sharedIndex)
            && sharedIndex >= 0
            && sharedIndex < sharedStrings.Count)
        {
            return sharedStrings[sharedIndex].Trim();
        }

        return rawValue.Trim();
    }

    private static int GetColumnIndex(string? cellReference)
    {
        var match = ColumnReferenceRegex().Match(cellReference ?? string.Empty);
        var letters = match.Success ? match.Groups[1].Value : "A";
        var index = 0;

        foreach (var letter in letters)
        {
            index = index * 26 + letter - 'A' + 1;
        }

        return index - 1;
    }

    [GeneratedRegex("^([A-Z]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ColumnReferenceRegex();
}
