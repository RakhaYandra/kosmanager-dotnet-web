# ADR-003: Frontend Clean Architecture-ish (DTO + feature services)

Tanggal: 2026-09-22. Status: diterima.

## Konteks

Page Blazor memanggil `ApiClient` langsung dan mem-parsing `JsonElement`
(`GetProperty`) inline di `@code`. Duplikasi mapping di 6 page, try/catch
per tombol, sulit di-unit-test.

## Keputusan

- `Models/` — record DTO per fitur (`RoomDto`, `TenantDto`, `BillRow`,
  `PayRow`, `DashboardDto`, `OverdueDto`) + helper format `Rp`/`id-ID`.
- `Services/` per fitur — `RoomService`, `TenantService`, `BillingService`,
  `PaymentService`, `DashboardService`: panggil `ApiClient`, return DTO.
- `ApiClient` turun jadi infrastruktur murni (HTTP + JWT + `LoginAsync`).
- `Snack.Try` terpusat untuk error → snackbar.
- Page tinggal render + event. `Login.razor` tetap memakai `LoginAsync`
  langsung (auth concern, bukan data fitur).
- Nol `GetProperty` / HTTP mentah di `Components/` (diverifikasi via grep).

## Konsekuensi

- Verifikasi: build hijau, 7 rute diklik ulang, screenshot identik baseline.
- Tech debt tercatat: belum ada unit test (bUnit) untuk service;
  layout statis + interactive islands dipertahankan (rendermode di layout
  merusak MudBlazor — lihat riwayat debug).
