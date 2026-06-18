using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FitnessCenterManagement.Repositories;

public class MembershipRepository : RepositoryBase
{
    public MembershipRepository(DatabaseHelper db) : base(db) { }

    public List<Membership> GetAll(string? search = null)
    {
        var sql = @"select ms.membership_id, ms.member_id, m.first_name + ' ' + m.last_name as member_name,
                           ms.membership_type_id, mt.type_name, ms.start_date, ms.end_date, ms.status,
                           ms.auto_renew, ms.discount_percent, ms.freeze_start, ms.freeze_end, ms.notes,
                           ms.isactive, ms.isdeleted
                    from memberships ms
                    inner join members m on ms.member_id = m.member_id
                    inner join membership_types mt on ms.membership_type_id = mt.membership_type_id
                    where ms.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (m.first_name like @search or m.last_name like @search or mt.type_name like @search or ms.status like @search)";

        sql += " order by ms.start_date desc";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<Membership>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Membership? GetById(int id)
    {
        const string sql = @"select ms.membership_id, ms.member_id, m.first_name + ' ' + m.last_name as member_name,
                                    ms.membership_type_id, mt.type_name, ms.start_date, ms.end_date, ms.status,
                                    ms.auto_renew, ms.discount_percent, ms.freeze_start, ms.freeze_end, ms.notes,
                                    ms.isactive, ms.isdeleted
                             from memberships ms
                             inner join members m on ms.member_id = m.member_id
                             inner join membership_types mt on ms.membership_type_id = mt.membership_type_id
                             where ms.membership_id = @id and ms.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(Membership membership)
    {
        const string sql = @"insert into memberships (member_id, membership_type_id, start_date, end_date, status,
                             auto_renew, discount_percent, freeze_start, freeze_end, notes, isactive)
                             values (@member, @type, @start, @end, @status, @renew, @discount, @freezeStart, @freezeEnd, @notes, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@member", membership.MemberId), Param("@type", membership.MembershipTypeId),
            Param("@start", membership.StartDate), Param("@end", membership.EndDate),
            Param("@status", membership.Status), Param("@renew", membership.AutoRenew),
            Param("@discount", membership.DiscountPercent), Param("@freezeStart", membership.FreezeStart),
            Param("@freezeEnd", membership.FreezeEnd), Param("@notes", membership.Notes),
            Param("@active", membership.IsActive)));
    }

