# Dart Cup Pro - Backend API

> **REST API Backend for Dart Cup Pro** - Bygget med NHost + .NET 8 + PostgreSQL

---

## 📌 Projekt Struktur

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

## 🚀 Hurtig Start (Lokal Udvikling)

### Forudsætninger
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started)

### 1. Klon Repository
```bash
git clone https://github.com/Svbramer/dartcup-backend.git
cd dartcup-backend
```

### 2. Start Databasen
```bash
docker-compose up -d postgres redis
```

### 3. Byg og Kør Backend
```bash
cd backend
dotnet restore
dotnet build
dotnet run
```

### 4. Test API'er
Backend kører på: `http://localhost:5000`

#### Test Auth API
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

## 🌐 Deploy til NHost.io

### 1. Opret NHost Projekt
1. Gå til [https://app.nhost.io](https://app.nhost.io)
2. Klik "New Project"
3. Vælg region (f.eks. `eu-central-1`)
4. Vælg Free tier
5. Klik "Create Project"

### 2. Forbind GitHub Repository
1. Gå til "Deployments" → "Connect GitHub"
2. Vælg dit repo (`dartcup-backend`)
3. Vælg branch (`main`)
4. Klik "Deploy"

### 3. Konfigurer Miljøvariable
Gå til "Settings" → "Environment Variables" og tilføj:
```bash
NHOST_JWT_SECRET=DIN_STÆRKE_NØGLE_1234567890
NHOST_JWT_EXPIRY=3600
```

---

## 📡 API Endpoints

### Authentication
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/sign-up` | ❌ | Opret bruger |
| POST | `/api/auth/sign-in` | ❌ | Log ind |
| GET | `/api/auth/me` | ✅ | Hent aktuelt login |

### Users
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/users/me` | ✅ | Hent min profil |
| PUT | `/api/users/me` | ✅ | Opdater min profil |

### Players
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/players/me` | ✅ | Hent mine spillere |
| POST | `/api/players` | ✅ | Opret spiller |

### Matches
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/matches/me` | ✅ | Hent mine kampe |
| POST | `/api/matches` | ✅ | Opret kamp |

### Leagues
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/leagues/me` | ✅ | Hent mine ligaer |
| POST | `/api/leagues` | ✅ | Opret liga |
| GET | `/api/leagues/{id}/standings` | ✅ | Hent liga placeringer |

---

## 🔐 Authentication Flow (Frontend Integration)

### JavaScript Example
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
  localStorage.setItem("accessToken", data.accessToken);
  localStorage.setItem("user", JSON.stringify(data.user));
  return data;
}
```

---

**Bygget med ❤️ af Vibe Code**
