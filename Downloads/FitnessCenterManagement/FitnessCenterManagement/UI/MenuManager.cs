using FitnessCenterManagement.Configuration;
using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using FitnessCenterManagement.Repositories;
using FitnessCenterManagement.Services;
using System.Text;
using System.Text.Json;

namespace FitnessCenterManagement.UI;

/// <summary>
/// Hierarchical console menu for fitness center management.
/// </summary>
public class MenuManager
{
    private DatabaseHelper _db = new();
    private readonly ReportService _reports;

    private RoleRepository Roles => new(_db);
    private UserRepository Users => new(_db);
    private BranchRepository Branches => new(_db);
    private MembershipTypeRepository MembershipTypes => new(_db);
    private MemberRepository Members => new(_db);
    private MembershipRepository Memberships => new(_db);
    private TrainerRepository Trainers => new(_db);
    private SpecializationRepository Specializations => new(_db);
    private TrainerSpecializationRepository TrainerSpecializations => new(_db);
    private FitnessClassRepository FitnessClasses => new(_db);
    private ClassScheduleRepository ClassSchedules => new(_db);
    private ClassEnrollmentRepository ClassEnrollments => new(_db);
    private EquipmentRepository Equipment => new(_db);
    private EquipmentMaintenanceRepository EquipmentMaintenance => new(_db);
    private PaymentRepository Payments => new(_db);
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
            Console.WriteLine("1. Members & Memberships");
            Console.WriteLine("2. Classes & Schedules");
            Console.WriteLine("3. Trainers & Specializations");
            Console.WriteLine("4. Equipment & Maintenance");
            Console.WriteLine("5. Branches & Membership Types");
            Console.WriteLine("6. Administration (Users, Roles)");
            Console.WriteLine("7. Reports & Analytics");
            Console.WriteLine("8. Audit Log");
            Console.WriteLine("9. Global Search");
            Console.WriteLine("10. Connection Settings");
            Console.WriteLine("0. Exit");
            Console.Write("Select option: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": ShowMembersMenu(); break;
                case "2": ShowClassesMenu(); break;
                case "3": ShowTrainersMenu(); break;
                case "4": ShowEquipmentMenu(); break;
                case "5": ShowBranchesMenu(); break;
                case "6": ShowAdminMenu(); break;
                case "7": ShowReportsMenu(); break;
                case "8": ShowAuditLog(); break;
                case "9": GlobalSearch(); break;
                case "10": OfferConnectionSetup(); break;
                case "0": return;
                default: Console.WriteLine("Invalid option."); break;
            }
        }
    }

    private static void PrintHeader()
    {
        Console.WriteLine("=================================================");
        Console.WriteLine("   FITNESS CENTER MANAGEMENT SYSTEM");
        Console.WriteLine("   Console Application (.NET / ADO.NET)");
        Console.WriteLine("=================================================");
    }

    private void ShowMembersMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- MEMBERS & MEMBERSHIPS ---");
            Console.WriteLine("1. Members CRUD");
            Console.WriteLine("2. Register Member (stored procedure)");
            Console.WriteLine("3. Memberships CRUD");
            Console.WriteLine("4. Create Membership (stored procedure)");
            Console.WriteLine("5. Payments CRUD");
            Console.WriteLine("6. Process Payment (stored procedure)");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": RunCrudMenu("Members", ListMembers, ViewMember, CreateMember, EditMember, DeleteMember, SearchMembers); break;
                case "2": RegisterMemberViaSp(); break;
                case "3": RunCrudMenu("Memberships", ListMemberships, ViewMembership, CreateMembership, EditMembership, DeleteMembership, SearchMemberships); break;
                case "4": CreateMembershipViaSp(); break;
                case "5": RunCrudMenu("Payments", ListPayments, ViewPayment, CreatePayment, EditPayment, DeletePayment, SearchPayments); break;
                case "6": ProcessPaymentViaSp(); break;
                case "0": return;
            }
        }
    }

    private void ShowClassesMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- CLASSES & SCHEDULES ---");
            Console.WriteLine("1. Fitness Classes CRUD");
            Console.WriteLine("2. Class Schedules CRUD");
            Console.WriteLine("3. Class Enrollments CRUD");
            Console.WriteLine("4. Enroll in Class (stored procedure)");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": RunCrudMenu("Fitness Classes", ListFitnessClasses, ViewFitnessClass, CreateFitnessClass, EditFitnessClass, DeleteFitnessClass, SearchFitnessClasses); break;
                case "2": RunCrudMenu("Class Schedules", ListClassSchedules, ViewClassSchedule, CreateClassSchedule, EditClassSchedule, DeleteClassSchedule, SearchClassSchedules); break;
                case "3": RunCrudMenu("Class Enrollments", ListClassEnrollments, ViewClassEnrollment, CreateClassEnrollment, EditClassEnrollment, DeleteClassEnrollment, SearchClassEnrollments); break;
                case "4": EnrollInClassViaSp(); break;
                case "0": return;
            }
        }
    }

    private void ShowTrainersMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- TRAINERS & SPECIALIZATIONS ---");
            Console.WriteLine("1. Trainers CRUD");
            Console.WriteLine("2. Specializations CRUD");
            Console.WriteLine("3. Trainer Specializations CRUD");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": RunCrudMenu("Trainers", ListTrainers, ViewTrainer, CreateTrainer, EditTrainer, DeleteTrainer, SearchTrainers); break;
                case "2": RunCrudMenu("Specializations", ListSpecializations, ViewSpecialization, CreateSpecialization, EditSpecialization, DeleteSpecialization, SearchSpecializations); break;
                case "3": RunCrudMenu("Trainer Specializations", ListTrainerSpecializations, ViewTrainerSpecialization, CreateTrainerSpecialization, EditTrainerSpecialization, DeleteTrainerSpecialization, SearchTrainerSpecializations); break;
                case "0": return;
            }
        }
    }

    private void ShowEquipmentMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- EQUIPMENT & MAINTENANCE ---");
            Console.WriteLine("1. Equipment CRUD");
            Console.WriteLine("2. Equipment Maintenance CRUD");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": RunCrudMenu("Equipment", ListEquipment, ViewEquipment, CreateEquipment, EditEquipment, DeleteEquipment, SearchEquipment); break;
                case "2": RunCrudMenu("Equipment Maintenance", ListEquipmentMaintenance, ViewEquipmentMaintenance, CreateEquipmentMaintenance, EditEquipmentMaintenance, DeleteEquipmentMaintenance, SearchEquipmentMaintenance); break;
                case "0": return;
            }
        }
    }

    private void ShowBranchesMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- BRANCHES & MEMBERSHIP TYPES ---");
            Console.WriteLine("1. Branches CRUD");
            Console.WriteLine("2. Membership Types CRUD");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": RunCrudMenu("Branches", ListBranches, ViewBranch, CreateBranch, EditBranch, DeleteBranch, SearchBranches); break;
                case "2": RunCrudMenu("Membership Types", ListMembershipTypes, ViewMembershipType, CreateMembershipType, EditMembershipType, DeleteMembershipType, SearchMembershipTypes); break;
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
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": RunCrudMenu("Roles", ListRoles, ViewRole, CreateRole, EditRole, DeleteRole, SearchRoles); break;
                case "2": RunCrudMenu("Users", ListUsers, ViewUser, CreateUser, EditUser, DeleteUser, SearchUsers); break;
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
            Console.WriteLine("2. Memberships by Status");
            Console.WriteLine("3. Classes by Category");
            Console.WriteLine("4. Trainer Workload");
            Console.WriteLine("5. Equipment Status");
            Console.WriteLine("6. Top Members");
            Console.WriteLine("7. Monthly Revenue");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": PrintReport("Revenue by Payment Method", _reports.GetRevenueByPaymentMethod()); break;
                case "2": PrintReport("Memberships by Status", _reports.GetMembershipsByStatus()); break;
                case "3": PrintReport("Classes by Category", _reports.GetClassesByCategory()); break;
                case "4": PrintReport("Trainer Workload", _reports.GetTrainerWorkload()); break;
                case "5": PrintReport("Equipment Status", _reports.GetEquipmentStatusSummary()); break;
                case "6": PrintReport("Top Members", _reports.GetTopMembers()); break;
                case "7": PrintReport("Monthly Revenue", _reports.GetMonthlyRevenue()); break;
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
        Console.WriteLine($"Members: {Members.GetAll(term).Count}");
        Console.WriteLine($"Trainers: {Trainers.GetAll(term).Count}");
        Console.WriteLine($"FitnessClasses: {FitnessClasses.GetAll(term).Count}");
        Console.WriteLine($"Memberships: {Memberships.GetAll(term).Count}");
        Console.WriteLine($"Equipment: {Equipment.GetAll(term).Count}");

        Console.Write("\nShow details for (members/trainers/classes/memberships/equipment/none): ");
        switch (Console.ReadLine()?.Trim().ToLowerInvariant())
        {
            case "members": ListMembers(term); break;
            case "trainers": ListTrainers(term); break;
            case "classes": ListFitnessClasses(term); break;
            case "memberships": ListMemberships(term); break;
            case "equipment": ListEquipment(term); break;
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
                    ["FitnessCenterDb"] = connectionString
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

    private void ListBranches(string? s = null) { foreach (var b in Branches.GetAll(s)) Console.WriteLine($"{b.BranchId}: {b.BranchName}, {b.City}, capacity: {b.Capacity}"); }
    private void ViewBranch(int id) { var b = Branches.GetById(id); Console.WriteLine(b == null ? "Not found." : $"{b.BranchName}, {b.Address}, {b.City}, {b.Phone}"); }

    private void ListMembershipTypes(string? s = null) { foreach (var t in MembershipTypes.GetAll(s)) Console.WriteLine($"{t.MembershipTypeId}: {t.TypeName} - {t.Price:N2} ({t.DurationDays} days)"); }
    private void ViewMembershipType(int id) { var t = MembershipTypes.GetById(id); Console.WriteLine(t == null ? "Not found." : $"{t.TypeName}, {t.AccessLevel}, {t.Price:N2}"); }

    private void ListMembers(string? s = null) { foreach (var m in Members.GetAll(s)) Console.WriteLine($"{m.MemberId}: {m.FirstName} {m.LastName}, {m.Phone}, branch: {m.BranchName}, points: {m.LoyaltyPoints}"); }
    private void ViewMember(int id) { var m = Members.GetById(id); Console.WriteLine(m == null ? "Not found." : $"{m.FirstName} {m.LastName}, {m.Phone}, {m.Email}, goal: {m.FitnessGoal}"); }

    private void ListMemberships(string? s = null) { foreach (var ms in Memberships.GetAll(s)) Console.WriteLine($"{ms.MembershipId}: {ms.MemberName} - {ms.TypeName} [{ms.Status}] {ms.StartDate:yyyy-MM-dd} to {ms.EndDate:yyyy-MM-dd}"); }
    private void ViewMembership(int id) { var ms = Memberships.GetById(id); Console.WriteLine(ms == null ? "Not found." : $"{ms.MemberName}, {ms.TypeName}, status: {ms.Status}, renew: {ms.AutoRenew}"); }

    private void ListTrainers(string? s = null) { foreach (var t in Trainers.GetAll(s)) Console.WriteLine($"{t.TrainerId}: {t.FirstName} {t.LastName} - {t.EmploymentType}, rate: {t.HourlyRate:N2}"); }
    private void ViewTrainer(int id) { var t = Trainers.GetById(id); Console.WriteLine(t == null ? "Not found." : $"{t.FirstName} {t.LastName}, {t.Email}, branch: {t.BranchName}, rating: {t.Rating}"); }

    private void ListSpecializations(string? s = null) { foreach (var sp in Specializations.GetAll(s)) Console.WriteLine($"{sp.SpecializationId}: {sp.SpecializationName} ({sp.Category})"); }
    private void ViewSpecialization(int id) { var sp = Specializations.GetById(id); Console.WriteLine(sp == null ? "Not found." : $"{sp.SpecializationName} - {sp.Description}"); }

    private void ListTrainerSpecializations(string? s = null) { foreach (var ts in TrainerSpecializations.GetAll(s)) Console.WriteLine($"{ts.TrainerSpecializationId}: {ts.TrainerName} - {ts.SpecializationName} (cert: {ts.CertifiedDate:yyyy-MM-dd})"); }
    private void ViewTrainerSpecialization(int id) { var ts = TrainerSpecializations.GetById(id); Console.WriteLine(ts == null ? "Not found." : $"{ts.TrainerName}, {ts.SpecializationName}, cert#: {ts.CertificationNumber}"); }

    private void ListFitnessClasses(string? s = null) { foreach (var c in FitnessClasses.GetAll(s)) Console.WriteLine($"{c.ClassId}: {c.ClassName} [{c.Category}] {c.DifficultyLevel}, cap: {c.MaxCapacity}"); }
    private void ViewFitnessClass(int id) { var c = FitnessClasses.GetById(id); Console.WriteLine(c == null ? "Not found." : $"{c.ClassName}, {c.Category}, {c.DurationMinutes} min, room: {c.RoomName}"); }

    private void ListClassSchedules(string? s = null) { foreach (var cs in ClassSchedules.GetAll(s)) Console.WriteLine($"{cs.ScheduleId}: {cs.ClassName} with {cs.TrainerName} [{cs.Status}] {cs.StartTime:yyyy-MM-dd HH:mm}"); }
    private void ViewClassSchedule(int id) { var cs = ClassSchedules.GetById(id); Console.WriteLine(cs == null ? "Not found." : $"{cs.ClassName}, {cs.TrainerName}, {cs.StartTime} - {cs.EndTime}, enrolled: {cs.CurrentEnrollment}"); }

    private void ListClassEnrollments(string? s = null) { foreach (var e in ClassEnrollments.GetAll(s)) Console.WriteLine($"{e.EnrollmentId}: {e.MemberName} in {e.ClassName} [{e.Status}]"); }
    private void ViewClassEnrollment(int id) { var e = ClassEnrollments.GetById(id); Console.WriteLine(e == null ? "Not found." : $"{e.MemberName}, class: {e.ClassName}, status: {e.Status}, enrolled: {e.EnrollmentDate:yyyy-MM-dd}"); }

    private void ListEquipment(string? s = null) { foreach (var e in Equipment.GetAll(s)) Console.WriteLine($"{e.EquipmentId}: {e.EquipmentName} [{e.Category}] {e.ConditionStatus} at {e.Location}"); }
    private void ViewEquipment(int id) { var e = Equipment.GetById(id); Console.WriteLine(e == null ? "Not found." : $"{e.EquipmentName}, serial: {e.SerialNumber}, branch: {e.BranchName}, condition: {e.ConditionStatus}"); }

    private void ListEquipmentMaintenance(string? s = null) { foreach (var m in EquipmentMaintenance.GetAll(s)) Console.WriteLine($"{m.MaintenanceId}: {m.EquipmentName} - {m.MaintenanceType} on {m.MaintenanceDate:yyyy-MM-dd}, cost: {m.Cost:N2}"); }
    private void ViewEquipmentMaintenance(int id) { var m = EquipmentMaintenance.GetById(id); Console.WriteLine(m == null ? "Not found." : $"{m.EquipmentName}, {m.MaintenanceType}, status: {m.Status}, next: {m.NextMaintenanceDate:yyyy-MM-dd}"); }

    private void ListPayments(string? s = null) { foreach (var p in Payments.GetAll(s)) Console.WriteLine($"{p.PaymentId}: {p.MemberName} {p.Amount:N2} via {p.PaymentMethod} [{p.Status}]"); }
    private void ViewPayment(int id) { var p = Payments.GetById(id); Console.WriteLine(p == null ? "Not found." : $"{p.MemberName}, {p.Amount:N2}, ref: {p.TransactionReference}, date: {p.PaymentDate:yyyy-MM-dd}"); }

    #endregion

    #region Create/Edit/Delete/Search

    private void CreateRole()
    {
        var role = new Role
        {
            RoleName = ReadRequired("Role name"),
            RoleDescription = ReadOptional("Description"),
            PermissionLevel = ReadInt("Permission level", 1)
        };
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

    private void CreateBranch()
    {
        var branch = new Branch
        {
            BranchName = ReadRequired("Branch name"),
            Address = ReadRequired("Address"),
            City = ReadRequired("City"),
            Phone = ReadRequired("Phone"),
            Email = ReadOptional("Email"),
            OpeningHours = ReadOptional("Opening hours"),
            Capacity = ReadInt("Capacity", 100),
            ManagerName = ReadOptional("Manager name"),
            ParkingSpaces = ReadInt("Parking spaces", 0),
            HasPool = ReadBool("Has pool", false)
        };
        Console.WriteLine($"Created branch ID: {Branches.Create(branch)}");
    }

    private void EditBranch()
    {
        if (!TryReadInt("Branch ID", out var id)) return;
        var branch = Branches.GetById(id);
        if (branch == null) { Console.WriteLine("Not found."); return; }
        branch.Phone = ReadOptional($"Phone [{branch.Phone}]") ?? branch.Phone;
        branch.Capacity = ReadInt($"Capacity [{branch.Capacity}]", branch.Capacity);
        branch.HasPool = ReadBool($"Has pool [{branch.HasPool}]", branch.HasPool);
        Console.WriteLine(Branches.Update(branch) ? "Updated." : "Update failed.");
    }

    private void DeleteBranch() { if (TryReadInt("Branch ID", out var id)) Console.WriteLine(Branches.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchBranches() { Console.Write("Search: "); ListBranches(Console.ReadLine()); }

    private void CreateMembershipType()
    {
        var type = new MembershipType
        {
            TypeName = ReadRequired("Type name"),
            Description = ReadOptional("Description"),
            DurationDays = ReadInt("Duration days", 30),
            Price = ReadDecimal("Price", 1000),
            AccessLevel = ReadOptional("Access level (standard/premium/vip)") ?? "standard",
            MaxClassesPerMonth = ReadInt("Max classes per month", 8),
            IncludesPersonalTraining = ReadBool("Includes personal training", false),
            GuestPasses = ReadInt("Guest passes", 0),
            FreezeDaysAllowed = ReadInt("Freeze days allowed", 14)
        };
        Console.WriteLine($"Created membership type ID: {MembershipTypes.Create(type)}");
    }

    private void EditMembershipType()
    {
        if (!TryReadInt("Membership type ID", out var id)) return;
        var type = MembershipTypes.GetById(id);
        if (type == null) { Console.WriteLine("Not found."); return; }
        type.Price = ReadDecimal($"Price [{type.Price}]", type.Price);
        type.DurationDays = ReadInt($"Duration days [{type.DurationDays}]", type.DurationDays);
        Console.WriteLine(MembershipTypes.Update(type) ? "Updated." : "Update failed.");
    }

    private void DeleteMembershipType() { if (TryReadInt("Membership type ID", out var id)) Console.WriteLine(MembershipTypes.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchMembershipTypes() { Console.Write("Search: "); ListMembershipTypes(Console.ReadLine()); }

    private void CreateMember()
    {
        var member = new Member
        {
            BranchId = ReadInt("Branch ID", 1),
            FirstName = ReadRequired("First name"),
            LastName = ReadRequired("Last name"),
            Phone = ReadRequired("Phone"),
            Email = ReadOptional("Email"),
            Gender = ReadOptional("Gender"),
            Address = ReadOptional("Address"),
            EmergencyContact = ReadOptional("Emergency contact"),
            FitnessGoal = ReadOptional("Fitness goal"),
            LoyaltyPoints = ReadInt("Loyalty points", 0),
            JoinDate = ReadDate("Join date", DateTime.Today),
            DateOfBirth = ReadNullableDate("Date of birth")
        };
        Console.WriteLine($"Created member ID: {Members.Create(member)}");
    }

    private void EditMember()
    {
        if (!TryReadInt("Member ID", out var id)) return;
        var member = Members.GetById(id);
        if (member == null) { Console.WriteLine("Not found."); return; }
        member.Phone = ReadOptional($"Phone [{member.Phone}]") ?? member.Phone;
        member.LoyaltyPoints = ReadInt($"Loyalty points [{member.LoyaltyPoints}]", member.LoyaltyPoints);
        member.FitnessGoal = ReadOptional($"Fitness goal [{member.FitnessGoal}]") ?? member.FitnessGoal;
        member.HealthNotes = ReadOptional($"Health notes [{member.HealthNotes}]") ?? member.HealthNotes;
        Console.WriteLine(Members.Update(member) ? "Updated." : "Update failed.");
    }

    private void DeleteMember() { if (TryReadInt("Member ID", out var id)) Console.WriteLine(Members.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchMembers() { Console.Write("Search: "); ListMembers(Console.ReadLine()); }

    private void RegisterMemberViaSp()
    {
        var member = new Member
        {
            BranchId = ReadInt("Branch ID", 1),
            FirstName = ReadRequired("First name"),
            LastName = ReadRequired("Last name"),
            Phone = ReadRequired("Phone"),
            Email = ReadOptional("Email")
        };
        var membershipTypeId = ReadNullableInt("Membership type ID (optional)");
        try
        {
            var id = Members.RegisterViaStoredProcedure(member, membershipTypeId);
            Console.WriteLine($"Member registered via SP. Member ID: {id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SP error: {ex.Message}");
        }
    }

    private void CreateMembership()
    {
        var membership = new Membership
        {
            MemberId = ReadInt("Member ID", 1),
            MembershipTypeId = ReadInt("Membership type ID", 1),
            StartDate = ReadDate("Start date", DateTime.Today),
            EndDate = ReadDate("End date", DateTime.Today.AddDays(30)),
            Status = ReadOptional("Status (active/expired/frozen/cancelled)") ?? "active",
            AutoRenew = ReadBool("Auto renew", false),
            DiscountPercent = ReadDecimal("Discount percent", 0),
            Notes = ReadOptional("Notes")
        };
        Console.WriteLine($"Created membership ID: {Memberships.Create(membership)}");
    }

    private void EditMembership()
    {
        if (!TryReadInt("Membership ID", out var id)) return;
        var membership = Memberships.GetById(id);
        if (membership == null) { Console.WriteLine("Not found."); return; }
        membership.Status = ReadOptional($"Status [{membership.Status}]") ?? membership.Status;
        membership.AutoRenew = ReadBool($"Auto renew [{membership.AutoRenew}]", membership.AutoRenew);
        membership.Notes = ReadOptional($"Notes [{membership.Notes}]") ?? membership.Notes;
        Console.WriteLine(Memberships.Update(membership) ? "Updated." : "Update failed.");
    }

    private void DeleteMembership() { if (TryReadInt("Membership ID", out var id)) Console.WriteLine(Memberships.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchMemberships() { Console.Write("Search: "); ListMemberships(Console.ReadLine()); }

    private void CreateMembershipViaSp()
    {
        var membership = new Membership
        {
            MemberId = ReadInt("Member ID", 1),
            MembershipTypeId = ReadInt("Membership type ID", 1),
            AutoRenew = ReadBool("Auto renew", false),
            DiscountPercent = ReadDecimal("Discount percent", 0)
        };
        try
        {
            var id = Memberships.CreateViaStoredProcedure(membership);
            Console.WriteLine($"Membership created via SP. Membership ID: {id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SP error: {ex.Message}");
        }
    }

    private void CreateTrainer()
    {
        var trainer = new Trainer
        {
            BranchId = ReadInt("Branch ID", 1),
            FirstName = ReadRequired("First name"),
            LastName = ReadRequired("Last name"),
            Phone = ReadRequired("Phone"),
            Email = ReadRequired("Email"),
            HireDate = ReadDate("Hire date", DateTime.Today),
            HourlyRate = ReadDecimal("Hourly rate", 500),
            EmploymentType = ReadOptional("Employment type (full_time/part_time/contract)") ?? "full_time",
            CertificationLevel = ReadOptional("Certification level"),
            Bio = ReadOptional("Bio"),
            MaxClientsPerDay = ReadInt("Max clients per day", 8)
        };
        Console.WriteLine($"Created trainer ID: {Trainers.Create(trainer)}");
    }

    private void EditTrainer()
    {
        if (!TryReadInt("Trainer ID", out var id)) return;
        var trainer = Trainers.GetById(id);
        if (trainer == null) { Console.WriteLine("Not found."); return; }
        trainer.HourlyRate = ReadDecimal($"Hourly rate [{trainer.HourlyRate}]", trainer.HourlyRate);
        trainer.Bio = ReadOptional($"Bio [{trainer.Bio}]") ?? trainer.Bio;
        trainer.MaxClientsPerDay = ReadInt($"Max clients per day [{trainer.MaxClientsPerDay}]", trainer.MaxClientsPerDay);
        Console.WriteLine(Trainers.Update(trainer) ? "Updated." : "Update failed.");
    }

    private void DeleteTrainer() { if (TryReadInt("Trainer ID", out var id)) Console.WriteLine(Trainers.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchTrainers() { Console.Write("Search: "); ListTrainers(Console.ReadLine()); }

    private void CreateSpecialization()
    {
        var spec = new Specialization
        {
            SpecializationName = ReadRequired("Specialization name"),
            Description = ReadOptional("Description"),
            Category = ReadOptional("Category"),
            DifficultyLevel = ReadOptional("Difficulty level"),
            RequiredCertification = ReadOptional("Required certification")
        };
        Console.WriteLine($"Created specialization ID: {Specializations.Create(spec)}");
    }

    private void EditSpecialization()
    {
        if (!TryReadInt("Specialization ID", out var id)) return;
        var spec = Specializations.GetById(id);
        if (spec == null) { Console.WriteLine("Not found."); return; }
        spec.SpecializationName = ReadOptional($"Name [{spec.SpecializationName}]") ?? spec.SpecializationName;
        spec.Description = ReadOptional($"Description [{spec.Description}]") ?? spec.Description;
        Console.WriteLine(Specializations.Update(spec) ? "Updated." : "Update failed.");
    }

    private void DeleteSpecialization() { if (TryReadInt("Specialization ID", out var id)) Console.WriteLine(Specializations.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchSpecializations() { Console.Write("Search: "); ListSpecializations(Console.ReadLine()); }

    private void CreateTrainerSpecialization()
    {
        var item = new TrainerSpecialization
        {
            TrainerId = ReadInt("Trainer ID", 1),
            SpecializationId = ReadInt("Specialization ID", 1),
            CertifiedDate = ReadDate("Certified date", DateTime.Today),
            CertificationNumber = ReadOptional("Certification number")
        };
        Console.WriteLine($"Created trainer specialization ID: {TrainerSpecializations.Create(item)}");
    }

    private void EditTrainerSpecialization()
    {
        if (!TryReadInt("Trainer specialization ID", out var id)) return;
        var item = TrainerSpecializations.GetById(id);
        if (item == null) { Console.WriteLine("Not found."); return; }
        item.CertificationNumber = ReadOptional($"Certification number [{item.CertificationNumber}]") ?? item.CertificationNumber;
        item.CertifiedDate = ReadDate($"Certified date [{item.CertifiedDate:yyyy-MM-dd}]", item.CertifiedDate);
        Console.WriteLine(TrainerSpecializations.Update(item) ? "Updated." : "Update failed.");
    }

    private void DeleteTrainerSpecialization() { if (TryReadInt("Trainer specialization ID", out var id)) Console.WriteLine(TrainerSpecializations.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchTrainerSpecializations() { Console.Write("Search: "); ListTrainerSpecializations(Console.ReadLine()); }

    private void CreateFitnessClass()
    {
        var fitnessClass = new FitnessClass
        {
            BranchId = ReadInt("Branch ID", 1),
            ClassName = ReadRequired("Class name"),
            Description = ReadOptional("Description"),
            Category = ReadRequired("Category"),
            DifficultyLevel = ReadOptional("Difficulty level (beginner/intermediate/advanced)") ?? "beginner",
            MaxCapacity = ReadInt("Max capacity", 20),
            DurationMinutes = ReadInt("Duration minutes", 60),
            RoomName = ReadOptional("Room name"),
            EquipmentRequired = ReadOptional("Equipment required")
        };
        Console.WriteLine($"Created class ID: {FitnessClasses.Create(fitnessClass)}");
    }

    private void EditFitnessClass()
    {
        if (!TryReadInt("Class ID", out var id)) return;
        var fitnessClass = FitnessClasses.GetById(id);
        if (fitnessClass == null) { Console.WriteLine("Not found."); return; }
        fitnessClass.MaxCapacity = ReadInt($"Max capacity [{fitnessClass.MaxCapacity}]", fitnessClass.MaxCapacity);
        fitnessClass.RoomName = ReadOptional($"Room name [{fitnessClass.RoomName}]") ?? fitnessClass.RoomName;
        Console.WriteLine(FitnessClasses.Update(fitnessClass) ? "Updated." : "Update failed.");
    }

    private void DeleteFitnessClass() { if (TryReadInt("Class ID", out var id)) Console.WriteLine(FitnessClasses.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchFitnessClasses() { Console.Write("Search: "); ListFitnessClasses(Console.ReadLine()); }

    private void CreateClassSchedule()
    {
        var schedule = new ClassSchedule
        {
            ClassId = ReadInt("Class ID", 1),
            TrainerId = ReadInt("Trainer ID", 1),
            StartTime = ReadDateTime("Start time (yyyy-MM-dd HH:mm)", DateTime.Now.AddDays(1)),
            EndTime = ReadDateTime("End time (yyyy-MM-dd HH:mm)", DateTime.Now.AddDays(1).AddHours(1)),
            Status = ReadOptional("Status (scheduled/completed/cancelled)") ?? "scheduled",
            RoomOverride = ReadOptional("Room override"),
            Notes = ReadOptional("Notes")
        };
        Console.WriteLine($"Created schedule ID: {ClassSchedules.Create(schedule)}");
    }

    private void EditClassSchedule()
    {
        if (!TryReadInt("Schedule ID", out var id)) return;
        var schedule = ClassSchedules.GetById(id);
        if (schedule == null) { Console.WriteLine("Not found."); return; }
        schedule.Status = ReadOptional($"Status [{schedule.Status}]") ?? schedule.Status;
        schedule.Notes = ReadOptional($"Notes [{schedule.Notes}]") ?? schedule.Notes;
        Console.WriteLine(ClassSchedules.Update(schedule) ? "Updated." : "Update failed.");
    }

    private void DeleteClassSchedule() { if (TryReadInt("Schedule ID", out var id)) Console.WriteLine(ClassSchedules.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchClassSchedules() { Console.Write("Search: "); ListClassSchedules(Console.ReadLine()); }

    private void CreateClassEnrollment()
    {
        var enrollment = new ClassEnrollment
        {
            ScheduleId = ReadInt("Schedule ID", 1),
            MemberId = ReadInt("Member ID", 1),
            EnrollmentDate = DateTime.Now,
            Status = ReadOptional("Status (enrolled/cancelled/attended/no_show)") ?? "enrolled",
            Notes = ReadOptional("Notes")
        };
        Console.WriteLine($"Created enrollment ID: {ClassEnrollments.Create(enrollment)}");
    }

    private void EditClassEnrollment()
    {
        if (!TryReadInt("Enrollment ID", out var id)) return;
        var enrollment = ClassEnrollments.GetById(id);
        if (enrollment == null) { Console.WriteLine("Not found."); return; }
        enrollment.Status = ReadOptional($"Status [{enrollment.Status}]") ?? enrollment.Status;
        enrollment.CancellationReason = ReadOptional($"Cancellation reason [{enrollment.CancellationReason}]") ?? enrollment.CancellationReason;
        enrollment.Notes = ReadOptional($"Notes [{enrollment.Notes}]") ?? enrollment.Notes;
        Console.WriteLine(ClassEnrollments.Update(enrollment) ? "Updated." : "Update failed.");
    }

    private void DeleteClassEnrollment() { if (TryReadInt("Enrollment ID", out var id)) Console.WriteLine(ClassEnrollments.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchClassEnrollments() { Console.Write("Search: "); ListClassEnrollments(Console.ReadLine()); }

    private void EnrollInClassViaSp()
    {
        var enrollment = new ClassEnrollment
        {
            ScheduleId = ReadInt("Schedule ID", 1),
            MemberId = ReadInt("Member ID", 1)
        };
        try
        {
            var id = ClassEnrollments.EnrollViaStoredProcedure(enrollment);
            Console.WriteLine($"Enrolled via SP. Enrollment ID: {id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SP error: {ex.Message}");
        }
    }

    private void CreateEquipment()
    {
        var item = new Equipment
        {
            BranchId = ReadInt("Branch ID", 1),
            EquipmentName = ReadRequired("Equipment name"),
            Category = ReadRequired("Category"),
            SerialNumber = ReadOptional("Serial number"),
            PurchaseDate = ReadNullableDate("Purchase date"),
            PurchasePrice = ReadNullableDecimal("Purchase price"),
            Location = ReadOptional("Location"),
            ConditionStatus = ReadOptional("Condition status (good/fair/poor/out_of_service)") ?? "good",
            Manufacturer = ReadOptional("Manufacturer")
        };
        Console.WriteLine($"Created equipment ID: {Equipment.Create(item)}");
    }

    private void EditEquipment()
    {
        if (!TryReadInt("Equipment ID", out var id)) return;
        var item = Equipment.GetById(id);
        if (item == null) { Console.WriteLine("Not found."); return; }
        item.ConditionStatus = ReadOptional($"Condition status [{item.ConditionStatus}]") ?? item.ConditionStatus;
        item.Location = ReadOptional($"Location [{item.Location}]") ?? item.Location;
        Console.WriteLine(Equipment.Update(item) ? "Updated." : "Update failed.");
    }

    private void DeleteEquipment() { if (TryReadInt("Equipment ID", out var id)) Console.WriteLine(Equipment.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchEquipment() { Console.Write("Search: "); ListEquipment(Console.ReadLine()); }

    private void CreateEquipmentMaintenance()
    {
        var maintenance = new EquipmentMaintenance
        {
            EquipmentId = ReadInt("Equipment ID", 1),
            MaintenanceDate = ReadDate("Maintenance date", DateTime.Today),
            MaintenanceType = ReadRequired("Maintenance type"),
            PerformedBy = ReadNullableInt("Performed by trainer ID"),
            Cost = ReadDecimal("Cost", 0),
            Description = ReadOptional("Description"),
            NextMaintenanceDate = ReadNullableDate("Next maintenance date"),
            Status = ReadOptional("Status (scheduled/in_progress/completed)") ?? "completed",
            VendorName = ReadOptional("Vendor name")
        };
        Console.WriteLine($"Created maintenance ID: {EquipmentMaintenance.Create(maintenance)}");
    }

    private void EditEquipmentMaintenance()
    {
        if (!TryReadInt("Maintenance ID", out var id)) return;
        var maintenance = EquipmentMaintenance.GetById(id);
        if (maintenance == null) { Console.WriteLine("Not found."); return; }
        maintenance.Status = ReadOptional($"Status [{maintenance.Status}]") ?? maintenance.Status;
        maintenance.Cost = ReadDecimal($"Cost [{maintenance.Cost}]", maintenance.Cost);
        Console.WriteLine(EquipmentMaintenance.Update(maintenance) ? "Updated." : "Update failed.");
    }

    private void DeleteEquipmentMaintenance() { if (TryReadInt("Maintenance ID", out var id)) Console.WriteLine(EquipmentMaintenance.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchEquipmentMaintenance() { Console.Write("Search: "); ListEquipmentMaintenance(Console.ReadLine()); }

    private void CreatePayment()
    {
        var payment = new Payment
        {
            MemberId = ReadInt("Member ID", 1),
            MembershipId = ReadNullableInt("Membership ID"),
            PaymentDate = DateTime.Now,
            Amount = ReadDecimal("Amount", 1000),
            PaymentMethod = ReadRequired("Payment method (cash/card/bank_transfer)"),
            TransactionReference = ReadOptional("Transaction reference"),
            ProcessedBy = ReadNullableInt("Processed by user ID"),
            Notes = ReadOptional("Notes")
        };
        try
        {
            Console.WriteLine($"Created payment ID: {Payments.Create(payment)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void EditPayment()
    {
        if (!TryReadInt("Payment ID", out var id)) return;
        var payment = Payments.GetById(id);
        if (payment == null) { Console.WriteLine("Not found."); return; }
        payment.Notes = ReadOptional($"Notes [{payment.Notes}]") ?? payment.Notes;
        payment.Status = ReadOptional($"Status [{payment.Status}]") ?? payment.Status;
        Console.WriteLine(Payments.Update(payment) ? "Updated." : "Update failed.");
    }

    private void DeletePayment() { if (TryReadInt("Payment ID", out var id)) Console.WriteLine(Payments.SoftDelete(id) ? "Soft deleted." : "Delete failed."); }
    private void SearchPayments() { Console.Write("Search: "); ListPayments(Console.ReadLine()); }

    private void ProcessPaymentViaSp()
    {
        var payment = new Payment
        {
            MemberId = ReadInt("Member ID", 1),
            MembershipId = ReadNullableInt("Membership ID"),
            Amount = ReadDecimal("Amount", 1000),
            PaymentMethod = ReadRequired("Payment method"),
            ProcessedBy = ReadNullableInt("Processed by user ID"),
            TransactionReference = ReadOptional("Transaction reference")
        };
        try
        {
            var id = Payments.ProcessViaStoredProcedure(payment);
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

    private static bool TryReadDate(string prompt, out DateTime value)
    {
        Console.Write($"{prompt}: ");
        return DateTime.TryParse(Console.ReadLine(), out value);
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

    private static decimal? ReadNullableDecimal(string prompt)
    {
        Console.Write($"{prompt} (empty = null): ");
        var input = Console.ReadLine()?.Trim();
        return decimal.TryParse(input, out var value) ? value : null;
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

    private static DateTime? ReadNullableDate(string prompt)
    {
        Console.Write($"{prompt} (yyyy-MM-dd, empty = null): ");
        var input = Console.ReadLine()?.Trim();
        return DateTime.TryParse(input, out var value) ? value : null;
    }

    private static DateTime ReadDateTime(string prompt, DateTime defaultValue)
    {
        Console.Write($"{prompt} [{defaultValue:yyyy-MM-dd HH:mm}]: ");
        var input = Console.ReadLine()?.Trim();
        return DateTime.TryParse(input, out var value) ? value : defaultValue;
    }

    #endregion
}
