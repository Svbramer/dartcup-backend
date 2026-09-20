# Dart Cup Pro - Backend API

> **REST API Backend for Dart Cup Pro** - Bygget med NHost + .NET 8 + PostgreSQL

---

## 📌 **Projekt Struktur**

```
dartcup-backend/
├── backend/                    # .NET 8 Backend
│   ├── Controllers/           # REST API Controllers
│   │   ├── AuthController.cs   # Login/Registrering
│   │   ├── UsersController.cs  # Bruger administration
│   │   ├── PlayersController.cs # Spiller administration
│   │   ├── MatchesController.cs # Kampe
│   │   ├── LeaguesController.cs # Ligaer
│   │   └── TrainingImagesController.cs # Træningsbilleder
│   ├── Services/              # Forretningslogik
│   │   ├── IUserService.cs    # Bruger service
│   │   ├── IPlayerService.cs  # Spiller service
│   │   ├── IMatchService.cs   # Kamp service
│   │   ├── ILeagueService.cs  # Liga service
│   │   └── IAuthService.cs    # Authentication service
│   ├── Models/                # Database modeller
│   │   ├── User.cs            # Bruger model
│   │   ├── Player.cs          # Spiller model
│   │   ├── Match.cs           # Kamp model
│   │   ├── League.cs          # Liga model
│   │   └── TrainingImage.cs   # Træningsbillede model
│   ├── Database/              # Database konfiguration
│   │   └── DbContext.cs       # Entity Framework DbContext
│   ├── Program.cs             # Startup konfiguration
│   ├── DartCupBackend.csproj  # .NET Projekt fil
│   ├── appsettings.json       # Konfiguration
│   └── Dockerfile             # Docker build
├── docker-compose.yml        # Docker opsætning
├── nhost.config.json         # NHost konfiguration
└── README.md                 # Denne fil
```

---

## 🚀 **Hurtig Start (Lokal Udvikling)**

