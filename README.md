# 🏥 Doctor Booking App

<div align="center">

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![EF Core](https://img.shields.io/badge/EF_Core-10.0-purple?style=for-the-badge&logo=microsoft)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?style=for-the-badge&logo=microsoftsqlserver)
![JWT](https://img.shields.io/badge/JWT-Auth-000000?style=for-the-badge&logo=jsonwebtokens)
![Swagger](https://img.shields.io/badge/Swagger-UI-85EA2D?style=for-the-badge&logo=swagger)
![License](https://img.shields.io/badge/License-Educational-blue?style=for-the-badge)

**A production-ready REST API for a complete Doctor Booking & Appointment Management Platform.**  
Built with Clean Architecture, ASP.NET Core (.NET 10), EF Core, and JWT Bearer authentication.

[📖 API Docs (Swagger)](#️-getting-started) • [🔐 Auth Flow](#-authentication-jwt) • [📦 Endpoints](#-api-endpoints) • [🚀 Quick Start](#️-getting-started)

</div>

---

## 📌 Overview

**Doctor Booking App** is a fully-featured healthcare appointment platform API that enables:

- 🧑‍⚕️ **Patients** to register, login, book appointments with doctors, chat, pay, and review
- 👨‍⚕️ **Doctors** to manage availability slots, respond to reviews, and handle bookings
- 🛡️ **Admins** to manage the entire platform — users, doctors, clinics, announcements, FAQs, and system logs
- 🔐 Secure **JWT-based authentication** with access token + refresh token rotation
- 📄 Full **Swagger UI** with Bearer token support for easy API exploration

---

## 🚀 Tech Stack

| Layer | Technology | Version |
|---|---|---|
| Framework | ASP.NET Core Web API | .NET 10 |
| ORM | Entity Framework Core | 10.0.9 |
| Database | SQL Server | 2022 / LocalDB |
| Authentication | JWT Bearer Tokens | System.IdentityModel.Tokens.Jwt 8.x |
| Password Hashing | BCrypt.Net-Next | 4.0.3 |
| API Docs | Swashbuckle / Swagger UI | 6.9.0 |
| Architecture | Clean Architecture | Domain / Application / Infrastructure / API |

---

## 🗂️ Project Structure

```
DoctorBookingApp/
│
├── Domain/                              # 🏛️ Core — entities, no external dependencies
│   └── Models/
│       ├── patient.cs                   # Patient accounts → maps to [users] table
│       ├── admin.cs                     # Admin accounts
│       ├── doctor.cs                    # Doctor profiles
│       ├── clinic.cs                    # Clinic information
│       ├── specialty.cs                 # Medical specialties
│       ├── booking.cs                   # Appointments
│       ├── doctor_availability_slot.cs  # Doctor time slots
│       ├── payment.cs                   # Booking payments
│       ├── review.cs                    # Patient reviews
│       ├── review_response.cs           # Doctor review replies
│       ├── chat_conversation.cs         # Chat sessions
│       ├── chat_message.cs              # Chat messages
│       ├── notification.cs              # In-app notifications
│       ├── notification_preference.cs   # Per-user notification settings
│       ├── favorite.cs                  # Saved/favourite doctors
│       ├── search_history.cs            # Patient search history
│       ├── announcement.cs              # Platform announcements
│       ├── faq.cs                       # FAQ entries
│       └── system_log.cs               # Admin system event logs
│
├── Application/                         # 📋 Use-case layer — DTOs & interfaces only
│   ├── DTOs/
│   │   └── Dtos.cs                      # RegisterRequest, LoginRequest, AuthResult,
│   │                                    #   PatientDto, UpdatePatientRequest, ApiResponse<T>
│   └── Interfaces/
│       ├── IAuthService.cs              # Register / Login / Refresh / Revoke
│       └── ITokenService.cs             # GenerateAccessToken / GenerateRefreshToken
│
├── Infstruction/                        # ⚙️ Infrastructure — EF Core, services, config
│   ├── Persistence/
│   │   └── AppDBContext.cs              # EF Core DbContext with all 19 DbSets
│   ├── Services/
│   │   ├── AuthService.cs               # Full auth business logic (BCrypt, token handling)
│   │   └── TokenService.cs             # JWT token generation (HMAC-SHA256)
│   └── Settings/
│       └── JwtSettings.cs              # Config POCO for JwtSettings section
│
└── DoctorBooking/                       # 🌐 API layer — controllers, middleware, startup
    ├── Controllers/
    │   ├── AuthController.cs            # POST /api/auth/register|login|refresh|revoke
    │   ├── PatientsController.cs        # GET|PUT /api/patients/me, Admin CRUD
    │   └── adminsController.cs          # Admin management endpoints
    ├── Program.cs                       # DI, JWT middleware, Swagger, CORS, pipeline
    └── appsettings.json                 # Connection string, JWT settings, logging
```

---

## 🧩 Domain Models

| Model | Table | Description |
|---|---|---|
| `patient` | `users` | Patient accounts with auth fields (email, password, refresh token) |
| `admin` | `admins` | Platform administrators with role-based access |
| `doctor` | `doctors` | Doctor profiles — specialty, clinic, fees, rating |
| `clinic` | `clinics` | Clinic details (name, address, country) |
| `specialty` | `specialties` | Medical specialties linked to doctors |
| `booking` | `bookings` | Appointments between a patient and doctor |
| `doctor_availability_slot` | `doctor_availability_slots` | Time slot scheduling per doctor |
| `payment` | `payments` | Payment records (status, gateway txn ID, currency) |
| `review` | `reviews` | Patient → Doctor star ratings and comments |
| `review_response` | `review_responses` | Doctor replies to reviews |
| `chat_conversation` | `chat_conversations` | 1-on-1 chat session (patient ↔ doctor) |
| `chat_message` | `chat_messages` | Messages with reply-to threading |
| `notification` | `notifications` | In-app push/email notifications |
| `notification_preference` | `notification_preferences` | Per-patient notification toggles |
| `favorite` | `favorites` | Patient's bookmarked doctors |
| `search_history` | `search_histories` | Recent search terms by patient |
| `announcement` | `announcements` | Admin-authored platform announcements |
| `faq` | `faqs` | FAQ entries (created / updated by admins) |
| `system_log` | `system_logs` | Audit trail for admin events |

---

## 🔐 Authentication (JWT)

The system uses a **dual-token strategy**:
- **Access Token** — short-lived JWT (default: 60 minutes), signed with HMAC-SHA256
- **Refresh Token** — long-lived cryptographic random string (default: 7 days), stored in DB

### Token Claims

```
sub    → Patient ID (long)
email  → Patient email
name   → Full name
jti    → Unique token ID (prevents replay)
iat    → Issued-at timestamp
roles  → ["Patient"]
```

### Auth Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | ❌ Public | Create a new patient account |
| `POST` | `/api/auth/login` | ❌ Public | Login with email + password |
| `POST` | `/api/auth/refresh` | ❌ Public | Exchange refresh token for new token pair |
| `POST` | `/api/auth/revoke` | 🔒 Bearer | Logout — clears refresh token |

### Register Request
```json
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "mobileNumber": "01012345678",
  "password": "Secret@123",
  "confirmPassword": "Secret@123"
}
```

### Login Request
```json
{
  "email": "john@example.com",
  "password": "Secret@123"
}
```

### Auth Response (register & login)
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "v8Kx3mN2pQ7rT1wY...",
    "expiresAt": "2026-07-01T20:00:00Z",
    "patient": {
      "id": 1,
      "fullName": "John Doe",
      "email": "john@example.com",
      "mobileNumber": "01012345678",
      "profilePhotoUrl": null,
      "birthdate": null,
      "isActive": true,
      "createdAt": "2026-06-30T17:00:00Z"
    }
  }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Login failed.",
  "errors": ["Invalid email or password."]
}
```

---

## 📦 API Endpoints

### 👤 Patients (`/api/patients`)

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/patients/me` | 🔒 Patient | Get own profile |
| `PUT` | `/api/patients/me` | 🔒 Patient | Update own profile |
| `GET` | `/api/patients?page=1&pageSize=20` | 🔒 Admin | Paginated list of all patients |
| `GET` | `/api/patients/{id}` | 🔒 Admin | Get specific patient by ID |
| `DELETE` | `/api/patients/{id}` | 🔒 Admin | Soft-delete patient (sets `deleted_at`) |

### Update Profile Request
```json
{
  "fullName": "John Updated",
  "mobileNumber": "01099999999",
  "birthdate": "1995-05-15",
  "profilePhotoUrl": "https://cdn.example.com/photo.jpg",
  "locationLabel": "Cairo, Egypt",
  "latitude": 30.0444,
  "longitude": 31.2357
}
```

---

## ⚙️ Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server 2019+ or SQL Server LocalDB
- Visual Studio 2022 v17.10+ **or** VS Code with C# Dev Kit

### 1. Clone the Repository

```bash
git clone https://github.com/BolesGamel123/DoctorBookingApp.git
cd DoctorBookingApp
```

### 2. Configure the Database

Edit `DoctorBooking/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CureDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong2026!",
    "Issuer": "DoctorBookingApp",
    "Audience": "DoctorBookingApp",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

> ⚠️ **Security**: Replace `SecretKey` with a strong, unique key of at least 32 characters before deploying to production. Never commit real secrets to source control.

### 3. Restore Packages & Build

```bash
dotnet restore
dotnet build
```

### 4. Apply Database Migrations

```bash
dotnet ef database update --project Infstruction --startup-project DoctorBooking
```

### 5. Run the Application

```bash
dotnet run --project DoctorBooking
```

### 6. Open Swagger UI

Navigate to `https://localhost:{port}/swagger` in your browser.

To test secured endpoints:
1. Call `POST /api/auth/login` to get your access token
2. Click **🔒 Authorize** in Swagger
3. Paste your access token and click **Authorize**

---

## 🌿 Branches

| Branch | Description |
|---|---|
| `main` | Stable, production-ready code |
| `feature/initial-setup` | Initial Clean Architecture scaffolding |
| `feature/full-solution` | ✅ Full solution — JWT auth, Patient model, all controllers |

---

## 🏛️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    DoctorBooking (API)                       │
│         Controllers · Program.cs · appsettings.json         │
└────────────────────────┬────────────────────────────────────┘
                         │ depends on
┌────────────────────────▼────────────────────────────────────┐
│                   Infstruction (Infrastructure)              │
│        AppDBContext · AuthService · TokenService             │
│                    JwtSettings                               │
└──────────┬─────────────────────────┬───────────────────────-┘
           │ depends on              │ depends on
┌──────────▼──────────┐   ┌─────────▼──────────────────────-─┐
│   Application       │   │         Domain                    │
│  IAuthService       │   │   patient · doctor · booking      │
│  ITokenService      │   │   clinic · payment · review ...   │
│  DTOs               │   │   (19 entity models)              │
└─────────────────────┘   └───────────────────────────────────┘
```

**Dependency Rule**: Each layer only depends inward — API → Infrastructure → Application → Domain. Domain has zero external dependencies.

---

## 🔒 Security Features

| Feature | Implementation |
|---|---|
| Password Hashing | BCrypt with work factor (BCrypt.Net-Next) |
| Token Signing | HMAC-SHA256 symmetric key |
| Refresh Token Storage | Hashed in `users` table with expiry |
| Token Rotation | New refresh token issued on every `/refresh` call |
| Soft Delete | `deleted_at` timestamp — data never hard-deleted |
| Banned Accounts | `is_banned` + `ban_reason` checked at login |
| CORS | Configurable, AllowAll in development |

---

## 👥 Contributors

| Name | Role |
|---|---|
| **Noureddine Osama** | Backend Architecture & JWT Implementation |
| **Mohammed Taher** | Domain Modeling & Database Design |

---

## 📄 License

This project is built for **educational and development purposes**.  
Feel free to use it as a reference for Clean Architecture with ASP.NET Core and JWT authentication.

---

<div align="center">
  Made with ❤️ using ASP.NET Core (.NET 10) & Clean Architecture
</div>
