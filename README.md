# 🏥 Doctor Booking App

A full-featured **Doctor Booking & Appointment Management System** built with **Clean Architecture**, **ASP.NET Core (.NET 10)**, **Entity Framework Core**, and **SQL Server**.

---

## 📌 Overview

This application allows patients to register, search for doctors by specialty, book appointments, chat with doctors, receive notifications, and manage payments — all through a secure REST API with JWT authentication.

---

## 🚀 Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core (.NET 10) |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Authentication | JWT Bearer Tokens |
| Architecture | Clean Architecture (Domain, Application, Infrastructure, API) |
| Validation | FluentValidation + MediatR Pipeline |

---

## 🗂️ Project Structure

```
DoctorBookingApp/
├── Domain/                         # Core business models & interfaces
│   └── Models/
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
│       ├── system_log.cs
│       └── user.cs
│
├── Application/                    # Use cases, CQRS handlers, validators
│
├── Infstruction/                   # EF Core DbContext, repositories, persistence
│   └── Persistence/
│       └── AppDBContext.cs
│
└── DoctorBooking/                  # ASP.NET Core Web API (controllers, middleware)
```

---

## 🧩 Domain Models

| Model | Description |
|---|---|
| `User` | Patient accounts with profile info |
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
| `Notification` | In-app notifications for users |
| `NotificationPreference` | Per-user notification settings |
| `Favorite` | Patient's saved/favorite doctors |
| `SearchHistory` | Patient's doctor search history |
| `Announcement` | Platform-wide announcements |
| `FAQ` | Frequently asked questions |
| `SystemLog` | Admin-level system event logs |

---

## 🔐 Authentication

- JWT Bearer token authentication
- Separate login endpoints for **Users**, **Doctors**, and **Admins**
- Token-based identity verification on all protected endpoints

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

The API will be available at `https://localhost:5001` (or configured port).

---

## 🌿 Branches

| Branch | Description |
|---|---|
| `main` | Stable production-ready code |
| `feature/initial-setup` | Initial Clean Architecture setup |
| `feature/full-solution` | Full solution with all domain models |

---

## 👥 Creators

- **Noureddine Osama**
- **Mohammed Taher**

---

## 📄 License

This project is for educational and development purposes.
