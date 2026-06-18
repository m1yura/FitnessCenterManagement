# Auto Service Management System

A full-stack solution for managing an auto repair shop: SQL Server database with roles, views, triggers, stored procedures, test data, and a C# console application using ADO.NET.

## Project Structure

```
AutoServiceManagement/
├── database/
│   └── autoservice_database.sql    # Single-file database deployment script
├── src/
│   └── AutoServiceManagement/
│       ├── Configuration/          # appsettings.json loader
│       ├── Data/                   # DatabaseHelper (ADO.NET)
│       ├── Models/                 # Entity models
│       ├── Repositories/           # CRUD repositories for all entities
│       ├── Services/               # Reports and analytics
│       ├── UI/                     # Hierarchical console menu
│       ├── Program.cs
│       └── appsettings.json        # Configurable connection string
├── AutoServiceManagement.sln
└── README.md
```

## Requirements

- **SQL Server** 2019+ (Express, Developer, or Standard)
- **.NET SDK** 8.0 or newer
- **Windows** (recommended for SQL Server Express LocalDB)

## Quick Start

### 1. Deploy the Database

Open **SQL Server Management Studio (SSMS)** or use `sqlcmd`:

```powershell
sqlcmd -S localhost -E -i "C:\Users\phara\AutoServiceManagement\database\autoservice_database.sql"
```

For SQL authentication:

```powershell
sqlcmd -S localhost -U sa -P YourPassword -i "database\autoservice_database.sql"
```

This creates database `autoservice_db` with:
- **16 tables** with PK/FK relationships
- **4 database roles** with GRANT/REVOKE permissions
- **5 views** for simplified data access
- **5 triggers** (audit, business rules, stock validation)
- **6 stored procedures** with TRY/CATCH and transactions
- **Test data** (25+ customers, 30 vehicles, 25 work orders, etc.)

### 2. Configure Connection String

Edit `src/AutoServiceManagement/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "AutoServiceDb": "Server=localhost;Database=autoservice_db;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Examples:

| Scenario | Connection String |
|----------|-------------------|
| LocalDB | `Server=(localdb)\\MSSQLLocalDB;Database=autoservice_db;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL Auth | `Server=localhost;Database=autoservice_db;User Id=sa;Password=YourPassword;TrustServerCertificate=True;` |
| Named instance | `Server=localhost\\SQLEXPRESS;Database=autoservice_db;Trusted_Connection=True;TrustServerCertificate=True;` |

You can also change the connection string at runtime via menu **9. Connection Settings**.

### 3. Build and Run

```powershell
cd C:\Users\phara\AutoServiceManagement
dotnet restore
dotnet build
dotnet run --project src\AutoServiceManagement\AutoServiceManagement.csproj
```

## Database Schema

### Entity Relationship Overview

```
roles ──< users
customers ──< vehicles
customers ──< appointments ──< work_orders
customers ──< work_orders ──< work_order_services
                          └──< work_order_parts
                          └──< payments
employees ──< appointments / work_orders
service_categories ──< services
suppliers ──< parts ──< work_order_parts
suppliers ──< part_supplies
audit_log (standalone audit trail)
```

### Tables (16 entities)

| Table | Description |
|-------|-------------|
| `roles` | RBAC roles (administrator, manager, mechanic, guest) |
| `users` | System users linked to roles |
| `customers` | Shop clients with loyalty points |
| `vehicles` | Customer vehicles (VIN, mileage, etc.) |
| `employees` | Mechanics and staff |
| `service_categories` | Service catalog categories |
| `services` | Service catalog with pricing |
| `suppliers` | Parts suppliers |
| `parts` | Inventory parts with stock levels |
| `appointments` | Scheduled visits |
| `work_orders` | Repair orders |
| `work_order_services` | Services performed on orders |
| `work_order_parts` | Parts used on orders |
| `payments` | Payment records |
| `part_supplies` | Stock replenishment deliveries |
| `audit_log` | Change audit trail |

### Extended Attributes (soft delete & metadata)

All main entities include:
- `isactive` — record active flag
- `isdeleted` — soft delete flag
- `createdat` / `updatedat` — timestamps

Additional domain fields examples:
- `customers.loyalty_points`, `preferred_contact`, `notes`
- `vehicles.engine_type`, `fuel_type`, `last_service_date`
- `work_orders.priority`, `discount_percent`, `diagnosis_notes`
- `parts.reorder_level`, `warehouse_location`, `weight_kg`

### Database Roles & Permissions

