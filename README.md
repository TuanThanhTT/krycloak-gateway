# 🔐 KeycloakGateway

Hệ thống WebAPI trung gian tích hợp Keycloak để:

* Đăng nhập lấy access token
* Quản lý người dùng & phân quyền
* Import người dùng từ Excel
* Đồng bộ user giữa các client
* Cung cấp API cho frontend

Dự án được xây dựng theo **Clean Architecture** trên **.NET 9**.

#D:\ThucTapTotNghiep\Test\API\KeycloakGateway
---

# 🏗️ Kiến trúc tổng thể

```
KeycloakGateway.sln
│
└── src
    ├── KeycloakGateway.Api
    ├── KeycloakGateway.Application
    ├── KeycloakGateway.Infrastructure
    └── KeycloakGateway.Domain
```

---

# 📦 Mô tả các layer

## 1️⃣ KeycloakGateway.Api (Presentation Layer)

**Vai trò:** Entry point của hệ thống — expose REST API.

### Cấu trúc

```
KeycloakGateway.Api
│
├── Controllers
│   ├── AuthController.cs          # Login lấy token
│   ├── UsersController.cs         # Tạo user + gán role
│   └── SyncController.cs          # Đồng bộ client
│
├── Middleware
│   └── ExceptionMiddleware.cs     # Global exception handler
│
├── Extensions
│   ├── ServiceExtensions.cs       # Đăng ký DI
│   └── SwaggerExtensions.cs
│
├── Filters
│   └── ValidationFilter.cs        # (optional)
│
├── Program.cs
└── appsettings.json
```

### Nhiệm vụ

✅ Nhận HTTP request
✅ Validate input
✅ Gọi Application layer
✅ Trả response

❌ Không chứa business logic
❌ Không gọi Keycloak trực tiếp

---

## 2️⃣ KeycloakGateway.Application (Business Layer)

**Vai trò:** Định nghĩa contract nghiệp vụ.

### Cấu trúc

```
KeycloakGateway.Application
│
├── Interfaces
│   ├── IKeycloakAuthService.cs
│   ├── IKeycloakUserService.cs
│   ├── IUserImportService.cs
│   └── IUserSyncService.cs
│
├── DTOs
│   ├── Auth
│   │   ├── LoginRequest.cs
│   │   └── TokenResponse.cs
│   │
│   ├── Users
│   │   ├── CreateUserRequest.cs
│   │   └── AssignRoleRequest.cs
│   │
│   └── Import
│       └── ImportUserDto.cs
│
├── Common
│   ├── Result.cs
│   └── Constants.cs
│
└── DependencyInjection.cs (optional)
```

### Nhiệm vụ

✅ Định nghĩa interface
✅ Định nghĩa DTO
✅ Business rules nhẹ

❌ Không gọi HTTP
❌ Không phụ thuộc Infrastructure

---

## 3️⃣ KeycloakGateway.Infrastructure (Integration Layer)

**Vai trò:** Giao tiếp thực tế với Keycloak và Excel.

### Cấu trúc

```
KeycloakGateway.Infrastructure
│
├── Keycloak
│   ├── Services
│   │   ├── KeycloakAuthService.cs
│   │   ├── KeycloakUserService.cs
│   │   └── KeycloakAdminTokenService.cs
│   │
│   ├── Models
│   │   ├── KeycloakTokenResponse.cs
│   │   └── KeycloakUserRepresentation.cs
│   │
│   └── KeycloakOptions.cs
│
├── Excel
│   └── ExcelImportService.cs
│
├── Sync
│   └── UserSyncService.cs
│
├── Http
│   └── HttpClientFactoryExtensions.cs
│
└── DependencyInjection.cs
```

### Nhiệm vụ

✅ Gọi Keycloak Token API
✅ Gọi Keycloak Admin API
✅ Tạo user
✅ Gán role
✅ Import Excel
✅ Đồng bộ client
✅ Quản lý admin token

---

## 4️⃣ KeycloakGateway.Domain (Domain Layer)

**Vai trò:** Entities thuần (future-proof).

### Cấu trúc

```
KeycloakGateway.Domain
│
├── Entities
│   └── GatewayUser.cs (optional)
│
├── Enums
│   └── RoleType.cs
│
└── Common
```

### Khi nào dùng mạnh

* Audit log
* Cache user
* Multi-tenant
* Mapping local user

---

# 🔗 Quy tắc phụ thuộc (QUAN TRỌNG)

```
Api → Application → Domain
Api → Infrastructure
Infrastructure → Application
Domain → (none)
```

❌ Application không được reference Infrastructure
❌ Domain không reference ai

---

# 🔄 Luồng chức năng chính

## 🔐 Login lấy access token

```
POST /api/auth/login
```

Flow:

```
AuthController
   ↓
IKeycloakAuthService
   ↓
KeycloakAuthService
   ↓
Keycloak Token Endpoint
```

---

## 👤 Tạo user + gán quyền

```
POST /api/users
```

Flow:

```
UsersController
   ↓
IKeycloakUserService
   ↓
KeycloakUserService
   ↓
Keycloak Admin API
```

---

## 📥 Import người dùng từ Excel

```
POST /api/users/import
```

Flow:

```
Upload file
   ↓
ExcelImportService
   ↓
Loop create user
   ↓
Assign role
```

**NuGet đề xuất:**

* EPPlus
* hoặc ClosedXML

---

## 🔄 Đồng bộ user giữa các client

```
POST /api/sync/clients
```

Flow:

```
SyncController
   ↓
IUserSyncService
   ↓
UserSyncService
   ↓
Keycloak Admin API
```

---

# ⚙️ Cấu hình Keycloak

Ví dụ trong `appsettings.json`:

```json
"Keycloak": {
  "BaseUrl": "http://localhost:8080",
  "Realm": "sso-realm",
  "ClientId": "admin-cli-test",
  "AdminClientId": "admin-cli",
  "AdminClientSecret": "xxxxx"
}
```

⚠️ **Username/password người dùng KHÔNG lưu trong config**

---

# 🚀 Công nghệ sử dụng

* .NET 9 WebAPI
* Clean Architecture
* HttpClientFactory
* Keycloak Admin REST API
* EPPlus / ClosedXML
* Swagger

---

# 🏁 Trạng thái kiến trúc

✅ Clean Architecture chuẩn
✅ Production-ready
✅ Microservice friendly
✅ Dễ mở rộng
✅ Phù hợp khóa luận
docker compose -f docker-gateway-compose.yml up --build
---
