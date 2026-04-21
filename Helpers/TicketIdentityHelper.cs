using System.Globalization;

namespace ASP_PROJECT.Helpers;

public static class TicketIdentityHelper
{
    private const string VerificationSalt = "EVENTURE-TICKET-CHECK";

    public static string GetTicketCode(int registrationId, int eventId, int ticketIndex)
        => $"REG{registrationId:D5}-EV{eventId:D4}-T{ticketIndex + 1:D2}";

    public static string GetVerificationCode(int registrationId, int eventId, DateTime startsAtUtc, int ticketIndex)
    {
        var seed = BuildVerificationSeed(registrationId, eventId, startsAtUtc, ticketIndex);
        var primary = ComputeFnv1a(seed).ToString("X8", CultureInfo.InvariantCulture);
        var secondary = ComputeFnv1a($"{seed}|{VerificationSalt}").ToString("X8", CultureInfo.InvariantCulture);
        return string.Concat(primary, secondary)[..12];
    }

    public static string GetSeatLabel(int registrationId, int ticketIndex)
    {
        var hallNumber = ((registrationId + ticketIndex) % 5) + 1;
        var seatNumber = ((registrationId * 13) + ((ticketIndex + 1) * 7)) % 80 + 1;
        return $"Hall {hallNumber} / Seat {seatNumber}";
    }

    public static bool TryParseTicketCode(string? ticketCode, out int registrationId, out int eventId, out int ticketIndex)
    {
        registrationId = 0;
        eventId = 0;
        ticketIndex = 0;

        if (string.IsNullOrWhiteSpace(ticketCode))
        {
            return false;
        }

        var parts = ticketCode.Trim().Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 3
            || !parts[0].StartsWith("REG", StringComparison.OrdinalIgnoreCase)
            || !parts[1].StartsWith("EV", StringComparison.OrdinalIgnoreCase)
            || !parts[2].StartsWith("T", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!int.TryParse(parts[0][3..], NumberStyles.None, CultureInfo.InvariantCulture, out registrationId)
            || !int.TryParse(parts[1][2..], NumberStyles.None, CultureInfo.InvariantCulture, out eventId)
            || !int.TryParse(parts[2][1..], NumberStyles.None, CultureInfo.InvariantCulture, out var ticketNumber))
        {
            return false;
        }

        ticketIndex = ticketNumber - 1;
        return ticketIndex >= 0;
    }

    private static string BuildVerificationSeed(int registrationId, int eventId, DateTime startsAtUtc, int ticketIndex)
        => $"{VerificationSalt}|{registrationId}|{eventId}|{ticketIndex}|{startsAtUtc:O}";

    private static uint ComputeFnv1a(string value)
    {
        const uint offsetBasis = 2166136261;
        const uint prime = 16777619;
        var hash = offsetBasis;

        foreach (var character in value)
        {
            hash ^= character;
            hash *= prime;
        }

        return hash;
    }
}
