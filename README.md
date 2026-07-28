# Laundry Management System

Enterprise-grade laundry management platform: ASP.NET Core 10 (Clean Architecture) +
Angular 22 (admin/back-office web app) + .NET MAUI 10 (delivery/staff mobile app) + SQL Server.

## What's in this scaffold

```
LaundryManagement.sln
src/
  LaundryMgmt.Domain/          Entities, enums, guarded Order status pipeline, domain events
  LaundryMgmt.Application/     CQRS (MediatR) — full Orders vertical slice as a template
  LaundryMgmt.Infrastructure/  EF Core + SQL Server, Identity/JWT, Hangfire, audit interceptor
  LaundryMgmt.API/             Controllers, SignalR hub, Swagger, global exception middleware
  LaundryMgmt.Shared/          DTOs shared by API and MAUI (auth contracts)
  client-web/                  Angular 22 app (standalone components, signals, SignalR client)
LaundryMgmt.Mobile/            .NET MAUI app for delivery boys / floor staff
tests/
  LaundryMgmt.UnitTests/       Domain rules (order pipeline transitions)
  LaundryMgmt.IntegrationTests/  WebApplicationFactory + EF Core InMemory
```

This was hand-written rather than generated via `dotnet new` / `ng new`, because this sandbox
has no network access to restore NuGet/npm packages. Everything is standard SDK-style project
structure, so it'll behave exactly like a CLI-scaffolded solution once you restore it locally.

## First-time setup (run these on your own machine)

### 1. Backend
```bash
cd LaundryManagement
dotnet restore
```

Update the connection string in `src/LaundryMgmt.API/appsettings.json`, or better, keep secrets
out of source control:
```bash
cd src/LaundryMgmt.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=LaundryManagementDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Key" "<a real 32+ character random secret>"
```

Create the initial migration and database once you've added an EF Core migrations package
reference is already in Infrastructure:
```bash
cd ../..
dotnet tool install --global dotnet-ef   # if you don't have it
dotnet ef migrations add InitialCreate --project src/LaundryMgmt.Infrastructure --startup-project src/LaundryMgmt.API
dotnet ef database update --project src/LaundryMgmt.Infrastructure --startup-project src/LaundryMgmt.API
```

Run the API:
```bash
dotnet run --project src/LaundryMgmt.API
```
Swagger UI opens at `https://localhost:5101/swagger`.

### 2. Angular web app
```bash
cd client-web
npm install
npm start
```
Runs at `http://localhost:4200` and proxies `/api` and `/hubs` to the API (see `proxy.conf.json`).

### 3. MAUI mobile app
```bash
cd LaundryMgmt.Mobile
dotnet workload install maui   # first time only
dotnet build -t:Run -f net10.0-android    # or -f net10.0-ios / net10.0-maccatalyst / net10.0-windows...
```
Update `MauiProgram.ApiBaseUrl` to point at your running API (`http://10.0.2.2:5100` for the
Android emulator talking to a host machine).

## What's implemented as a full example

The **Orders** module is built end-to-end as the template for every other module:
- Domain: `Order.AdvanceTo()` enforces your exact pipeline (New → Received → Sorting → Washing →
  Drying → Ironing → Packing → ReadyForDelivery → Delivered, with Cancel allowed until Delivered)
- Application: `CreateOrderCommand`, `AdvanceOrderStatusCommand`, `GetOrdersQuery` (MediatR + FluentValidation)
- API: `OrdersController` (`GET/POST /api/v1/orders`, `PATCH /api/v1/orders/{id}/status`)
- Angular: `OrdersService`, `OrderListComponent` with live status updates via SignalR
- MAUI: `OrderQueuePage`/`DeliveryConfirmationPage` for the delivery-boy side (OTP confirmation)
- Tests: domain rule tests for the status pipeline, integration test harness

**Auth** is also wired end-to-end:
- Infrastructure: `ApplicationUser : IdentityUser<Guid>`, ASP.NET Core Identity registered in DI
  (`AddIdentityCore` + EF stores), `JwtTokenService` (access + refresh token generation),
  `IdentityAuthService` bridging Identity to the Application layer without leaking the
  `Microsoft.AspNetCore.Identity` dependency into Application
- Application: `LoginCommand` (MediatR + FluentValidation)
- API: `AuthController` → `POST /api/v1/auth/login`
- Angular/MAUI: both already call this endpoint (`AuthService.login()` / `ApiClient.LoginAsync()`)

**Real-time order updates** are fully wired, not just stubbed:
- `Order.AdvanceTo()` raises `OrderStatusChangedEvent` → `ApplicationDbContext.SaveChangesAsync`
  publishes it via MediatR → `OrderStatusChangedSignalRHandler` (API layer, since it needs
  `IHubContext<OrderStatusHub>`) pushes to both the specific `order-{id}` group and a shared
  `dashboard` group → Angular's `OrderListComponent` and MAUI's `DeliveryOrderHubService` both
  listen for the same `OrderStatusChanged` payload shape

Still missing before login actually works end-to-end: an endpoint/seed data to **create** the
first `ApplicationUser` + assign a role (there's no `RegisterCommand`/seeder yet — add one or
seed a user directly via `UserManager` in a startup hook or EF seed data).

## What still needs building

Everything else from your spec follows the **same pattern** (Domain entity → Application
CQRS handlers → API controller → Angular feature module → tests). Entities for Customer,
Garment, Service, Invoice, Payment, InventoryItem, Employee, Machine, Complaint, and
PickupDelivery already exist in `LaundryMgmt.Domain/Entities` — they just need their own
Application handlers, controllers, and Angular feature folders, following exactly what's
in `Orders/`.

Suggested build order (matches the phased roadmap discussed earlier):
1. ~~Auth~~ ✅ done — add a `RegisterCommand`/user seeder next so there's actually a user to log in as
2. Customer + Garment/Service catalog CRUD
3. Billing/Invoicing + Payments
4. Pickup/Delivery + Barcode/QR + Notifications (Hangfire jobs + Twilio/SendGrid)
5. Inventory, Employee, Machine management
6. Loyalty/Membership, Complaints, Expense tracking
7. Reports dashboard
8. Settings + Audit log viewer

## Notes
- GST is currently a placeholder flat 5% in `CreateOrderCommandHandler` — wire it to
  `Service.GstPercentage` per line item once the Pricing module is built out.
- `INotificationService` is a stub — hook it to Twilio (SMS/WhatsApp) and SendGrid (email),
  queued through Hangfire so a slow provider never blocks a request.
- Refresh tokens are generated but not yet persisted/rotated against the user record — add
  that in `IdentityAuthService` (or a dedicated `RefreshTokens` table) plus a
  `POST /api/v1/auth/refresh` endpoint before relying on them.
