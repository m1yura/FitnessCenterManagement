using AutoServiceManagement.Configuration;
using AutoServiceManagement.Data;
using AutoServiceManagement.Models;
using AutoServiceManagement.Repositories;
using AutoServiceManagement.Services;
using System.Text;
using System.Text.Json;

namespace AutoServiceManagement.UI;

/// <summary>
/// Hierarchical console menu for auto service management.
/// </summary>
public class MenuManager
{
    private DatabaseHelper _db = new();
    private readonly ReportService _reports;

    private RoleRepository Roles => new(_db);
    private UserRepository Users => new(_db);
    private CustomerRepository Customers => new(_db);
    private VehicleRepository Vehicles => new(_db);
    private EmployeeRepository Employees => new(_db);
    private ServiceCategoryRepository ServiceCategories => new(_db);
    private ServiceRepository Services => new(_db);
    private SupplierRepository Suppliers => new(_db);
    private PartRepository Parts => new(_db);
    private AppointmentRepository Appointments => new(_db);
    private WorkOrderRepository WorkOrders => new(_db);
    private WorkOrderServiceRepository WorkOrderServices => new(_db);
    private WorkOrderPartRepository WorkOrderParts => new(_db);
    private PaymentRepository Payments => new(_db);
    private PartSupplyRepository PartSupplies => new(_db);
    private AuditLogRepository AuditLogs => new(_db);

    public MenuManager()
    {
        _reports = new ReportService(_db);
    }

    public void Run()
    {
        Console.OutputEncoding = Encoding.UTF8;
        PrintHeader();

        if (!_db.TestConnection(out var message))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Database connection failed: {message}");
            Console.ResetColor();
            Console.WriteLine("Update appsettings.json and redeploy the database script.");
            OfferConnectionSetup();
            if (!_db.TestConnection(out message))
            {
                Console.WriteLine("Cannot continue without database. Press any key to exit.");
                Console.ReadKey();
                return;
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== MAIN MENU ===");
            Console.WriteLine("1. Customers & Vehicles");
            Console.WriteLine("2. Operations (Appointments, Work Orders, Payments)");
            Console.WriteLine("3. Inventory (Parts, Suppliers, Supplies)");
            Console.WriteLine("4. Catalog (Services, Categories)");
            Console.WriteLine("5. Administration (Users, Roles, Employees)");
            Console.WriteLine("6. Reports & Analytics");
            Console.WriteLine("7. Audit Log");
            Console.WriteLine("8. Global Search");
            Console.WriteLine("9. Connection Settings");
            Console.WriteLine("0. Exit");
            Console.Write("Select option: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": ShowCustomersMenu(); break;
                case "2": ShowOperationsMenu(); break;
                case "3": ShowInventoryMenu(); break;
                case "4": ShowCatalogMenu(); break;
                case "5": ShowAdminMenu(); break;
                case "6": ShowReportsMenu(); break;
                case "7": ShowAuditLog(); break;
                case "8": GlobalSearch(); break;
                case "9": OfferConnectionSetup(); break;
                case "0": return;
                default: Console.WriteLine("Invalid option."); break;
            }
        }
    }

    private static void PrintHeader()
    {
        Console.WriteLine("=================================================");
        Console.WriteLine("   AUTO SERVICE MANAGEMENT SYSTEM");
        Console.WriteLine("   Console Application (.NET / ADO.NET)");
        Console.WriteLine("=================================================");
    }

