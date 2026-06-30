# Doctor Booking App

This branch implements a Clean Architecture for the Doctor Booking App using ASP.NET Core (.NET 10), Entity Framework, and SQL Server.

## Features
- Clean Architecture structure
- Entity Framework Core integration
- Standard CRUD API controllers for all domain models
- SQL Server as the database provider

## Project Structure
- **Domain**: Domain models and interfaces
- **Infstruction**: Infrastructure and persistence (AppDBContext)
- **DoctorBooking**: ASP.NET Core Web API (controllers, startup, etc.)
- **Application**: Application layer (if present)

## Getting Started
1. Clone the repository
2. Restore NuGet packages
3. Update your SQL Server connection string in `appsettings.json`
4. Run database migrations if needed
5. Build and run the solution

## Creators
- Noureddine Osama
- Mohammed Taher

---
This project is maintained on the `Clean-Architecture` branch.
