using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FitnessCenterManagement.Repositories;

/// <summary>
/// Generic CRUD repository base for ADO.NET data access.
/// </summary>
public abstract class RepositoryBase
{
    protected readonly DatabaseHelper Db;

    protected RepositoryBase(DatabaseHelper db)
    {
        Db = db;
    }

    protected static int GetInt(SqlDataReader reader, string column) =>
        reader.GetInt32(reader.GetOrdinal(column));

    protected static int? GetNullableInt(SqlDataReader reader, string column) =>
        reader.IsDBNull(reader.GetOrdinal(column)) ? null : reader.GetInt32(reader.GetOrdinal(column));

    protected static long GetLong(SqlDataReader reader, string column) =>
        reader.GetInt64(reader.GetOrdinal(column));

    protected static string GetString(SqlDataReader reader, string column) =>
        reader.IsDBNull(reader.GetOrdinal(column)) ? string.Empty : reader.GetString(reader.GetOrdinal(column));

    protected static string? GetNullableString(SqlDataReader reader, string column) =>
        reader.IsDBNull(reader.GetOrdinal(column)) ? null : reader.GetString(reader.GetOrdinal(column));

    protected static bool GetBool(SqlDataReader reader, string column) =>
        reader.GetBoolean(reader.GetOrdinal(column));

    protected static decimal GetDecimal(SqlDataReader reader, string column) =>
        reader.GetDecimal(reader.GetOrdinal(column));

    protected static decimal? GetNullableDecimal(SqlDataReader reader, string column) =>
        reader.IsDBNull(reader.GetOrdinal(column)) ? null : reader.GetDecimal(reader.GetOrdinal(column));

    protected static DateTime GetDateTime(SqlDataReader reader, string column) =>
        reader.GetDateTime(reader.GetOrdinal(column));

    protected static DateTime? GetNullableDateTime(SqlDataReader reader, string column) =>
        reader.IsDBNull(reader.GetOrdinal(column)) ? null : reader.GetDateTime(reader.GetOrdinal(column));

    protected static SqlParameter Param(string name, object? value) =>
        new(name, value ?? DBNull.Value);
}

public class RoleRepository : RepositoryBase
{
    public RoleRepository(DatabaseHelper db) : base(db) { }

    public List<Role> GetAll(string? search = null)
    {
        var sql = "select role_id, role_name, role_description, permission_level, isactive, isdeleted from roles where isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and role_name like @search";

        sql += " order by permission_level desc";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<Role>();
        while (reader.Read())
        {
            list.Add(new Role
            {
                RoleId = GetInt(reader, "role_id"),
                RoleName = GetString(reader, "role_name"),
                RoleDescription = GetNullableString(reader, "role_description"),
                PermissionLevel = GetInt(reader, "permission_level"),
                IsActive = GetBool(reader, "isactive"),
                IsDeleted = GetBool(reader, "isdeleted")
            });
        }

        return list;
    }

    public Role? GetById(int id)
    {
        const string sql = "select role_id, role_name, role_description, permission_level, isactive, isdeleted from roles where role_id = @id and isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        if (!reader.Read()) return null;
        return new Role
        {
            RoleId = GetInt(reader, "role_id"),
            RoleName = GetString(reader, "role_name"),
            RoleDescription = GetNullableString(reader, "role_description"),
            PermissionLevel = GetInt(reader, "permission_level"),
            IsActive = GetBool(reader, "isactive"),
            IsDeleted = GetBool(reader, "isdeleted")
        };
    }

