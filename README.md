# Baby & Toddler Essentials

Baby & Toddler Essentials is an ASP.NET Core MVC e-commerce web application for baby and toddler products.

---

# Requirements

Before running the project, make sure you have:

* .NET 10 SDK
* SQL Server or SQL Server Express
* Git
* Visual Studio or another compatible IDE

Verify that .NET is installed:

```bash
dotnet --version
```

The project requires:

```text
.NET 10
```

---

# First-Time Setup

Follow these steps after downloading or cloning the repository for the first time.

---

## 1. Open the Repository Folder

Open a terminal inside the repository root.

The repository structure should look similar to:

```text
BabyToddlerEssentials/
│
├── BabyToddlerEssentials/
├── BabyToddlerEssentials.slnx
├── dotnet-tools.json
├── global.json
└── README.md
```

---

## 2. Restore Project Dependencies

From the repository root, run:

```bash
dotnet restore
```

---

## 3. Restore .NET Tools

The project uses a local version of Entity Framework Core CLI.

Run:

```bash
dotnet tool restore
```

Verify that EF Core CLI is available:

```bash
dotnet ef --version
```

You should see Entity Framework Core .NET Command-line Tools version `10.x`.

---

## 4. Enter the ASP.NET Core Project

Run:

```bash
cd BabyToddlerEssentials
```

You should now be inside the folder containing:

```text
BabyToddlerEssentials.csproj
Program.cs
appsettings.json
```

---

# SQL Server Setup

Each developer uses their own local SQL Server instance and local database.

The database itself is **not included** in the repository.

---

## 5. Make Sure SQL Server Is Running

Make sure SQL Server or SQL Server Express is installed and running.

You can find your SQL Server name using SQL Server Management Studio (SSMS).

Common server names may look like:

```text
YOUR-PC-NAME\SQLEXPRESS
```

or:

```text
localhost\SQLEXPRESS
```

Use the SQL Server instance that works on your own computer.

---

## 6. Configure the Database Connection

The project uses .NET User Secrets for local configuration.

The project is already initialized for User Secrets.

You do **not** need to run:

```bash
dotnet user-secrets init
```

Set your local connection string:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SQL_SERVER;Database=BabyToddlerEssentialsDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Replace:

```text
YOUR_SQL_SERVER
```

with your own SQL Server instance.

Example:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR-PC-NAME\SQLEXPRESS;Database=BabyToddlerEssentialsDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Do not copy another developer's computer name or SQL Server instance.

Each developer should use their own SQL Server.

---

# Admin Account Setup

The application contains a database initializer that creates:

```text
Admin Role
User Role
```

It can also create a local Admin account.

The Admin credentials are **not stored in the repository**.

---

## 7. Configure Admin Email

Run:

```bash
dotnet user-secrets set "SeedAdmin:Email" "YOUR_ADMIN_EMAIL"
```

Example:

```bash
dotnet user-secrets set "SeedAdmin:Email" "admin@babytoddler.com"
```

---

## 8. Configure Admin Password

Run:

```bash
dotnet user-secrets set "SeedAdmin:Password" "YOUR_ADMIN_PASSWORD"
```

Choose your own local password that satisfies ASP.NET Core Identity password requirements.

Do not put the password inside:

```text
README.md
appsettings.json
Program.cs
DbInitializer.cs
```

---

## 9. Verify Local Configuration

Run:

```bash
dotnet user-secrets list
```

You should have these keys:

```text
ConnectionStrings:DefaultConnection
SeedAdmin:Email
SeedAdmin:Password
```

> Warning: `dotnet user-secrets list` displays the real values in the terminal. Do not share screenshots containing passwords or other sensitive information.

---

# Database Creation

You do **not** need to manually create `BabyToddlerEssentialsDb` in SQL Server.

The project already contains EF Core migrations.

---

## 10. Apply Database Migrations

Run:

```bash
dotnet ef database update
```

This command will create:

```text
BabyToddlerEssentialsDb
```

on your configured SQL Server instance and create the required tables.

This includes Identity tables and application tables.

Examples include:

```text
AspNetUsers
AspNetRoles
AspNetUserRoles

Categories
Products
ProductImages
ProductReviews
Testimonials
WishlistItems
Orders
OrderItems
```

During first-time setup, do **not** create another `InitialCreate` migration.

Do not run:

```bash
dotnet ef migrations add InitialCreate
```

The required migrations are already included in the repository.

Only run:

```bash
dotnet ef database update
```

---

# Build the Project

## 11. Build

Run:

```bash
dotnet build
```

