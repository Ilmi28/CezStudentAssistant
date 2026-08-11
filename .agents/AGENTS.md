# CezStudentAssistant Architecture & Project Guidelines

This file defines the project coding standards, UI design tokens, component architecture rules, backend clean architecture guidelines, testing standards, and file modification rules for **CEZ Student Assistant**.

---

## 🎨 UI Design Tokens & Theme Palette

- **Dark Obsidian Background**: `#0b0f17` / Tailwind `bg-background`
- **Dark Slate Cards**: `#131b2e` / Tailwind `bg-card`
- **Dark Slate Sidebar & Topbar Header**: `#182238` / Tailwind `bg-sidebar`
- **Neutral Dark Borders**: `#1e293b` / Tailwind `border-border`
- **CEZ WI PB Brand Primary Blue**: `#0f4c81` (Light Primary), `#0284c7` (Hover Accent) & `#2563eb` (Dark Primary)
- **Typography**: `Roboto Slab, serif` for headings, `Inter / sans-serif` for body, `font-mono` for index numbers/IDs/code inputs.

---

## 🧩 Frontend Component Architecture Rules

1. **Component Extraction & Reusability**: ALWAYS extract self-contained, repeated, or modular UI elements (e.g. Modals, Banners, Cards, Inputs, Buttons, Headers) into dedicated reusable components in `src/components/` instead of writing raw inline HTML/JSX inside pages.
2. **No Hardcoded UI Duplication**: NEVER duplicate hardcoded UI elements, inputs, buttons, header layouts, or alert banners. Always import and use standard shared components in `src/components/`.
3. **Generic Container Pattern**: Prefer generic, composable container components (e.g. `<Card borderLeftPrimary hoverEffect>`) with configurable props and children over fragmented, single-purpose component variants.
4. **Form Error Display**: Form validation errors (empty fields, bad credentials) MUST be rendered inline inside forms using shared inline alert/banner components. NEVER use floating toast popups for form validation errors.
5. **Domain-Specific Services**: Frontend API requests MUST be modularized into separate domain-specific service files (e.g. Auth, Course, Quiz, User) rather than bundled into a single monolithic API file. Low-level fetch wrappers, response parsing, and authentication interceptors MUST be encapsulated in a shared base client module.

---

## ⚙️ Backend Architecture Guidelines (.NET 9 / ASP.NET Core)

### 1. Clean Architecture & Layer Responsibilities
- **`CezStudentAssistant.Domain`**: Pure domain entities, domain exceptions, and repository interfaces. NO dependencies on EF Core, MediatR, or API infrastructure.
- **`CezStudentAssistant.Application`**: Business logic layer following CQRS with MediatR:
  - Commands (`IRequest<TResponse>`) and Queries (`IRequest<TResponse>`).
  - Handlers extending `BaseCommandHandler<TCommand>` or `BaseQueryHandler<TQuery, TResponse>`.
  - Pipelines (`UserContextBehavior` for injecting `UserId`, validation behaviors).
  - Common responses extending `BaseResponse<T>` / `ApiResponse`.
- **`CezStudentAssistant.Infrastructure`**: Persistence (`DbContext`, EF Core configurations, Repositories, UnitOfWork) and external integration services (`CurrentUserService`, `TokenService`, `PasswordHasher`, external API services).
- **`CezStudentAssistant.API`**: Minimal API Endpoints mapped via endpoint extension methods, middleware pipeline, and OpenAPI/Swagger configuration.

### 2. CQRS Handler & Result Standards
- **Commands & Queries**: Store request inputs in clean records or DTOs.
- **User Context Pipeline**: Authenticated requests flow through `UserContextBehavior`, automatically populating `command.UserId` from `ICurrentUserService` claims context.
- **Standard Responses**: Use `BaseResponse<T>` / `ApiResponse` wrappers with standardized success constants.

