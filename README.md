# TaskJect

TaskJect is a lightweight project and task management platform built for small technical teams.

The project was originally created as an internal task manager for tracking work, planning sprints, analyzing team productivity, and managing project workflows. Later it evolved into a SaaS-style product with team management, tariff plans, integrations, notifications, and background processing.

## Features

- Project and task management
- Kanban-style task workflow
- Team and organization management
- Role-based project access
- Task complexity / point tracking
- Time tracking
- Notifications
- Telegram integration
- GitHub integration for repository and pull request workflows
- Gmail-based email sending
- Tariff plans and subscription logic
- Payment integrations
- Export functionality
- Localization support: English and Ukrainian
- Background jobs via Azure Functions

## Tech Stack

### Backend

- ASP.NET Core 8
- Razor Pages / MVC
- Entity Framework Core
- ASP.NET Core Identity
- SQL Server / Azure SQL
- AutoMapper
- SignalR
- Application Insights

### Frontend

- Razor Pages
- Bootstrap 5
- JavaScript
- HTML / CSS / SCSS

### Infrastructure

- Azure App Service
- Azure SQL
- Azure Functions
- GitHub Actions

### Main Modules
TaskJect.Web

Main web application responsible for:

UI rendering
Authentication and authorization
Organization and project management
Task management
Notifications
Telegram integration
GitHub integration
Email sending
Payment webhook handling
Localization
Data

Data access layer that contains:

ApplicationDbContext
EF Core migrations
Repository classes
Data services
Dependency injection configuration
Domain

Domain layer that contains:

Business entities
Domain events
Domain handlers
Shared domain logic
AzureFunctions

Background processing layer used for scheduled maintenance tasks:

Removing old notifications
Checking tariff expiration
Calculating used organization storage
Locking members based on tariff limits
Deleting outdated information
Integrations

TaskJect supports several external integrations:

GitHub — repository and pull request workflow automation
Telegram — task and notification updates
Gmail — email delivery
Gumroad / WayForPay — payment and subscription flows
Google Analytics — analytics tracking

## Repository Structure

```txt
TaskJect/
├── TaskJect.Web/          # Main web application
├── Data/                  # Database context, repositories, migrations, data services
├── Domain/                # Domain models, domain events and business logic
├── AzureFunctions/        # Background jobs and scheduled functions
├── .github/workflows/     # GitHub Actions workflows
├── MappingConfig.cs
├── TaskJect.sln
└── README.md



