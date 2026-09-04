# 📝 Task Management Tool

A full-stack Task Management application built with **ASP.NET Core Web API** and **React.js**. The application provides secure authentication, role-based authorization, task management, dashboard analytics, real-time task updates, CSV import/export, logging, exception handling, and automated testing.

---

## 🚀 Project Overview

The Task Management Tool allows users to create, manage, assign, track, and organize tasks through a modern web interface.

The project follows a layered architecture with a clear separation between:

* Presentation / API layer
* Application layer
* Domain layer
* Infrastructure layer
* React frontend
* Automated tests

---

## ✨ Features

### 🔐 Authentication & Authorization

* User registration
* User login
* JWT-based authentication
* Role-based authorization
* Admin and User roles
* Secure password hashing using BCrypt
* Protected API endpoints
* Protected frontend routes
* Logout functionality

### 📋 Task Management

* Create tasks
* View tasks
* View task details
* Update tasks
* Delete tasks
* Assign tasks to users
* Task status management
* Task priority management
* Task categories
* Due dates
* Search tasks
* Filter tasks
* Sort tasks

### 📊 Dashboard

The dashboard provides a productivity overview including:

* Pending tasks
* In-progress tasks
* Completed tasks
* Total tasks
* Overall completion percentage
* Task distribution

### ⚡ Real-Time Updates

The application uses **SignalR** for real-time task updates.

When task-related changes occur, connected clients can receive updates without manually refreshing the page.

### 📥 CSV Import / 📤 CSV Export

Users can:

* Export tasks to CSV
* Download a CSV template
* Import tasks from CSV
* Validate imported task data
* View import success/failure results

Supported task fields include:

* Title
* Description
* Status
* Priority
* Category
* Due Date
* Assigned User

### 🛡️ Exception Handling

The API includes centralized exception handling middleware that:

* Handles unexpected exceptions
* Returns meaningful error responses
* Prevents internal exception details from being exposed
* Logs errors using Serilog

### 📝 Logging

The project uses **Serilog** for application logging.

Logs are written to:

```text
Logs/
```

Daily rolling log files are generated automatically.

### 🚦 Rate Limiting

Authentication endpoints are protected using ASP.NET Core rate limiting to reduce repeated authentication attempts.

### 🧪 Automated Testing

The backend includes xUnit tests covering application functionality.

Current test suite:

```text
26 tests passed
```

---

# 🛠️ Technology Stack

## Backend

* ASP.NET Core 8
* C#
* Entity Framework Core
* SQL Server
* JWT Authentication
* BCrypt
* SignalR
* Serilog
* Swagger / OpenAPI
* ASP.NET Core Rate Limiting

## Frontend

* React.js
* Vite
* React Router
* Axios
* SignalR Client
* JavaScript
* CSS

## Testing & Quality

* xUnit
* SonarQube
* Git
* GitHub

---

# 🏗️ Architecture

The backend follows a layered architecture:

```text
TaskManagement
│
├── TaskManagement.API
│   ├── Controllers
│   ├── Middleware
│   ├── Hubs
│   └── Program.cs
│
├── TaskManagement.Application
│   ├── Interfaces
│   ├── Services
│   └── DTOs
│
├── TaskManagement.Domain
│   └── Entities
│
├── TaskManagement.Infrastructure
│   ├── Data
│   ├── Repositories
│   └── Services
│
└── TaskManagement.Tests
```

The frontend is located in:

```text
taskmanagement-client/
```

---

# 📁 Project Structure

```text
cohort-9-dotnet-7284-kulsoom
│
├── TaskManagement
│   ├── TaskManagement.API
│   ├── TaskManagement.Application
│   ├── TaskManagement.Domain
│   ├── TaskManagement.Infrastructure
│   └── TaskManagement.Tests
│
├── taskmanagement-client
│
├── README.md
└── .gitignore
```

---

# ⚙️ Project Setup

## 1. Prerequisites

Install the following:

* .NET 8 SDK
* Node.js
* npm
* SQL Server
* Git

Optional development tools:

* Visual Studio / VS Code
* SonarQube
* Swagger

---

## 2. Clone the Repository

```bash
git clone https://github.com/KULSOOM2001/cohort-9-dotnet-7284-kulsoom.git
cd cohort-9-dotnet-7284-kulsoom
```

---

# 🗄️ Database Setup

The project uses **SQL Server** with Entity Framework Core.

Configure the SQL Server connection string in the API configuration/user secrets.

Then apply the existing Entity Framework migration:

```bash
dotnet ef database update
```

If Entity Framework CLI is not installed:

```bash
dotnet tool install --global dotnet-ef
```

The project contains an initial migration:

```text
20260730154557_InitialCreate
```

---

# 🧪 Test Accounts

The project can be tested using the following existing application accounts:

| Name                 | Email                                                       | Role  |
| -------------------- | ----------------------------------------------------------- | ----- |
| Kulsoom Jawed Sheikh | [kulsoomjawed38@gmail.com](mailto:kulsoomjawed38@gmail.com) | Admin |
| Mariyah              | [mariyahtest@gmail.com](mailto:mariyahtest@gmail.com)       | User  |

> Test account passwords are intentionally not stored in this public repository. They can be provided separately for project evaluation.