### **Forudsætninger**
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started)
- [Git](https://git-scm.com/downloads)

### **1. Klon Repository**
```bash
git clone <dit-repo-url>
cd dartcup-backend
```

### **2. Start Databasen (PostgreSQL)**
```bash
# Start PostgreSQL og Redis via Docker
docker-compose up -d postgres redis
```

### **3. Konfigurer Database**
```bash
# Vent 5-10 sekunder, så databasen er klar
# Test forbindelsen
psql -h localhost -U postgres -d dartcup
# Adgangskode: postgres
```

### **4. Byg og Kør Backend**
```bash
# Gå til backend mappen
cd backend

# Installer dependencies
dotnet restore

# Byg projektet
dotnet build

# Start backend (port 5000)
dotnet run
```

### **5. Test API'er**
Backend kører på: `http://localhost:5000`

#### **Test Auth API**
```bash
# Opret bruger
curl -X POST http://localhost:5000/api/auth/sign-up \
  -H "Content-Type: application/json" \
  -d '{"email": "test@test.com", "password": "12345678", "firstName": "Test", "lastName": "User"}'

# Log ind
curl -X POST http://localhost:5000/api/auth/sign-in \
  -H "Content-Type: application/json" \
  -d '{"email": "test@test.com", "password": "12345678"}'
```

---

## 🌐 **Deploy til NHost.io**

### **1. Opret NHost Projekt**
1. Gå til [https://app.nhost.io](https://app.nhost.io)
2. Klik **"New Project"**
3. Vælg **region** (f.eks. `eu-central-1`)
4. Vælg **Free tier**
5. Klik **"Create Project"**

### **2. Forbind GitHub Repository**
1. Gå til **"Deployments"** → **"Connect GitHub"**
2. Autoriser NHost til at tilgå dit GitHub-repo
3. Vælg **dit repo** (`dartcup-backend`)
4. Vælg **branch** (f.eks. `main`)
5. Klik **"Deploy"**

### **3. Konfigurer Miljøvariable**
Gå til **"Settings"** → **"Environment Variables"** og tilføj:

```bash
# Database
NHOST_POSTGRES_HOST=<host>
NHOST_POSTGRES_PORT=5432
NHOST_POSTGRES_USER=postgres
NHOST_POSTGRES_PASSWORD=<adgangskode>
NHOST_POSTGRES_DATABASE=dartcup

# JWT
NHOST_JWT_SECRET=DIN_STÆRKE_NØGLE_1234567890
NHOST_JWT_EXPIRY=3600

# Google OAuth (valgfrit)
NHOST_GOOGLE_CLIENT_ID=DIN_CLIENT_ID
NHOST_GOOGLE_CLIENT_SECRET=DIN_CLIENT_SECRET
```

### **4. Deploy**
1. Klik **"Deploy"** i NHost Dashboard
2. Vent 2-5 minutter
3. Dit API kører nu på: `https://<dit-projekt>.nhost.run`

---

## 📡 **API Endpoints**

### **Authentication**
| Method | Endpoint | Beskrivelse | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/sign-up` | Opret bruger | ❌ |
| POST | `/api/auth/sign-in` | Log ind | ❌ |
| GET | `/api/auth/me` | Hent aktuelt login | ✅ |
| POST | `/api/auth/refresh` | Refresh token | ✅ |
| POST | `/api/auth/google` | Google login | ❌ |

### **Users**
| Method | Endpoint | Beskrivelse | Auth |
|--------|----------|-------------|------|
| GET | `/api/users/me` | Hent min profil | ✅ |
| PUT | `/api/users/me` | Opdater min profil | ✅ |
| GET | `/api/users/{id}` | Hent bruger | ✅ |
| GET | `/api/users/me/stats` | Hent mine statistikker | ✅ |

### **Players**
| Method | Endpoint | Beskrivelse | Auth |
|--------|----------|-------------|------|
| GET | `/api/players` | Hent alle spillere | ✅ |
| GET | `/api/players/me` | Hent mine spillere | ✅ |
| POST | `/api/players` | Opret spiller | ✅ |
| PUT | `/api/players/{id}` | Opdater spiller | ✅ |
| DELETE | `/api/players/{id}` | Slet spiller | ✅ |

### **Matches**
| Method | Endpoint | Beskrivelse | Auth |
|--------|----------|-------------|------|
| GET | `/api/matches` | Hent seneste kampe | ✅ |
| GET | `/api/matches/me` | Hent mine kampe | ✅ |
| GET | `/api/matches/{id}` | Hent kamp | ✅ |
| POST | `/api/matches` | Opret kamp | ✅ |
| PUT | `/api/matches/{id}` | Opdater kamp | ✅ |
| DELETE | `/api/matches/{id}` | Slet kamp | ✅ |
| GET | `/api/matches/league/{leagueId}` | Hent kampe i liga | ✅ |
| GET | `/api/matches/player/{playerId}` | Hent kampe for spiller | ✅ |

### **Leagues**
| Method | Endpoint | Beskrivelse | Auth |
|--------|----------|-------------|------|
| GET | `/api/leagues` | Hent alle ligaer | ✅ |
| GET | `/api/leagues?activeOnly=true` | Hent aktive ligaer | ✅ |
| GET | `/api/leagues/me` | Hent mine ligaer | ✅ |
| GET | `/api/leagues/{id}` | Hent liga | ✅ |
| POST | `/api/leagues` | Opret liga | ✅ |
| PUT | `/api/leagues/{id}` | Opdater liga | ✅ |
| DELETE | `/api/leagues/{id}` | Slet liga | ✅ |
| GET | `/api/leagues/{id}/standings` | Hent liga stillinger | ✅ |
| GET | `/api/leagues/{id}/standings/me` | Hent min stilling | ✅ |

### **Training Images**
| Method | Endpoint | Beskrivelse | Auth |
|--------|----------|-------------|------|
| GET | `/api/trainingimages/pro` | Hent Pro træningsbilleder | ✅ (Pro) |
| GET | `/api/trainingimages/public` | Hent offentlige træningsbilleder | ❌ |
| POST | `/api/trainingimages` | Upload træningsbillede | ✅ |

---

## 🔐 **Authentication Flow (Frontend Integration)**

### **1. Login Flow (JavaScript/React)**
```javascript
// Sign Up
async function signUp(email, password, firstName, lastName) {
  const response = await fetch("https://<dit-projekt>.nhost.run/api/auth/sign-up", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password, firstName, lastName })
  });
  return await response.json();
}

