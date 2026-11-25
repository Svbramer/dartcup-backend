-- Online rooms (fase 1) — schema til NHost Postgres.
-- Kør denne fil én gang mod din NHost-database (psql eller NHost Console -> SQL).
-- Bemærk: Hvis backend kører med EF EnsureCreated/Migrate, skal du IKKE køre denne fil
-- (så opretter EF selv tabellerne). Vælg én af delene.

CREATE TABLE IF NOT EXISTS "Rooms" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "Code" varchar(6) NOT NULL UNIQUE,
    "GameType" text NOT NULL DEFAULT '201',
    "Player1Id" uuid NOT NULL,
    "Player2Id" uuid,
    "Status" text NOT NULL DEFAULT 'waiting',          -- waiting | active | finished
    "CurrentTurn" integer NOT NULL DEFAULT 1,           -- 1 = Player1, 2 = Player2
    "BoardHolderSessionId" uuid,
    "BoardRequestFromSessionId" uuid,
    "CreatedAt" timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS "RoomSessions" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "RoomId" uuid NOT NULL REFERENCES "Rooms"("Id") ON DELETE CASCADE,
    "UserId" uuid NOT NULL,                              -- NHost auth user id (sub)
    "DeviceName" text NOT NULL,
    "Roles" text NOT NULL DEFAULT 'viewer',             -- csv: scorer,view-opponent,camera-board
    "JoinedAt" timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS "ScoreEntries" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "RoomId" uuid NOT NULL REFERENCES "Rooms"("Id") ON DELETE CASCADE,
    "PlayerId" uuid NOT NULL,
    "VisitNumber" integer NOT NULL,                     -- 1-3
    "Value" integer NOT NULL,
    "Status" text NOT NULL DEFAULT 'pending',            -- pending (3 sek. vindue) | confirmed
    "SubmittedAt" timestamptz NOT NULL DEFAULT now(),
    "ConfirmedAt" timestamptz
);

CREATE INDEX IF NOT EXISTS "IX_RoomSessions_RoomId" ON "RoomSessions"("RoomId");
CREATE INDEX IF NOT EXISTS "IX_ScoreEntries_RoomId" ON "ScoreEntries"("RoomId");

-- WebRTC-signaling mellem kamera- og seer-enheder (polling i stedet for websockets)
CREATE TABLE IF NOT EXISTS "Signals" (
    "Id" bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "RoomId" uuid NOT NULL REFERENCES "Rooms"("Id") ON DELETE CASCADE,
    "FromSession" uuid,
    "Type" text NOT NULL,              -- offer | answer | candidate | camera-ready
    "Payload" text NOT NULL,
    "CreatedAt" timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS "IX_Signals_RoomId_Id" ON "Signals"("RoomId", "Id");