    public int CreateViaStoredProcedure(Membership membership)
    {
        using var connection = Db.CreateConnection();
        using var command = new SqlCommand("sp_create_membership", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@member_id", membership.MemberId);
        command.Parameters.AddWithValue("@membership_type_id", membership.MembershipTypeId);
        command.Parameters.AddWithValue("@auto_renew", membership.AutoRenew);
        command.Parameters.AddWithValue("@discount_percent", membership.DiscountPercent);
        var output = new SqlParameter("@membership_id", SqlDbType.Int) { Direction = ParameterDirection.Output };
        command.Parameters.Add(output);
        connection.Open();
        command.ExecuteNonQuery();
        return (int)output.Value;
    }

    public bool Update(Membership membership)
    {
        const string sql = @"update memberships set member_id = @member, membership_type_id = @type, start_date = @start,
                             end_date = @end, status = @status, auto_renew = @renew, discount_percent = @discount,
                             freeze_start = @freezeStart, freeze_end = @freezeEnd, notes = @notes, isactive = @active,
                             updatedat = sysutcdatetime()
                             where membership_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@member", membership.MemberId), Param("@type", membership.MembershipTypeId),
            Param("@start", membership.StartDate), Param("@end", membership.EndDate),
            Param("@status", membership.Status), Param("@renew", membership.AutoRenew),
            Param("@discount", membership.DiscountPercent), Param("@freezeStart", membership.FreezeStart),
            Param("@freezeEnd", membership.FreezeEnd), Param("@notes", membership.Notes),
            Param("@active", membership.IsActive), Param("@id", membership.MembershipId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update memberships set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where membership_id = @id", Param("@id", id)) > 0;

    private static Membership Map(SqlDataReader r) => new()
    {
        MembershipId = GetInt(r, "membership_id"),
        MemberId = GetInt(r, "member_id"),
        MemberName = GetNullableString(r, "member_name"),
        MembershipTypeId = GetInt(r, "membership_type_id"),
        TypeName = GetNullableString(r, "type_name"),
        StartDate = GetDateTime(r, "start_date"),
        EndDate = GetDateTime(r, "end_date"),
        Status = GetString(r, "status"),
        AutoRenew = GetBool(r, "auto_renew"),
        DiscountPercent = GetDecimal(r, "discount_percent"),
        FreezeStart = GetNullableDateTime(r, "freeze_start"),
        FreezeEnd = GetNullableDateTime(r, "freeze_end"),
        Notes = GetNullableString(r, "notes"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class TrainerRepository : RepositoryBase
{
    public TrainerRepository(DatabaseHelper db) : base(db) { }

    public List<Trainer> GetAll(string? search = null)
    {
        var sql = @"select t.trainer_id, t.branch_id, b.branch_name, t.first_name, t.last_name, t.phone, t.email,
                           t.hire_date, t.hourly_rate, t.employment_type, t.certification_level, t.certification_expiry,
                           t.bio, t.rating, t.max_clients_per_day, t.isactive, t.isdeleted
                    from trainers t inner join branches b on t.branch_id = b.branch_id
                    where t.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (t.first_name like @search or t.last_name like @search or t.email like @search)";

        sql += " order by t.last_name, t.first_name";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<Trainer>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Trainer? GetById(int id)
    {
        const string sql = @"select t.trainer_id, t.branch_id, b.branch_name, t.first_name, t.last_name, t.phone, t.email,
                                    t.hire_date, t.hourly_rate, t.employment_type, t.certification_level, t.certification_expiry,
                                    t.bio, t.rating, t.max_clients_per_day, t.isactive, t.isdeleted
                             from trainers t inner join branches b on t.branch_id = b.branch_id
                             where t.trainer_id = @id and t.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(Trainer trainer)
    {
        const string sql = @"insert into trainers (branch_id, first_name, last_name, phone, email, hire_date, hourly_rate,
                             employment_type, certification_level, certification_expiry, bio, rating, max_clients_per_day, isactive)
                             values (@branch, @fn, @ln, @phone, @email, @hire, @rate, @empType, @certLevel, @certExpiry,
                             @bio, @rating, @maxClients, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@branch", trainer.BranchId), Param("@fn", trainer.FirstName), Param("@ln", trainer.LastName),
            Param("@phone", trainer.Phone), Param("@email", trainer.Email), Param("@hire", trainer.HireDate),
            Param("@rate", trainer.HourlyRate), Param("@empType", trainer.EmploymentType),
            Param("@certLevel", trainer.CertificationLevel), Param("@certExpiry", trainer.CertificationExpiry),
            Param("@bio", trainer.Bio), Param("@rating", trainer.Rating), Param("@maxClients", trainer.MaxClientsPerDay),
            Param("@active", trainer.IsActive)));
    }

    public bool Update(Trainer trainer)
    {
        const string sql = @"update trainers set branch_id = @branch, first_name = @fn, last_name = @ln, phone = @phone,
                             email = @email, hire_date = @hire, hourly_rate = @rate, employment_type = @empType,
                             certification_level = @certLevel, certification_expiry = @certExpiry, bio = @bio,
                             rating = @rating, max_clients_per_day = @maxClients, isactive = @active,
                             updatedat = sysutcdatetime()
                             where trainer_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@branch", trainer.BranchId), Param("@fn", trainer.FirstName), Param("@ln", trainer.LastName),
            Param("@phone", trainer.Phone), Param("@email", trainer.Email), Param("@hire", trainer.HireDate),
            Param("@rate", trainer.HourlyRate), Param("@empType", trainer.EmploymentType),
            Param("@certLevel", trainer.CertificationLevel), Param("@certExpiry", trainer.CertificationExpiry),
            Param("@bio", trainer.Bio), Param("@rating", trainer.Rating), Param("@maxClients", trainer.MaxClientsPerDay),
            Param("@active", trainer.IsActive), Param("@id", trainer.TrainerId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update trainers set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where trainer_id = @id", Param("@id", id)) > 0;

    private static Trainer Map(SqlDataReader r) => new()
    {
        TrainerId = GetInt(r, "trainer_id"),
        BranchId = GetInt(r, "branch_id"),
        BranchName = GetNullableString(r, "branch_name"),
        FirstName = GetString(r, "first_name"),
        LastName = GetString(r, "last_name"),
        Phone = GetString(r, "phone"),
        Email = GetString(r, "email"),
        HireDate = GetDateTime(r, "hire_date"),
        HourlyRate = GetDecimal(r, "hourly_rate"),
        EmploymentType = GetString(r, "employment_type"),
        CertificationLevel = GetNullableString(r, "certification_level"),
        CertificationExpiry = GetNullableDateTime(r, "certification_expiry"),
        Bio = GetNullableString(r, "bio"),
        Rating = GetNullableDecimal(r, "rating"),
        MaxClientsPerDay = GetInt(r, "max_clients_per_day"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class SpecializationRepository : RepositoryBase
{
    public SpecializationRepository(DatabaseHelper db) : base(db) { }

    public List<Specialization> GetAll(string? search = null)
    {
        var sql = @"select specialization_id, specialization_name, description, category, difficulty_level,
                           required_certification, isactive, isdeleted
                    from specializations where isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (specialization_name like @search or category like @search)";

        sql += " order by specialization_name";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<Specialization>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Specialization? GetById(int id)
    {
        const string sql = @"select specialization_id, specialization_name, description, category, difficulty_level,
                                    required_certification, isactive, isdeleted
                             from specializations where specialization_id = @id and isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(Specialization specialization)
    {
        const string sql = @"insert into specializations (specialization_name, description, category, difficulty_level,
                             required_certification, isactive)
                             values (@name, @desc, @category, @difficulty, @cert, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@name", specialization.SpecializationName), Param("@desc", specialization.Description),
            Param("@category", specialization.Category), Param("@difficulty", specialization.DifficultyLevel),
            Param("@cert", specialization.RequiredCertification), Param("@active", specialization.IsActive)));
    }

    public bool Update(Specialization specialization)
    {
        const string sql = @"update specializations set specialization_name = @name, description = @desc, category = @category,
                             difficulty_level = @difficulty, required_certification = @cert, isactive = @active,
                             updatedat = sysutcdatetime()
                             where specialization_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@name", specialization.SpecializationName), Param("@desc", specialization.Description),
            Param("@category", specialization.Category), Param("@difficulty", specialization.DifficultyLevel),
            Param("@cert", specialization.RequiredCertification), Param("@active", specialization.IsActive),
            Param("@id", specialization.SpecializationId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update specializations set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where specialization_id = @id", Param("@id", id)) > 0;

    private static Specialization Map(SqlDataReader r) => new()
    {
        SpecializationId = GetInt(r, "specialization_id"),
        SpecializationName = GetString(r, "specialization_name"),
        Description = GetNullableString(r, "description"),
        Category = GetNullableString(r, "category"),
        DifficultyLevel = GetNullableString(r, "difficulty_level"),
        RequiredCertification = GetNullableString(r, "required_certification"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class TrainerSpecializationRepository : RepositoryBase
{
    public TrainerSpecializationRepository(DatabaseHelper db) : base(db) { }

    public List<TrainerSpecialization> GetAll(string? search = null)
    {
        var sql = @"select ts.trainer_specialization_id, ts.trainer_id, t.first_name + ' ' + t.last_name as trainer_name,
                           ts.specialization_id, s.specialization_name, ts.certified_date, ts.certification_number,
                           ts.expiry_date, ts.isactive, ts.isdeleted
                    from trainer_specializations ts
                    inner join trainers t on ts.trainer_id = t.trainer_id
                    inner join specializations s on ts.specialization_id = s.specialization_id
                    where ts.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (t.first_name like @search or t.last_name like @search or s.specialization_name like @search)";

        sql += " order by ts.certified_date desc";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<TrainerSpecialization>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public TrainerSpecialization? GetById(int id)
    {
        const string sql = @"select ts.trainer_specialization_id, ts.trainer_id, t.first_name + ' ' + t.last_name as trainer_name,
                                    ts.specialization_id, s.specialization_name, ts.certified_date, ts.certification_number,
                                    ts.expiry_date, ts.isactive, ts.isdeleted
                             from trainer_specializations ts
                             inner join trainers t on ts.trainer_id = t.trainer_id
                             inner join specializations s on ts.specialization_id = s.specialization_id
                             where ts.trainer_specialization_id = @id and ts.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(TrainerSpecialization item)
    {
        const string sql = @"insert into trainer_specializations (trainer_id, specialization_id, certified_date,
                             certification_number, expiry_date, isactive)
                             values (@trainer, @spec, @certDate, @certNum, @expiry, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@trainer", item.TrainerId), Param("@spec", item.SpecializationId),
            Param("@certDate", item.CertifiedDate), Param("@certNum", item.CertificationNumber),
            Param("@expiry", item.ExpiryDate), Param("@active", item.IsActive)));
    }

    public bool Update(TrainerSpecialization item)
    {
        const string sql = @"update trainer_specializations set trainer_id = @trainer, specialization_id = @spec,
                             certified_date = @certDate, certification_number = @certNum, expiry_date = @expiry,
                             isactive = @active, updatedat = sysutcdatetime()
                             where trainer_specialization_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@trainer", item.TrainerId), Param("@spec", item.SpecializationId),
            Param("@certDate", item.CertifiedDate), Param("@certNum", item.CertificationNumber),
            Param("@expiry", item.ExpiryDate), Param("@active", item.IsActive),
            Param("@id", item.TrainerSpecializationId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update trainer_specializations set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where trainer_specialization_id = @id", Param("@id", id)) > 0;

    private static TrainerSpecialization Map(SqlDataReader r) => new()
    {
        TrainerSpecializationId = GetInt(r, "trainer_specialization_id"),
        TrainerId = GetInt(r, "trainer_id"),
        TrainerName = GetNullableString(r, "trainer_name"),
        SpecializationId = GetInt(r, "specialization_id"),
        SpecializationName = GetNullableString(r, "specialization_name"),
        CertifiedDate = GetDateTime(r, "certified_date"),
        CertificationNumber = GetNullableString(r, "certification_number"),
        ExpiryDate = GetNullableDateTime(r, "expiry_date"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class FitnessClassRepository : RepositoryBase
{
    public FitnessClassRepository(DatabaseHelper db) : base(db) { }

    public List<FitnessClass> GetAll(string? search = null)
    {
        var sql = @"select fc.class_id, fc.branch_id, b.branch_name, fc.class_name, fc.description, fc.category,
                           fc.difficulty_level, fc.max_capacity, fc.duration_minutes, fc.room_name, fc.equipment_required,
                           fc.calories_burn_estimate, fc.isactive, fc.isdeleted
                    from fitness_classes fc inner join branches b on fc.branch_id = b.branch_id
                    where fc.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (fc.class_name like @search or fc.category like @search)";

        sql += " order by fc.class_name";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<FitnessClass>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public FitnessClass? GetById(int id)
    {
        const string sql = @"select fc.class_id, fc.branch_id, b.branch_name, fc.class_name, fc.description, fc.category,
                                    fc.difficulty_level, fc.max_capacity, fc.duration_minutes, fc.room_name, fc.equipment_required,
                                    fc.calories_burn_estimate, fc.isactive, fc.isdeleted
                             from fitness_classes fc inner join branches b on fc.branch_id = b.branch_id
                             where fc.class_id = @id and fc.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(FitnessClass fitnessClass)
    {
        const string sql = @"insert into fitness_classes (branch_id, class_name, description, category, difficulty_level,
                             max_capacity, duration_minutes, room_name, equipment_required, calories_burn_estimate, isactive)
                             values (@branch, @name, @desc, @category, @difficulty, @capacity, @duration, @room, @equipment, @calories, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@branch", fitnessClass.BranchId), Param("@name", fitnessClass.ClassName),
            Param("@desc", fitnessClass.Description), Param("@category", fitnessClass.Category),
            Param("@difficulty", fitnessClass.DifficultyLevel), Param("@capacity", fitnessClass.MaxCapacity),
            Param("@duration", fitnessClass.DurationMinutes), Param("@room", fitnessClass.RoomName),
            Param("@equipment", fitnessClass.EquipmentRequired), Param("@calories", fitnessClass.CaloriesBurnEstimate),
            Param("@active", fitnessClass.IsActive)));
    }

    public bool Update(FitnessClass fitnessClass)
    {
        const string sql = @"update fitness_classes set branch_id = @branch, class_name = @name, description = @desc,
                             category = @category, difficulty_level = @difficulty, max_capacity = @capacity,
                             duration_minutes = @duration, room_name = @room, equipment_required = @equipment,
                             calories_burn_estimate = @calories, isactive = @active, updatedat = sysutcdatetime()
                             where class_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@branch", fitnessClass.BranchId), Param("@name", fitnessClass.ClassName),
            Param("@desc", fitnessClass.Description), Param("@category", fitnessClass.Category),
            Param("@difficulty", fitnessClass.DifficultyLevel), Param("@capacity", fitnessClass.MaxCapacity),
            Param("@duration", fitnessClass.DurationMinutes), Param("@room", fitnessClass.RoomName),
            Param("@equipment", fitnessClass.EquipmentRequired), Param("@calories", fitnessClass.CaloriesBurnEstimate),
            Param("@active", fitnessClass.IsActive), Param("@id", fitnessClass.ClassId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update fitness_classes set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where class_id = @id", Param("@id", id)) > 0;

    private static FitnessClass Map(SqlDataReader r) => new()
    {
        ClassId = GetInt(r, "class_id"),
        BranchId = GetInt(r, "branch_id"),
        BranchName = GetNullableString(r, "branch_name"),
        ClassName = GetString(r, "class_name"),
        Description = GetNullableString(r, "description"),
        Category = GetString(r, "category"),
        DifficultyLevel = GetString(r, "difficulty_level"),
        MaxCapacity = GetInt(r, "max_capacity"),
        DurationMinutes = GetInt(r, "duration_minutes"),
        RoomName = GetNullableString(r, "room_name"),
        EquipmentRequired = GetNullableString(r, "equipment_required"),
        CaloriesBurnEstimate = GetNullableInt(r, "calories_burn_estimate"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}
