# TaskManager API

A secure RESTful API built with ASP.NET Core for managing users and their tasks with JWT authentication. This project demonstrates advanced CRUD operations, Entity Framework Core with SQLite, FluentValidation, pagination, JWT security, custom error handling, and professional API design patterns.

## 🚀 Features

- **JWT Authentication**: Secure token-based authentication and authorization
- **User Management**: Profile management with authentication
- **Task Management**: Complete CRUD operations with user isolation
- **Data Validation**: FluentValidation with custom business rules
- **Pagination**: Efficient data loading with metadata
- **Database Seeding**: Automatic sample data generation
- **Data Relationships**: Tasks linked to users via foreign keys
- **Database**: SQLite with Entity Framework Core
- **API Documentation**: Swagger/OpenAPI integration with XML comments
- **Custom Error Handling**: 405 Method Not Allowed and comprehensive exception handling
- **CORS Support**: Cross-origin resource sharing enabled
- **User Isolation**: Users can only access their own data
- **RESTful Design**: Follows REST principles with proper HTTP methods and status codes

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

## 🛠️ Installation & Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/TaskManagerAPI.git
   cd TaskManagerAPI
   ```

2. **Navigate to the API project**
   ```bash
   cd TaskManagerAPI
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Configure JWT Settings** (Optional - defaults are provided)
   
   Update `appsettings.json` if needed:
   ```json
   {
     "Jwt": {
       "SecretKey": "ThisIsASecretKeyForJWTTokenGeneration12345",
       "Issuer": "TaskManagerAPI",
       "Audience": "TaskManagerAPI-Users"
     }
   }
   ```

5. **Run the application** (Database will be created and seeded automatically)
   ```bash
   dotnet run
   ```

6. **Access the API**
   - API Base URL: `http://localhost:5246`
   - Swagger UI: `http://localhost:5246/swagger`

## 🔐 Authentication

This API uses JWT (JSON Web Token) authentication. You must authenticate to access protected endpoints.

### Getting Started with Authentication

1. **Register a new account** or **login** with seeded users
2. **Use the returned JWT token** in the Authorization header
3. **Access protected endpoints** with your token

### Seeded User Accounts
```
Username: john_doe, Password: Password123
Username: jane_smith, Password: Password123  
Username: mike_wilson, Password: Password123
Username: sarah_connor, Password: Password123
Username: admin_user, Password: Password123
```

## 📊 Database Schema

### Users Table
| Column    | Type      | Description            | Constraints           |
|-----------|-----------|------------------------|-----------------------|
| Id        | int       | Primary key (auto)     | Identity, Not Null    |
| UserName  | string    | Unique username        | 3-50 chars, Required  |
| PassWord  | string    | User password          | 6-100 chars, Required |
| CreatedAt | DateTime  | Creation timestamp     | Not Null              |
| UpdatedAt | DateTime  | Last update timestamp  | Not Null              |

### Tasks Table
| Column      | Type         | Description                    | Constraints            |
|-------------|--------------|--------------------------------|------------------------|
| Id          | int          | Primary key (auto)             | Identity, Not Null     |
| Title       | string       | Task title                     | 1-100 chars, Required  |
| Description | string       | Task description               | 1-500 chars, Required  |
| Status      | TaskStatus   | Task status enum               | 0=Ongoing, 1=Stopped, 2=Finished |
| DueDate     | DateTime     | Due date                       | Required, Future Date  |
| CreatedAt   | DateTime     | Creation timestamp             | Not Null               |
| UpdatedAt   | DateTime     | Last update timestamp          | Not Null               |
| UserId      | int          | Foreign key to Users table     | Required, Must Exist   |

### Task Status Enum
```csharp
public enum TaskStatus
{
    Ongoing = 0,
    Stopped = 1,
    Finished = 2
}
```

## 🔗 API Endpoints

### Authentication (Public)

| Method | Endpoint              | Description           | Request Body         | Response Code |
|--------|-----------------------|-----------------------|----------------------|---------------|
| POST   | `/api/Auth/login`     | Login user            | `LoginRequest`       | 200 OK / 401  |
| POST   | `/api/Auth/register`  | Register new user     | `RegisterRequest`    | 201 Created   |

