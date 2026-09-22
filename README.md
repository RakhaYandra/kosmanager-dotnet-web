# kosmanager-dotnet-web

Web KosManager — **Blazor Server + MudBlazor**, bahasa desain Synapse (light, Inter + Space Grotesk + JetBrains Mono, radius 8px, tombol pill). Mengonsumsi REST API via HttpClient + JWT (session storage, tahan refresh). Full API coverage: 17/17 endpoint terpanggil dari UI.

## Layar

/login (split hero + form) · /register (daftar penghuni) · / board kamar (+Tambah, edit/hapus per kartu) · /tenants (+Tambah, hapus, import CSV) · /bills (+generate, filter chips pill) · /verify (Setuju/Tolak) · /dashboard (4 kartu + tunggakan + test-kirim + Unduh CSV) · /my-bills (penghuni + SUDAH BAYAR)

## Quickstart

```bash
API_URL=http://localhost:8090 dotnet run --project KosManager.Web --urls http://localhost:5175
# buka http://localhost:5175/login — owner@kos.local / sinta@kos.local (fiktif)
```

## Arsitektur (CA-ish, lihat `docs/ADR-003-frontend.md`)

- `Models/` — record DTO per fitur + helper format `Rp`/`id-ID`. Nol `GetProperty`/HTTP mentah di `Components/` (diverifikasi via grep).
- `Services/` per fitur — `RoomService`, `TenantService`, `BillingService`, `PaymentService`, `DashboardService`; `ApiClient` tinggal infrastruktur HTTP + JWT + login/register; `Snack.Try` terpusat untuk error → snackbar.
- Auth state di ProtectedSessionStorage → tahan refresh; guard per-halaman di `OnInitializedAsync` + `forceLoad` saat login/logout.
- Layout statis + interactive islands (`UserMenu`, `AuthNav`, `Providers`) — pola Blazor Server yang benar (rendermode di layout merusak MudBlazor; provider Mud* statis tak bisa render dialog/snackbar).
- Sidebar rail custom 60px + toggle hamburger via `UiState` (pengganti MudDrawer yang selalu overlay); offset appbar 64px di `theme.css`.
- Prerender dimatikan di halaman interaktif (guard butuh JS/session).
- Filter status Tagihan = chips pill (MudSelect `Value`+`ValueChanged` glitch: melebar penuh + teks ganda).
- Download CSV via JS interop (`kmDownload` di `App.razor`); upload CSV via `InputFile` (maks 2MB).
