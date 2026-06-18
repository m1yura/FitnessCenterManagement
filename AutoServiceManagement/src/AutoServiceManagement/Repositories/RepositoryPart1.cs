using AutoServiceManagement.Data;
using AutoServiceManagement.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AutoServiceManagement.Repositories;

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

public class CustomerRepository : RepositoryBase
{
    public CustomerRepository(DatabaseHelper db) : base(db) { }

    public List<Customer> GetAll(string? search = null)
    {
        var sql = @"select customer_id, first_name, last_name, phone, email, address, city, postal_code,
                           loyalty_points, preferred_contact, notes, isactive, isdeleted
                    from customers where isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (first_name like @search or last_name like @search or phone like @search or email like @search)";

        sql += " order by last_name, first_name";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<Customer>();
        while (reader.Read()) list.Add(MapCustomer(reader));
        return list;
    }

    public Customer? GetById(int id)
    {
        const string sql = @"select customer_id, first_name, last_name, phone, email, address, city, postal_code,
                                    loyalty_points, preferred_contact, notes, isactive, isdeleted
                             from customers where customer_id = @id and isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? MapCustomer(reader) : null;
    }

    public int Create(Customer customer)
    {
        const string sql = @"insert into customers (first_name, last_name, phone, email, address, city, postal_code,
                             loyalty_points, preferred_contact, notes, isactive)
                             values (@fn, @ln, @phone, @email, @addr, @city, @postal, @points, @contact, @notes, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@fn", customer.FirstName), Param("@ln", customer.LastName), Param("@phone", customer.Phone),
            Param("@email", customer.Email), Param("@addr", customer.Address), Param("@city", customer.City),
            Param("@postal", customer.PostalCode), Param("@points", customer.LoyaltyPoints),
            Param("@contact", customer.PreferredContact), Param("@notes", customer.Notes),
            Param("@active", customer.IsActive)));
    }

    public bool Update(Customer customer)
    {
        const string sql = @"update customers set first_name = @fn, last_name = @ln, phone = @phone, email = @email,
                             address = @addr, city = @city, postal_code = @postal, loyalty_points = @points,
                             preferred_contact = @contact, notes = @notes, isactive = @active, updatedat = sysutcdatetime()
                             where customer_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@fn", customer.FirstName), Param("@ln", customer.LastName), Param("@phone", customer.Phone),
            Param("@email", customer.Email), Param("@addr", customer.Address), Param("@city", customer.City),
            Param("@postal", customer.PostalCode), Param("@points", customer.LoyaltyPoints),
            Param("@contact", customer.PreferredContact), Param("@notes", customer.Notes),
            Param("@active", customer.IsActive), Param("@id", customer.CustomerId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update customers set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where customer_id = @id", Param("@id", id)) > 0;

    private static Customer MapCustomer(SqlDataReader reader) => new()
    {
        CustomerId = GetInt(reader, "customer_id"),
        FirstName = GetString(reader, "first_name"),
        LastName = GetString(reader, "last_name"),
        Phone = GetString(reader, "phone"),
        Email = GetNullableString(reader, "email"),
        Address = GetNullableString(reader, "address"),
        City = GetNullableString(reader, "city"),
        PostalCode = GetNullableString(reader, "postal_code"),
        LoyaltyPoints = GetInt(reader, "loyalty_points"),
        PreferredContact = GetNullableString(reader, "preferred_contact"),
        Notes = GetNullableString(reader, "notes"),
        IsActive = GetBool(reader, "isactive"),
        IsDeleted = GetBool(reader, "isdeleted")
    };
}

public class VehicleRepository : RepositoryBase
{
    public VehicleRepository(DatabaseHelper db) : base(db) { }

    public List<Vehicle> GetAll(string? search = null, int? customerId = null)
    {
        var sql = @"select v.vehicle_id, v.customer_id, c.first_name + ' ' + c.last_name as customer_name,
                           v.make, v.model, v.year, v.vin, v.license_plate, v.color, v.mileage,
                           v.engine_type, v.fuel_type, v.last_service_date, v.isactive, v.isdeleted
                    from vehicles v inner join customers c on v.customer_id = c.customer_id
                    where v.isdeleted = 0";
        if (customerId.HasValue) sql += " and v.customer_id = @customerId";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (v.make like @search or v.model like @search or v.license_plate like @search or v.vin like @search)";

        sql += " order by v.vehicle_id desc";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"), Param("@customerId", customerId));
        var list = new List<Vehicle>();
        while (reader.Read()) list.Add(MapVehicle(reader));
        return list;
    }

    public Vehicle? GetById(int id)
    {
        const string sql = @"select v.vehicle_id, v.customer_id, c.first_name + ' ' + c.last_name as customer_name,
                                    v.make, v.model, v.year, v.vin, v.license_plate, v.color, v.mileage,
                                    v.engine_type, v.fuel_type, v.last_service_date, v.isactive, v.isdeleted
                             from vehicles v inner join customers c on v.customer_id = c.customer_id
                             where v.vehicle_id = @id and v.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? MapVehicle(reader) : null;
    }

    public int Create(Vehicle vehicle)
    {
        const string sql = @"insert into vehicles (customer_id, make, model, year, vin, license_plate, color, mileage,
                             engine_type, fuel_type, last_service_date, isactive)
                             values (@cid, @make, @model, @year, @vin, @plate, @color, @mileage, @engine, @fuel, @last, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@cid", vehicle.CustomerId), Param("@make", vehicle.Make), Param("@model", vehicle.Model),
            Param("@year", vehicle.Year), Param("@vin", vehicle.Vin), Param("@plate", vehicle.LicensePlate),
            Param("@color", vehicle.Color), Param("@mileage", vehicle.Mileage), Param("@engine", vehicle.EngineType),
            Param("@fuel", vehicle.FuelType), Param("@last", vehicle.LastServiceDate), Param("@active", vehicle.IsActive)));
    }

    public bool Update(Vehicle vehicle)
    {
        const string sql = @"update vehicles set customer_id = @cid, make = @make, model = @model, year = @year,
                             vin = @vin, license_plate = @plate, color = @color, mileage = @mileage,
                             engine_type = @engine, fuel_type = @fuel, last_service_date = @last, isactive = @active,
                             updatedat = sysutcdatetime() where vehicle_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@cid", vehicle.CustomerId), Param("@make", vehicle.Make), Param("@model", vehicle.Model),
            Param("@year", vehicle.Year), Param("@vin", vehicle.Vin), Param("@plate", vehicle.LicensePlate),
            Param("@color", vehicle.Color), Param("@mileage", vehicle.Mileage), Param("@engine", vehicle.EngineType),
            Param("@fuel", vehicle.FuelType), Param("@last", vehicle.LastServiceDate), Param("@active", vehicle.IsActive),
            Param("@id", vehicle.VehicleId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update vehicles set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where vehicle_id = @id", Param("@id", id)) > 0;

    private static Vehicle MapVehicle(SqlDataReader reader) => new()
    {
        VehicleId = GetInt(reader, "vehicle_id"),
        CustomerId = GetInt(reader, "customer_id"),
        CustomerName = GetNullableString(reader, "customer_name"),
        Make = GetString(reader, "make"),
        Model = GetString(reader, "model"),
        Year = GetInt(reader, "year"),
        Vin = GetString(reader, "vin"),
        LicensePlate = GetString(reader, "license_plate"),
        Color = GetNullableString(reader, "color"),
        Mileage = GetInt(reader, "mileage"),
        EngineType = GetNullableString(reader, "engine_type"),
        FuelType = GetNullableString(reader, "fuel_type"),
        LastServiceDate = GetNullableDateTime(reader, "last_service_date"),
        IsActive = GetBool(reader, "isactive"),
        IsDeleted = GetBool(reader, "isdeleted")
    };
}