    private void ShowCustomersMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- CUSTOMERS & VEHICLES ---");
            Console.WriteLine("1. Customers CRUD");
            Console.WriteLine("2. Vehicles CRUD");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": RunCrudMenu("Customers", ListCustomers, ViewCustomer, CreateCustomer, EditCustomer, DeleteCustomer, SearchCustomers); break;
                case "2": RunCrudMenu("Vehicles", ListVehicles, ViewVehicle, CreateVehicle, EditVehicle, DeleteVehicle, SearchVehicles); break;
                case "0": return;
            }
        }
    }

    private void ShowOperationsMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- OPERATIONS ---");
            Console.WriteLine("1. Appointments CRUD");
            Console.WriteLine("2. Work Orders CRUD");
            Console.WriteLine("3. Work Order Services CRUD");
            Console.WriteLine("4. Work Order Parts CRUD");
            Console.WriteLine("5. Payments CRUD");
            Console.WriteLine("6. Create Work Order (stored procedure)");
            Console.WriteLine("7. Process Payment (stored procedure)");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": RunCrudMenu("Appointments", ListAppointments, ViewAppointment, CreateAppointment, EditAppointment, DeleteAppointment, SearchAppointments); break;
                case "2": RunCrudMenu("Work Orders", ListWorkOrders, ViewWorkOrder, CreateWorkOrder, EditWorkOrder, DeleteWorkOrder, SearchWorkOrders); break;
                case "3": RunCrudMenu("Work Order Services", ListWorkOrderServices, ViewWorkOrderService, CreateWorkOrderService, EditWorkOrderService, DeleteWorkOrderService, null); break;
                case "4": RunCrudMenu("Work Order Parts", ListWorkOrderParts, ViewWorkOrderPart, CreateWorkOrderPart, EditWorkOrderPart, DeleteWorkOrderPart, null); break;
                case "5": RunCrudMenu("Payments", ListPayments, ViewPayment, CreatePayment, EditPayment, DeletePayment, SearchPayments); break;
                case "6": CreateWorkOrderViaSp(); break;
                case "7": ProcessPaymentViaSp(); break;
                case "0": return;
            }
        }
    }

    private void ShowInventoryMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- INVENTORY ---");
            Console.WriteLine("1. Parts CRUD");
            Console.WriteLine("2. Suppliers CRUD");
            Console.WriteLine("3. Part Supplies CRUD");
            Console.WriteLine("4. Low Stock Parts");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": RunCrudMenu("Parts", ListParts, ViewPart, CreatePart, EditPart, DeletePart, SearchParts); break;
                case "2": RunCrudMenu("Suppliers", ListSuppliers, ViewSupplier, CreateSupplier, EditSupplier, DeleteSupplier, SearchSuppliers); break;
                case "3": RunCrudMenu("Part Supplies", ListPartSupplies, ViewPartSupply, CreatePartSupply, EditPartSupply, DeletePartSupply, SearchPartSupplies); break;
                case "4": ListLowStockParts(); break;
                case "0": return;
            }
        }
    }

    private void ShowCatalogMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- CATALOG ---");
            Console.WriteLine("1. Service Categories CRUD");
            Console.WriteLine("2. Services CRUD");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": RunCrudMenu("Service Categories", ListCategories, ViewCategory, CreateCategory, EditCategory, DeleteCategory, SearchCategories); break;
                case "2": RunCrudMenu("Services", ListServices, ViewService, CreateService, EditService, DeleteService, SearchServices); break;
                case "0": return;
            }
        }
    }

    private void ShowAdminMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- ADMINISTRATION ---");
            Console.WriteLine("1. Roles CRUD");
            Console.WriteLine("2. Users CRUD");
            Console.WriteLine("3. Employees CRUD");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": RunCrudMenu("Roles", ListRoles, ViewRole, CreateRole, EditRole, DeleteRole, SearchRoles); break;
                case "2": RunCrudMenu("Users", ListUsers, ViewUser, CreateUser, EditUser, DeleteUser, SearchUsers); break;
                case "3": RunCrudMenu("Employees", ListEmployees, ViewEmployee, CreateEmployee, EditEmployee, DeleteEmployee, SearchEmployees); break;
                case "0": return;
            }
        }
    }

    private void ShowReportsMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- REPORTS & ANALYTICS ---");
            Console.WriteLine("1. Revenue by Payment Method");
            Console.WriteLine("2. Work Orders by Status");
            Console.WriteLine("3. Inventory Status Summary");
            Console.WriteLine("4. Employee Workload");
            Console.WriteLine("5. Top Customers");
            Console.WriteLine("6. Monthly Revenue Trend");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": PrintReport("Revenue by Payment Method", _reports.GetRevenueByPaymentMethod()); break;
                case "2": PrintReport("Work Orders by Status", _reports.GetWorkOrdersByStatus()); break;
                case "3": PrintReport("Inventory Status", _reports.GetInventoryStatusSummary()); break;
                case "4": PrintReport("Employee Workload", _reports.GetEmployeeWorkload()); break;
                case "5": PrintReport("Top Customers", _reports.GetTopCustomers()); break;
                case "6": PrintReport("Monthly Revenue", _reports.GetMonthlyRevenue()); break;
                case "0": return;
            }
        }
    }

    private delegate void ListAction(string? filter = null);
    private delegate void ViewAction(int id);
    private delegate void CreateAction();
    private delegate void EditAction();
    private delegate void DeleteAction();
    private delegate void SearchAction();

    private static void RunCrudMenu(string title, ListAction list, ViewAction view, CreateAction create, EditAction edit, DeleteAction delete, SearchAction? search)
    {
        while (true)
        {
            Console.WriteLine($"\n--- {title.ToUpper()} ---");
            Console.WriteLine("1. List all");
            Console.WriteLine("2. View by ID");
            Console.WriteLine("3. Create");
            Console.WriteLine("4. Update");
            Console.WriteLine("5. Soft delete");
            if (search != null) Console.WriteLine("6. Search / Filter");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": list(); break;
                case "2":
                    if (TryReadInt("Enter ID", out var viewId)) view(viewId);
                    break;
                case "3": create(); break;
                case "4": edit(); break;
                case "5": delete(); break;
                case "6" when search != null: search(); break;
                case "0": return;
            }
        }
    }

    private static void PrintReport(string title, List<ReportRow> rows)
    {
        Console.WriteLine($"\n=== {title.ToUpper()} ===");
        if (rows.Count == 0)
        {
            Console.WriteLine("No data.");
            return;
        }

        foreach (var row in rows)
            Console.WriteLine($"{row.Label,-30} {row.Value}");
    }

    private void ShowAuditLog()
    {
        Console.Write("Filter by table name (optional): ");
        var table = Console.ReadLine()?.Trim();
        var logs = AuditLogs.GetAll(string.IsNullOrWhiteSpace(table) ? null : table);
        Console.WriteLine($"\n=== AUDIT LOG ({logs.Count} records) ===");
        foreach (var log in logs.Take(50))
        {
            Console.WriteLine($"[{log.ChangedAt:yyyy-MM-dd HH:mm}] {log.TableName} #{log.RecordId} {log.ActionType} by {log.ChangedBy}");
            if (!string.IsNullOrWhiteSpace(log.NewValues)) Console.WriteLine($"  new: {log.NewValues}");
            if (!string.IsNullOrWhiteSpace(log.OldValues)) Console.WriteLine($"  old: {log.OldValues}");
        }
    }

    private void GlobalSearch()
    {
        Console.Write("Enter search term: ");
        var term = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(term)) return;

        Console.WriteLine("\n--- SEARCH RESULTS ---");
        Console.WriteLine($"Customers: {Customers.GetAll(term).Count}");
        Console.WriteLine($"Vehicles: {Vehicles.GetAll(term).Count}");
        Console.WriteLine($"Work Orders: {WorkOrders.GetAll(term).Count}");
        Console.WriteLine($"Parts: {Parts.GetAll(term).Count}");
        Console.WriteLine($"Services: {Services.GetAll(term).Count}");
        Console.WriteLine($"Employees: {Employees.GetAll(term).Count}");

        Console.Write("\nShow details for (customers/vehicles/workorders/parts/services/employees/none): ");
        switch (Console.ReadLine()?.Trim().ToLowerInvariant())
        {
            case "customers": ListCustomers(term); break;
            case "vehicles": ListVehicles(term); break;
            case "workorders": ListWorkOrders(term); break;
            case "parts": ListParts(term); break;
            case "services": ListServices(term); break;
            case "employees": ListEmployees(term); break;
        }
    }

    private void OfferConnectionSetup()
    {
        Console.WriteLine("\nCurrent connection uses appsettings.json.");
        Console.Write("Enter new connection string (leave empty to keep current): ");
        var cs = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(cs)) return;

        UpdateConnectionString(cs);
        _db = new DatabaseHelper(cs);
        Console.WriteLine("Connection string updated in appsettings.json.");
    }

    private static void UpdateConnectionString(string connectionString)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var json = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(json);
        var root = new Dictionary<string, object>();
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            if (prop.NameEquals("ConnectionStrings"))
            {
                root["ConnectionStrings"] = new Dictionary<string, string>
                {
                    ["AutoServiceDb"] = connectionString
                };
            }
            else if (prop.NameEquals("AppSettings"))
            {
                root["AppSettings"] = JsonSerializer.Deserialize<Dictionary<string, object>>(prop.Value.GetRawText())!;
            }
            else
            {
                root[prop.Name] = JsonSerializer.Deserialize<object>(prop.Value.GetRawText())!;
            }
        }

        File.WriteAllText(path, JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true }));
    }

    #region List/View helpers

    private void ListRoles(string? s = null) { foreach (var r in Roles.GetAll(s)) Console.WriteLine($"{r.RoleId}: {r.RoleName} (level {r.PermissionLevel})"); }
    private void ViewRole(int id) { var r = Roles.GetById(id); Console.WriteLine(r == null ? "Not found." : $"{r.RoleName} - {r.RoleDescription}"); }

    private void ListUsers(string? s = null) { foreach (var u in Users.GetAll(s)) Console.WriteLine($"{u.UserId}: {u.Username} ({u.RoleName}) {u.Email}"); }
    private void ViewUser(int id) { var u = Users.GetById(id); Console.WriteLine(u == null ? "Not found." : $"{u.Username}, role: {u.RoleName}, active: {u.IsActive}"); }

    private void ListCustomers(string? s = null) { foreach (var c in Customers.GetAll(s)) Console.WriteLine($"{c.CustomerId}: {c.FirstName} {c.LastName}, {c.Phone}, points: {c.LoyaltyPoints}"); }
    private void ViewCustomer(int id) { var c = Customers.GetById(id); Console.WriteLine(c == null ? "Not found." : $"{c.FirstName} {c.LastName}, {c.Phone}, {c.Email}, {c.City}"); }

    private void ListVehicles(string? s = null) { foreach (var v in Vehicles.GetAll(s)) Console.WriteLine($"{v.VehicleId}: {v.Make} {v.Model} ({v.LicensePlate}) owner: {v.CustomerName}"); }
    private void ViewVehicle(int id) { var v = Vehicles.GetById(id); Console.WriteLine(v == null ? "Not found." : $"{v.Make} {v.Model} {v.Year}, VIN: {v.Vin}, mileage: {v.Mileage}"); }

    private void ListEmployees(string? s = null) { foreach (var e in Employees.GetAll(s)) Console.WriteLine($"{e.EmployeeId}: {e.FirstName} {e.LastName} - {e.Position}"); }
    private void ViewEmployee(int id) { var e = Employees.GetById(id); Console.WriteLine(e == null ? "Not found." : $"{e.FirstName} {e.LastName}, {e.Position}, rate: {e.HourlyRate:N2}"); }

    private void ListCategories(string? s = null) { foreach (var c in ServiceCategories.GetAll(s)) Console.WriteLine($"{c.CategoryId}: {c.CategoryName}"); }
    private void ViewCategory(int id) { var c = ServiceCategories.GetById(id); Console.WriteLine(c == null ? "Not found." : $"{c.CategoryName} - {c.Description}"); }

    private void ListServices(string? s = null) { foreach (var sItem in Services.GetAll(s)) Console.WriteLine($"{sItem.ServiceId}: {sItem.ServiceName} - {sItem.BasePrice:N2} ({sItem.CategoryName})"); }
    private void ViewService(int id) { var s = Services.GetById(id); Console.WriteLine(s == null ? "Not found." : $"{s.ServiceName}, price: {s.BasePrice:N2}, duration: {s.EstimatedDurationMinutes} min"); }

    private void ListSuppliers(string? s = null) { foreach (var s in Suppliers.GetAll(s)) Console.WriteLine($"{s.SupplierId}: {s.SupplierName}, {s.Phone}"); }
    private void ViewSupplier(int id) { var s = Suppliers.GetById(id); Console.WriteLine(s == null ? "Not found." : $"{s.SupplierName}, rating: {s.Rating}"); }

    private void ListParts(string? s = null) { foreach (var p in Parts.GetAll(s)) Console.WriteLine($"{p.PartId}: {p.PartNumber} - {p.PartName}, stock: {p.QuantityInStock}"); }
    private void ViewPart(int id) { var p = Parts.GetById(id); Console.WriteLine(p == null ? "Not found." : $"{p.PartName}, stock: {p.QuantityInStock}, price: {p.UnitPrice:N2}"); }

    private void ListAppointments(string? s = null) { foreach (var a in Appointments.GetAll(s)) Console.WriteLine($"{a.AppointmentId}: {a.AppointmentDate:yyyy-MM-dd HH:mm} {a.CustomerName} - {a.Status}"); }
    private void ViewAppointment(int id) { var a = Appointments.GetById(id); Console.WriteLine(a == null ? "Not found." : $"{a.AppointmentDate}, {a.CustomerName}, {a.VehicleInfo}, reason: {a.Reason}"); }

    private void ListWorkOrders(string? s = null) { foreach (var w in WorkOrders.GetAll(s)) Console.WriteLine($"{w.WorkOrderId}: {w.OrderNumber} [{w.Status}] {w.CustomerName} - {w.TotalAmount:N2}"); }
    private void ViewWorkOrder(int id) { var w = WorkOrders.GetById(id); Console.WriteLine(w == null ? "Not found." : $"{w.OrderNumber}, status: {w.Status}, total: {w.TotalAmount:N2}, complaint: {w.CustomerComplaint}"); }

    private void ListWorkOrderServices(string? _ = null)
    {
        Console.Write("Work Order ID (empty = all): ");
        int? woId = int.TryParse(Console.ReadLine(), out var id) ? id : null;
        foreach (var item in WorkOrderServices.GetAll(woId))
            Console.WriteLine($"{item.WorkOrderServiceId}: WO#{item.WorkOrderId} {item.ServiceName} x{item.Quantity} = {item.LineTotal:N2}");
    }

    private void ViewWorkOrderService(int id) { var item = WorkOrderServices.GetById(id); Console.WriteLine(item == null ? "Not found." : $"{item.ServiceName}, total: {item.LineTotal:N2}"); }

    private void ListWorkOrderParts(string? _ = null)
    {
        Console.Write("Work Order ID (empty = all): ");
        int? woId = int.TryParse(Console.ReadLine(), out var id) ? id : null;
        foreach (var item in WorkOrderParts.GetAll(woId))
            Console.WriteLine($"{item.WorkOrderPartId}: WO#{item.WorkOrderId} {item.PartName} x{item.Quantity} = {item.LineTotal:N2}");
    }

    private void ViewWorkOrderPart(int id) { var item = WorkOrderParts.GetById(id); Console.WriteLine(item == null ? "Not found." : $"{item.PartName}, total: {item.LineTotal:N2}"); }

    private void ListPayments(string? s = null) { foreach (var p in Payments.GetAll(s)) Console.WriteLine($"{p.PaymentId}: {p.OrderNumber} {p.Amount:N2} via {p.PaymentMethod}"); }
    private void ViewPayment(int id) { var p = Payments.GetById(id); Console.WriteLine(p == null ? "Not found." : $"{p.OrderNumber}, {p.Amount:N2}, ref: {p.TransactionReference}"); }

    private void ListPartSupplies(string? s = null) { foreach (var ps in PartSupplies.GetAll(s)) Console.WriteLine($"{ps.SupplyId}: {ps.PartName} x{ps.Quantity} from {ps.SupplierName}"); }
    private void ViewPartSupply(int id) { var ps = PartSupplies.GetById(id); Console.WriteLine(ps == null ? "Not found." : $"{ps.PartName}, qty: {ps.Quantity}, cost: {ps.TotalCost:N2}"); }

    private void ListLowStockParts() { foreach (var p in Parts.GetAll(stockStatus: "low")) Console.WriteLine($"{p.PartId}: {p.PartName}, stock: {p.QuantityInStock}, reorder: {p.ReorderLevel}"); }

    #endregion

    #region Create/Edit/Delete/Search

    private void CreateRole()
    {
        var role = new Role();
        role.RoleName = ReadRequired("Role name");
        role.RoleDescription = ReadOptional("Description");
        role.PermissionLevel = ReadInt("Permission level", 1);
        Console.WriteLine($"Created role ID: {Roles.Create(role)}");
    }

    private void EditRole()
    {
        if (!TryReadInt("Role ID", out var id)) return;
        var role = Roles.GetById(id);
        if (role == null) { Console.WriteLine("Not found."); return; }
        role.RoleName = ReadOptional($"Role name [{role.RoleName}]") ?? role.RoleName;
        role.PermissionLevel = ReadInt($"Permission level [{role.PermissionLevel}]", role.PermissionLevel);
        Console.WriteLine(Roles.Update(role) ? "Updated." : "Update failed.");
    }

    private void DeleteRole() { if (TryReadInt("Role ID", out var id)) Console.WriteLine(Roles.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchRoles() { Console.Write("Search: "); ListRoles(Console.ReadLine()); }

    private void CreateUser()
    {
        var user = new User
        {
            Username = ReadRequired("Username"),
            PasswordHash = ReadRequired("Password hash"),
            Email = ReadRequired("Email"),
            RoleId = ReadInt("Role ID", 2)
        };
        Console.WriteLine($"Created user ID: {Users.Create(user)}");
    }

    private void EditUser()
    {
        if (!TryReadInt("User ID", out var id)) return;
        var user = Users.GetById(id);
        if (user == null) { Console.WriteLine("Not found."); return; }
        user.Email = ReadOptional($"Email [{user.Email}]") ?? user.Email;
        user.RoleId = ReadInt($"Role ID [{user.RoleId}]", user.RoleId);
        user.IsActive = ReadBool($"Active [{user.IsActive}]", user.IsActive);
        Console.WriteLine(Users.Update(user) ? "Updated." : "Update failed.");
    }

    private void DeleteUser() { if (TryReadInt("User ID", out var id)) Console.WriteLine(Users.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchUsers() { Console.Write("Search: "); ListUsers(Console.ReadLine()); }

    private void CreateCustomer()
    {
        var c = new Customer
        {
            FirstName = ReadRequired("First name"),
            LastName = ReadRequired("Last name"),
            Phone = ReadRequired("Phone"),
            Email = ReadOptional("Email"),
            City = ReadOptional("City"),
            LoyaltyPoints = ReadInt("Loyalty points", 0)
        };
        Console.WriteLine($"Created customer ID: {Customers.Create(c)}");
    }

    private void EditCustomer()
    {
        if (!TryReadInt("Customer ID", out var id)) return;
        var c = Customers.GetById(id);
        if (c == null) { Console.WriteLine("Not found."); return; }
        c.Phone = ReadOptional($"Phone [{c.Phone}]") ?? c.Phone;
        c.LoyaltyPoints = ReadInt($"Loyalty points [{c.LoyaltyPoints}]", c.LoyaltyPoints);
        c.Notes = ReadOptional($"Notes [{c.Notes}]") ?? c.Notes;
        Console.WriteLine(Customers.Update(c) ? "Updated." : "Update failed.");
    }

    private void DeleteCustomer() { if (TryReadInt("Customer ID", out var id)) Console.WriteLine(Customers.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchCustomers() { Console.Write("Search: "); ListCustomers(Console.ReadLine()); }

    private void CreateVehicle()
    {
        var v = new Vehicle
        {
            CustomerId = ReadInt("Customer ID", 1),
            Make = ReadRequired("Make"),
            Model = ReadRequired("Model"),
            Year = ReadInt("Year", DateTime.Now.Year),
            Vin = ReadRequired("VIN"),
            LicensePlate = ReadRequired("License plate"),
            Mileage = ReadInt("Mileage", 0)
        };
        Console.WriteLine($"Created vehicle ID: {Vehicles.Create(v)}");
    }

    private void EditVehicle()
    {
        if (!TryReadInt("Vehicle ID", out var id)) return;
        var v = Vehicles.GetById(id);
        if (v == null) { Console.WriteLine("Not found."); return; }
        v.Mileage = ReadInt($"Mileage [{v.Mileage}]", v.Mileage);
        v.Color = ReadOptional($"Color [{v.Color}]") ?? v.Color;
        Console.WriteLine(Vehicles.Update(v) ? "Updated." : "Update failed.");
    }

    private void DeleteVehicle() { if (TryReadInt("Vehicle ID", out var id)) Console.WriteLine(Vehicles.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchVehicles() { Console.Write("Search: "); ListVehicles(Console.ReadLine()); }

    private void CreateEmployee()
    {
        var e = new Employee
        {
            FirstName = ReadRequired("First name"),
            LastName = ReadRequired("Last name"),
            Phone = ReadRequired("Phone"),
            Email = ReadRequired("Email"),
            Position = ReadRequired("Position"),
            HireDate = ReadDate("Hire date", DateTime.Today),
            HourlyRate = ReadDecimal("Hourly rate", 500)
        };
        Console.WriteLine($"Created employee ID: {Employees.Create(e)}");
    }

    private void EditEmployee()
    {
        if (!TryReadInt("Employee ID", out var id)) return;
        var e = Employees.GetById(id);
        if (e == null) { Console.WriteLine("Not found."); return; }
        e.HourlyRate = ReadDecimal($"Hourly rate [{e.HourlyRate}]", e.HourlyRate);
        e.Specialization = ReadOptional($"Specialization [{e.Specialization}]") ?? e.Specialization;
        Console.WriteLine(Employees.Update(e) ? "Updated." : "Update failed.");
    }

    private void DeleteEmployee() { if (TryReadInt("Employee ID", out var id)) Console.WriteLine(Employees.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchEmployees() { Console.Write("Search: "); ListEmployees(Console.ReadLine()); }

    private void CreateCategory()
    {
        var c = new ServiceCategory { CategoryName = ReadRequired("Category name"), Description = ReadOptional("Description"), DisplayOrder = ReadInt("Display order", 0) };
        Console.WriteLine($"Created category ID: {ServiceCategories.Create(c)}");
    }

    private void EditCategory()
    {
        if (!TryReadInt("Category ID", out var id)) return;
        var c = ServiceCategories.GetById(id);
        if (c == null) { Console.WriteLine("Not found."); return; }
        c.CategoryName = ReadOptional($"Name [{c.CategoryName}]") ?? c.CategoryName;
        Console.WriteLine(ServiceCategories.Update(c) ? "Updated." : "Update failed.");
    }

    private void DeleteCategory() { if (TryReadInt("Category ID", out var id)) Console.WriteLine(ServiceCategories.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchCategories() { Console.Write("Search: "); ListCategories(Console.ReadLine()); }

    private void CreateService()
    {
        var s = new ServiceItem
        {
            CategoryId = ReadInt("Category ID", 1),
            ServiceName = ReadRequired("Service name"),
            BasePrice = ReadDecimal("Base price", 1000),
            EstimatedDurationMinutes = ReadInt("Duration minutes", 60)
        };
        Console.WriteLine($"Created service ID: {Services.Create(s)}");
    }

    private void EditService()
    {
        if (!TryReadInt("Service ID", out var id)) return;
        var s = Services.GetById(id);
        if (s == null) { Console.WriteLine("Not found."); return; }
        s.BasePrice = ReadDecimal($"Base price [{s.BasePrice}]", s.BasePrice);
        Console.WriteLine(Services.Update(s) ? "Updated." : "Update failed.");
    }

    private void DeleteService() { if (TryReadInt("Service ID", out var id)) Console.WriteLine(Services.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchServices() { Console.Write("Search: "); ListServices(Console.ReadLine()); }

    private void CreateSupplier()
    {
        var s = new Supplier { SupplierName = ReadRequired("Supplier name"), Phone = ReadRequired("Phone"), Email = ReadOptional("Email") };
        Console.WriteLine($"Created supplier ID: {Suppliers.Create(s)}");
    }

    private void EditSupplier()
    {
        if (!TryReadInt("Supplier ID", out var id)) return;
        var s = Suppliers.GetById(id);
        if (s == null) { Console.WriteLine("Not found."); return; }
        s.Phone = ReadOptional($"Phone [{s.Phone}]") ?? s.Phone;
        Console.WriteLine(Suppliers.Update(s) ? "Updated." : "Update failed.");
    }

    private void DeleteSupplier() { if (TryReadInt("Supplier ID", out var id)) Console.WriteLine(Suppliers.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchSuppliers() { Console.Write("Search: "); ListSuppliers(Console.ReadLine()); }

    private void CreatePart()
    {
        var p = new Part
        {
            SupplierId = ReadInt("Supplier ID", 1),
            PartNumber = ReadRequired("Part number"),
            PartName = ReadRequired("Part name"),
            UnitPrice = ReadDecimal("Unit price", 100),
            QuantityInStock = ReadInt("Quantity", 0),
            ReorderLevel = ReadInt("Reorder level", 5)
        };
        Console.WriteLine($"Created part ID: {Parts.Create(p)}");
    }

    private void EditPart()
    {
        if (!TryReadInt("Part ID", out var id)) return;
        var p = Parts.GetById(id);
        if (p == null) { Console.WriteLine("Not found."); return; }
        p.QuantityInStock = ReadInt($"Stock [{p.QuantityInStock}]", p.QuantityInStock);
        p.UnitPrice = ReadDecimal($"Price [{p.UnitPrice}]", p.UnitPrice);
        Console.WriteLine(Parts.Update(p) ? "Updated." : "Update failed.");
    }

    private void DeletePart() { if (TryReadInt("Part ID", out var id)) Console.WriteLine(Parts.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchParts() { Console.Write("Search: "); ListParts(Console.ReadLine()); }

    private void CreateAppointment()
    {
        var a = new Appointment
        {
            CustomerId = ReadInt("Customer ID", 1),
            VehicleId = ReadInt("Vehicle ID", 1),
            EmployeeId = ReadNullableInt("Employee ID"),
            AppointmentDate = ReadDateTime("Appointment date (yyyy-MM-dd HH:mm)", DateTime.Now.AddDays(1)),
            Reason = ReadOptional("Reason"),
            DurationMinutes = ReadInt("Duration minutes", 60)
        };
        Console.WriteLine($"Created appointment ID: {Appointments.Create(a)}");
    }

    private void EditAppointment()
    {
        if (!TryReadInt("Appointment ID", out var id)) return;
        var a = Appointments.GetById(id);
        if (a == null) { Console.WriteLine("Not found."); return; }
        a.Status = ReadOptional($"Status [{a.Status}]") ?? a.Status;
        a.Notes = ReadOptional($"Notes [{a.Notes}]") ?? a.Notes;
        Console.WriteLine(Appointments.Update(a) ? "Updated." : "Update failed.");
    }

    private void DeleteAppointment() { if (TryReadInt("Appointment ID", out var id)) Console.WriteLine(Appointments.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchAppointments()
    {
        Console.Write("Status filter (scheduled/completed/cancelled, empty=all): ");
        ListAppointments(Console.ReadLine());
    }

    private void CreateWorkOrder()
    {
        var wo = new WorkOrder
        {
            CustomerId = ReadInt("Customer ID", 1),
            VehicleId = ReadInt("Vehicle ID", 1),
            EmployeeId = ReadNullableInt("Employee ID"),
            OrderNumber = ReadRequired("Order number"),
            Priority = ReadOptional("Priority (normal/high/low)") ?? "normal",
            CustomerComplaint = ReadOptional("Customer complaint")
        };
        Console.WriteLine($"Created work order ID: {WorkOrders.Create(wo)}");
    }

    private void EditWorkOrder()
    {
        if (!TryReadInt("Work Order ID", out var id)) return;
        var wo = WorkOrders.GetById(id);
        if (wo == null) { Console.WriteLine("Not found."); return; }
        wo.Status = ReadOptional($"Status [{wo.Status}]") ?? wo.Status;
        wo.DiagnosisNotes = ReadOptional($"Diagnosis [{wo.DiagnosisNotes}]") ?? wo.DiagnosisNotes;
        try
        {
            Console.WriteLine(WorkOrders.Update(wo) ? "Updated." : "Update failed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void DeleteWorkOrder() { if (TryReadInt("Work Order ID", out var id)) Console.WriteLine(WorkOrders.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchWorkOrders()
    {
        Console.Write("Status filter (open/in_progress/completed, empty=all): ");
        var status = Console.ReadLine()?.Trim();
        Console.Write("Search term: ");
        var term = Console.ReadLine()?.Trim();
        foreach (var w in WorkOrders.GetAll(term, string.IsNullOrWhiteSpace(status) ? null : status))
            Console.WriteLine($"{w.WorkOrderId}: {w.OrderNumber} [{w.Status}]");
    }

    private void CreateWorkOrderService()
    {
        var item = new WorkOrderService
        {
            WorkOrderId = ReadInt("Work Order ID", 1),
            ServiceId = ReadInt("Service ID", 1),
            Quantity = ReadInt("Quantity", 1),
            UnitPrice = ReadDecimal("Unit price", 1000),
            LineTotal = ReadDecimal("Line total", 1000)
        };
        Console.WriteLine($"Created line ID: {WorkOrderServices.Create(item)}");
    }

    private void EditWorkOrderService()
    {
        if (!TryReadInt("Line ID", out var id)) return;
        var item = WorkOrderServices.GetById(id);
        if (item == null) { Console.WriteLine("Not found."); return; }
        item.Quantity = ReadInt($"Quantity [{item.Quantity}]", item.Quantity);
        item.LineTotal = item.UnitPrice * item.Quantity - item.DiscountAmount;
        Console.WriteLine(WorkOrderServices.Update(item) ? "Updated." : "Update failed.");
    }

    private void DeleteWorkOrderService() { if (TryReadInt("Line ID", out var id)) Console.WriteLine(WorkOrderServices.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }

    private void CreateWorkOrderPart()
    {
        var item = new WorkOrderPart
        {
            WorkOrderId = ReadInt("Work Order ID", 1),
            PartId = ReadInt("Part ID", 1),
            Quantity = ReadInt("Quantity", 1),
            UnitPrice = ReadDecimal("Unit price", 500),
            LineTotal = ReadDecimal("Line total", 500)
        };
        try
        {
            Console.WriteLine($"Created line ID: {WorkOrderParts.Create(item)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error (check stock): {ex.Message}");
        }
    }

    private void EditWorkOrderPart()
    {
        if (!TryReadInt("Line ID", out var id)) return;
        var item = WorkOrderParts.GetById(id);
        if (item == null) { Console.WriteLine("Not found."); return; }
        item.Quantity = ReadInt($"Quantity [{item.Quantity}]", item.Quantity);
        item.LineTotal = item.UnitPrice * item.Quantity;
        Console.WriteLine(WorkOrderParts.Update(item) ? "Updated." : "Update failed.");
    }

    private void DeleteWorkOrderPart() { if (TryReadInt("Line ID", out var id)) Console.WriteLine(WorkOrderParts.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }

    private void CreatePayment()
    {
        var p = new Payment
        {
            WorkOrderId = ReadInt("Work Order ID", 1),
            Amount = ReadDecimal("Amount", 1000),
            PaymentMethod = ReadRequired("Payment method (cash/card/bank_transfer)"),
            TransactionReference = ReadOptional("Transaction reference"),
            ProcessedBy = ReadNullableInt("Processed by employee ID")
        };
        try
        {
            Console.WriteLine($"Created payment ID: {Payments.Create(p)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void EditPayment()
    {
        if (!TryReadInt("Payment ID", out var id)) return;
        var p = Payments.GetById(id);
        if (p == null) { Console.WriteLine("Not found."); return; }
        p.Notes = ReadOptional($"Notes [{p.Notes}]") ?? p.Notes;
        Console.WriteLine(Payments.Update(p) ? "Updated." : "Update failed.");
    }

    private void DeletePayment() { if (TryReadInt("Payment ID", out var id)) Console.WriteLine(Payments.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchPayments() { Console.Write("Search: "); ListPayments(Console.ReadLine()); }

    private void CreatePartSupply()
    {
        var ps = new PartSupply
        {
            SupplierId = ReadInt("Supplier ID", 1),
            PartId = ReadInt("Part ID", 1),
            SupplyDate = ReadDate("Supply date", DateTime.Today),
            Quantity = ReadInt("Quantity", 1),
            UnitCost = ReadDecimal("Unit cost", 100),
            TotalCost = ReadDecimal("Total cost", 100),
            InvoiceNumber = ReadOptional("Invoice number")
        };
        Console.WriteLine($"Created supply ID: {PartSupplies.Create(ps)}");
    }

    private void EditPartSupply()
    {
        if (!TryReadInt("Supply ID", out var id)) return;
        var ps = PartSupplies.GetById(id);
        if (ps == null) { Console.WriteLine("Not found."); return; }
        ps.Status = ReadOptional($"Status [{ps.Status}]") ?? ps.Status;
        Console.WriteLine(PartSupplies.Update(ps) ? "Updated." : "Update failed.");
    }

    private void DeletePartSupply() { if (TryReadInt("Supply ID", out var id)) Console.WriteLine(PartSupplies.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchPartSupplies() { Console.Write("Search: "); ListPartSupplies(Console.ReadLine()); }

    private void CreateWorkOrderViaSp()
    {
        var wo = new WorkOrder
        {
            CustomerId = ReadInt("Customer ID", 1),
            VehicleId = ReadInt("Vehicle ID", 1),
            EmployeeId = ReadNullableInt("Employee ID"),
            Priority = ReadOptional("Priority") ?? "normal",
            CustomerComplaint = ReadOptional("Complaint")
        };
        try
        {
            var id = WorkOrders.CreateViaStoredProcedure(wo);
            var created = WorkOrders.GetById(id);
            Console.WriteLine($"Work order created via SP. ID: {id}, Number: {created?.OrderNumber}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SP error: {ex.Message}");
        }
    }

    private void ProcessPaymentViaSp()
    {
        var payment = new Payment
        {
            WorkOrderId = ReadInt("Work Order ID", 1),
            Amount = ReadDecimal("Amount", 1000),
            PaymentMethod = ReadRequired("Payment method"),
            ProcessedBy = ReadNullableInt("Processed by"),
            TransactionReference = ReadOptional("Reference")
        };
        try
        {
            var id = Payments.CreateViaStoredProcedure(payment);
            Console.WriteLine($"Payment processed via SP. Payment ID: {id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SP error: {ex.Message}");
        }
    }

    #endregion

    #region Input helpers

    private static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            var value = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(value)) return value;
            Console.WriteLine("Value is required.");
        }
    }

    private static string? ReadOptional(string prompt)
    {
        Console.Write($"{prompt}: ");
        var value = Console.ReadLine()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static bool TryReadInt(string prompt, out int value)
    {
        Console.Write($"{prompt}: ");
        return int.TryParse(Console.ReadLine(), out value);
    }

    private static int ReadInt(string prompt, int defaultValue)
    {
        Console.Write($"{prompt} [{defaultValue}]: ");
        var input = Console.ReadLine()?.Trim();
        return int.TryParse(input, out var value) ? value : defaultValue;
    }

    private static int? ReadNullableInt(string prompt)
    {
        Console.Write($"{prompt} (empty = null): ");
        var input = Console.ReadLine()?.Trim();
        return int.TryParse(input, out var value) ? value : null;
    }

    private static decimal ReadDecimal(string prompt, decimal defaultValue)
    {
        Console.Write($"{prompt} [{defaultValue}]: ");
        var input = Console.ReadLine()?.Trim();
        return decimal.TryParse(input, out var value) ? value : defaultValue;
    }

    private static bool ReadBool(string prompt, bool defaultValue)
    {
        Console.Write($"{prompt} (y/n) [{defaultValue}]: ");
        var input = Console.ReadLine()?.Trim().ToLowerInvariant();
        return input switch
        {
            "y" or "yes" or "true" or "1" => true,
            "n" or "no" or "false" or "0" => false,
            "" or null => defaultValue,
            _ => defaultValue
        };
    }

    private static DateTime ReadDate(string prompt, DateTime defaultValue)
    {
        Console.Write($"{prompt} (yyyy-MM-dd) [{defaultValue:yyyy-MM-dd}]: ");
        var input = Console.ReadLine()?.Trim();
        return DateTime.TryParse(input, out var value) ? value : defaultValue;
    }

    private static DateTime ReadDateTime(string prompt, DateTime defaultValue)
    {
        Console.Write($"{prompt} [{defaultValue:yyyy-MM-dd HH:mm}]: ");
        var input = Console.ReadLine()?.Trim();
        return DateTime.TryParse(input, out var value) ? value : defaultValue;
    }

    #endregion
}