Expected result:

```text
Build succeeded.
```

Do not continue development if the project does not build successfully.

---

# Run the Project

## 12. Start the Application

Run:

```bash
dotnet run
```

The terminal will display one or more local addresses, for example:

```text
https://localhost:xxxx
http://localhost:xxxx
```

Open the displayed address in your browser.

---

# First Application Run

When the application starts, `DbInitializer` runs automatically.

It checks whether the following roles exist:

```text
Admin
User
```

If they do not exist, they are created.

If Admin credentials were configured using User Secrets, the Admin account is also created.

The flow is:

```text
dotnet run
    ↓
Application Starts
    ↓
Connects to BabyToddlerEssentialsDb
    ↓
DbInitializer Runs
    ↓
Admin Role Created If Missing
    ↓
User Role Created If Missing
    ↓
Admin Account Created If Missing
    ↓
Application Ready
```

---

# User Registration

Normal users can register through the Register page.

The registration form contains:

```text
Full Name
Email
Password
Confirm Password
```

After successful registration:

```text
Register
    ↓
ApplicationUser Created
    ↓
User Role Assigned Automatically
    ↓
User Signed In
```

Every normal account created through Register automatically receives:

```text
User
```

The Register page does not create Admin accounts.

---

# Login

You can test two types of accounts:

### Normal User

Create one using the Register page.

### Admin

Use the Admin email and password that you configured locally using:

```bash
dotnet user-secrets set "SeedAdmin:Email" "..."
dotnet user-secrets set "SeedAdmin:Password" "..."
```

---

# Verify the Setup

Before starting development, make sure all of the following work:

```text
Project Build      ✅
Database           ✅
Home Page          ✅
Register           ✅
Login              ✅
User Role          ✅
Admin Role         ✅
Admin Account      ✅
```

---

# If the Database Connection Fails

First verify your configured connection string:

```bash
dotnet user-secrets list
```

Make sure:

```text
Server
```

matches your local SQL Server instance.

Then make sure SQL Server is running.

Try connecting to the same server using SQL Server Management Studio.

After fixing the connection, run again:

```bash
dotnet ef database update
```

Then:

```bash
dotnet run
```

---

# If HTTPS Certificate Causes a Local Error

If your computer does not trust the ASP.NET Core development HTTPS certificate, run:

```bash
dotnet dev-certs https --trust
```

Then restart the application:

```bash
dotnet run
```

---

# After Pulling Database Changes

If another team member changes the database Models and adds a new EF Core migration, after receiving those changes run:

```bash
dotnet ef database update
```

Do **not** recreate their migration.

Example:

```text
New Migration Received
        ↓
dotnet ef database update
        ↓
Your Local Database Updated
```

---

# Important Database Rule

If your task requires changing the database schema, coordinate with the team before creating a migration.

Database schema changes include:

* Adding a new Model
* Adding or removing a Model property
* Changing a relationship
* Adding a new table
* Changing a Foreign Key
* Changing database constraints

If you are the team member responsible for the migration:

```bash
dotnet ef migrations add MigrationName
```

Then:

```bash
dotnet ef database update
```

The migration files must be included with your code changes.

Other team members only need to run:

```bash
dotnet ef database update
```

after receiving the migration.

---

# Security

Never commit:

* Admin passwords
* Database passwords
* API keys
* Tokens
* Private credentials
* User Secrets

Local secrets should always be configured using:

```bash
dotnet user-secrets set "Key" "Value"
```

---

# What Is Shared With the Project

The repository contains:

```text
Source Code                    ✅
Application Models             ✅
ApplicationDbContext           ✅
DbInitializer                  ✅
EF Core Migrations             ✅
Admin/User Role Seed Logic     ✅
Register Logic                 ✅
Local dotnet-ef Configuration  ✅
```

The repository does **not** contain:

```text
Your Local SQL Database        ❌
Your SQL Server Name           ❌
Your Connection String         ❌
Your Admin Password            ❌
Your User Secrets              ❌
```

Each developer creates their own local database from the shared migrations.

---

# Quick Start

After getting the project for the first time:

```bash
dotnet restore

dotnet tool restore

cd BabyToddlerEssentials

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SQL_SERVER;Database=BabyToddlerEssentialsDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"

dotnet user-secrets set "SeedAdmin:Email" "YOUR_ADMIN_EMAIL"

dotnet user-secrets set "SeedAdmin:Password" "YOUR_ADMIN_PASSWORD"

dotnet ef database update

dotnet build

dotnet run
```

That's it. The project should now be ready to run locally.