// Sign In
async function signIn(email, password) {
  const response = await fetch("https://<dit-projekt>.nhost.run/api/auth/sign-in", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password })
  });
  const data = await response.json();
  
  // Gem token
  localStorage.setItem("accessToken", data.accessToken);
  localStorage.setItem("user", JSON.stringify(data.user));
  
  return data;
}

// Hent aktuelt login
async function getCurrentUser() {
  const token = localStorage.getItem("accessToken");
  const response = await fetch("https://<dit-projekt>.nhost.run/api/auth/me", {
    headers: { 
      "Authorization": `Bearer ${token}` 
    }
  });
  return await response.json();
}

// Log ud
function signOut() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("user");
}
```

### **2. Beskyttede Routes**
```javascript
// Middleware til at tjekke login
function ProtectedRoute({ children }) {
  const token = localStorage.getItem("accessToken");
  const user = JSON.parse(localStorage.getItem("user") || "{}");
  
  if (!token) {
    window.location.href = "/login";
    return null;
  }
  
  // Tjek om brugeren er Pro (for Pro-indhold)
  if (children.props?.proOnly && !user?.isPro) {
    window.location.href = "/upgrade";
    return null;
  }
  
  return children;
}

// Brug i din app
<ProtectedRoute>
  <Dashboard />
</ProtectedRoute>

<ProtectedRoute proOnly>
  <ProTrainingImages />
