# UserManagementAPI

User Management API for TechHive Solutions (HR and IT departments).
Built with ASP.NET Core 8 Web API. Developed with help from Microsoft Copilot.

## How to run
1. Install the .NET 8 SDK.
2. `cd UserManagementAPI`
3. `dotnet restore`
4. `dotnet run --launch-profile http` (or just `dotnet run` and check the URL in the console)
5. Open `/swagger` in the browser, or use `UserManagementAPI.http` / Postman.

All `/api/*` endpoints need this header:
`Authorization: Bearer techhive-secret-token-123`
(Tokens are set in `appsettings.json` under `Auth:Tokens`.)

## Endpoints
| Method | Route | Description | Success | Errors |
|---|---|---|---|---|
| GET | /api/users | List users (paging, `department`, `search`) | 200 | 400, 401 |
| GET | /api/users/{id} | Get one user | 200 | 404, 401 |
| POST | /api/users | Create user | 201 | 400, 409, 401 |
| PUT | /api/users/{id} | Update user | 200 | 400, 404, 409, 401 |
| DELETE | /api/users/{id} | Delete user | 204 | 404, 401 |

## Middleware pipeline (order)
1. `ErrorHandlingMiddleware` - catches exceptions, returns `{ "error": "Internal server error." }`
2. `TokenAuthenticationMiddleware` - checks Bearer token, returns 401 if invalid
3. `RequestLoggingMiddleware` - logs method, path, status code, time

## Project structure
- `Program.cs` - setup and middleware pipeline
- `Models/User.cs` - `User` and `UserRequest` (with validation rules)
- `Services/` - in-memory user store (thread-safe)
- `Controllers/UsersController.cs` - CRUD endpoints
- `Middleware/` - three middleware classes
- `UserManagementAPI.http` - test requests

---

## Copilot documentation

### Activity 1: Writing and enhancing API code
- Prompt: "Create an ASP.NET Core Web API project named UserManagementAPI with boilerplate Program.cs." -> Copilot gave the project setup, `AddControllers`, Swagger and `MapControllers`.
- Prompt: "Generate CRUD endpoints for a User model with GET (all and by id), POST, PUT, DELETE." -> Copilot gave the first controller with all five endpoints.
- Enhancements Copilot suggested:
  - Move data logic into a service (`IUserService`) with dependency injection.
  - Use correct status codes: 201 with `CreatedAtAction`, 204 for delete, 404 for missing user.
  - Add a separate `UserRequest` model so clients cannot set the `Id`.
  - Add Swagger so endpoints can be tested in the browser.
- Testing: all CRUD endpoints tested in Postman / `.http` file (see test table below).

### Activity 2: Debugging
| Bug reported | How Copilot helped | Fix |
|---|---|---|
| Users added without validation | Asked Copilot to review POST for missing checks | DataAnnotations on `UserRequest` (required, email format, max length, no blank text); `[ApiController]` returns 400 automatically |
| Error when user does not exist | Copilot found the code returned `null`/crashed | `GetById` returns null; controller returns 404 with JSON message |
| API crashed on unhandled exceptions | Copilot suggested try-catch | Global try-catch in `ErrorHandlingMiddleware` |
| Slow GET /users | Copilot pointed out the full list was returned each time | Added paging (`page`, `pageSize`, max 100), filters, and `X-Total-Count` header |
| Duplicate users | Copilot suggested an email uniqueness check | Returns 409 Conflict |
| Race conditions | Copilot suggested thread-safe collections | `ConcurrentDictionary` and `Interlocked.Increment` for ids |

### Activity 3: Middleware
- Prompt: "Generate middleware to log HTTP requests and responses in ASP.NET Core." -> `RequestLoggingMiddleware` (method, path, status code, time).
- Prompt: "Create middleware that catches unhandled exceptions and returns JSON errors." -> `ErrorHandlingMiddleware`.
- Prompt: "Write middleware that validates Bearer tokens and returns 401 for invalid tokens." -> `TokenAuthenticationMiddleware`. Copilot also suggested reading tokens from config (not hard-coding) and using `CryptographicOperations.FixedTimeEquals`.
- Prompt: "Check my middleware order in Program.cs." -> confirmed order: error handling, authentication, logging.
- Result: errors are consistent, access is secured, and every allowed request is logged.

### Test results
| # | Test | Expected | Result |
|---|---|---|---|
| 1 | GET /api/users | 200 + list | Pass |
| 2 | GET /api/users/1 | 200 | Pass |
| 3 | GET /api/users/999 | 404 | Pass |
| 4 | POST valid user | 201 | Pass |
| 5 | POST empty name / bad email | 400 | Pass |
| 6 | POST duplicate email | 409 | Pass |
| 7 | PUT valid | 200 | Pass |
| 8 | PUT id 999 | 404 | Pass |
| 9 | DELETE valid | 204 | Pass |
| 10 | DELETE again | 404 | Pass |
| 11 | No token | 401 | Pass |
| 12 | Wrong token | 401 | Pass |
| 13 | GET /debug/throw | 500 + `{"error":"Internal server error."}` | Pass |

> Update this table with your own real results after running the tests, and add your Postman screenshots.
