using System.Buffers;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.Core.Data;

/// <summary>Why an export file or a set of records cannot be imported. The App owns the wording.</summary>
public enum DueArchiveProblem
{
    NotAnExport,
    NewerVersion,
    TooLarge,
    NoDues,
    InvalidDue,
    DuplicateId,
    AppNotEmpty,
}

/// <summary>Carries no user data: only the problem, the due's position in the file and the field name.</summary>
public sealed class DueArchiveException(DueArchiveProblem problem, int? dueNumber = null, string? field = null)
    : Exception($"Import refused: {problem}" + (dueNumber is { } n ? $" at due {n}" : "") + (field is null ? "" : $" ({field})"))
{
    public DueArchiveProblem Problem { get; } = problem;
    /// <summary>1-based position of the due in the file, when one due is at fault.</summary>
    public int? DueNumber { get; } = dueNumber;
    /// <summary>The file's field name (for example <c>dueDate</c>), when one field is at fault.</summary>
    public string? Field { get; } = field;
}

public sealed record DueArchiveContents(DateTimeOffset ExportedAt, IReadOnlyList<DueRecord> Dues)
{
    public int DeletedCount => Dues.Count(due => due.IsDeleted);
    public int ActiveCount => Dues.Count - DeletedCount;
}

/// <summary>
/// The UX05 export file, format <c>duewise.dues</c> version 1 (ADR 010): UTF-8 JSON with
/// <c>format</c>, <c>version</c>, <c>exportedAt</c> and <c>dues</c>, one object per stored row.
/// </summary>
/// <remarks>
/// Written and read by hand with <see cref="Utf8JsonWriter"/> and <see cref="JsonDocument"/> rather than
/// <see cref="JsonSerializer"/>: the Android Release build trims code and turns off reflection-based
/// serialization, and reading by hand makes every rule explicit (exact field set, types, no duplicates).
/// Reading checks <c>format</c> and <c>version</c> first, so a newer file is reported as newer even if
/// its shape changed.
/// </remarks>
public static class DueArchive
{
    public const string FormatName = "duewise.dues";
    public const int CurrentVersion = 1;
    public const int MaxBytes = 5 * 1024 * 1024;

    private const string DateFormat = "yyyy-MM-dd";
    private const string TimestampFormat = "O";

    private static readonly string[] FileFields = ["format", "version", "exportedAt", "dues"];
    private static readonly string[] DueFields =
    [
        "id", "title", "amountCentavos", "currency", "dueDate", "notes", "isPaid", "timezone",
        "createdAt", "updatedAt", "deletedAt", "category", "repeatRule", "repeatDay", "repeatSourceId",
    ];

