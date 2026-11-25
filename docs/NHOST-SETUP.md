# NHost setup — Dart Cup Pro (kopier-venlig guide)

Mål: hele online-spillet kører på NHost, så webhotellet kun skal vise statiske filer.

Dit NHost-projekt: subdomain `adqpgvjcesrgxxlbxjme`, region `eu-north-1`
- Auth: `https://adqpgvjcesrgxxlbxjme.auth.eu-north-1.nhost.run/v1`
- GraphQL: `https://adqpgvjcesrgxxlbxjme.graphql.eu-north-1.nhost.run/v1` (POST — bemærk: IKKE `/v1/graphql`, det giver 404)

## Trin 1 — Kør SQL (kopier ind i NHost Console)
1. Log ind på https://app.nhost.io → dit projekt → i venstre menu klik **Database** → fanen **SQL**
2. Kopier hele indholdet af filen `db/online_rooms.sql` (ligger i dette repo) ind og kør den
3. Tjek at tabellerne kom: `Rooms`, `RoomSessions`, `ScoreEntries`, `Signals`

## Trin 2 — Track tabeller + giv rettigheder (i den nye NHost-konsol ligger det under Database, IKKE GraphQL→Data)
1. Venstre menu → **Database** → fanen **Tables**
2. Find hver af de 4 tabeller (`Rooms`, `RoomSessions`, `ScoreEntries`, `Signals`):
   - Klik på **⋯ (tre punktummer)** ud for tabellen → vælg **Track** (så den bliver tilgængelig i GraphQL)
   - Klik igen på **⋯** → **Edit Permissions** → tilføj role **`user`** → sæt flue i **select, insert, update** (delete ikke nødvendig) → gem
   - **OBS for `Signals`:** ved Insert skal du fjerne fluen ved kolonnen **`Id`** (den er auto-tæller og ikke insertable — ellers får du fejlen "Column Id is not insertable")
   - Dette gør at alle indloggede brugere må læse/skrive i rum-tabellerne

## Trin 3 — Auth-indstillinger (i den nye NHost-konsol)
1. Venstre menu → **Auth → Settings → Sign-In Methods → Email and Password**: sørg for at **"Require Verified Emails" er slået TIL** (det er standard — så får signup ingen session med det samme, og login.html viser "bekræftelses-mail sendt")
2. **Auth → SMTP** (egen fane under Auth): udfyld SMTP-serveren fra din mail-udbyder, f.eks. Brevo gratis-plan:
   - Host: `smtp-relay.brevo.com` · Port: `587` · Brugernavn: din Brevo-konto-mail · Adgangskode: en SMTP API-nøgle fra Brevo
   - Alternativ: Gmail med app-kode — Host `smtp.gmail.com`, Port `587`, brugernavn = gmail-adresse, password = app password (myaccount.google.com/apppasswords). Max ~100 mails/dag
   - Bemærk: mail.com kræver Premium for SMTP — gratis konti virker ikke
   - Uden SMTP kan bekræftelsesmails ikke sendes

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