</ProtectedRoute>
```

---

## 🗃️ **Database Struktur**

### **Users**
| Field | Type | Beskrivelse |
|-------|------|-------------|
| Id | UUID | Primær nøgle |
| Email | String | Email (unique) |
| PasswordHash | String | Password hash |
| FirstName | String | Fornavn |
| LastName | String | Efternavn |
| ProfileImageUrl | String | Profilbillede URL |
| CreatedAt | DateTime | Oprettelsesdato |
| LastLogin | DateTime | Sidste login |
| IsPro | Boolean | Pro-status |
| GoogleId | String | Google OAuth ID |
| FacebookId | String | Facebook OAuth ID |

### **Players**
| Field | Type | Beskrivelse |
|-------|------|-------------|
| Id | UUID | Primær nøgle |
| UserId | UUID | Fremmednøgle → Users |
| Name | String | Spillernavn |
| AvatarUrl | String | Avatar URL |
| CreatedAt | DateTime | Oprettelsesdato |

### **Matches**
| Field | Type | Beskrivelse |
|-------|------|-------------|
| Id | UUID | Primær nøgle |
| Player1Id | UUID | Fremmednøgle → Players |
| Player2Id | UUID | Fremmednøgle → Players |
| WinnerId | UUID | Fremmednøgle → Players |
| ScorePlayer1 | Integer | Score for spiller 1 |
| ScorePlayer2 | Integer | Score for spiller 2 |
| Date | DateTime | Kampdato |
| LeagueId | UUID | Fremmednøgle → Leagues |
| GameType | String | Spiltype (501, 301, Cricket, etc.) |
| IsCompleted | Boolean | Er kampen færdig? |
| Notes | String | Noter |

### **Leagues**
| Field | Type | Beskrivelse |
|-------|------|-------------|
| Id | UUID | Primær nøgle |
| Name | String | Liganavn |
| OwnerId | UUID | Fremmednøgle → Users |
| StartDate | DateTime | Startdato |
| EndDate | DateTime | Slutdato |
| IsActive | Boolean | Er ligaen aktiv? |
| Description | String | Beskrivelse |

### **LeagueStandings**
| Field | Type | Beskrivelse |
|-------|------|-------------|
| Id | UUID | Primær nøgle |
| LeagueId | UUID | Fremmednøgle → Leagues |
| PlayerId | UUID | Fremmednøgle → Players |
| Position | Integer | Placering |
| Points | Integer | Point |
| Wins | Integer | Antal sejre |
| Losses | Integer | Antal nederlag |
| WeeklyRank | Integer | Ugentlig placering |
| MonthlyRank | Integer | Månedlig placering |
| UpdatedAt | DateTime | Sidste opdatering |

### **TrainingImages**
| Field | Type | Beskrivelse |
|-------|------|-------------|
| Id | UUID | Primær nøgle |
| Title | String | Titel |
| ImageUrl | String | Billed-URL |
| Description | String | Beskrivelse |
| CreatedAt | DateTime | Oprettelsesdato |
| IsProOnly | Boolean | Kun for Pro-brugere |
| UserId | UUID | Fremmednøgle → Users |
| HighScore | Integer | High score (valgfrit) |
| GameType | String | Spiltype (valgfrit) |
| AchievedAt | DateTime | Dato for præstation |

---

## 🔧 **Konfiguration**

### **JWT Indstillinger**
```json
{
  "Jwt": {
    "Secret": "DIN_STÆRKE_NØGLE_1234567890",
    "Issuer": "dartcup-backend",
    "Audience": "dartcup-backend",
    "Expiry": 3600
  }
}
```

> ⚠️ **VIGTIGT**: Skift `Jwt.Secret` til en **lang, tilfældig streng** (f.eks. genereret med `openssl rand -base64 32`)

### **Google OAuth**
1. Gå til [Google Cloud Console](https://console.cloud.google.com/)
2. Opret et nyt projekt
3. Gå til **APIs & Services** → **Credentials**
4. Klik **Create Credentials** → **OAuth Client ID**
5. Vælg **Web Application**
6. Tilføj **Authorized JavaScript Origins**:
   - `http://localhost:3000`
   - `http://localhost:5173`
   - `https://beta.dartcup.eu`
   - `https://dartcup.eu`
7. Tilføj **Authorized Redirect URIs**:
   - `https://<dit-projekt>.nhost.run/auth/callback/google`
8. Kopier **Client ID** og **Client Secret** til `appsettings.json`

