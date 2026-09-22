namespace KosManager.Web.Models;

public record RoomDto(int Id, string Number, string Type, string Price, string Status, string? Tenant);
public record TenantDto(int Id, string Name, string Phone, string Room, string MoveIn, bool HasTelegram);
public record BillRow(int Id, string Tenant, string Period, string Amount, string Due, string Status);
public record PayRow(int Id, string Name, string Meta);
public record OverdueDto(string Tenant, string Amount, string Due, int DaysLate);
public record DashboardDto(string Occupancy, string Kas, string Tunggakan, List<OverdueDto> Overdue, int Reminders);

public static class Format
{
    private static readonly System.Globalization.CultureInfo Id = new("id-ID");
    public static string Rp(decimal v) => "Rp " + v.ToString("N0", Id);
}