| Role | Access |
|------|--------|
| `autoservice_admin_role` | Full CRUD + execute all procedures |
| `autoservice_manager_role` | Operational CRUD, no user/role delete |
| `autoservice_mechanic_role` | Work orders, appointments, read-only customers |
| `autoservice_guest_role` | Read-only via views and catalog |

### Views

| View | Purpose |
|------|---------|
| `vw_active_work_orders` | Active repair orders with customer/vehicle info |
| `vw_customer_vehicles` | Customer-vehicle join |
| `vw_employee_workload` | Open orders and upcoming appointments per employee |
| `vw_inventory_status` | Stock status (in_stock / low_stock / out_of_stock) |
| `vw_revenue_summary` | Daily revenue by payment method |

### Stored Procedures

| Procedure | Description |
|-----------|-------------|
| `sp_create_work_order` | Creates work order in transaction with validation |
| `sp_add_service_to_work_order` | Adds service line and recalculates total |
| `sp_process_payment` | Records payment, validates amount, updates status |
| `sp_register_appointment` | Registers appointment with schedule conflict check |
| `sp_receive_part_supply` | Records supply and updates inventory |
| `sp_write_audit_log` | Helper for manual audit entries |

### Triggers

| Trigger | Purpose |
|---------|---------|
| `trg_work_orders_audit` | Logs work order insert/update/delete |
| `trg_payments_audit` | Logs payment changes |
| `trg_work_orders_status_check` | Prevents completing unpaid orders |
| `trg_parts_stock_check` | Validates and decrements stock on part usage |
| `trg_customers_soft_delete` | Logs customer soft delete events |

## Console Application Features

### Hierarchical Menu

1. **Customers & Vehicles** — CRUD for clients and cars
2. **Operations** — Appointments, work orders, payments, stored procedures
3. **Inventory** — Parts, suppliers, supplies, low stock alert
4. **Catalog** — Service categories and services
5. **Administration** — Users, roles, employees
6. **Reports & Analytics** — 6 built-in reports
7. **Audit Log** — View change history
8. **Global Search** — Cross-entity search
9. **Connection Settings** — Runtime connection string update

### CRUD Operations

Full Create, Read, Update, Delete (soft delete) for all 16 entities via ADO.NET repositories using:
- `ExecuteReader` — SELECT queries
- `ExecuteNonQuery` — INSERT/UPDATE/DELETE
- `ExecuteScalar` — Identity returns

### Reports (4+ analytics)

1. Revenue by payment method
2. Work orders by status
3. Inventory status summary
4. Employee workload
5. Top customers by loyalty
6. Monthly revenue trend

## Sample SQL Queries

```sql
-- Active work orders with customer details
select * from vw_active_work_orders where status = 'in_progress';

-- Low stock parts
select * from vw_inventory_status where stock_status = 'low_stock';

-- Employee workload
select * from vw_employee_workload order by open_work_orders desc;

-- Revenue this month
select sum(total_revenue) as monthly_revenue
from vw_revenue_summary
where payment_day >= datefromparts(year(getdate()), month(getdate()), 1);

-- Create work order via stored procedure
declare @id int;
exec sp_create_work_order
    @customer_id = 1,
    @vehicle_id = 1,
    @employee_id = 1,
    @priority = 'normal',
    @customer_complaint = 'strange noise',
    @work_order_id = @id output;
select @id as new_work_order_id;

-- Process payment
declare @payment_id int;
exec sp_process_payment
    @work_order_id = 3,
    @amount = 5000.00,
    @payment_method = 'card',
    @processed_by = 8,
    @transaction_reference = 'txn-test-001',
    @payment_id = @payment_id output;

-- Audit log for work orders
select * from audit_log where table_name = 'work_orders' order by changed_at desc;
```

## Test Data Summary

| Entity | Records |
|--------|---------|
| Customers | 25 |
| Vehicles | 30 |
| Employees | 8 |
| Services | 20 |
| Parts | 20 |
| Appointments | 25 |
| Work Orders | 25 |
| Payments | 14 |
| Part Supplies | 15 |

Default users: `admin`, `manager1`, `mechanic1`, `guest1` (password hashes are placeholders).

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Connection failed | Verify SQL Server is running, database deployed, connection string correct |
| Trigger error on part insert | Ensure sufficient stock in `parts.quantity_in_stock` |
| Cannot complete work order | Full payment required before status `completed` (business rule trigger) |
| `TrustServerCertificate` error | Add `TrustServerCertificate=True` to connection string |

## License

Educational project — free to use and modify.