### **Facebook OAuth**
1. Gå til [Facebook Developers](https://developers.facebook.com/)
2. Opret en ny app
3. Gå til **Settings** → **Basic**
4. Tilføj **Valid OAuth Redirect URIs**:
   - `https://<dit-projekt>.nhost.run/auth/callback/facebook`
5. Kopier **App ID** og **App Secret** til `appsettings.json`

---

## ⚡ **Realtime Updates (SignalR)**

Backend understøtter **realtime updates** via SignalR:

```javascript
// Forbind til SignalR Hub
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://<dit-projekt>.nhost.run/hub")
  .build();

// Lyt til match updates
connection.on("MatchUpdated", (data) => {
  console.log("Match updated:", data);
});

// Lyt til league updates
connection.on("LeagueUpdated", (data) => {
  console.log("League updated:", data);
});

// Lyt til chat beskeder
connection.on("ChatMessage", (data) => {
  console.log("New chat message:", data);
});

// Start forbindelsen
connection.start().catch(err => console.error(err));

// Send match update
connection.invoke("SendMatchUpdate", matchId, "Match completed");

// Send chat besked
connection.invoke("SendChatMessage", userId, userName, "Hello!");
```

---

## 📦 **Stripe Integration (Pro Betaling)**

> **Kommer snart** - Stripe integration for at håndtere Pro-betalinger

---

## 🤝 **Frontend Integration**

### **1. Miljøvariable (Frontend)**
```javascript
// I din frontend (React/Vite)
const API_BASE_URL = import.meta.env.VITE_API_URL || "http://localhost:5000";
```

### **2. HTTP Client (Axios)**
```javascript
import axios from "axios";

const api = axios.create({
  baseURL: API_BASE_URL,
});

// Tilføj token til alle requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
```

### **3. API Service (Eksempel)**
```javascript
import api from "./api";

export const authService = {
  signUp: (data) => api.post("/api/auth/sign-up", data),
  signIn: (data) => api.post("/api/auth/sign-in", data),
  getCurrentUser: () => api.get("/api/auth/me"),
};

export const userService = {
  getMe: () => api.get("/api/users/me"),
  updateMe: (data) => api.put("/api/users/me", data),
  getStats: () => api.get("/api/users/me/stats"),
};

export const playerService = {
  getAll: () => api.get("/api/players"),
  getMine: () => api.get("/api/players/me"),
  create: (data) => api.post("/api/players", data),
  update: (id, data) => api.put(`/api/players/${id}`, data),
  delete: (id) => api.delete(`/api/players/${id}`),
};

export const matchService = {
  getAll: (count) => api.get(`/api/matches?count=${count}`),
  getMine: () => api.get("/api/matches/me"),
  getById: (id) => api.get(`/api/matches/${id}`),
  create: (data) => api.post("/api/matches", data),
  update: (id, data) => api.put(`/api/matches/${id}`, data),
  delete: (id) => api.delete(`/api/matches/${id}`),
  getByLeague: (leagueId) => api.get(`/api/matches/league/${leagueId}`),
  getByPlayer: (playerId) => api.get(`/api/matches/player/${playerId}`),
};

export const leagueService = {
  getAll: (activeOnly) => api.get(`/api/leagues?activeOnly=${activeOnly}`),
  getMine: () => api.get("/api/leagues/me"),
  getById: (id) => api.get(`/api/leagues/${id}`),
  create: (data) => api.post("/api/leagues", data),
  update: (id, data) => api.put(`/api/leagues/${id}`, data),
  delete: (id) => api.delete(`/api/leagues/${id}`),
  getStandings: (leagueId) => api.get(`/api/leagues/${leagueId}/standings`),
  getMyStanding: (leagueId) => api.get(`/api/leagues/${leagueId}/standings/me`),
};
```

---

## 🐛 **Fejlfinding**

### **Problemer med Database**
- **Fejl**: `Connection refused` → Tjek om PostgreSQL kører (`docker ps`)
- **Fejl**: `Authentication failed` → Tjek credentials i `appsettings.json`
- **Fejl**: `Database does not exist` → Opret databasen: `createdb dartcup`

### **Problemer med Authentication**
- **Fejl**: `Invalid token` → Tjek JWT-secret i `appsettings.json`
- **Fejl**: `User not found` → Tjek om brugeren findes i databasen
- **Fejl**: `Password incorrect` → Tjek password-hash

### **Problemer med Docker**
- **Fejl**: `Port already in use` → Stop den eksisterende container: `docker stop dartcup-postgres`
- **Fejl**: `Image not found` → Træk image: `docker pull postgres:15-alpine`

---

## 📞 **Support**

- **NHost Dokumentation**: [https://docs.nhost.io/](https://docs.nhost.io/)
- **NHost Discord**: [https://discord.gg/5VqcMvR](https://discord.gg/5VqcMvR)
- **NHost GitHub**: [https://github.com/nhost/nhost](https://github.com/nhost/nhost)

---

## 🎉 **Næste Skridt**

1. **Integrer frontend** med backend API'er
2. **Deploy til NHost.io**
3. **Konfigurer Google/Facebook OAuth**
4. **Tilføj Stripe integration** for Pro-betaling
5. **Implementer realtime updates** med SignalR
6. **Tilføj mere funktioner** (chat, notifications, etc.)

---

**Bygget med ❤️ af Vibe Code**
