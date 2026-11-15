# NHost setup — Dart Cup Pro (kopier-venlig guide)

Mål: hele online-spillet kører på NHost, så webhotellet kun skal vise statiske filer.

Dit NHost-projekt: subdomain `adqpgvjcesrgxxlbxjme`, region `eu-north-1`
- Auth: `https://adqpgvjcesrgxxlbxjme.auth.eu-north-1.nhost.run/v1`
- GraphQL: `https://adqpgvjcesrgxxlbxjme.graphql.eu-north-1.nhost.run/v1/graphql`

## Trin 1 — Kør SQL (kopier ind i NHost Console)
1. Log ind på https://app.nhost.io → dit projekt → **Database → SQL**
2. Kopier hele indholdet af filen `db/online_rooms.sql` (ligger i dette repo) ind og kør den
3. Tjek at tabellerne kom: `Rooms`, `RoomSessions`, `ScoreEntries`, `Signals`

## Trin 2 — Track tabeller + giv rettigheder (klik, ikke kode)
1. **GraphQL → Data** i NHost Console
2. For hver af de 4 tabeller: klik "Track table"
3. For hver tabel: **Permissions → role `user`** → sæt flue i **select, insert, update** (delete ikke nødvendig)
   - Dette gør at alle indloggede brugere må læse/skrive i rum-tabellerne

## Trin 3 — Auth-indstillinger
1. **Authentication → Providers → Email**: slå **email-verification TIL**
2. **Authentication → SMTP**: tilslut en SMTP-udbyder (f.eks. Brevo gratis-plan), så bekræftelsesmails kan sendes

## Trin 4 — Frontend på webhotellet
Upload kun statiske filer fra `Svbramer/dartcup` (branch `vibe/online-nhost-frontend-4f4acf`):
`index.html`, `login.html`, `online.html`, `assets/`, ikonerne, `manifest.webmanifest`, `sw.js`

I `online.html` står NHost-URL'erne øverst (`NHOST_SUBDOMAIN`, `NHOST_REGION`) — de passer allerede til dit projekt.

## Trin 5 — Test
1. `login.html` på to enheder → opret bruger samme konto (bekræft mailen) → log ind
2. `online.html` på begge → "Opret kamp" → kode → "Deltag" på den anden
3. Vælg roller → spil 201 → tavle-indikatoren skifter med lyd → kamera viser modstanderens skive

## Bemærkninger
- Realtime kører som polling (hver 2. sek.) via GraphQL — virker overalt, også hvor websockets er lukket
- .NET-backend'en (`backend/`) er ikke nødvendig for denne løsning; behold den hvis du senere vil have liga/cup på serveren