### Users (Protected)

| Method | Endpoint              | Description           | Auth Required | Request Body        | Response Code |
|--------|-----------------------|-----------------------|---------------|---------------------|---------------|
| GET    | `/api/Users/profile`  | Get current user      | ✅ Yes        | -                   | 200 OK / 401  |
| PUT    | `/api/Users/profile`  | Update current user   | ✅ Yes        | `UpdateUserRequest` | 200 OK / 401  |

### Tasks (Protected)

| Method | Endpoint           | Description           | Auth Required | Request Body        | Response Code |
|--------|--------------------|-----------------------|---------------|---------------------|---------------|
| GET    | `/api/Task`        | Get user's tasks      | ✅ Yes        | Query: `page`, `pageSize` | 200 OK / 401  |
| GET    | `/api/Task/{id}`   | Get user's task by ID | ✅ Yes        | -                   | 200 OK / 404 / 401 |
| POST   | `/api/Task`        | Create new task       | ✅ Yes        | `CreateTaskRequest` | 201 Created / 401   |
| PUT    | `/api/Task/{id}`   | Update user's task    | ✅ Yes        | `UpdateTaskRequest` | 200 OK / 404 / 401  |
| DELETE | `/api/Task/{id}`   | Delete user's task    | ✅ Yes        | -                   | 204 / 404 / 401     |

## 🔑 Authentication Flow

### 1. Register New User
```http
POST /api/Auth/register
Content-Type: application/json

{
  "userName": "newuser123",
  "passWord": "Password123",
  "confirmPassword": "Password123"
}
```

**Response (201 Created):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires": "2025-07-02T09:41:03Z",
  "user": {
    "id": 6,
    "userName": "newuser123",
    "createdAt": "2025-07-02T08:41:03Z"
  }
}
```

### 2. Login Existing User
```http
POST /api/Auth/login
Content-Type: application/json

{
  "userName": "john_doe",
  "passWord": "Password123"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires": "2025-07-02T09:41:03Z",
  "user": {
    "id": 1,
    "userName": "john_doe",
    "createdAt": "2025-06-01T12:00:00Z"
  }
}
```

### 3. Use Token for Protected Endpoints
```http
GET /api/Users/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 📄 Pagination

All list endpoints support pagination with the following query parameters:

| Parameter | Type | Default | Max | Description |
|-----------|------|---------|-----|-------------|
| `page`    | int  | 1       | -   | Page number (starts at 1) |
| `pageSize`| int  | 10      | 100 | Number of items per page |

### Pagination Response Format
```json
{
  "data": [...],
  "page": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

## 📝 Request/Response Examples

### Authentication

#### Register User
```http
POST /api/Auth/register
Content-Type: application/json

{
  "userName": "john_doe123",
  "passWord": "SecurePassword123",
  "confirmPassword": "SecurePassword123"
}
```

**Validation Rules:**
- Username: 3-50 characters, alphanumeric + underscore only, must be unique
- Password: 6-100 characters, must contain lowercase, uppercase, and number
- ConfirmPassword: Must match password

#### Login User
```http
POST /api/Auth/login
Content-Type: application/json

{
  "userName": "john_doe123",
  "passWord": "SecurePassword123"
}
```

### Profile Management

#### Get Current User Profile
```http
GET /api/Users/profile
Authorization: Bearer your-jwt-token-here
```

**Response (200 OK):**
```json
{
  "id": 6,
  "userName": "john_doe123",
  "createdAt": "2025-07-02T08:41:03Z",
  "updatedAt": "2025-07-02T08:41:03Z"
}
```

#### Update Profile
```http
PUT /api/Users/profile
Authorization: Bearer your-jwt-token-here
Content-Type: application/json

{
  "userName": "john_doe_updated",
  "passWord": "NewPassword123"
}
```

### Task Management

#### Create Task (No UserId needed - auto-assigned)
```http
POST /api/Task
Authorization: Bearer your-jwt-token-here
Content-Type: application/json

