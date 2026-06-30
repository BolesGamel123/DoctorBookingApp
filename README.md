# 🏥 Doctor Booking App

A full-featured **Doctor Booking & Appointment Management System** built with **Clean Architecture**, **ASP.NET Core (.NET 10)**, **Entity Framework Core**, and **SQL Server**.

---

## 📌 Overview

This application allows **patients** to register, search for doctors by specialty, book appointments, chat with doctors, receive notifications, and manage payments — all through a secure REST API with JWT authentication.

---

## 🚀 Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core (.NET 10) |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Authentication | JWT Bearer Tokens |
| Architecture | Clean Architecture (Domain, Application, Infrastructure, API) |
| Password Hashing | BCrypt.Net |

---

## 🗂️ Project Structure

```
DoctorBookingApp/
├── Domain/                          # Core business models & interfaces
│   └── Models/
│       ├── patient.cs               # Patient (maps to "users" table)
│       ├── admin.cs
│       ├── announcement.cs
│       ├── booking.cs
│       ├── chat_conversation.cs
│       ├── chat_message.cs
│       ├── clinic.cs
│       ├── doctor.cs
│       ├── doctor_availability_slot.cs
│       ├── faq.cs
│       ├── favorite.cs
│       ├── notification.cs
│       ├── notification_preference.cs
│       ├── payment.cs
│       ├── review.cs
│       ├── review_response.cs
│       ├── search_history.cs
│       ├── specialty.cs
│       └── system_log.cs
│
├── Application/                     # DTOs & service interfaces
│   ├── DTOs/
│   │   └── Dtos.cs                  # PatientDto, AuthResult, ApiResponse<T> ...
│   └── Interfaces/
│       ├── IAuthService.cs
│       └── ITokenService.cs
│
├── Infstruction/                    # EF Core DbContext, services, settings
│   ├── Persistence/
│   │   └── AppDBContext.cs
│   ├── Services/
│   │   ├── AuthService.cs           # Register / Login / Refresh / Revoke
│   │   └── TokenService.cs          # JWT access + refresh token generation
│   └── Settings/
│       └── JwtSettings.cs           # JWT config binding
│
└── DoctorBooking/                   # ASP.NET Core Web API
    ├── Controllers/
    │   ├── AuthController.cs        # POST register / login / refresh / revoke
    │   ├── PatientsController.cs    # GET/PUT me, Admin: list / get / delete
    │   └── adminsController.cs      # Admin CRUD
    ├── Swagger/
    │   └── SwaggerConfig.cs         # Swagger + JWT Bearer button
    ├── Program.cs
    └── appsettings.json
```

---

## 🧩 Domain Models

| Model | Description |
|---|---|
| `Patient` | Patient accounts (maps to `users` table in DB) |
| `Admin` | Admin accounts for platform management |
| `Doctor` | Doctor profiles with specialty & clinic info |
| `Clinic` | Clinic details linked to doctors |
| `Specialty` | Medical specialties |
| `Booking` | Appointment booking between patient and doctor |
| `DoctorAvailabilitySlot` | Doctor's available time slots |
| `Payment` | Payment records for bookings |
| `Review` | Patient reviews for doctors |
| `ReviewResponse` | Doctor responses to reviews |
| `ChatConversation` | Chat session between patient and doctor |
| `ChatMessage` | Individual messages within a conversation |
| `Notification` | In-app notifications |
| `NotificationPreference` | Per-patient notification settings |
| `Favorite` | Patient's saved/favorite doctors |
| `SearchHistory` | Patient's doctor search history |
| `Announcement` | Platform-wide announcements |
| `FAQ` | Frequently asked questions |
| `SystemLog` | Admin-level system event logs |

---

## 🔐 Authentication (JWT)

All auth endpoints are under `api/auth`:

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| `POST` | `/api/auth/register` | Register a new patient account | ❌ |
| `POST` | `/api/auth/login` | Login and get access + refresh token | ❌ |
| `POST` | `/api/auth/refresh` | Get new access token via refresh token | ❌ |
| `POST` | `/api/auth/revoke` | Logout / revoke refresh token | ✅ |

**Token response includes:**
```json
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "base64string...",
  "expiresAt": "2026-07-01T...",
  "patient": { "id": 1, "fullName": "...", "email": "..." }
}
```

---

## 👤 Patient Endpoints

| Method | Endpoint | Description | Role |
|---|---|---|---|
| `GET` | `/api/patients/me` | Get my profile | Patient |
| `PUT` | `/api/patients/me` | Update my profile | Patient |
| `GET` | `/api/patients` | List all patients (paginated) | Admin |
| `GET` | `/api/patients/{id}` | Get patient by ID | Admin |
| `DELETE` | `/api/patients/{id}` | Soft-delete patient | Admin |

---

## ⚙️ Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or remote)
- Visual Studio 2022+ or VS Code

### Setup Steps

```bash
# 1. Clone the repository
git clone https://github.com/BolesGamel123/DoctorBookingApp.git
cd DoctorBookingApp

# 2. Restore NuGet packages
dotnet restore

# 3. Update the connection string
# Edit DoctorBooking/appsettings.json → "DefaultConnection"

# 4. Apply database migrations
dotnet ef database update --project Infstruction --startup-project DoctorBooking

# 5. Run the application
dotnet run --project DoctorBooking
```

Swagger UI: `https://localhost:{port}/swagger`

---

## 🌿 Branches

| Branch | Description |
|---|---|
| `main` | Stable production-ready code |
| `feature/initial-setup` | Initial Clean Architecture setup |
| `feature/full-solution` | Full solution with all models, JWT, Swagger |

---

## 👥 Creators

- **Noureddine Osama**
- **Mohammed Taher**

---

## 📄 License

This project is for educational and development purposes.
