# 🚀 Task Management Tool

### Full-Stack Task Management Application — .NET 8 + React

A modern, secure, and responsive **Task Management Tool** built as a full-stack application using **ASP.NET Core Web API** and **React.js**.

The application provides authentication, role-based authorization, task management, filtering, sorting, CSV import/export, dashboard analytics, user profiles, and real-time task updates using SignalR.

---

## ✨ Features

### 🔐 Authentication & Authorization

* User registration and login
* JWT-based authentication
* Role-based authorization
* Protected application routes
* Secure session/token handling
* Profile information
* Logout functionality

### 📋 Task Management

* Create tasks
* View tasks
* Update tasks
* Delete tasks
* Assign tasks to users
* Task status management
* Task priority management
* Categories
* Due dates
* Task details page

### 🔎 Search, Filter & Sort

The task management interface supports:

* 🔍 Search by title or category
* 📌 Filter by status
* ⚡ Filter by priority
* 📅 Filter by due date
* ↕️ Sort by due date
* 🔤 Sort alphabetically by title
* ⭐ Sort by priority
* Clear filters functionality

### 📊 Dashboard

The dashboard provides an overview of task activity with status-based task counts.

### ⚡ Real-Time Updates

Integrated **ASP.NET Core SignalR** for real-time task updates.

When a task is created, updated, or deleted, connected clients can receive updates without manually refreshing the page.

### 📁 CSV Import & Export

The application supports complete CSV task management.

#### Export

Export filtered task data into a CSV file.

#### Import

Import multiple tasks from a CSV file with validation for:

* Title
* Description
* Status
* Priority
* Category
* Due date
* Assigned user ID

The importer also handles common date formats and converts them into the API-compatible date format.

#### CSV Template

A ready-to-use CSV template can be downloaded directly from the Task Management page.

### 👤 User Profile

The profile page displays authenticated user information and provides logout functionality.

### 🛡️ Error Handling & Validation

* API validation responses
* Client-side CSV validation
* Authentication protection
* User-friendly error messages
* Global exception handling middleware on the backend

---

# 🛠️ Technology Stack

## Backend

| Technology                | Purpose                 |
| ------------------------- | ----------------------- |
| **ASP.NET Core Web API**  | REST API                |
| **C#**                    | Backend development     |
| **Entity Framework Core** | ORM / database access   |
| **SQL Server**            | Database                |
| **JWT**                   | Authentication          |
| **SignalR**               | Real-time communication |
| **Serilog**               | Application logging     |
| **xUnit**                 | Unit testing            |

## Frontend

| Technology         | Purpose                |
| ------------------ | ---------------------- |
| **React.js**       | UI development         |
| **Vite**           | Frontend build tooling |
| **React Router**   | Client-side routing    |
| **Axios**          | HTTP requests          |
| **SignalR Client** | Real-time updates      |
| **CSS**            | Styling                |

## Development Tools

* Git
* GitHub
* Visual Studio / VS Code
* SQL Server
* .NET CLI
* npm

---

# 🏗️ Architecture

The backend follows a layered architecture designed to keep responsibilities separated.

```text
┌───────────────────────────────────────────────┐
│                  React Client                 │
│                                               │
│ Login • Dashboard • Tasks • Profile           │
└───────────────────────┬───────────────────────┘
                        │
                        │ HTTP / JWT
                        ▼
┌───────────────────────────────────────────────┐
│              ASP.NET Core API                 │
│                                               │
│ Controllers • Middleware • SignalR Hubs       │
└───────────────────────┬───────────────────────┘
                        │
                        ▼
┌───────────────────────────────────────────────┐
│                Application                    │
│                                               │
│ DTOs • Services • Business Logic              │
└───────────────────────┬───────────────────────┘
                        │
                        ▼
┌───────────────────────────────────────────────┐
│                  Domain                       │
│                                               │
│ Entities • Core Business Models               │
└───────────────────────┬───────────────────────┘
                        │
                        ▼
┌───────────────────────────────────────────────┐
│               Infrastructure                  │
│                                               │
│ EF Core • DbContext • Database Access         │
└───────────────────────┬───────────────────────┘
                        │
                        ▼
                  SQL Server
```

---

# 📂 Project Structure

```text
cohort-9-dotnet-7284-kulsoom/
│
├── TaskManagement/
│   │
│   ├── TaskManagement.API/
│   │   ├── Controllers/
│   │   ├── Hubs/
│   │   ├── Middleware/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── TaskManagement.Application/
│   │   ├── DTOs/
│   │   └── Services/
│   │
│   ├── TaskManagement.Domain/
│   │   └── Entities/
│   │
│   ├── TaskManagement.Infrastructure/
│   │   ├── Data/
│   │   └── Services/
│   │
│   └── TaskManagement.Tests/
│
├── taskmanagement-client/
│   │
│   ├── src/
│   │   ├── api/
│   │   ├── components/
│   │   │   └── Navbar.jsx
│   │   ├── context/
│   │   ├── pages/
│   │   │   ├── Dashboard.jsx
│   │   │   ├── Login.jsx
│   │   │   ├── Profile.jsx
│   │   │   ├── Register.jsx
│   │   │   └── TaskList.jsx
│   │   └── App.jsx
│   │
│   ├── package.json
│   └── vite.config.js
│
└── README.md
```

---

# ⚙️ Getting Started

## Prerequisites

Make sure the following are installed:

* [.NET SDK](https://dotnet.microsoft.com/)
* [Node.js](https://nodejs.org/)
* SQL Server
* Git

---

# 🔧 Backend Setup

Navigate to the API directory:

```bash
cd TaskManagement/TaskManagement.API
```

Restore dependencies:

```bash
dotnet restore
```

---

## 🔐 JWT Configuration

The API requires a JWT signing key.

For local development, configure the key using **.NET User Secrets** instead of committing sensitive values to source control.

Initialize User Secrets:

```bash
dotnet user-secrets init
```

Set the JWT key:

```bash
dotnet user-secrets set "Jwt:Key" "your-development-secret-key-at-least-32-characters-long"
```

> ⚠️ Never commit production secrets, passwords, connection strings, or JWT signing keys to GitHub.

---

# 🗄️ Database Setup

Configure your SQL Server connection according to the project's environment configuration.

Then apply Entity Framework migrations:

```bash
dotnet ef database update
```

If Entity Framework CLI is not installed:

```bash
dotnet tool install --global dotnet-ef
```

---

# ▶️ Run the Backend

From:

```text
TaskManagement/TaskManagement.API
```

run:

```bash
dotnet run
```

The local API runs on:

```text
http://localhost:5025
```

---

# 💻 Frontend Setup

Open a second terminal and navigate to:

```bash
cd taskmanagement-client
```

Install dependencies:

```bash
npm install
```

Start the development server:

```bash
npm run dev
```

The Vite development server will normally be available at:

```text
http://localhost:5173
```

---

# 🔗 Frontend API Configuration

Create/update the frontend environment file:

```text
taskmanagement-client/.env
```

Example:

```env
VITE_API_BASE_URL=http://localhost:5025/api
```

Make sure the backend is running before using the frontend.

---

# 🧪 Build & Validation

To create a production build:

```bash
npm run build
```

To run linting:

```bash
npm run lint
```

The frontend build should complete successfully before deployment.

---

# 📡 API Overview

### Authentication

```text
POST /api/Auth/register
POST /api/Auth/login
```

### Tasks

```text
GET    /api/Tasks
POST   /api/Tasks
GET    /api/Tasks/{id}
PUT    /api/Tasks/{id}
DELETE /api/Tasks/{id}
```

### Dashboard

```text
GET /api/Dashboard
```

### SignalR

```text
/hubs/tasks
```

---

# 🔄 Real-Time Task Flow

```text
User A
  │
  │ Create / Update / Delete Task
  ▼
React Client
  │
  │ HTTP Request
  ▼
ASP.NET Core API
  │
  ├── Save changes
  │
  └── SignalR notification
           │
           ▼
      Connected Clients
           │
           ▼
     Updated Task List
```

---

# 📄 CSV Format

The supported CSV columns are:

```text
title,
description,
status,
priority,
category,
dueDate,
assignedToUserId
```

Example:

```csv
title,description,status,priority,category,dueDate,assignedToUserId
Example Task,Example description,Pending,Medium,Work,2026-09-15,1
```

### Supported Status Values

```text
Pending
In Progress
Completed
```

### Supported Priority Values

```text
Low
Medium
High
```

---

# 🔒 Security

The application includes:

* JWT authentication
* Role-based authorization
* Protected API endpoints
* Authenticated frontend routes
* User-specific authentication state
* User Secrets for local JWT configuration
* Validation of incoming task data

Sensitive configuration should always remain outside source control.

---

# 🧪 Test Accounts

The project can be tested using the following existing application accounts:

| Name                 | Email                                                       | Role  | Password            |
| -------------------- | ----------------------------------------------------------- | ----- | ------------------- |
| Kulsoom Jawed Sheikh | [kulsoomjawed38@gmail.com](mailto:kulsoomjawed38@gmail.com) | Admin | Provided separately |
| Mariyah              | [mariyahtest@gmail.com](mailto:mariyahtest@gmail.com)       | User  | Provided separately |

> Test account passwords are provided separately to the project evaluator for security purposes.

### Admin Account

The Admin account can be used to verify:

* Admin authentication
* Role-based authorization
* Task management functionality
* Protected functionality

### User Account

The User account can be used to verify:

* User authentication
* Standard task management functionality
* Protected routes
* Task CRUD operations

# 🧪 Testing

The backend includes an xUnit test project:

```text
TaskManagement.Tests
```

Run tests from the `TaskManagement` solution directory:

```bash
dotnet test
```

---

# 📈 Development History

The project was developed incrementally through feature branches and pull requests.

Major development stages included:

```text
#1  Layered Architecture
#2  JWT Authentication
#3  Task CRUD + RBAC
#4  xUnit Unit Tests
#5  Global Exception Handling
#6  Dashboard API
#7  React Frontend Setup
#8  React Authentication
#9  React Dashboard
#10 React Task Management
#11 Task Management Fixes
#12 SignalR + Task UI
#13 React UI Improvements
```

All listed project pull requests were merged into the repository.

---

# 🌟 Current Application Highlights

```text
🔐 JWT Authentication
👥 Role-Based Authorization
📋 Complete Task CRUD
📊 Dashboard Analytics
🔎 Search & Filtering
↕️ Task Sorting
📁 CSV Import
📤 CSV Export
📄 CSV Template
⚡ SignalR Real-Time Updates
👤 User Profile
🛡️ Validation & Exception Handling
🧪 xUnit Tests
📝 Structured Logging
```

---

# 🎯 Project Goals

The project demonstrates practical full-stack development using:

* Clean separation of backend responsibilities
* RESTful API development
* Authentication and authorization
* Database-driven application design
* Modern React development
* Real-time communication
* Client-side validation
* File-based data import/export
* Automated testing
* Git-based collaborative development

---

# 👩‍💻 Author

**Kulsoom Jawed**

Cohort 9 — .NET Fullstack Assignment

---

## ⭐ Project Status

**Development Status: Completed**

The application currently includes the core authentication, task management, dashboard, profile, CSV, and real-time functionality required for the project.

---

> Built with ❤️ using **ASP.NET Core + React + SQL Server + SignalR**
