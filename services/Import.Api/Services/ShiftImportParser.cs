using System.Globalization;
using System.Text;
using Timegrip.Import.Api.Models;
using Timegrip.Import.Api.Requests;
using Timegrip.Import.Api.Responses;

namespace Timegrip.Import.Api.Services;

internal static class ShiftImportParser
{
    private static readonly Dictionary<string, string> DateAliases = CreateAliasMap("date", "dato", "day", "dag");
    private static readonly Dictionary<string, string> StartAliases = CreateAliasMap(
        "start",
        "starttime",
        "starttid",
        "datetimefrom",
        "from",
        "fra",
        "begin"
    );
    private static readonly Dictionary<string, string> EndAliases = CreateAliasMap(
        "end",
        "endtime",
        "slut",
        "sluttid",
        "datetimeto",
        "to",
        "til"
    );
    private static readonly HashSet<string> ReservedColumns =
    [
        ..DateAliases.Keys,
        ..StartAliases.Keys,
        ..EndAliases.Keys,
        "isholiday",
        "holiday",
        "helligdag",
        "isweekend",
        "isclosed",
        "closed",
        "customerid",
        "customer_id",
        "orgelement",
        "region",
        "weekday",
        "dayofweek",
        "daytype",
        "forecast",
        "prediction",
        "predicted",
        "weather",
        "temperature",
        "temperature2m",
        "temperature_2m",
        "revenue",
        "total",
        "sum",
        "department",
        "afdeling",
        "location",
        "lokation",
    ];

    public static ShiftImportParseResult Parse(
        List<List<string>> rows,
        IReadOnlyCollection<RoleReferenceRequest> roles
    )
    {
        var result = new ShiftImportParseResult();
        if (rows.Count == 0)
        {
            result.Errors.Add(new ShiftImportRowErrorResponse
            {
                RowNumber = 0,
                Message = "Excel-filen er tom.",
            });
            return result;
        }

        var headers = rows[0];
        var normalizedHeaders = headers.Select(Normalize).ToList();
        var roleColumns = GetRoleColumns(headers, normalizedHeaders, roles);
        var unknownRoles = GetUnknownRoleColumns(rows, headers, normalizedHeaders, roleColumns);

        result.MatchedRoles = roleColumns.Select(column => column.Role.Name).Distinct().Order().ToList();
        result.UnknownRoles = unknownRoles.Distinct(StringComparer.OrdinalIgnoreCase).Order().ToList();

        var dateColumn = FindColumn(normalizedHeaders, DateAliases);
        var startColumn = FindColumn(normalizedHeaders, StartAliases);
        var endColumn = FindColumn(normalizedHeaders, EndAliases);

        if (startColumn is null || endColumn is null)
        {
            result.Errors.Add(new ShiftImportRowErrorResponse
            {
                RowNumber = 1,
                Message = "Excel-filen skal indeholde start- og slutkolonner.",
            });
            return result;
        }

        for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            var rowNumber = rowIndex + 1;
            var row = rows[rowIndex];

            if (row.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            if (!TryParseDateTime(row, dateColumn, startColumn.Value, out var startTime)
                || !TryParseDateTime(row, dateColumn, endColumn.Value, out var endTime))
            {
                result.Errors.Add(new ShiftImportRowErrorResponse
                {
                    RowNumber = rowNumber,
                    Message = "Start- eller sluttidspunkt kunne ikke læses.",
                });
                continue;
            }

            if (endTime <= startTime)
            {
                result.Errors.Add(new ShiftImportRowErrorResponse
                {
                    RowNumber = rowNumber,
                    Message = "Rækken har et ugyldigt tidsinterval.",
                });
                continue;
            }

            var requirements = GetRequirements(row, roleColumns, rowNumber, result.Errors);
            if (requirements.Count == 0)
            {
                result.Errors.Add(new ShiftImportRowErrorResponse
                {
                    RowNumber = rowNumber,
                    Message = "Rækken har ingen rollekrav.",
                });
                continue;
            }

            result.Shifts.Add(new ImportedShift
            {
                RowNumber = rowNumber,
                StartTime = startTime,
                EndTime = endTime,
                ShiftRequirements = requirements,
            });
        }

        return result;
    }

    private static List<RoleColumn> GetRoleColumns(
        IReadOnlyList<string> headers,
        IReadOnlyList<string> normalizedHeaders,
        IReadOnlyCollection<RoleReferenceRequest> roles
    )
    {
        var rolesByName = roles
            .GroupBy(role => Normalize(role.Name))
            .ToDictionary(group => group.Key, group => group.First());

        return normalizedHeaders
            .Select((header, index) => new { Header = header, Index = index })
            .Where(item => rolesByName.ContainsKey(item.Header))
            .Select(item => new RoleColumn(item.Index, headers[item.Index], rolesByName[item.Header]))
            .ToList();
    }