{
  "title": "Complete project documentation",
  "description": "Write comprehensive README and API docs",
  "status": 0,
  "dueDate": "2025-12-01T10:00:00Z"
}
```

**Validation Rules:**
- Title: 1-100 characters, required
- Description: 1-500 characters, required
- DueDate: Must be in the future
- Status: Must be valid enum value (0, 1, or 2)
- UserId: Automatically set to current authenticated user

#### Get User's Tasks
```http
GET /api/Task?page=1&pageSize=5
Authorization: Bearer your-jwt-token-here
```

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": 1,
      "title": "Complete project documentation",
      "description": "Write comprehensive README and API docs",
      "status": 0,
      "dueDate": "2025-12-01T10:00:00Z",
      "createdAt": "2025-07-02T08:45:00Z",
      "updatedAt": "2025-07-02T08:45:00Z",
      "userId": 6,
      "user": {
        "id": 6,
        "userName": "john_doe123"
      }
    }
  ],
  "page": 1,
  "pageSize": 5,
  "totalCount": 1,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

## 🏗️ Project Structure

```
TaskManagerAPI/
├── Controllers/
│   ├── AuthController.cs           # Authentication endpoints
│   ├── BaseController.cs           # Base controller with JWT utilities
│   ├── UsersController.cs          # User profile management
│   └── TaskController.cs           # Task CRUD operations
├── Data/
│   ├── AppDbContext.cs             # Entity Framework context
│   └── SeedData.cs                 # Database seeding utility
├── DTOs/
│   ├── Auth/
│   │   ├── LoginRequest.cs         # Login credentials
│   │   ├── RegisterRequest.cs      # Registration data
│   │   └── AuthResponse.cs         # JWT token response
│   ├── Common/
│   │   ├── PaginationRequest.cs    # Pagination query parameters
│   │   └── PaginatedResponse.cs    # Pagination response wrapper
│   ├── User/
│   │   ├── CreateUserRequest.cs    # User creation DTO
│   │   └── UpdateUserRequest.cs    # User update DTO
│   └── Task/
│       ├── CreateTaskRequest.cs    # Task creation DTO
│       └── UpdateTaskRequest.cs    # Task update DTO
├── Filters/
│   └── ValidationFilterAttribute.cs # Custom validation filter
├── Middleware/
│   └── ErrorHandlingMiddleware.cs  # Custom error handling
├── Models/
│   ├── Entities/
│   │   ├── User.cs                 # User entity
│   │   └── Task.cs                 # Task entity
│   └── Enums/
│       └── TaskStatus.cs           # Task status enumeration
├── Services/
│   ├── IJwtService.cs              # JWT service interface
│   └── JwtService.cs               # JWT service implementation
├── Validators/
│   ├── Auth/
│   │   ├── LoginRequestValidator.cs
│   │   └── RegisterRequestValidator.cs
│   ├── User/
│   │   ├── CreateUserRequestValidator.cs
│   │   └── UpdateUserRequestValidator.cs
│   └── Task/
│       ├── CreateTaskRequestValidator.cs
│       └── UpdateTaskRequestValidator.cs
├── appsettings.json                # Configuration including JWT settings
├── Program.cs                      # Application entry point
└── TaskManagerAPI.csproj           # Project file
```

## 🔧 Configuration

### Database Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=TaskManager.db"
  }
}
```

### JWT Configuration
```json
{
  "Jwt": {
    "SecretKey": "ThisIsASecretKeyForJWTTokenGeneration12345",
    "Issuer": "TaskManagerAPI",
    "Audience": "TaskManagerAPI-Users"
  }
}
```

## 🌱 Database Seeding

The application automatically seeds the database with sample data on startup:

**Sample Users (All with password: "Password123"):**
- john_doe
- jane_smith  
- mike_wilson
- sarah_connor
- admin_user

**Sample Tasks:**
- 15 realistic tasks distributed across users
- Various statuses (Ongoing, Stopped, Finished)
- Different due dates and creation times

## ✅ Validation Rules

