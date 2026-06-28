# JWT Authorization Flow

This document details how the JWT (JSON Web Token) authentication and authorization flow is implemented across the Leave Management System, including how Access Tokens and Refresh Tokens are generated, used, and validated.

---

## 1. Login & Token Generation Flow

When a user logs in, the `AuthService` handles credential verification and issues both an **Access Token** and a **Refresh Token**.

### Step-by-Step Login Flow:
1. **Client Request**: The client sends a `POST /api/auth/login` request containing an `Email` and `Password`.
2. **Credential Verification**: The `AuthService` checks the database for the user and verifies the password hash using `BCrypt`.
3. **Cross-Service Data Fetch**: 
   * The `AuthService` makes an internal HTTP `GET` request to the `EmployeeService` (`/api/employee/roles/{userId}`).
   * It fetches the user's `EmployeeId` and their assigned `Roles` (e.g., Admin, Manager).
4. **Access Token Generation**: 
   * The `JwtTokenService` generates a signed JWT (Access Token).
   * **Claims Included**: `NameIdentifier` (UserId), `Email`, `EmployeeId` (custom claim), and multiple `Role` claims.
   * **Expiration**: The token is valid for 60 minutes (as configured in `appsettings.json`).
5. **Refresh Token Generation**:
   * The `JwtTokenService` generates a cryptographically secure random 64-byte string encoded as Base64.
   * The token is saved in the `auth.RefreshTokens` PostgreSQL database table.
   * **Expiration**: It is set to expire in 7 days (`ExpiresAt = DateTime.UtcNow.AddDays(7)`).
6. **Response**: Both the JWT Access Token and the Refresh Token are returned to the client in the `LoginResponse`.

---

## 2. Resource Access (Authorization Flow)

Once the client has the Access Token, they use it to access protected endpoints in the `EmployeeService` or `LeaveService`.

### Step-by-Step Authorization Flow:
1. **Client Request**: The client adds the Access Token to the HTTP Headers of their request:
   ```http
   Authorization: Bearer <Your-JWT-Access-Token>
   ```
2. **Token Validation**: 
   * The receiving service (e.g., `LeaveService`) receives the request.
   * Its local `JwtBearer` middleware intercepts the request.
   * It validates the token's cryptographic signature using the shared symmetric key (`Jwt:Key` from `appsettings.json`), ensuring it was issued by `AuthService` and hasn't expired.
3. **Role-Based Access Control (RBAC)**:
   * Endpoints decorated with `[Authorize(Roles = "Admin,Manager")]` will automatically check if the JWT contains the required `Role` claim.
   * If the user doesn't have the required role, the API returns `403 Forbidden`.
4. **Extracting User Identity**:
   * For endpoints that need to know *who* the user is (e.g., `POST /api/leave`), the service extracts the `EmployeeId` directly from the validated JWT claims.
   * In `LeaveService`, this is done using your custom extension method: `User.GetEmployeeId()`.

---

## 3. Refresh Token Flow 

Access Tokens have a short lifespan (60 minutes) for security reasons. Refresh Tokens have a longer lifespan (7 days) and are used to get new Access Tokens without forcing the user to log in again.

### How it *should* work:
1. The client's Access Token expires.
2. The client attempts an API call and receives a `401 Unauthorized` response.
3. The client calls a `POST /api/auth/refresh` endpoint, sending their expired Access Token and their valid Refresh Token.
4. The backend validates the Refresh Token against the `RefreshTokens` database table (checking if it matches, hasn't expired, and `IsRevoked == false`).
5. The backend issues a brand new Access Token and a new Refresh Token, returning them to the client.

> [!WARNING]
> **Implementation Gap Identified**:
> While your `AuthService` successfully generates Refresh Tokens and saves them to the database during Login, **there is currently no `/refresh` endpoint implemented in your `AuthController.cs` or `AuthService.cs`** to actually consume the Refresh Token and issue a new Access Token.
> 
> *Action Required*: You will need to implement a `RefreshAsync` method in `IAuthService` and a `POST /api/auth/refresh` endpoint in `AuthController` to complete this flow.

---

## Summary of Lifespans
* **Access Token (JWT)**: 60 Minutes (Short-lived, stateless, carries user roles/identity).
* **Refresh Token**: 7 Days (Long-lived, stateful/stored in DB, used only to get new Access Tokens).
