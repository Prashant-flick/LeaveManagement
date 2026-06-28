# Step-by-Step Developer Execution Flow

Follow this step-by-step workflow to reset your database inside Docker, automatically generate the schemas and tables, seed the admin user, and verify the authentication setup.

---

## Step 1: Run the Database Reset Script
This script verifies that your Docker Postgres container is running and drops/recreates a clean, blank database.

In the root of the workspace directory, run:
```bash
./reset-db.sh
```

**What this does**:
* Checks if a container named `postgres-db` is running on port `5432`. If not, it starts or creates a new one using the `postgres:latest` image.
* Drops the existing `employeeleave` database and recreates it as a blank database.

---

## Step 2: Configure the Initialization Strategy
Before starting the services for the first time, you must tell them to build the schemas and seed the initial data.

Open your `appsettings.json` (or `appsettings.local.json` if you are using overrides) for all three services and ensure the strategy is set to `"Recreate"`:

```json
"DatabaseSettings": {
  "InitStrategy": "Recreate"
}
```

* **Where to find configs**:
  - `AuthService/Auth.API/appsettings.json`
  - `EmployeeService/Employee.API/appsettings.json`
  - `LeaveService/Leave.API/appsettings.json`

---

## Step 3: Run the Services
Start all three services so they can build their schemas (via EF migrations) and seed the initial values.

Open three separate terminals in the root of the workspace directory and execute:

* **Auth Service**:
  ```bash
  cd AuthService && dotnet run --project Auth.API/Auth.API.csproj
  ```
* **Employee Service**:
  ```bash
  cd EmployeeService && dotnet run --project Employee.API/Employee.API.csproj
  ```
* **Leave Service**:
  ```bash
  cd LeaveService && dotnet run --project Leave.API/Leave.API.csproj
  ```

Once started, you will see logs indicating that the `auth`, `employee`, and `leave` schemas were dropped (if they existed), migrations were applied, and default roles and the admin user/employee were seeded.

---

## Step 4: Revert the Initialization Strategy
To prevent the services from dropping and recreating your database tables every single time you restart them, change the strategy back to `"Update"`:

```json
"DatabaseSettings": {
  "InitStrategy": "Update"
}
```

* **Update Strategy**: When set to `"Update"`, the services will only apply *new* pending migrations (without dropping any data) and safely skip seeding if the admin user or roles are already present in the database.

---

## Step 5: Verify & Login
To verify the setup works, make a `POST` request to log in with the seeded admin credentials.

You can use the following `curl` command:
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@example.com", "password": "AdminPassword123!"}'
```

*(Note: Adjust the port `5000` if your AuthService is running on a different port)*

**Expected Response (`200 OK`)**:
```json
{
  "userId": 1,
  "employeeId": 1,
  "email": "admin@example.com",
  "roles": ["Admin"],
  "token": "eyJhbGciOi...",
  "refreshToken": "A9zH..."
}
```

You can now use the returned JWT `token` in the `Authorization: Bearer <token>` header to access any role-restricted endpoints in `LeaveService` or `EmployeeService`.
