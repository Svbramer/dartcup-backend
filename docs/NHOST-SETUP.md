# NHost setup — Dart Cup Pro (trin for trin)

Projektet kører på NHost (subdomain `adqpgvjcesrgxxlbxjme`, region `eu-north-1`).
Auth-URL som frontend bruger: `https://adqpgvjcesrgxxlbxjme.auth.eu-north-1.nhost.run/v1`

## 1. Bekræft at projektet er vågent
- `https://adqpgvjcesrgxxlbxjme.auth.eu-north-1.nhost.run/v1/healthz` → skal svare `"OK"`
- `https://adqpgvjcesrgxxlbxjme.auth.eu-north-1.nhost.run/v1/version` → viser auth-version

## 2. Auth-indstillinger (NHost Console)
1. Log ind på https://app.nhost.io → vælg projektet
2. **Authentication → Providers → Email**: sørg for at **email-verification er slået til**
   (så signup returnerer ingen session med det samme, og login.html viser "bekræftelses-mail sendt" — det matcher PR #5 i `Svbramer/dartcup`)
3. **Authentication → SMTP**: tilslut en SMTP-udbyder (eller brug NHosts test-mail i udvikling), ellers kan bekræftelsesmails ikke sendes

## 3. Database (Postgres på NHost)
Hent connection string i NHost Console: **Database → Connect → Connection string**.

Sæt den i backend via miljøvariabel (lokalt eller på server):
```
ConnectionStrings__DefaultConnection=<connection string fra NHost>
```

### Opret tabellerne til online rooms
Vælg én af:
- **A (anbefalet):** Lad EF lave skemaet — backend kører `EnsureCreated()`/`Migrate()` automatisk ved startup (se `Program.cs`). Så skal du ikke køre SQL selv.
- **B:** Kør `nhost/sql/online_rooms.sql` manuelt i NHost Console (SQL editor) eller via psql.

## 4. Kør .NET-backend et sted med .NET 8
NHost hoster ikke selv .NET-kode, så backend skal køre et sted der kan køre .NET 8 + Docker:
- **Lokalt til test:** `docker-compose up -d` er kun til lokal Postgres; i stedet sæt connection string til NHost (punkt 3) og kør `cd backend && dotnet run`
- **Produktion:** en lille VPS (f.eks. Hetzner) eller Azure/Railway/Render med Docker — peg `ConnectionStrings__DefaultConnection` på NHost Postgres

## 5. Frontend (`Svbramer/dartcup`)
- `login.html` peger allerede på NHost auth ✅
- `online.html`: ret `var API_BASE = "..."` øverst til den URL hvor .NET-backend kører (f.eks. `https://api.ditdomæne.dk`)
- Upload de statiske filer (index.html, login.html, online.html, assets/, ikoner) til dit webhotel

## 6. Test end-to-end
1. Åbn `login.html` på to enheder → opret/log ind med samme konto
2. `online.html` på begge → "Opret kamp" giver kode → "Deltag" med koden på den anden
3. Vælg roller (kamera / score+se) → spil 201 → tavle-indikatoren skifter grøn/grå med lyd