### Authentication Validation
- **Username**: 3-50 characters, alphanumeric and underscore only, must be unique
- **Password**: 6-100 characters, must contain at least one lowercase, uppercase, and number
- **ConfirmPassword**: Must match password exactly

### Task Validation
- **Title**: 1-100 characters, required
- **Description**: 1-500 characters, required
- **DueDate**: Must be in the future
- **Status**: Must be valid TaskStatus enum value

### Business Rules
- Users can only access their own tasks
- JWT tokens expire after 60 minutes
- Username uniqueness is enforced
- Password complexity requirements

## 🛡️ Security Features

- **JWT Authentication**: Secure token-based authentication
- **User Isolation**: Users can only access their own data
- **Password Security**: Strong password requirements
- **Token Expiration**: 60-minute token lifetime
- **CORS Protection**: Configurable cross-origin policies
- **Input Validation**: Comprehensive validation on all inputs

## 🧪 Testing with Swagger

### Using Swagger UI
1. Navigate to `http://localhost:5246/swagger`
2. **Register or Login** to get a JWT token
3. Click the **"Authorize"** button (🔒) 
4. Enter: `Bearer your-jwt-token-here`
5. Test protected endpoints

### Testing Flow
1. **POST /api/Auth/register** - Create account
2. **Copy the token** from response
3. **Click Authorize** in Swagger
4. **Enter token** with "Bearer " prefix
5. **Test protected endpoints** like GET /api/Task

## 🧾 Status Codes & Error Handling

| Code | Description | When Used |
|------|-------------|-----------|
| 200  | OK | Successful GET, PUT requests |
| 201  | Created | Successful POST requests |
| 204  | No Content | Successful DELETE requests |
| 400  | Bad Request | Validation errors, business rule violations |
| 401  | Unauthorized | Missing or invalid JWT token |
| 404  | Not Found | Resource doesn't exist or access denied |
| 405  | Method Not Allowed | Invalid HTTP method for endpoint |
| 500  | Internal Server Error | Unexpected server errors |

### Custom Error Response Examples

#### Validation Error
```json
{
  "errors": {
    "UserName": ["Username must be between 3 and 50 characters"],
    "PassWord": ["Password must contain at least one lowercase letter, one uppercase letter, and one number"]
  }
}
```

#### 405 Method Not Allowed
```json
{
  "error": "Method Not Allowed",
  "message": "The POST method is not allowed for this endpoint", 
  "allowedMethods": ["GET", "PUT", "DELETE"],
  "path": "/api/Task/1",
  "statusCode": 405
}
```

## 🧪 API Testing Examples

### Using curl

#### Register and Login
```bash
# Register new user
curl -X POST "http://localhost:5246/api/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser123",
    "passWord": "TestPassword123", 
    "confirmPassword": "TestPassword123"
  }'

# Login user
curl -X POST "http://localhost:5246/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser123",
    "passWord": "TestPassword123"
  }'
```

#### Use Protected Endpoints
```bash
# Get profile (replace TOKEN with actual JWT)
curl -X GET "http://localhost:5246/api/Users/profile" \
  -H "Authorization: Bearer TOKEN"

# Create task
curl -X POST "http://localhost:5246/api/Task" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test task",
    "description": "Testing API",
    "status": 0,
    "dueDate": "2025-12-01T10:00:00Z"
  }'

# Get user's tasks
curl -X GET "http://localhost:5246/api/Task?page=1&pageSize=5" \
  -H "Authorization: Bearer TOKEN"
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Jeffrey Daquigan**
- GitHub: [Jeffrey Daquigan](https://github.com/DaikiDaiki6)

## 🙏 Acknowledgments

- Built with [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- Database powered by [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- Validation with [FluentValidation](https://fluentvalidation.net/)
- JWT Authentication with [Microsoft.AspNetCore.Authentication.JwtBearer](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer/)
- Documentation generated with [Swagger/OpenAPI](https://swagger.io/)

---

**🎉 This is a production-ready API with JWT authentication, user isolation, comprehensive validation, custom error handling, and professional documentation - perfect for real-world applications!**