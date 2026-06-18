using AutoServiceManagement.Data;
using AutoServiceManagement.Models;
using Microsoft.Data.SqlClient;

namespace AutoServiceManagement.Repositories;

public class EmployeeRepository : RepositoryBase
{
    public EmployeeRepository(DatabaseHelper db) : base(db) { }

    public List<Employee> GetAll(string? search = null)
    {
        var sql = @"select employee_id, first_name, last_name, phone, email, position, hire_date, hourly_rate,
                           specialization, certification_level, emergency_contact, isactive, isdeleted
                    from employees where isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (first_name like @search or last_name like @search or position like @search)";

        sql += " order by last_name";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<Employee>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Employee? GetById(int id)
    {
        const string sql = @"select employee_id, first_name, last_name, phone, email, position, hire_date, hourly_rate,
                                    specialization, certification_level, emergency_contact, isactive, isdeleted
                             from employees where employee_id = @id and isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(Employee e)
    {
        const string sql = @"insert into employees (first_name, last_name, phone, email, position, hire_date, hourly_rate,
                             specialization, certification_level, emergency_contact, isactive)
                             values (@fn, @ln, @phone, @email, @pos, @hire, @rate, @spec, @cert, @emerg, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@fn", e.FirstName), Param("@ln", e.LastName), Param("@phone", e.Phone), Param("@email", e.Email),
            Param("@pos", e.Position), Param("@hire", e.HireDate), Param("@rate", e.HourlyRate),
            Param("@spec", e.Specialization), Param("@cert", e.CertificationLevel),
            Param("@emerg", e.EmergencyContact), Param("@active", e.IsActive)));
    }

