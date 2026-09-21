# kosmanager-dotnet-web

Web KosManager — **Blazor Server + MudBlazor**, bahasa desain Synapse (light, Inter + Space Grotesk + JetBrains Mono, radius 8px, tombol pill). Mengonsumsi REST API via HttpClient + JWT (session storage, tahan refresh).

## Layar

/login (split hero + form) · / board kamar · /tenants · /bills (+generate) · /verify (Setuju/Tolak) · /dashboard (4 kartu + tunggakan + test-kirim) · /my-bills (penghuni + SUDAH BAYAR)

## Quickstart

```bash
API_URL=http://localhost:8090 dotnet run --project KosManager.Web --urls http://localhost:5175
# buka http://localhost:5175/login — owner@kos.local / sinta@kos.local (fiktif)
```

## Catatan arsitektur

- Auth state di ProtectedSessionStorage → tahan refresh; guard per-halaman di `OnInitializedAsync`.
- Layout statis + interactive islands (`UserMenu`, `AuthNav`) — pola Blazor Server yang benar (rendermode di layout merusak MudBlazor).
- Prerender dimatikan di halaman interaktif (guard butuh JS/session).
