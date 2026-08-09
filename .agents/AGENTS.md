# CezStudentAssistant Project Guidelines & UI Design System

This file defines the project coding standards, UI design tokens, component architecture, frontend rules, backend architecture guidelines, and testing standards for **CEZ Student Assistant**.

---

## 🎨 UI Design Tokens & Palette

- **Dark Obsidian Background**: `#0b0f17` / Tailwind `bg-background`
- **Dark Slate Cards**: `#131b2e` / Tailwind `bg-card`
- **Dark Slate Sidebar & Topbar Header**: `#182238` / Tailwind `bg-sidebar`
- **Neutral Dark Borders**: `#1e293b` / Tailwind `border-border`
- **Politechnika Białostocka Primary Brand Green**: `#00494B` (Primary) & `#007C66` (Hover Accent)
- **Typography**: `Roboto Slab, serif` for headings, `Inter / sans-serif` for body, `font-mono` for index numbers/IDs/code inputs.

---

## 🧩 Reusable Frontend Component Architecture

**Rule**: NEVER duplicate hardcoded UI elements, inputs, buttons, header layouts, or alert banners. Always import and use the standard shared components in `src/components/`:

### 1. Buttons (`src/components/Button.tsx`)
- **`<PrimaryButton>`**: For main user actions (e.g. submit, log in, create, sync).
  - Uses PB brand green `#00494B`.
  - Supports `loading` prop (displays spinner loader next to constant text, dims opacity).
- **`<SecondaryButton>`**: For outline / secondary actions (e.g. CEZ login, cancel, back).

### 2. Form Inputs (`src/components/Input.tsx`)
- **`<Input>`**: Reusable labeled input field.
  - Handles labels (`label`), error states (`error`), input type, and monospace formatting automatically.

### 3. Alerts (`src/components/Alert.tsx`)
- **`<Alert>`**: Reusable inline error / warning banner.
  - Replaces raw `div` error blocks inside forms and cards.

### 4. Auth Layout (`src/components/AuthLayout.tsx`)
- **`<AuthLayout>`**: Shell layout for unauthenticated pages (`LoginPage`, `CezLoginPage`, `RegisterPage`).
  - Contains PB logo (`pb-logo.png`), academic header, floating top-right language switcher (`PL / EN`), and responsive card container.

### 5. Toast Notifications (`src/components/Toast.tsx`)
- **`<Toast>`**: Reusable floating notification popup for success and error messages.
  - Supports `variant="success"` and `variant="error"`.
  - Replaces inline `div` popups in `App.tsx`.

---

## ⚙️ Backend Architecture Guidelines (.NET 9 / ASP.NET Core)

### 1. Clean Architecture & Layer Responsibilities
- **`CezStudentAssistant.Domain`**: Pure domain entities (e.g. `User`, `Course`, `CourseFile`, `Quiz`), domain exceptions, and repository interfaces. NO dependencies on EF Core, MediatR, or API infrastructure.
- **`CezStudentAssistant.Application`**: Business logic layer following CQRS with MediatR:
  - Commands (`IRequest<TResponse>`) and Queries (`IRequest<TResponse>`).
  - Handlers extending `BaseCommandHandler<TCommand>` or `BaseQueryHandler<TQuery, TResponse>`.
  - Pipelines (`UserContextBehavior` for injecting `UserId`, validation behaviors).
  - Common responses extending `BaseResponse<T>` / `ApiResponse`.
- **`CezStudentAssistant.Infrastructure`**: Persistence (`DbContext`, EF Core configurations, Repositories, UnitOfWork) and external services (`CurrentUserService`, `TokenService`, `PasswordHasher`, `CezSyncService`, AI Services).
- **`CezStudentAssistant.API`**: Minimal API Endpoints (`AuthEndpoints`, `CourseEndpoints`, `QuizEndpoints`), middleware pipeline, and OpenAPI/Swagger configuration.

### 2. CQRS Handler & Result Standards
- **Commands & Queries**: Use MediatR requests. Store request inputs in clean records or DTOs.
- **User Context Pipeline**: Every authenticated request flows through `UserContextBehavior`, automatically populating `command.UserId` from the `ICurrentUserService` claims context.
- **Standard Responses**: Use `BaseResponse<T>` / `ApiResponse` wrappers. Standardize success message constants (`SuccessMessage = "..."`).

