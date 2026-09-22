using System.Text.Json;
using KosManager.Web.Models;

namespace KosManager.Web.Services;

public class RoomService(ApiClient api)
{
    public async Task<List<RoomDto>> ListAsync()
    {
        var raw = await api.Get<List<JsonElement>>("/api/rooms") ?? [];
        return raw.Select(r => new RoomDto(
            r.GetProperty("id").GetInt32(),
            r.GetProperty("number").GetString()!,
            r.GetProperty("type").GetString()!,
            r.GetProperty("monthlyPrice").GetDecimal(),
            r.GetProperty("status").GetString()!,
            r.TryGetProperty("tenant", out var t) && t.ValueKind != JsonValueKind.Null ? t.GetString() : null
        )).ToList();
    }

    public Task CreateAsync(string number, string type, decimal price) =>
        api.Post<bool>("/api/rooms", new { number, type, monthlyPrice = price, status = "kosong" });

    public Task UpdateAsync(int id, string number, string type, decimal price, string status) =>
        api.Put($"/api/rooms/{id}", new { id, number, type, monthlyPrice = price, status });

    public Task DeleteAsync(int id) => api.Delete($"/api/rooms/{id}");
}

public class TenantService(ApiClient api)
{
    public async Task<List<TenantDto>> ListAsync()
    {
        var raw = await api.Get<List<JsonElement>>("/api/tenants") ?? [];
        return raw.Select(t =>
        {
            var room = t.TryGetProperty("room", out var r) && r.ValueKind != JsonValueKind.Null
                ? r.GetProperty("number").GetString()! : "—";
            var tg = t.TryGetProperty("telegramChatId", out var c) && c.ValueKind != JsonValueKind.Null;
            return new TenantDto(
                t.GetProperty("id").GetInt32(),
                t.GetProperty("name").GetString()!,
                t.GetProperty("phone").GetString()!,
                room,
                t.GetProperty("moveInDate").GetString()!,
                tg);
        }).ToList();
    }

    public Task CreateAsync(string name, string phone, string moveIn) =>
        api.Post<bool>("/api/tenants", new { name, phone, moveInDate = moveIn });

    public Task DeleteAsync(int id) => api.Delete($"/api/tenants/{id}");

    public async Task<(int Imported, int Failed)> ImportAsync(Stream csv, string fileName)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(csv), "file", fileName);
        var res = await api.PostRaw("/api/tenants/import", content);
        res.EnsureSuccessStatusCode();
        var doc = await res.Content.ReadFromJsonAsync<JsonElement>();
        return (doc.GetProperty("imported").GetInt32(), doc.GetProperty("failed").GetInt32());
    }
}

public class BillingService(ApiClient api)
{
    public async Task<List<BillRow>> ListAsync(string status = "")
    {
        var q = string.IsNullOrEmpty(status) ? "/api/bills" : $"/api/bills?status={status}";
        var raw = await api.Get<List<JsonElement>>(q) ?? [];
        return raw.Select(b => new BillRow(
            b.GetProperty("id").GetInt32(),
            b.GetProperty("tenant").GetString()!,
            b.GetProperty("period").GetString()!,
            Format.Rp(b.GetProperty("amount").GetDecimal()),
            b.GetProperty("dueDate").GetString()!,
            b.GetProperty("status").GetString()!)).ToList();
    }

    public async Task<int> GenerateAsync(string periode)
    {
        var doc = await api.Post<JsonElement>($"/api/bills/generate?periode={periode}", new { });
        return doc.GetProperty("generated").GetInt32();
    }

    public async Task<byte[]> ReceiptAsync(int id)
    {
        var res = await api.GetRaw($"/api/bills/{id}/receipt.pdf");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsByteArrayAsync();
    }
}

public class PaymentService(ApiClient api)
{
    public Task PayAsync(int billId) =>
        api.Post<bool>("/api/payments", new { billId, method = "transfer" });

    public Task DecideAsync(int id, bool approve) =>
        api.Post<bool>($"/api/payments/{id}/verify", new { approve });

    public async Task<List<PayRow>> QueueAsync()
    {
        var raw = await api.Get<List<JsonElement>>("/api/payments/queue") ?? [];
        return raw.Select(p => new PayRow(
            p.GetProperty("id").GetInt32(),
            p.GetProperty("bill").GetProperty("tenant").GetProperty("name").GetString()!,
            p.GetProperty("method").GetString()! + " · " + p.GetProperty("createdAt").GetString()!
        )).ToList();
    }
}

public class DashboardService(ApiClient api)
{
    public async Task<DashboardDto> GetAsync()
    {
        var d = await api.Get<JsonElement>("/api/dashboard");
        return new DashboardDto(
            $"{d.GetProperty("occupancy").GetProperty("filled").GetInt32()}/{d.GetProperty("occupancy").GetProperty("total").GetInt32()}",
            Format.Rp(d.GetProperty("kas").GetDecimal()),
            Format.Rp(d.GetProperty("tunggakan").GetDecimal()),
            d.GetProperty("overdue").EnumerateArray().Select(o => new OverdueDto(
                o.GetProperty("tenant").GetString()!,
                Format.Rp(o.GetProperty("amount").GetDecimal()),
                o.GetProperty("dueDate").GetString()!,
                o.GetProperty("daysLate").GetInt32())).ToList(),
            d.GetProperty("reminders").GetInt32());
    }

    public async Task<string> TestSendAsync(string chatId)
    {
        var r = await api.Post<JsonElement>("/api/notify/test", new { chatId });
        return r.GetProperty("channel").GetString()!;
    }

    public async Task<byte[]> ReportCsvAsync()
    {
        var res = await api.GetRaw("/api/dashboard/report.csv");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsByteArrayAsync();
    }

    public async Task<List<TrendPoint>> TrendAsync()
    {
        var raw = await api.Get<List<JsonElement>>("/api/dashboard/trend") ?? [];
        return raw.Select(t => new TrendPoint(
            t.GetProperty("period").GetString()!,
            t.GetProperty("kas").GetDecimal(),
            t.GetProperty("tunggakan").GetDecimal())).ToList();
    }
}