    public static byte[] Write(IReadOnlyList<DueRecord> dues, DateTimeOffset exportedAt)
    {
        var buffer = new ArrayBufferWriter<byte>();
        // Letters in any script stay readable; quotes and HTML-sensitive characters are still escaped.
        var options = new JsonWriterOptions { Indented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
        using (var writer = new Utf8JsonWriter(buffer, options))
        {
            writer.WriteStartObject();
            writer.WriteString("format", FormatName);
            writer.WriteNumber("version", CurrentVersion);
            writer.WriteString("exportedAt", exportedAt.ToString(TimestampFormat, CultureInfo.InvariantCulture));
            writer.WriteStartArray("dues");
            foreach (var due in dues)
            {
                writer.WriteStartObject();
                writer.WriteString("id", due.Id);
                writer.WriteString("title", due.Title);
                WriteNumber(writer, "amountCentavos", due.AmountCentavos);
                writer.WriteString("currency", due.Currency);
                writer.WriteString("dueDate", due.DueDate);
                writer.WriteString("notes", due.Notes);
                writer.WriteBoolean("isPaid", due.IsPaid);
                writer.WriteString("timezone", due.Timezone);
                writer.WriteString("createdAt", due.CreatedAt);
                writer.WriteString("updatedAt", due.UpdatedAt);
                writer.WriteString("deletedAt", due.DeletedAt);
                writer.WriteNumber("category", due.Category);
                writer.WriteNumber("repeatRule", due.RepeatRule);
                WriteNumber(writer, "repeatDay", due.RepeatDay);
                writer.WriteString("repeatSourceId", due.RepeatSourceId);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        return buffer.WrittenSpan.ToArray();
    }

    /// <summary>Parses and fully validates a file. Throws <see cref="DueArchiveException"/>; never partial.</summary>
    public static DueArchiveContents Read(ReadOnlyMemory<byte> file)
    {
        if (file.Length > MaxBytes) throw new DueArchiveException(DueArchiveProblem.TooLarge);
        if (file.Span.StartsWith(new byte[] { 0xEF, 0xBB, 0xBF })) file = file[3..]; // UTF-8 byte order mark

        try
        {
            using var document = JsonDocument.Parse(file, new JsonDocumentOptions { MaxDepth = 8 });
            var root = Fields(document.RootElement, dueNumber: null);

            if (!root.TryGetValue("format", out var format) || format.ValueKind != JsonValueKind.String || format.GetString() != FormatName
                || !root.TryGetValue("version", out var versionElement) || versionElement.ValueKind != JsonValueKind.Number
                || !versionElement.TryGetInt32(out var version))
                throw NotAnExport();
            if (version > CurrentVersion) throw new DueArchiveException(DueArchiveProblem.NewerVersion);
            if (version < 1 || !SameFields(root, FileFields, out _)) throw NotAnExport();

            if (root["exportedAt"].ValueKind != JsonValueKind.String
                || !DateTimeOffset.TryParseExact(root["exportedAt"].GetString(), TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exportedAt)
                || root["dues"].ValueKind != JsonValueKind.Array)
                throw NotAnExport();

            var records = new List<DueRecord>();
            foreach (var element in root["dues"].EnumerateArray())
            {
                records.Add(ReadDue(element, records.Count + 1));
            }
            return new DueArchiveContents(exportedAt, Validate(records));
        }
        catch (JsonException) { throw NotAnExport(); }
        catch (InvalidOperationException) { throw NotAnExport(); } // text that is not valid UTF-8
    }

    /// <summary>
    /// Checks every record against the domain rules and returns them in the app's own form (lower-case
    /// ids, trimmed titles). Also used by the store right before writing, so no caller can skip it.
    /// </summary>
    public static IReadOnlyList<DueRecord> Validate(IReadOnlyList<DueRecord> records)
    {
        if (records.Count == 0) throw new DueArchiveException(DueArchiveProblem.NoDues);
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<DueRecord>(records.Count);
        for (var i = 0; i < records.Count; i++)
        {
            var record = Normalize(records[i], i + 1);
            if (!ids.Add(record.Id)) throw new DueArchiveException(DueArchiveProblem.DuplicateId, i + 1, "id");
            result.Add(record);
        }
        return result;
    }

    private static DueRecord Normalize(DueRecord r, int number)
    {
        DueArchiveException Invalid(string field) => new(DueArchiveProblem.InvalidDue, number, field);

        if (!Guid.TryParseExact(r.Id, "D", out var id) || id == Guid.Empty) throw Invalid("id");
        if (string.IsNullOrWhiteSpace(r.Title)) throw Invalid("title");
        if (r.AmountCentavos is < 0) throw Invalid("amountCentavos");
        if (string.IsNullOrWhiteSpace(r.Currency)) throw Invalid("currency");
        if (!DateOnly.TryParseExact(r.DueDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDate)) throw Invalid("dueDate");
        if (string.IsNullOrWhiteSpace(r.Timezone)) throw Invalid("timezone");
        if (!TryTimestamp(r.CreatedAt, out var createdAt)) throw Invalid("createdAt");
        if (!TryTimestamp(r.UpdatedAt, out var updatedAt)) throw Invalid("updatedAt");
        DateTimeOffset? deletedAt = null;
        if (r.DeletedAt is not null)
        {
            if (!TryTimestamp(r.DeletedAt, out var deleted)) throw Invalid("deletedAt");
            deletedAt = deleted;
        }
        if (!Enum.IsDefined((DueCategory)r.Category)) throw Invalid("category");
        if (!Enum.IsDefined((RepeatRule)r.RepeatRule)) throw Invalid("repeatRule");
        Guid? source = null;
        if (r.RepeatSourceId is not null)
        {
            if (!Guid.TryParseExact(r.RepeatSourceId, "D", out var parsed) || parsed == Guid.Empty) throw Invalid("repeatSourceId");
            source = parsed;
        }

        // The same domain rules every app write passes (amount required by type, repeat and anchor rules).
        var due = new Due(id, r.Title, r.AmountCentavos is { } centavos ? Money.FromCentavos(centavos) : null, dueDate, r.Notes, r.IsPaid,
            createdAt, updatedAt, deletedAt, (DueCategory)r.Category, (RepeatRule)r.RepeatRule, r.RepeatDay, source);
        try
        {
            due.Validate();
        }
        catch (ArgumentException e)
        {
            throw Invalid(e.ParamName switch
            {
                nameof(Due.Amount) => "amountCentavos",
                nameof(Due.Repeat) => "repeatRule",
                nameof(Due.RepeatDay) => "repeatDay",
                _ => "dues",
            });
        }

        return r with { Id = id.ToString(), Title = due.Title, RepeatSourceId = source?.ToString() };
    }

    private static DueRecord ReadDue(JsonElement element, int number)
    {
        var fields = Fields(element, number);
        foreach (var name in DueFields)
        {
            if (!fields.ContainsKey(name)) throw new DueArchiveException(DueArchiveProblem.InvalidDue, number, name);
        }
        if (!SameFields(fields, DueFields, out var unknown)) throw new DueArchiveException(DueArchiveProblem.InvalidDue, number, unknown);

        return new DueRecord(
            Id: Text(fields, "id", number),
            Title: Text(fields, "title", number),
            AmountCentavos: OptionalLong(fields, "amountCentavos", number),
            Currency: Text(fields, "currency", number),
            DueDate: Text(fields, "dueDate", number),
            Notes: OptionalText(fields, "notes", number),
            IsPaid: fields["isPaid"].ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => throw new DueArchiveException(DueArchiveProblem.InvalidDue, number, "isPaid"),
            },
            Timezone: Text(fields, "timezone", number),
            CreatedAt: Text(fields, "createdAt", number),
            UpdatedAt: Text(fields, "updatedAt", number),
            DeletedAt: OptionalText(fields, "deletedAt", number),
            Category: Int(fields, "category", number),
            RepeatRule: Int(fields, "repeatRule", number),
            RepeatDay: OptionalLong(fields, "repeatDay", number) is { } day
                ? day is >= int.MinValue and <= int.MaxValue ? (int)day : throw new DueArchiveException(DueArchiveProblem.InvalidDue, number, "repeatDay")
                : null,
            RepeatSourceId: OptionalText(fields, "repeatSourceId", number));
    }

    // An object's fields by name. A repeated name makes the file ambiguous, so it is refused.
    private static Dictionary<string, JsonElement> Fields(JsonElement element, int? dueNumber)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw dueNumber is { } n ? new DueArchiveException(DueArchiveProblem.InvalidDue, n) : NotAnExport();
        var fields = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!fields.TryAdd(property.Name, property.Value))
                throw dueNumber is { } n ? new DueArchiveException(DueArchiveProblem.InvalidDue, n, property.Name) : NotAnExport();
        }
        return fields;
    }

    private static bool SameFields(Dictionary<string, JsonElement> fields, string[] expected, out string? unknown)
    {
        unknown = fields.Keys.FirstOrDefault(name => !expected.Contains(name));
        return unknown is null && fields.Count == expected.Length;
    }

    private static string Text(Dictionary<string, JsonElement> fields, string name, int number) =>
        OptionalText(fields, name, number) ?? throw new DueArchiveException(DueArchiveProblem.InvalidDue, number, name);

    private static string? OptionalText(Dictionary<string, JsonElement> fields, string name, int number) => fields[name].ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.String => fields[name].GetString(),
        _ => throw new DueArchiveException(DueArchiveProblem.InvalidDue, number, name),
    };

    private static long? OptionalLong(Dictionary<string, JsonElement> fields, string name, int number) => fields[name].ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.Number when fields[name].TryGetInt64(out var value) => value,
        _ => throw new DueArchiveException(DueArchiveProblem.InvalidDue, number, name),
    };

    private static int Int(Dictionary<string, JsonElement> fields, string name, int number) =>
        fields[name].ValueKind == JsonValueKind.Number && fields[name].TryGetInt32(out var value)
            ? value
            : throw new DueArchiveException(DueArchiveProblem.InvalidDue, number, name);

    private static bool TryTimestamp(string text, out DateTimeOffset value) =>
        DateTimeOffset.TryParseExact(text, TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);

    private static void WriteNumber(Utf8JsonWriter writer, string name, long? value)
    {
        if (value is { } number) writer.WriteNumber(name, number);
        else writer.WriteNull(name);
    }

    private static DueArchiveException NotAnExport() => new(DueArchiveProblem.NotAnExport);
}