### 3. Exception Handling & HTTP Mapping
- **Domain & Application Exceptions**: Throw strongly-typed exceptions inheriting from `AppException`:
  - `UnauthorizedException` -> Maps to HTTP 401 `UnauthorizedResponse`
  - `NotFoundException` -> Maps to HTTP 404 `NotFoundResponse`
  - `ConflictException` -> Maps to HTTP 409 `ConflictResponse`
  - `ApiValidationException` / `BadRequestException` -> Maps to HTTP 400 `BadRequestResponse` / `ValidationResponse`
- **Global Exception Middleware**: Exception handling in `Program.cs` intercepts all thrown `AppException` types and serializes a clean, standardized JSON response. NEVER return unhandled stack trace 500 HTML pages.

### 4. Authentication & Security Best Practices
- **HttpOnly Cookies**: JWT `accessToken` and `refreshToken` MUST be stored and transmitted via HttpOnly, Secure, SameSite cookies managed by `ICurrentUserService.SetSession()` and cleared by `ICurrentUserService.ClearSession()`.
- **Password Hashing**: Use `IPasswordHasherService` (PBKDF2 with SHA256) for secure password hashing and verification.
- **Authorization**: Secure Minimal API endpoints with `.RequireAuthorization()`.

---

## 🧪 Testing Guidelines & Standards

### 1. Backend Testing Strategy (.NET / xUnit)
- **Unit Tests (`tests/CezStudentAssistant.UnitTests`)**:
  - Test all CQRS Handlers, Domain Logic, Services, and Exception conditions in isolation.
  - Use `xUnit` for test runner, `NSubstitute` / `Moq` for mocking interfaces (`IUserRepository`, `ICurrentUserService`), and `FluentAssertions` for readable assertions.
  - Follow AAA pattern (Arrange, Act, Assert).
- **Integration Tests (`tests/CezStudentAssistant.IntegrationTests`)**:
  - Test API endpoints using `WebApplicationFactory<Program>` end-to-end.
  - Verify HTTP status codes (200 OK, 400 Bad Request, 401 Unauthorized, 404 Not Found, 409 Conflict) and JSON response shapes (`ApiResponse<T>`).
  - Verify HttpOnly cookie creation (`accessToken`, `refreshToken`) and expiration on logout.
- **Architecture Tests (`tests/CezStudentAssistant.ArchitectureTests`)**:
  - Enforce Clean Architecture rules using NetArchTest:
    - Domain layer must NOT depend on Application, Infrastructure, or API.
    - Application layer must NOT depend on Infrastructure or API.
    - CQRS Handlers must inherit from `BaseCommandHandler` or `BaseQueryHandler`.

### 2. Frontend Testing Strategy (React / Vitest)
- **Component Tests (`*.test.tsx`)**:
  - Test user interactions (button clicks, form inputs, loading states, inline error displays).
  - Always mock API calls using `vi.mock("../services/api")`.
  - Verify that `<PrimaryButton>` shows spinner when `loading={true}` and preserves button text.
  - Verify that form validation errors render inline via `<Alert message={error} />`.
- **Commands to Run Tests**:
  - Backend: `dotnet test`
  - Frontend: `npm test` or `npm run test:run`

---

## 🔒 Session & API Error Guidelines (Frontend-Backend Integration)

1. **Inline Form Validation**: Validation errors (empty fields, bad password/login) MUST be rendered inline inside the form using `<Alert message={error} />`. NEVER show floating popup toasts for form validation errors.
2. **Silent Unauthenticated Startup Check**: When checking auth session on startup (`checkAuth()`), HTTP `401 Unauthorized` responses indicate the user is logged out — do NOT show an API connection error toast for logged-out users.
3. **HTTP Cookie Logout**: Expire HttpOnly `accessToken` and `refreshToken` cookies via server endpoint `POST /auth/logout`.

---

## 📁 Image & Asset Placement

- Store ALL image assets exclusively in `src/assets/`:
  - `src/assets/pb-logo.png` (Full PB logo)
  - `src/assets/pb-emblem.png` (PB eagle emblem)
  - `src/assets/cez-logo.png` (CEZ favicon)
- DO NOT duplicate image files into the `public/` directory.
