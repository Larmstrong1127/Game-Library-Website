# GG Game Library

A full-featured ASP.NET Core 6 MVC web application for managing a personal game library. Features user authentication, genre filtering, pagination, and full CRUD operations backed by Entity Framework Core and SQLite.

**Developer:** Landon Armstrong
**GitHub:** [Larmstrong1127](https://github.com/Larmstrong1127)
**Email:** Landon.Armstrong@stmartin.edu

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 6 MVC |
| Language | C# 10 |
| Database | SQLite + Entity Framework Core 7 |
| Authentication | ASP.NET Core Identity |
| Frontend | Razor Views, Bootstrap 5, Bootstrap Icons |

---

## Features

- **User Authentication** — Register, login, and logout via ASP.NET Core Identity
- **Full CRUD** — Add, edit, view details, and delete games (authenticated users only)
- **Search and Filter** — Search by game name, filter by genre, sort by name or price
- **Pagination** — Displays 8 games per page for clean browsing
- **Cover Image Upload** — Upload a custom cover image per game stored in the database
- **Flash Messages** — Success and error feedback after every action
- **Landing Page** — Shows library statistics and recently added games
- **Responsive UI** — Dark gaming theme, works on mobile and desktop

---

## Project Structure

```
CSC595-Week04/
├── Controllers/
│   ├── HomeController.cs       # Landing page with stats
│   ├── ProductController.cs    # Game CRUD, search, filter, pagination
│   └── AccountController.cs   # Register, login, logout
├── Models/
│   ├── Product.cs              # Game entity (Name, Price, Genre, Image)
│   └── ViewModels/             # Login and register form models
├── Data/
│   ├── ProductDBContext.cs     # EF Core DbContext with seed data
│   └── ProductUser.cs         # Extended Identity user
├── Views/
│   ├── Home/Index.cshtml       # Landing page
│   └── Product/               # Index, Add, Edit, Delete, ShowDetails
└── wwwroot/
    └── css/Gamesite.css       # Custom dark gaming theme
```

---

## Setup Instructions

### Prerequisites
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- Visual Studio 2022 or VS Code with the C# extension

### Run Locally

```bash
# Clone the repository
git clone https://github.com/Larmstrong1127/game-library.git
cd CSC595-Week04

# Restore NuGet packages
dotnet restore

# Apply database migrations to create the SQLite database
dotnet ef database update

# Run the application
dotnet run
```

Then open `http://localhost:5000` in your browser.

> **Note:** A SQLite database file (`myProductData.db`) will be created automatically on first run. This file is excluded from version control. If you want a clean database, delete the `.db` file and run `dotnet ef database update` again.

---

## Default Seed Data

The database is seeded with 10 game titles on first run:

| Game | Genre | Price |
|---|---|---|
| The Legend of Zelda: Breath of the Wild | Adventure | $59.99 |
| God of War | Action | $39.99 |
| Red Dead Redemption 2 | Action | $49.99 |
| The Witcher 3: Wild Hunt | RPG | $29.99 |
| Elden Ring | RPG | $59.99 |
| Minecraft | Simulation | $26.99 |
| Stardew Valley | Simulation | $14.99 |
| Hollow Knight | Platform | $14.99 |
| Call of Duty: Modern Warfare II | Shooter | $59.99 |
| FIFA 24 | Sports | $69.99 |

---

## License

This project is for educational and portfolio purposes.

**Developer:** Landon Armstrong · [GitHub](https://github.com/Larmstrong1127)