### 3. Exception Handling & HTTP Mapping
- **Domain & Application Exceptions**: Throw strongly-typed exceptions inheriting from `AppException` (e.g. `UnauthorizedException` -> HTTP 401, `ForbiddenException` -> HTTP 403, `NotFoundException` -> HTTP 404, `ConflictException` -> HTTP 409, `BadRequestException` -> HTTP 400).
- **Global Exception Middleware**: Exception handling in `Program.cs` intercepts all `AppException` types and serializes standardized JSON responses. NEVER return unhandled stack trace 500 HTML pages.

### 4. Authentication & Security Best Practices
- **HttpOnly Cookies**: JWT `accessToken` and `refreshToken` MUST be stored and transmitted via HttpOnly, Secure, SameSite cookies managed by `ICurrentUserService`.
- **Password Hashing**: Use `IPasswordHasherService` (PBKDF2 with SHA256) for secure password hashing and verification.
- **Authorization**: Secure Minimal API endpoints with `.RequireAuthorization()`.

### 5. Configuration & Settings Management
- **No Hardcoded Configuration or Expiration Durations**: NEVER hardcode configuration values (e.g. container names, cookie TTLs, token expiration times, API timeout limits, or retry counts) directly in C# code files. ALL configuration parameters MUST be defined in `appsettings.json`.
- **Direct IConfiguration Injection for Simple Settings**: For simple configuration strings, container names, or individual keys, inject `IConfiguration` directly instead of creating single-property wrapper classes or `IOptions<T>` objects. Reserve strongly-typed `IOptions<T>` settings classes strictly for complex multi-property settings sections (e.g. `JwtSettings`).

---

## 🧪 Testing Guidelines & Standards

### 1. Backend Testing Strategy (.NET / NUnit)
- **Mandatory Test Coverage Rule**: EVERY CQRS Handler (Command & Query) and Infrastructure/Domain Service MUST have a dedicated set of unit tests AND integration tests covering both happy paths and specific error/exception conditions.
- **Unit Tests (`tests/CezStudentAssistant.UnitTests`)**:
  - Test all CQRS Handlers, Domain Logic, Services, and Exception conditions in isolation using `NUnit`, `NSubstitute`, and `FluentAssertions` following AAA pattern.
  - MUST explicitly verify strongly-typed domain/application exceptions (e.g. `UnauthorizedException` -> 401, `ForbiddenException` -> 403, `NotFoundException` -> 404, `ConflictException` -> 409, `BadRequestException` -> 400).
- **Integration Tests (`tests/CezStudentAssistant.IntegrationTests`)**:
  - Test API endpoints using `WebApplicationFactory<Program>` end-to-end.
  - Verify success responses as well as specific HTTP error status codes (400, 401, 403, 404, 409), JSON response shapes (`ApiResponse<T>`), and HttpOnly cookie management.
- **Architecture Tests (`tests/CezStudentAssistant.ArchitectureTests`)**:
  - Enforce Clean Architecture dependencies using NetArchTest.

### 2. Frontend Testing Strategy (React / Vitest)
- **Component Tests**: Test user interactions, loading states, and inline error displays using `Vitest` and `React Testing Library`.

---

## 🔒 Session & Asset Guidelines

1. **Asset Storage**: Store ALL image assets exclusively in `src/assets/`. DO NOT duplicate image files into `public/`.
2. **Silent Startup Auth Check**: HTTP `401 Unauthorized` responses on startup auth checks indicate logged-out state — do NOT display error connection toasts for unauthenticated users.

---

## 📝 Guidelines for Modifying AGENTS.md

1. **Focus on Architectural Rules, Not Specific Component Names**: NEVER list specific individual components, file paths, or transient feature details (e.g. do NOT list `Navbar`, `SyncBanner`, `StatCard`, or specific endpoint routes).
2. **Maintain General Standards**: Keep this file focused exclusively on universal architectural rules, design system tokens, clean architecture layer standards, security patterns, and testing guidelines.
3. **High-Level Rule Enforcement**: Any future updates to `AGENTS.md` must state general behavioral constraints and design rules rather than enumerating specific component implementations.
