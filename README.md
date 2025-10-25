# Project Manager - Full Stack Application

---

## **Live Deployment** 

| Component | Link |
|-----------|------|
| **Frontend App** | [https://project-manager-rho-three.vercel.app/](https://project-manager-rho-three.vercel.app/) |
| **Backend API** | [https://projectmanager-production-4893.up.railway.app/swagger](https://projectmanager-production-4893.up.railway.app/swagger) |

---

## ** Assignment **

- ✅ **Base Requirements**  - Full-stack app with auth, CRUD operations
- ✅ **Bonus Feature**  - Smart Task Scheduler with dependency resolution

---

## **Tech Stack**

| Layer | Technology |
|-------|------------|
| **Frontend** | React + TypeScript + Vite |
| **Backend** | ASP.NET Core 8.0 Web API |
| **Database** | PostgreSQL |
| **Auth** | JWT |
| **Deployment** | Vercel (Frontend) + Railway (Backend) |

---

## **Key Features**

- User authentication (register/login)
- Create and manage projects
- Add tasks with due dates
- Mark tasks as complete
- **Smart Scheduler** - Orders tasks based on dependencies 

---

## **Bonus Feature: Smart Task Scheduler**

**Endpoint:** `POST /api/projects/{projectId}/schedule`

**What it does:** Automatically orders tasks based on dependencies using topological sort.

**Try it:** Go to [Swagger UI](https://projectmanager-production-4893.up.railway.app/swagger) → Authorize → Test the schedule endpoint

**Example Request:**
```json
{
  "tasks": [
    {"title": "Design API", "estimatedHours": 5, "dependencies": []},
    {"title": "Build Backend", "estimatedHours": 12, "dependencies": ["Design API"]},
    {"title": "Build Frontend", "estimatedHours": 10, "dependencies": ["Design API"]},
    {"title": "End-to-End Test", "estimatedHours": 8, "dependencies": ["Build Backend", "Build Frontend"]}
  ]
}
```

**Response:**
```json
{
  "recommendedOrder": ["Design API", "Build Backend", "Build Frontend", "End-to-End Test"],
  "message": "Successfully scheduled 4 tasks"
}
```

---

## **Project Structure**
```
ProjectManager/
├── frontend/              # React + TypeScript app
│   ├── src/
│   │   ├── components/   # UI components
│   │   ├── pages/        # Page components
│   │   ├── services/     # API calls
│   │   └── contexts/     # Auth context
│   └── package.json
│
├── ProjectManager.Api/   # ASP.NET Core Web API
│   ├── Controllers/      # API endpoints
│   ├── Services/         # Business logic (includes SchedulingService)
│   ├── Models/           # Data models
│   └── Data/             # Database context
│
└── Dockerfile            # Railway deployment
```

---

## **Quick Start (Local Development)**

### Frontend
```bash
cd frontend
npm install
npm run dev
# Runs on http://localhost:5173
```

### Backend
```bash
cd ProjectManager.Api
dotnet restore
dotnet run
# Runs on http://localhost:5070
```

---

## **API Endpoints**

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login user |
| GET | `/api/projects` | Get all projects |
| POST | `/api/projects` | Create project |
| GET | `/api/projects/{id}/tasks` | Get tasks |
| POST | `/api/projects/{id}/tasks` | Create task |
| **POST** | **`/api/projects/{id}/schedule`** | **Smart Scheduler (Bonus)** |

**Full API Documentation:** [Swagger UI](https://projectmanager-production-4893.up.railway.app/swagger)

---

## **Testing the App**

1. **Frontend:** Visit [https://project-manager-rho-three.vercel.app/](https://project-manager-rho-three.vercel.app/)
   - Register a new account
   - Create projects and tasks
   - Mark tasks complete

2. **Smart Scheduler:** Visit [Swagger UI](https://projectmanager-production-4893.up.railway.app/swagger)
   - Login to get JWT token
   - Authorize in Swagger
   - Test `POST /api/projects/{id}/schedule` endpoint
   - Use the example request above

---

### Core Application
- Full-stack CRUD application
- User authentication with JWT
- Responsive UI
- Protected routes
- Error handling

### Bonus Feature
- **Smart Task Scheduler** using Kahn's Algorithm
- Dependency graph resolution
- Circular dependency detection
- Topological sorting

### Deployment
- Frontend on Vercel (auto-deploy from GitHub)
- Backend on Railway with PostgreSQL
- CI/CD pipeline configured
