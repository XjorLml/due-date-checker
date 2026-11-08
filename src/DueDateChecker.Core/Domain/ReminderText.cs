using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace DueDateChecker.Core.Domain;

/// <summary>N1: sensitive shade text and a deliberately generic public version. Never render notes.</summary>
public sealed record ReminderText(string Title, string Body, string PublicBody, string Version)
{
    public static ReminderText Create(Due due, DateOnly deliveryDate)
    {
        var when = due.DueDate == deliveryDate ? "today" : due.DueDate.DayNumber - deliveryDate.DayNumber == 1
            ? "tomorrow" : "on " + due.DueDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        var amount = due.Amount?.ToString() ?? $"{due.Category}, no amount";
        var repeat = due.Repeat == RepeatRule.Never ? "" : $" · repeats {due.Repeat.ToString().ToLowerInvariant()}";
        return new($"{due.Title} is due {when}",
            $"{amount} · {due.DueDate.ToString("ddd dd MMM", CultureInfo.InvariantCulture)}{repeat}",
            $"You have a due {when}", Fingerprint(due));
    }

    /// <summary>Opaque snapshot identity, including edits that share a timestamp. Contains no plaintext fields.</summary>
    public static string Fingerprint(Due due)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(due.Id.ToString("N")); writer.Write(due.Title);
            writer.Write(due.Amount.HasValue); writer.Write(due.Amount?.Centavos ?? 0);
            writer.Write(due.DueDate.DayNumber); writer.Write(due.Notes is not null); writer.Write(due.Notes ?? "");
            writer.Write(due.IsPaid); writer.Write(due.CreatedAt.UtcTicks); writer.Write(due.UpdatedAt.UtcTicks);
            writer.Write(due.DeletedAt?.UtcTicks ?? 0); writer.Write((int)due.Category); writer.Write((int)due.Repeat);
            writer.Write(due.RepeatDay ?? 0); writer.Write(due.RepeatSourceId?.ToString("N") ?? "");
            writer.Write(due.CategoryId);
        }
        return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
    }
}
