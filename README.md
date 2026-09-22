# InventorySystemApi - RESTful Web API

A modern, production-ready ASP.NET Core Web API (.NET 10) built to power the Inventory Hub management ecosystem. Provides comprehensive RESTful endpoints for inventory control, product catalog, supplier and category tracking, JWT Bearer authentication, role-based authorization (Admin / Employee), stock movement ledger, analytics, and automated tax / monthly reporting.

---

## 🚀 Key Features

- **Framework & Runtime**: .NET 10 (`net10.0`), ASP.NET Core Web API.
- **Authentication & Authorization**:
  - ASP.NET Core Identity (`ApplicationUser`, Identity Roles: `Admin`, `Employee`).
  - JWT Bearer Authentication with HMAC-SHA256 signature validation and clock skew set to zero.
  - Role-based endpoint guards (`[Authorize(Roles = "Admin")]` on sensitive operations like supplier management and product deletions).
- **Product Management & Image Uploads**:
  - Full CRUD operations with category and supplier relational mapping.
  - Multipart file upload support (`IFormFile`) storing product images in `wwwroot/uploads/products/` with unique GUID prefixes and automatic disk cleanup on update/delete.
  - Filter by category, supplier, low-stock threshold, search query, and pagination.
- **Stock Movement & Audit Ledger**:
  - Atomic stock-in and stock-out operations.
  - Strict validation preventing negative stock levels.
  - Audit trail logging user identity, movement type, timestamp, reference note, and unit costs.
- **Dashboard & Monthly Reporting**:
  - Real-time KPI summaries: Total Products, Units in Stock, Low Stock count, Total Inventory Valuation.
  - 6-month transaction trends, 7-day movement volume, and category inventory breakdown.
  - Monthly financial breakdown with calculated taxable bases and VAT (14%).
- **Interactive Documentation**:
  - Swagger UI / OpenAPI 3.0 configured with JWT Bearer authorization support.

---

## 🛠️ Tech Stack & Packages

- **Target Framework**: .NET 10.0
- **Database**: Microsoft SQL Server via `Microsoft.EntityFrameworkCore.SqlServer` (v10.0.11)
- **Identity**: `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (v10.0.11)
- **JWT**: `Microsoft.AspNetCore.Authentication.JwtBearer` (v10.0.11)
- **Swagger**: `Swashbuckle.AspNetCore` (v6.6.2)

---

## ⚙️ Configuration & Quick Start

### 1. Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- Microsoft SQL Server running locally or in Docker:
  ```bash
  docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=StrongPassword123!" \
     -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
  ```

### 2. Configuration (`appsettings.json`)
Verify connection string and JWT secret parameters:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1,1433;Database=InventorySystemDb;User Id=sa;Password=StrongPassword123!;MultipleActiveResultSets=true;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "SuperSecretKeyForInventorySystemApiProject2026!#SecureJWTBearerTokenGeneratorKey",
    "Issuer": "InventorySystemApi",
    "Audience": "InventorySystemClients",
    "ExpiryInDays": 7
  }
}
```

### 3. Run the API
```bash
dotnet restore
dotnet build
dotnet run
```
The database migrations and default role/user seeds execute automatically on startup.

---

## 🔐 Seeded Credentials

| Role | Email | Password |
|---|---|---|
| **Admin** | `admin@inventory.local` | `Admin123!` |
| **Employee** | `employee@inventory.local` | `Employee123!` |

---

## 📋 API Endpoints Reference

### Authentication (`/api/auth`)
- `POST /api/auth/login` — Authenticate and retrieve JWT token.
- `POST /api/auth/register` — Register a new account (`Admin` role required to create new admins).
- `GET /api/auth/me` — Retrieve authenticated user profile and roles (`[Authorize]`).

### Categories (`/api/categories`)
- `GET /api/categories` — List all categories with product counts.
- `GET /api/categories/{id}` — Get category by ID.
- `POST /api/categories` — Create category (`[Authorize]`).
- `PUT /api/categories/{id}` — Update category (`[Authorize]`).
- `DELETE /api/categories/{id}` — Delete category (`[Authorize(Roles = "Admin")]`).

### Suppliers (`/api/suppliers`)
- `GET /api/suppliers` — List all suppliers with product counts (`[Authorize(Roles = "Admin")]`).
- `GET /api/suppliers/{id}` — Get supplier details (`[Authorize(Roles = "Admin")]`).
- `POST /api/suppliers` — Create new supplier (`[Authorize(Roles = "Admin")]`).
- `PUT /api/suppliers/{id}` — Update supplier (`[Authorize(Roles = "Admin")]`).
- `DELETE /api/suppliers/{id}` — Delete supplier (`[Authorize(Roles = "Admin")]`).

### Products (`/api/products`)
- `GET /api/products` — Filterable product catalog (`searchTerm`, `categoryId`, `supplierId`, `lowStockOnly`, `pageNumber`, `pageSize`).
- `GET /api/products/{id}` — Get product by ID.
- `GET /api/products/low-stock` — Query all items at or below reorder threshold.
- `POST /api/products` — Create product (`multipart/form-data` with optional `ImageFile`).
- `PUT /api/products/{id}` — Update product (`multipart/form-data`).
- `DELETE /api/products/{id}` — Remove product and image (`[Authorize(Roles = "Admin")]`).

### Stock Management (`/api/stock`)
- `GET /api/stock/transactions` — View stock transaction ledger (`productId`, `type`, `pageNumber`, `pageSize`).
- `POST /api/stock/in` — Record incoming inventory batch and update stock level.
- `POST /api/stock/out` — Record outgoing inventory release with negative stock safeguards.

### Reports & Analytics (`/api/reports`)
- `GET /api/reports/dashboard` — Complete executive overview (KPI cards, inventory valuation, 6-month trends, 7-day volume, category distribution).
- `GET /api/reports/monthly` — Monthly financial breakdown, total sales, purchases, taxable amounts, and 14% VAT.

---

## 📖 Swagger Documentation

Once started, open Swagger UI in your browser:
```text
http://localhost:5000/swagger
```
Click the **Authorize** button at the top right, enter `Bearer <your_token>`, and test all protected endpoints directly.