    private static List<string> GetUnknownRoleColumns(
        IReadOnlyList<List<string>> rows,
        IReadOnlyList<string> headers,
        IReadOnlyList<string> normalizedHeaders,
        IReadOnlyCollection<RoleColumn> roleColumns
    )
    {
        var knownRoleIndexes = roleColumns.Select(column => column.Index).ToHashSet();
        var unknownRoles = new List<string>();

        for (var columnIndex = 0; columnIndex < normalizedHeaders.Count; columnIndex++)
        {
            if (knownRoleIndexes.Contains(columnIndex)
                || ReservedColumns.Contains(normalizedHeaders[columnIndex])
                || string.IsNullOrWhiteSpace(headers[columnIndex]))
            {
                continue;
            }

            var looksLikeRoleAmount = rows
                .Skip(1)
                .Any(row => TryGetPositiveAmount(GetCell(row, columnIndex), out _));

            if (looksLikeRoleAmount)
            {
                unknownRoles.Add(headers[columnIndex]);
            }
        }

        return unknownRoles;
    }

    private static List<ImportedShiftRequirement> GetRequirements(
        IReadOnlyList<string> row,
        IReadOnlyCollection<RoleColumn> roleColumns,
        int rowNumber,
        List<ShiftImportRowErrorResponse> errors
    )
    {
        var requirements = new List<ImportedShiftRequirement>();

        foreach (var roleColumn in roleColumns)
        {
            var value = GetCell(row, roleColumn.Index);
            if (string.IsNullOrWhiteSpace(value) || value == "0")
            {
                continue;
            }

            if (!TryGetPositiveAmount(value, out var amount))
            {
                errors.Add(new ShiftImportRowErrorResponse
                {
                    RowNumber = rowNumber,
                    Message = $"Rollekolonnen '{roleColumn.Header}' skal indeholde et heltal mellem 0 og 255.",
                });
                continue;
            }

            requirements.Add(new ImportedShiftRequirement
            {
                RoleId = roleColumn.Role.RoleId,
                Amount = amount,
            });
        }

        return requirements;
    }

    private static bool TryParseDateTime(
        IReadOnlyList<string> row,
        int? dateColumn,
        int timeColumn,
        out DateTime dateTime
    )
    {
        var rawTime = GetCell(row, timeColumn);
        if (dateColumn is null)
        {
            return TryParseDateTimeValue(rawTime, out dateTime);
        }

        var rawDate = GetCell(row, dateColumn.Value);
        if (!TryParseDateValue(rawDate, out var date) || !TryParseTimeValue(rawTime, out var time))
        {
            dateTime = default;
            return false;
        }

        dateTime = date.Date.Add(time);
        return true;
    }

    private static bool TryParseDateTimeValue(string value, out DateTime dateTime)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var serial))
        {
            dateTime = DateTime.FromOADate(serial);
            return true;
        }

        return DateTime.TryParse(value, CultureInfo.GetCultureInfo("da-DK"), DateTimeStyles.None, out dateTime)
            || DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
    }

    private static bool TryParseDateValue(string value, out DateTime date)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var serial))
        {
            date = DateTime.FromOADate(serial).Date;
            return true;
        }

        return DateTime.TryParse(value, CultureInfo.GetCultureInfo("da-DK"), DateTimeStyles.None, out date)
            || DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    private static bool TryParseTimeValue(string value, out TimeSpan time)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var serial))
        {
            time = DateTime.FromOADate(serial).TimeOfDay;
            return true;
        }

        return TimeSpan.TryParse(value, CultureInfo.GetCultureInfo("da-DK"), out time)
            || TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out time)
            || (DateTime.TryParse(value, CultureInfo.GetCultureInfo("da-DK"), DateTimeStyles.None, out var dateTime)
                && (time = dateTime.TimeOfDay) >= TimeSpan.Zero);
    }

    private static bool TryGetPositiveAmount(string value, out byte amount)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue)
            && decimalValue % 1 == 0
            && decimalValue is > 0 and <= byte.MaxValue)
        {
            amount = (byte)decimalValue;
            return true;
        }

        amount = 0;
        return false;
    }

    private static int? FindColumn(IReadOnlyList<string> normalizedHeaders, IReadOnlyDictionary<string, string> aliases)
    {
        for (var index = 0; index < normalizedHeaders.Count; index++)
        {
            if (aliases.ContainsKey(normalizedHeaders[index]))
            {
                return index;
            }
        }

        return null;
    }

    private static string GetCell(IReadOnlyList<string> row, int index) =>
        index < row.Count ? row[index] : string.Empty;

    private static Dictionary<string, string> CreateAliasMap(params string[] aliases) =>
        aliases.ToDictionary(Normalize, alias => alias);

    private static string Normalize(string value)
    {
        var builder = new StringBuilder();
        foreach (var character in value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    private sealed record RoleColumn(int Index, string Header, RoleReferenceRequest Role);
}
