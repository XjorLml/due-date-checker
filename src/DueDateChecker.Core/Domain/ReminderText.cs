using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace DueDateChecker.Core.Domain;

/// <summary>Which Android channel a reminder belongs to (UX29, 2026-11-16).</summary>
public enum ReminderKind { ComingUp, DueToday, Overdue }

/// <summary>N1: sensitive shade text and a deliberately generic public version. Never render notes.</summary>
public sealed record ReminderText(string Title, string Body, string PublicBody, string Version, ReminderKind Kind = ReminderKind.DueToday)
{
    /// <summary>UX32: shown for a due whose amount changes each month and has not been entered yet.</summary>
    public const string AmountNeeded = "Amount needed";

    public static ReminderText Create(Due due, DateOnly deliveryDate)
    {
        var amount = due.Amount?.ToString() ?? (due.AmountVaries ? AmountNeeded : $"{due.Category}, no amount");
        var repeat = due.Repeat == RepeatRule.Never ? "" : $" · repeats {RepeatPhrase(due.Repeat)}";
        var body = $"{amount} · {due.DueDate.ToString("ddd dd MMM", CultureInfo.InvariantCulture)}{repeat}";
        var late = deliveryDate.DayNumber - due.DueDate.DayNumber;
        if (late > 0)
        {
            // OD02: an overdue reminder says how late it is; the lock screen stays generic.
            return new($"{due.Title} is {late} {(late == 1 ? "day" : "days")} late", body, "You have an overdue due",
                Fingerprint(due), ReminderKind.Overdue);
        }
        var when = late == 0 ? "today" : late == -1
            ? "tomorrow" : "on " + due.DueDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        return new($"{due.Title} is due {when}", body, $"You have a due {when}", Fingerprint(due),
            late == 0 ? ReminderKind.DueToday : ReminderKind.ComingUp);
    }

    /// <summary>The repeat as spoken in running text: "monthly", "twice a month".</summary>
    public static string RepeatPhrase(RepeatRule repeat) => repeat switch
    {
        RepeatRule.Weekly => "weekly",
        RepeatRule.Monthly => "monthly",
        RepeatRule.TwiceMonthly => "twice a month",
        RepeatRule.Yearly => "yearly",
        _ => "never",
    };

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
            // Written only when present, so dues without a second day keep their earlier fingerprint
            // and notifications delivered before the upgrade stay actionable.
            if (due.RepeatDay2 is { } second) writer.Write(second);
            if (due.AmountVaries) writer.Write("amount-varies");
        }
        return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
    }
}