The Admin account can be used to verify administrative functionality and role-based access control.

The User account can be used to verify standard user functionality.

---

# 🔑 JWT Configuration

JWT configuration is required for authentication.

Configure the following values through application configuration or user secrets:

```text
Jwt:Key
Jwt:Issuer
Jwt:Audience
```

For development, sensitive JWT values should be stored using **.NET User Secrets** rather than committing them to source control.

Example:

```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "your-development-secret-key"
dotnet user-secrets set "Jwt:Issuer" "your-issuer"
dotnet user-secrets set "Jwt:Audience" "your-audience"
```

Do not commit real secrets, passwords, or tokens to GitHub.

---

# ▶️ Running the Backend

From the repository root:

```bash
cd TaskManagement/TaskManagement.API
dotnet run
```

The API runs on:

```text
http://localhost:5025
```

Swagger is available during development through the API's Swagger endpoint.

---

# ▶️ Running the Frontend

Open a second terminal:

```bash
cd taskmanagement-client
npm install
npm run dev
```

The React application runs on:

```text
http://localhost:5173
```

---

# 🔗 API Configuration

The frontend uses the API base URL configured through:

```text
VITE_API_BASE_URL
```

Example development configuration:

```text
VITE_API_BASE_URL=http://localhost:5025/api
```

Make sure the frontend API URL matches the backend URL.

---

# 🔐 Authentication Flow

The authentication flow works as follows:

```text
User
 │
 ▼
React Login/Register
 │
 ▼
ASP.NET Core Auth API
 │
 ▼
JWT Authentication
 │
 ▼
Protected API Endpoints
 │
 ▼
React Dashboard / Tasks
```

JWT access tokens are used for authenticated API requests.

SignalR connections also use the authenticated access token.

---

# ⚡ SignalR Flow

Real-time task updates are implemented using SignalR.

```text
React Client
     │
     ▼
SignalR Hub
     │
     ▼
Task Management API
     │
     ▼
Connected Clients
```

The task hub endpoint is:

```text
/hubs/tasks
```

This allows connected clients to receive task updates in real time.

---

# 📥 CSV Import

The application supports importing tasks through CSV files.

A CSV template can be downloaded from the Tasks page.

The import process validates task information and reports successful and failed rows.

Date values are converted to the API-compatible date format before submission.

---

# 📤 CSV Export

Tasks can be exported from the Tasks page as a CSV file.

The exported file contains task information that can be used for reporting or further processing.

---

# 🧾 Task Status & Priority

### Status

```text
Pending
InProgress
Completed
```

### Priority

```text
Low
Medium
High
```

---

# 🧪 Testing

Run all backend tests from the repository root:

```bash
dotnet test TaskManagement/TaskManagement.sln
```

The current test suite contains:

```text
26/26 tests passed
```

---

# 🏗️ Build Verification

## Backend

```bash
dotnet build TaskManagement/TaskManagement.sln
```

## Frontend

```bash
cd taskmanagement-client
npm run build
```

Both backend and frontend builds have been verified successfully.

---

# 🔍 SonarQube

The project was analyzed using SonarQube for code quality.

SonarQube project:

```text
Task Management Tool
```

Project key:

```text
TaskManagement
```

Main branch:

```text
develop
```

The latest SonarQube analysis completed successfully and the **Quality Gate passed**.

---

# 🔒 Security

The project follows basic security practices including:

* JWT authentication
* BCrypt password hashing
* Role-based authorization
* Protected API endpoints
* Rate limiting for authentication endpoints
* Centralized exception handling
* Sensitive configuration through user secrets
* Secrets excluded from source control

---

# 🔄 Development Workflow

The project was developed using feature branches and pull requests.

The main development branch is:

```text
develop
```

Feature work was implemented through separate branches and merged into `develop`.

The repository also includes pull request history covering major project features and improvements.

---

# 📚 Major Development Milestones

Major implementation areas included:

1. Project setup and architecture
2. Authentication and authorization
3. Task management APIs
4. React authentication UI
5. Dashboard
6. Task list
7. Task creation and editing
8. Profile and logout
9. Exception handling
10. Unit testing
11. SignalR real-time updates
12. CSV import/export
13. UI improvements
14. SonarQube analysis
15. Final documentation and verification

---

# ✅ Final Verification

The project has been verified with:

* ✅ Backend build successful
* ✅ Frontend production build successful
* ✅ 26/26 backend tests passed
* ✅ Authentication tested
* ✅ Dashboard tested
* ✅ Task CRUD tested
* ✅ Role-based access tested
* ✅ SignalR functionality implemented
* ✅ CSV import/export tested
* ✅ Exception handling implemented
* ✅ Serilog logging implemented
* ✅ SonarQube Quality Gate passed
* ✅ Git working tree clean
* ✅ `develop` branch pushed to GitHub

---

# 🎯 Project Status

**Completed**

The Task Management Tool is a functional full-stack application demonstrating:

* Full-stack .NET + React development
* REST API development
* Authentication and authorization
* Database integration
* Task management
* Real-time communication
* CSV processing
* Logging
* Exception handling
* Automated testing
* Code quality analysis
* Git/GitHub workflow

---

# 👩‍💻 Author

**Kulsoom Jawed Sheikh**

Task Management Tool — .NET Fullstack Project