    public int Create(Role role)
    {
        const string sql = @"insert into roles (role_name, role_description, permission_level, isactive)
                             values (@name, @desc, @level, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@name", role.RoleName),
            Param("@desc", role.RoleDescription),
            Param("@level", role.PermissionLevel),
            Param("@active", role.IsActive)));
    }

    public bool Update(Role role)
    {
        const string sql = @"update roles set role_name = @name, role_description = @desc,
                             permission_level = @level, isactive = @active, updatedat = sysutcdatetime()
                             where role_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@name", role.RoleName),
            Param("@desc", role.RoleDescription),
            Param("@level", role.PermissionLevel),
            Param("@active", role.IsActive),
            Param("@id", role.RoleId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update roles set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where role_id = @id", Param("@id", id)) > 0;
}

public class UserRepository : RepositoryBase
{
    public UserRepository(DatabaseHelper db) : base(db) { }

    public List<User> GetAll(string? search = null)
    {
        var sql = @"select u.user_id, u.username, u.password_hash, u.email, u.role_id, r.role_name,
                           u.last_login, u.login_attempts, u.isactive, u.isdeleted
                    from users u inner join roles r on u.role_id = r.role_id
                    where u.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (u.username like @search or u.email like @search)";

        sql += " order by u.user_id";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<User>();
        while (reader.Read()) list.Add(MapUser(reader));
        return list;
    }

    public User? GetById(int id)
    {
        const string sql = @"select u.user_id, u.username, u.password_hash, u.email, u.role_id, r.role_name,
                                    u.last_login, u.login_attempts, u.isactive, u.isdeleted
                             from users u inner join roles r on u.role_id = r.role_id
                             where u.user_id = @id and u.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? MapUser(reader) : null;
    }

    public int Create(User user)
    {
        const string sql = @"insert into users (username, password_hash, email, role_id, isactive)
                             values (@username, @hash, @email, @roleId, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@username", user.Username),
            Param("@hash", user.PasswordHash),
            Param("@email", user.Email),
            Param("@roleId", user.RoleId),
            Param("@active", user.IsActive)));
    }

    public bool Update(User user)
    {
        const string sql = @"update users set username = @username, password_hash = @hash, email = @email,
                             role_id = @roleId, isactive = @active, updatedat = sysutcdatetime()
                             where user_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@username", user.Username),
            Param("@hash", user.PasswordHash),
            Param("@email", user.Email),
            Param("@roleId", user.RoleId),
            Param("@active", user.IsActive),
            Param("@id", user.UserId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update users set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where user_id = @id", Param("@id", id)) > 0;

    private static User MapUser(SqlDataReader reader) => new()
    {
        UserId = GetInt(reader, "user_id"),
        Username = GetString(reader, "username"),
        PasswordHash = GetString(reader, "password_hash"),
        Email = GetString(reader, "email"),
        RoleId = GetInt(reader, "role_id"),
        RoleName = GetNullableString(reader, "role_name"),
        LastLogin = GetNullableDateTime(reader, "last_login"),
        LoginAttempts = GetInt(reader, "login_attempts"),
        IsActive = GetBool(reader, "isactive"),
        IsDeleted = GetBool(reader, "isdeleted")
    };
}

public class BranchRepository : RepositoryBase
{
    public BranchRepository(DatabaseHelper db) : base(db) { }

    public List<Branch> GetAll(string? search = null)
    {
        var sql = @"select branch_id, branch_name, address, city, phone, email, opening_hours, capacity,
                           manager_name, parking_spaces, has_pool, isactive, isdeleted
                    from branches where isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (branch_name like @search or city like @search or address like @search)";

        sql += " order by branch_name";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<Branch>();
        while (reader.Read()) list.Add(MapBranch(reader));
        return list;
    }

    public Branch? GetById(int id)
    {
        const string sql = @"select branch_id, branch_name, address, city, phone, email, opening_hours, capacity,
                                    manager_name, parking_spaces, has_pool, isactive, isdeleted
                             from branches where branch_id = @id and isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? MapBranch(reader) : null;
    }

    public int Create(Branch branch)
    {
        const string sql = @"insert into branches (branch_name, address, city, phone, email, opening_hours, capacity,
                             manager_name, parking_spaces, has_pool, isactive)
                             values (@name, @addr, @city, @phone, @email, @hours, @capacity, @manager, @parking, @pool, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@name", branch.BranchName), Param("@addr", branch.Address), Param("@city", branch.City),
            Param("@phone", branch.Phone), Param("@email", branch.Email), Param("@hours", branch.OpeningHours),
            Param("@capacity", branch.Capacity), Param("@manager", branch.ManagerName),
            Param("@parking", branch.ParkingSpaces), Param("@pool", branch.HasPool), Param("@active", branch.IsActive)));
    }

    public bool Update(Branch branch)
    {
        const string sql = @"update branches set branch_name = @name, address = @addr, city = @city, phone = @phone,
                             email = @email, opening_hours = @hours, capacity = @capacity, manager_name = @manager,
                             parking_spaces = @parking, has_pool = @pool, isactive = @active, updatedat = sysutcdatetime()
                             where branch_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@name", branch.BranchName), Param("@addr", branch.Address), Param("@city", branch.City),
            Param("@phone", branch.Phone), Param("@email", branch.Email), Param("@hours", branch.OpeningHours),
            Param("@capacity", branch.Capacity), Param("@manager", branch.ManagerName),
            Param("@parking", branch.ParkingSpaces), Param("@pool", branch.HasPool),
            Param("@active", branch.IsActive), Param("@id", branch.BranchId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update branches set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where branch_id = @id", Param("@id", id)) > 0;

    private static Branch MapBranch(SqlDataReader reader) => new()
    {
        BranchId = GetInt(reader, "branch_id"),
        BranchName = GetString(reader, "branch_name"),
        Address = GetString(reader, "address"),
        City = GetString(reader, "city"),
        Phone = GetString(reader, "phone"),
        Email = GetNullableString(reader, "email"),
        OpeningHours = GetNullableString(reader, "opening_hours"),
        Capacity = GetInt(reader, "capacity"),
        ManagerName = GetNullableString(reader, "manager_name"),
        ParkingSpaces = GetInt(reader, "parking_spaces"),
        HasPool = GetBool(reader, "has_pool"),
        IsActive = GetBool(reader, "isactive"),
        IsDeleted = GetBool(reader, "isdeleted")
    };
}

public class MembershipTypeRepository : RepositoryBase
{
    public MembershipTypeRepository(DatabaseHelper db) : base(db) { }

    public List<MembershipType> GetAll(string? search = null)
    {
        var sql = @"select membership_type_id, type_name, description, duration_days, price, access_level,
                           max_classes_per_month, includes_personal_training, guest_passes, freeze_days_allowed,
                           isactive, isdeleted
                    from membership_types where isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and type_name like @search";

        sql += " order by price";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<MembershipType>();
        while (reader.Read()) list.Add(MapMembershipType(reader));
        return list;
    }

    public MembershipType? GetById(int id)
    {
        const string sql = @"select membership_type_id, type_name, description, duration_days, price, access_level,
                                    max_classes_per_month, includes_personal_training, guest_passes, freeze_days_allowed,
                                    isactive, isdeleted
                             from membership_types where membership_type_id = @id and isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? MapMembershipType(reader) : null;
    }

    public int Create(MembershipType type)
    {
        const string sql = @"insert into membership_types (type_name, description, duration_days, price, access_level,
                             max_classes_per_month, includes_personal_training, guest_passes, freeze_days_allowed, isactive)
                             values (@name, @desc, @duration, @price, @access, @maxClasses, @training, @guests, @freeze, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@name", type.TypeName), Param("@desc", type.Description), Param("@duration", type.DurationDays),
            Param("@price", type.Price), Param("@access", type.AccessLevel), Param("@maxClasses", type.MaxClassesPerMonth),
            Param("@training", type.IncludesPersonalTraining), Param("@guests", type.GuestPasses),
            Param("@freeze", type.FreezeDaysAllowed), Param("@active", type.IsActive)));
    }

    public bool Update(MembershipType type)
    {
        const string sql = @"update membership_types set type_name = @name, description = @desc, duration_days = @duration,
                             price = @price, access_level = @access, max_classes_per_month = @maxClasses,
                             includes_personal_training = @training, guest_passes = @guests, freeze_days_allowed = @freeze,
                             isactive = @active, updatedat = sysutcdatetime()
                             where membership_type_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@name", type.TypeName), Param("@desc", type.Description), Param("@duration", type.DurationDays),
            Param("@price", type.Price), Param("@access", type.AccessLevel), Param("@maxClasses", type.MaxClassesPerMonth),
            Param("@training", type.IncludesPersonalTraining), Param("@guests", type.GuestPasses),
            Param("@freeze", type.FreezeDaysAllowed), Param("@active", type.IsActive),
            Param("@id", type.MembershipTypeId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update membership_types set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where membership_type_id = @id", Param("@id", id)) > 0;

    private static MembershipType MapMembershipType(SqlDataReader reader) => new()
    {
        MembershipTypeId = GetInt(reader, "membership_type_id"),
        TypeName = GetString(reader, "type_name"),
        Description = GetNullableString(reader, "description"),
        DurationDays = GetInt(reader, "duration_days"),
        Price = GetDecimal(reader, "price"),
        AccessLevel = GetString(reader, "access_level"),
        MaxClassesPerMonth = GetInt(reader, "max_classes_per_month"),
        IncludesPersonalTraining = GetBool(reader, "includes_personal_training"),
        GuestPasses = GetInt(reader, "guest_passes"),
        FreezeDaysAllowed = GetInt(reader, "freeze_days_allowed"),
        IsActive = GetBool(reader, "isactive"),
        IsDeleted = GetBool(reader, "isdeleted")
    };
}

public class MemberRepository : RepositoryBase
{
    public MemberRepository(DatabaseHelper db) : base(db) { }

    public List<Member> GetAll(string? search = null)
    {
        var sql = @"select m.member_id, m.branch_id, b.branch_name, m.first_name, m.last_name, m.phone, m.email,
                           m.date_of_birth, m.gender, m.address, m.emergency_contact, m.health_notes, m.referral_source,
                           m.fitness_goal, m.loyalty_points, m.join_date, m.isactive, m.isdeleted
                    from members m inner join branches b on m.branch_id = b.branch_id
                    where m.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (m.first_name like @search or m.last_name like @search or m.phone like @search or m.email like @search)";

        sql += " order by m.last_name, m.first_name";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<Member>();
        while (reader.Read()) list.Add(MapMember(reader));
        return list;
    }

    public Member? GetById(int id)
    {
        const string sql = @"select m.member_id, m.branch_id, b.branch_name, m.first_name, m.last_name, m.phone, m.email,
                                    m.date_of_birth, m.gender, m.address, m.emergency_contact, m.health_notes, m.referral_source,
                                    m.fitness_goal, m.loyalty_points, m.join_date, m.isactive, m.isdeleted
                             from members m inner join branches b on m.branch_id = b.branch_id
                             where m.member_id = @id and m.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? MapMember(reader) : null;
    }

    public int Create(Member member)
    {
        const string sql = @"insert into members (branch_id, first_name, last_name, phone, email, date_of_birth, gender,
                             address, emergency_contact, health_notes, referral_source, fitness_goal, loyalty_points, join_date, isactive)
                             values (@branch, @fn, @ln, @phone, @email, @dob, @gender, @addr, @emerg, @health, @referral,
                             @goal, @points, @join, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@branch", member.BranchId), Param("@fn", member.FirstName), Param("@ln", member.LastName),
            Param("@phone", member.Phone), Param("@email", member.Email), Param("@dob", member.DateOfBirth),
            Param("@gender", member.Gender), Param("@addr", member.Address), Param("@emerg", member.EmergencyContact),
            Param("@health", member.HealthNotes), Param("@referral", member.ReferralSource),
            Param("@goal", member.FitnessGoal), Param("@points", member.LoyaltyPoints),
            Param("@join", member.JoinDate), Param("@active", member.IsActive)));
    }

    public int RegisterViaStoredProcedure(Member member, int? membershipTypeId = null)
    {
        using var connection = Db.CreateConnection();
        using var command = new SqlCommand("sp_register_member", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@branch_id", member.BranchId);
        command.Parameters.AddWithValue("@first_name", member.FirstName);
        command.Parameters.AddWithValue("@last_name", member.LastName);
        command.Parameters.AddWithValue("@phone", member.Phone);
        command.Parameters.AddWithValue("@email", (object?)member.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("@membership_type_id", (object?)membershipTypeId ?? DBNull.Value);
        var output = new SqlParameter("@member_id", SqlDbType.Int) { Direction = ParameterDirection.Output };
        command.Parameters.Add(output);
        connection.Open();
        command.ExecuteNonQuery();
        return (int)output.Value;
    }

    public bool Update(Member member)
    {
        const string sql = @"update members set branch_id = @branch, first_name = @fn, last_name = @ln, phone = @phone,
                             email = @email, date_of_birth = @dob, gender = @gender, address = @addr,
                             emergency_contact = @emerg, health_notes = @health, referral_source = @referral,
                             fitness_goal = @goal, loyalty_points = @points, join_date = @join, isactive = @active,
                             updatedat = sysutcdatetime()
                             where member_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@branch", member.BranchId), Param("@fn", member.FirstName), Param("@ln", member.LastName),
            Param("@phone", member.Phone), Param("@email", member.Email), Param("@dob", member.DateOfBirth),
            Param("@gender", member.Gender), Param("@addr", member.Address), Param("@emerg", member.EmergencyContact),
            Param("@health", member.HealthNotes), Param("@referral", member.ReferralSource),
            Param("@goal", member.FitnessGoal), Param("@points", member.LoyaltyPoints),
            Param("@join", member.JoinDate), Param("@active", member.IsActive),
            Param("@id", member.MemberId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update members set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where member_id = @id", Param("@id", id)) > 0;

    private static Member MapMember(SqlDataReader reader) => new()
    {
        MemberId = GetInt(reader, "member_id"),
        BranchId = GetInt(reader, "branch_id"),
        BranchName = GetNullableString(reader, "branch_name"),
        FirstName = GetString(reader, "first_name"),
        LastName = GetString(reader, "last_name"),
        Phone = GetString(reader, "phone"),
        Email = GetNullableString(reader, "email"),
        DateOfBirth = GetNullableDateTime(reader, "date_of_birth"),
        Gender = GetNullableString(reader, "gender"),
        Address = GetNullableString(reader, "address"),
        EmergencyContact = GetNullableString(reader, "emergency_contact"),
        HealthNotes = GetNullableString(reader, "health_notes"),
        ReferralSource = GetNullableString(reader, "referral_source"),
        FitnessGoal = GetNullableString(reader, "fitness_goal"),
        LoyaltyPoints = GetInt(reader, "loyalty_points"),
        JoinDate = GetDateTime(reader, "join_date"),
        IsActive = GetBool(reader, "isactive"),
        IsDeleted = GetBool(reader, "isdeleted")
    };
}