    public bool Update(Employee e)
    {
        const string sql = @"update employees set first_name = @fn, last_name = @ln, phone = @phone, email = @email,
                             position = @pos, hire_date = @hire, hourly_rate = @rate, specialization = @spec,
                             certification_level = @cert, emergency_contact = @emerg, isactive = @active,
                             updatedat = sysutcdatetime() where employee_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@fn", e.FirstName), Param("@ln", e.LastName), Param("@phone", e.Phone), Param("@email", e.Email),
            Param("@pos", e.Position), Param("@hire", e.HireDate), Param("@rate", e.HourlyRate),
            Param("@spec", e.Specialization), Param("@cert", e.CertificationLevel),
            Param("@emerg", e.EmergencyContact), Param("@active", e.IsActive), Param("@id", e.EmployeeId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update employees set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where employee_id = @id", Param("@id", id)) > 0;

    private static Employee Map(SqlDataReader r) => new()
    {
        EmployeeId = GetInt(r, "employee_id"),
        FirstName = GetString(r, "first_name"),
        LastName = GetString(r, "last_name"),
        Phone = GetString(r, "phone"),
        Email = GetString(r, "email"),
        Position = GetString(r, "position"),
        HireDate = GetDateTime(r, "hire_date"),
        HourlyRate = GetDecimal(r, "hourly_rate"),
        Specialization = GetNullableString(r, "specialization"),
        CertificationLevel = GetNullableString(r, "certification_level"),
        EmergencyContact = GetNullableString(r, "emergency_contact"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class ServiceCategoryRepository : RepositoryBase
{
    public ServiceCategoryRepository(DatabaseHelper db) : base(db) { }

    public List<ServiceCategory> GetAll(string? search = null)
    {
        var sql = "select category_id, category_name, description, display_order, icon_name, isactive, isdeleted from service_categories where isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search)) sql += " and category_name like @search";
        sql += " order by display_order";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<ServiceCategory>();
        while (reader.Read())
        {
            list.Add(new ServiceCategory
            {
                CategoryId = GetInt(reader, "category_id"),
                CategoryName = GetString(reader, "category_name"),
                Description = GetNullableString(reader, "description"),
                DisplayOrder = GetInt(reader, "display_order"),
                IconName = GetNullableString(reader, "icon_name"),
                IsActive = GetBool(reader, "isactive"),
                IsDeleted = GetBool(reader, "isdeleted")
            });
        }

        return list;
    }

    public ServiceCategory? GetById(int id)
    {
        const string sql = "select category_id, category_name, description, display_order, icon_name, isactive, isdeleted from service_categories where category_id = @id and isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        if (!reader.Read()) return null;
        return new ServiceCategory
        {
            CategoryId = GetInt(reader, "category_id"),
            CategoryName = GetString(reader, "category_name"),
            Description = GetNullableString(reader, "description"),
            DisplayOrder = GetInt(reader, "display_order"),
            IconName = GetNullableString(reader, "icon_name"),
            IsActive = GetBool(reader, "isactive"),
            IsDeleted = GetBool(reader, "isdeleted")
        };
    }

    public int Create(ServiceCategory c)
    {
        const string sql = @"insert into service_categories (category_name, description, display_order, icon_name, isactive)
                             values (@name, @desc, @order, @icon, @active); select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@name", c.CategoryName), Param("@desc", c.Description), Param("@order", c.DisplayOrder),
            Param("@icon", c.IconName), Param("@active", c.IsActive)));
    }

    public bool Update(ServiceCategory c)
    {
        const string sql = @"update service_categories set category_name = @name, description = @desc, display_order = @order,
                             icon_name = @icon, isactive = @active, updatedat = sysutcdatetime()
                             where category_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@name", c.CategoryName), Param("@desc", c.Description), Param("@order", c.DisplayOrder),
            Param("@icon", c.IconName), Param("@active", c.IsActive), Param("@id", c.CategoryId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update service_categories set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where category_id = @id", Param("@id", id)) > 0;
}

public class ServiceRepository : RepositoryBase
{
    public ServiceRepository(DatabaseHelper db) : base(db) { }

    public List<ServiceItem> GetAll(string? search = null, int? categoryId = null)
    {
        var sql = @"select s.service_id, s.category_id, c.category_name, s.service_name, s.description, s.base_price,
                           s.estimated_duration_minutes, s.skill_level_required, s.warranty_days, s.isactive, s.isdeleted
                    from services s inner join service_categories c on s.category_id = c.category_id
                    where s.isdeleted = 0";
        if (categoryId.HasValue) sql += " and s.category_id = @catId";
        if (!string.IsNullOrWhiteSpace(search)) sql += " and s.service_name like @search";
        sql += " order by s.service_name";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"), Param("@catId", categoryId));
        var list = new List<ServiceItem>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public ServiceItem? GetById(int id)
    {
        const string sql = @"select s.service_id, s.category_id, c.category_name, s.service_name, s.description, s.base_price,
                                    s.estimated_duration_minutes, s.skill_level_required, s.warranty_days, s.isactive, s.isdeleted
                             from services s inner join service_categories c on s.category_id = c.category_id
                             where s.service_id = @id and s.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(ServiceItem s)
    {
        const string sql = @"insert into services (category_id, service_name, description, base_price, estimated_duration_minutes,
                             skill_level_required, warranty_days, isactive)
                             values (@cat, @name, @desc, @price, @dur, @skill, @warranty, @active); select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@cat", s.CategoryId), Param("@name", s.ServiceName), Param("@desc", s.Description),
            Param("@price", s.BasePrice), Param("@dur", s.EstimatedDurationMinutes),
            Param("@skill", s.SkillLevelRequired), Param("@warranty", s.WarrantyDays), Param("@active", s.IsActive)));
    }

    public bool Update(ServiceItem s)
    {
        const string sql = @"update services set category_id = @cat, service_name = @name, description = @desc, base_price = @price,
                             estimated_duration_minutes = @dur, skill_level_required = @skill, warranty_days = @warranty,
                             isactive = @active, updatedat = sysutcdatetime() where service_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@cat", s.CategoryId), Param("@name", s.ServiceName), Param("@desc", s.Description),
            Param("@price", s.BasePrice), Param("@dur", s.EstimatedDurationMinutes),
            Param("@skill", s.SkillLevelRequired), Param("@warranty", s.WarrantyDays),
            Param("@active", s.IsActive), Param("@id", s.ServiceId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update services set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where service_id = @id", Param("@id", id)) > 0;

    private static ServiceItem Map(SqlDataReader r) => new()
    {
        ServiceId = GetInt(r, "service_id"),
        CategoryId = GetInt(r, "category_id"),
        CategoryName = GetNullableString(r, "category_name"),
        ServiceName = GetString(r, "service_name"),
        Description = GetNullableString(r, "description"),
        BasePrice = GetDecimal(r, "base_price"),
        EstimatedDurationMinutes = GetInt(r, "estimated_duration_minutes"),
        SkillLevelRequired = GetNullableString(r, "skill_level_required"),
        WarrantyDays = GetInt(r, "warranty_days"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class SupplierRepository : RepositoryBase
{
    public SupplierRepository(DatabaseHelper db) : base(db) { }

    public List<Supplier> GetAll(string? search = null)
    {
        var sql = @"select supplier_id, supplier_name, contact_person, phone, email, address, payment_terms, rating, isactive, isdeleted
                    from suppliers where isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search)) sql += " and supplier_name like @search";
        sql += " order by supplier_name";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<Supplier>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Supplier? GetById(int id)
    {
        const string sql = @"select supplier_id, supplier_name, contact_person, phone, email, address, payment_terms, rating, isactive, isdeleted
                             from suppliers where supplier_id = @id and isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(Supplier s)
    {
        const string sql = @"insert into suppliers (supplier_name, contact_person, phone, email, address, payment_terms, rating, isactive)
                             values (@name, @contact, @phone, @email, @addr, @terms, @rating, @active); select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@name", s.SupplierName), Param("@contact", s.ContactPerson), Param("@phone", s.Phone),
            Param("@email", s.Email), Param("@addr", s.Address), Param("@terms", s.PaymentTerms),
            Param("@rating", s.Rating), Param("@active", s.IsActive)));
    }

    public bool Update(Supplier s)
    {
        const string sql = @"update suppliers set supplier_name = @name, contact_person = @contact, phone = @phone, email = @email,
                             address = @addr, payment_terms = @terms, rating = @rating, isactive = @active, updatedat = sysutcdatetime()
                             where supplier_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@name", s.SupplierName), Param("@contact", s.ContactPerson), Param("@phone", s.Phone),
            Param("@email", s.Email), Param("@addr", s.Address), Param("@terms", s.PaymentTerms),
            Param("@rating", s.Rating), Param("@active", s.IsActive), Param("@id", s.SupplierId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update suppliers set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where supplier_id = @id", Param("@id", id)) > 0;

    private static Supplier Map(SqlDataReader r) => new()
    {
        SupplierId = GetInt(r, "supplier_id"),
        SupplierName = GetString(r, "supplier_name"),
        ContactPerson = GetNullableString(r, "contact_person"),
        Phone = GetString(r, "phone"),
        Email = GetNullableString(r, "email"),
        Address = GetNullableString(r, "address"),
        PaymentTerms = GetNullableString(r, "payment_terms"),
        Rating = GetNullableDecimal(r, "rating"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class PartRepository : RepositoryBase
{
    public PartRepository(DatabaseHelper db) : base(db) { }

    public List<Part> GetAll(string? search = null, string? stockStatus = null)
    {
        var sql = @"select p.part_id, p.supplier_id, s.supplier_name, p.part_number, p.part_name, p.description,
                           p.unit_price, p.quantity_in_stock, p.reorder_level, p.warehouse_location, p.weight_kg,
                           p.isactive, p.isdeleted
                    from parts p inner join suppliers s on p.supplier_id = s.supplier_id
                    where p.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (p.part_name like @search or p.part_number like @search)";
        if (stockStatus == "low")
            sql += " and p.quantity_in_stock <= p.reorder_level";
        if (stockStatus == "out")
            sql += " and p.quantity_in_stock = 0";

        sql += " order by p.part_name";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<Part>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Part? GetById(int id)
    {
        const string sql = @"select p.part_id, p.supplier_id, s.supplier_name, p.part_number, p.part_name, p.description,
                                    p.unit_price, p.quantity_in_stock, p.reorder_level, p.warehouse_location, p.weight_kg,
                                    p.isactive, p.isdeleted
                             from parts p inner join suppliers s on p.supplier_id = s.supplier_id
                             where p.part_id = @id and p.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(Part p)
    {
        const string sql = @"insert into parts (supplier_id, part_number, part_name, description, unit_price, quantity_in_stock,
                             reorder_level, warehouse_location, weight_kg, isactive)
                             values (@sid, @num, @name, @desc, @price, @qty, @reorder, @loc, @weight, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@sid", p.SupplierId), Param("@num", p.PartNumber), Param("@name", p.PartName),
            Param("@desc", p.Description), Param("@price", p.UnitPrice), Param("@qty", p.QuantityInStock),
            Param("@reorder", p.ReorderLevel), Param("@loc", p.WarehouseLocation),
            Param("@weight", p.WeightKg), Param("@active", p.IsActive)));
    }

    public bool Update(Part p)
    {
        const string sql = @"update parts set supplier_id = @sid, part_number = @num, part_name = @name, description = @desc,
                             unit_price = @price, quantity_in_stock = @qty, reorder_level = @reorder,
                             warehouse_location = @loc, weight_kg = @weight, isactive = @active, updatedat = sysutcdatetime()
                             where part_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@sid", p.SupplierId), Param("@num", p.PartNumber), Param("@name", p.PartName),
            Param("@desc", p.Description), Param("@price", p.UnitPrice), Param("@qty", p.QuantityInStock),
            Param("@reorder", p.ReorderLevel), Param("@loc", p.WarehouseLocation),
            Param("@weight", p.WeightKg), Param("@active", p.IsActive), Param("@id", p.PartId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update parts set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where part_id = @id", Param("@id", id)) > 0;

    private static Part Map(SqlDataReader r) => new()
    {
        PartId = GetInt(r, "part_id"),
        SupplierId = GetInt(r, "supplier_id"),
        SupplierName = GetNullableString(r, "supplier_name"),
        PartNumber = GetString(r, "part_number"),
        PartName = GetString(r, "part_name"),
        Description = GetNullableString(r, "description"),
        UnitPrice = GetDecimal(r, "unit_price"),
        QuantityInStock = GetInt(r, "quantity_in_stock"),
        ReorderLevel = GetInt(r, "reorder_level"),
        WarehouseLocation = GetNullableString(r, "warehouse_location"),
        WeightKg = GetNullableDecimal(r, "weight_kg"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}
