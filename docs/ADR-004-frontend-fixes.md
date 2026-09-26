# ADR-004: Perbaikan frontend pasca-MVP (providers, sidebar, filter)

Tanggal: 2026-09-22. Status: diterima. Melengkapi (bukan mengubah) ADR-003.

## 1. Provider Mud* sebagai interactive island

Gejala: dialog (`MudDialog`) tak terbuka dan snackbar tak muncul — tanpa error.
Penyebab: `MudPopoverProvider`/`MudDialogProvider`/`MudSnackbarProvider`
dirender statis di layout sehingga tak terhubung ke circuit interaktif.
Perbaikan: komponen `Providers.razor` (`@rendermode InteractiveServer`,
`prerender: false`) dipakai oleh `MainLayout` statis. Pelajaran: di Blazor
Server, semua yang merespons event harus hidup di island interaktif.

## 2. Sidebar rail custom ganti MudDrawer

Gejala: `MudDrawer Mini + Open=true` selalu expanded-overlay menutupi konten;
binding `Open` + toggle tak bereaksi (subscription event hilang).
Perbaikan: rail `div` 60px + expand 220px via `UiState` service + CSS flex
(`theme.css`), hamburger di AppBar. Plus offset `padding-top: 64px` karena
AppBar fixed menutupi atas konten (ditemukan dari screenshot).
Keputusan sadar: melepas komponen teruji demi determinisme — didokumentasikan,
bukan disembunyikan.

## 3. Filter Tagihan: chips ganti MudSelect

Gejala: `MudSelect` melebar penuh + teks terpilih ganda/berbayang
(pola `Value`+`ValueChanged` tak terkontrol penuh + Dense).
Perbaikan: 4 chips pill (Semua/Belum/Pending/Lunas) — cocok bahasa Synapse,
1 klik lebih sedikit, nol quirk. Diverifikasi manual (screenshot tiap filter);
belum ada test UI otomatis di repo ini.

## 4. Navigasi pasca-login: forceLoad

`Nav.NavigateTo(..., forceLoad: true)` agar circuit selalu fresh dan state
dibaca dari session storage (konsisten by construction). Lihat ADR-003
untuk konteks prerender vs session.

## 5. Troubleshooting frontend

Entri ini sebelumnya berada di runbook `-ops`; dipindah ke sini karena
semua masalah layer frontend, bukan operasi.

| Gejala | Penyebab | Fix |
|---|---|---|
| Login bounce `/login`→`/`→`/login` | guard + prerender tanpa JS | `prerender:false` + session storage + `forceLoad` |
| Drawer/layout tak reaktif | `@rendermode` di layout merusak MudBlazor | interactive islands (`UserMenu`, `AuthNav`) |
| MudBlazor JS 404 | content-root salah saat run DLL | workdir = folder publish |
| MudBlazor 9 vs net8 | v9 butuh net lebih baru | pin 8.13.0 |
| `MudDialog`/Snackbar tak muncul | provider Mud* statis di layout | island `Providers.razor` interaktif |
| MudDrawer selalu overlay | Mini + `Open=true` + tanpa subscription | rail custom + `UiState` + offset appbar 64px |
| MiniProfiler 404 | lupa `UseMiniProfiler()` / route | `RouteBasePath = "/profiler"` + middleware sebelum auth |
