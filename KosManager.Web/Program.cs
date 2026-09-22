using KosManager.Web.Components;
using KosManager.Web.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddHttpClient("api", c =>
    c.BaseAddress = new Uri(builder.Configuration["API_URL"] ?? "http://localhost:8090"));
builder.Services.AddScoped<AuthState>();
builder.Services.AddScoped<UiState>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<TenantService>();
builder.Services.AddScoped<BillingService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<DashboardService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
