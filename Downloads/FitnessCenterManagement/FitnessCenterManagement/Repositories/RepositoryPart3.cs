using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FitnessCenterManagement.Repositories;

public class ClassScheduleRepository : RepositoryBase
{
    public ClassScheduleRepository(DatabaseHelper db) : base(db) { }

    public List<ClassSchedule> GetAll(string? search = null)
    {
        var sql = @"select cs.schedule_id, cs.class_id, fc.class_name, cs.trainer_id,
                           t.first_name + ' ' + t.last_name as trainer_name, cs.start_time, cs.end_time, cs.status,
                           cs.current_enrollment, cs.room_override, cs.notes, cs.isactive, cs.isdeleted
                    from class_schedules cs
                    inner join fitness_classes fc on cs.class_id = fc.class_id
                    inner join trainers t on cs.trainer_id = t.trainer_id
                    where cs.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (fc.class_name like @search or t.first_name like @search or t.last_name like @search or cs.status like @search)";

        sql += " order by cs.start_time desc";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<ClassSchedule>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public ClassSchedule? GetById(int id)
    {
        const string sql = @"select cs.schedule_id, cs.class_id, fc.class_name, cs.trainer_id,
                                    t.first_name + ' ' + t.last_name as trainer_name, cs.start_time, cs.end_time, cs.status,
                                    cs.current_enrollment, cs.room_override, cs.notes, cs.isactive, cs.isdeleted
                             from class_schedules cs
                             inner join fitness_classes fc on cs.class_id = fc.class_id
                             inner join trainers t on cs.trainer_id = t.trainer_id
                             where cs.schedule_id = @id and cs.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(ClassSchedule schedule)
    {
        const string sql = @"insert into class_schedules (class_id, trainer_id, start_time, end_time, status,
                             current_enrollment, room_override, notes, isactive)
                             values (@class, @trainer, @start, @end, @status, @enrollment, @room, @notes, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@class", schedule.ClassId), Param("@trainer", schedule.TrainerId),
            Param("@start", schedule.StartTime), Param("@end", schedule.EndTime),
            Param("@status", schedule.Status), Param("@enrollment", schedule.CurrentEnrollment),
            Param("@room", schedule.RoomOverride), Param("@notes", schedule.Notes),
            Param("@active", schedule.IsActive)));
    }

    public bool Update(ClassSchedule schedule)
    {
        const string sql = @"update class_schedules set class_id = @class, trainer_id = @trainer, start_time = @start,
                             end_time = @end, status = @status, current_enrollment = @enrollment, room_override = @room,
                             notes = @notes, isactive = @active, updatedat = sysutcdatetime()
                             where schedule_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@class", schedule.ClassId), Param("@trainer", schedule.TrainerId),
            Param("@start", schedule.StartTime), Param("@end", schedule.EndTime),
            Param("@status", schedule.Status), Param("@enrollment", schedule.CurrentEnrollment),
            Param("@room", schedule.RoomOverride), Param("@notes", schedule.Notes),
            Param("@active", schedule.IsActive), Param("@id", schedule.ScheduleId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update class_schedules set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where schedule_id = @id", Param("@id", id)) > 0;

    private static ClassSchedule Map(SqlDataReader r) => new()
    {
        ScheduleId = GetInt(r, "schedule_id"),
        ClassId = GetInt(r, "class_id"),
        ClassName = GetNullableString(r, "class_name"),
        TrainerId = GetInt(r, "trainer_id"),
        TrainerName = GetNullableString(r, "trainer_name"),
        StartTime = GetDateTime(r, "start_time"),
        EndTime = GetDateTime(r, "end_time"),
        Status = GetString(r, "status"),
        CurrentEnrollment = GetInt(r, "current_enrollment"),
        RoomOverride = GetNullableString(r, "room_override"),
        Notes = GetNullableString(r, "notes"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class ClassEnrollmentRepository : RepositoryBase
{
    public ClassEnrollmentRepository(DatabaseHelper db) : base(db) { }

    public List<ClassEnrollment> GetAll(string? search = null)
    {
        var sql = @"select ce.enrollment_id, ce.schedule_id, fc.class_name, ce.member_id,
                           m.first_name + ' ' + m.last_name as member_name, ce.enrollment_date, ce.status,
                           ce.check_in_time, ce.cancellation_reason, ce.notes, ce.isactive, ce.isdeleted
                    from class_enrollments ce
                    inner join class_schedules cs on ce.schedule_id = cs.schedule_id
                    inner join fitness_classes fc on cs.class_id = fc.class_id
                    inner join members m on ce.member_id = m.member_id
                    where ce.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (m.first_name like @search or m.last_name like @search or fc.class_name like @search or ce.status like @search)";

        sql += " order by ce.enrollment_date desc";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<ClassEnrollment>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public ClassEnrollment? GetById(int id)
    {
        const string sql = @"select ce.enrollment_id, ce.schedule_id, fc.class_name, ce.member_id,
                                    m.first_name + ' ' + m.last_name as member_name, ce.enrollment_date, ce.status,
                                    ce.check_in_time, ce.cancellation_reason, ce.notes, ce.isactive, ce.isdeleted
                             from class_enrollments ce
                             inner join class_schedules cs on ce.schedule_id = cs.schedule_id
                             inner join fitness_classes fc on cs.class_id = fc.class_id
                             inner join members m on ce.member_id = m.member_id
                             where ce.enrollment_id = @id and ce.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(ClassEnrollment enrollment)
    {
        const string sql = @"insert into class_enrollments (schedule_id, member_id, enrollment_date, status,
                             check_in_time, cancellation_reason, notes, isactive)
                             values (@schedule, @member, @date, @status, @checkIn, @reason, @notes, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@schedule", enrollment.ScheduleId), Param("@member", enrollment.MemberId),
            Param("@date", enrollment.EnrollmentDate), Param("@status", enrollment.Status),
            Param("@checkIn", enrollment.CheckInTime), Param("@reason", enrollment.CancellationReason),
            Param("@notes", enrollment.Notes), Param("@active", enrollment.IsActive)));
    }

    public int EnrollViaStoredProcedure(ClassEnrollment enrollment)
    {
        using var connection = Db.CreateConnection();
        using var command = new SqlCommand("sp_enroll_in_class", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@schedule_id", enrollment.ScheduleId);
        command.Parameters.AddWithValue("@member_id", enrollment.MemberId);
        var output = new SqlParameter("@enrollment_id", SqlDbType.Int) { Direction = ParameterDirection.Output };
        command.Parameters.Add(output);
        connection.Open();
        command.ExecuteNonQuery();
        return (int)output.Value;
    }

    public bool Update(ClassEnrollment enrollment)
    {
        const string sql = @"update class_enrollments set schedule_id = @schedule, member_id = @member,
                             enrollment_date = @date, status = @status, check_in_time = @checkIn,
                             cancellation_reason = @reason, notes = @notes, isactive = @active,
                             updatedat = sysutcdatetime()
                             where enrollment_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@schedule", enrollment.ScheduleId), Param("@member", enrollment.MemberId),
            Param("@date", enrollment.EnrollmentDate), Param("@status", enrollment.Status),
            Param("@checkIn", enrollment.CheckInTime), Param("@reason", enrollment.CancellationReason),
            Param("@notes", enrollment.Notes), Param("@active", enrollment.IsActive),
            Param("@id", enrollment.EnrollmentId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update class_enrollments set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where enrollment_id = @id", Param("@id", id)) > 0;

    private static ClassEnrollment Map(SqlDataReader r) => new()
    {
        EnrollmentId = GetInt(r, "enrollment_id"),
        ScheduleId = GetInt(r, "schedule_id"),
        ClassName = GetNullableString(r, "class_name"),
        MemberId = GetInt(r, "member_id"),
        MemberName = GetNullableString(r, "member_name"),
        EnrollmentDate = GetDateTime(r, "enrollment_date"),
        Status = GetString(r, "status"),
        CheckInTime = GetNullableDateTime(r, "check_in_time"),
        CancellationReason = GetNullableString(r, "cancellation_reason"),
        Notes = GetNullableString(r, "notes"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class EquipmentRepository : RepositoryBase
{
    public EquipmentRepository(DatabaseHelper db) : base(db) { }

    public List<Equipment> GetAll(string? search = null)
    {
        var sql = @"select e.equipment_id, e.branch_id, b.branch_name, e.equipment_name, e.category, e.serial_number,
                           e.purchase_date, e.purchase_price, e.warranty_until, e.location, e.condition_status,
                           e.last_maintenance_date, e.manufacturer, e.isactive, e.isdeleted
                    from equipment e inner join branches b on e.branch_id = b.branch_id
                    where e.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (e.equipment_name like @search or e.serial_number like @search or e.category like @search)";

        sql += " order by e.equipment_name";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<Equipment>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Equipment? GetById(int id)
    {
        const string sql = @"select e.equipment_id, e.branch_id, b.branch_name, e.equipment_name, e.category, e.serial_number,
                                    e.purchase_date, e.purchase_price, e.warranty_until, e.location, e.condition_status,
                                    e.last_maintenance_date, e.manufacturer, e.isactive, e.isdeleted
                             from equipment e inner join branches b on e.branch_id = b.branch_id
                             where e.equipment_id = @id and e.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(Equipment equipment)
    {
        const string sql = @"insert into equipment (branch_id, equipment_name, category, serial_number, purchase_date,
                             purchase_price, warranty_until, location, condition_status, last_maintenance_date,
                             manufacturer, isactive)
                             values (@branch, @name, @category, @serial, @purchase, @price, @warranty, @location,
                             @condition, @lastMaint, @manufacturer, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@branch", equipment.BranchId), Param("@name", equipment.EquipmentName),
            Param("@category", equipment.Category), Param("@serial", equipment.SerialNumber),
            Param("@purchase", equipment.PurchaseDate), Param("@price", equipment.PurchasePrice),
            Param("@warranty", equipment.WarrantyUntil), Param("@location", equipment.Location),
            Param("@condition", equipment.ConditionStatus), Param("@lastMaint", equipment.LastMaintenanceDate),
            Param("@manufacturer", equipment.Manufacturer), Param("@active", equipment.IsActive)));
    }

    public bool Update(Equipment equipment)
    {
        const string sql = @"update equipment set branch_id = @branch, equipment_name = @name, category = @category,
                             serial_number = @serial, purchase_date = @purchase, purchase_price = @price,
                             warranty_until = @warranty, location = @location, condition_status = @condition,
                             last_maintenance_date = @lastMaint, manufacturer = @manufacturer, isactive = @active,
                             updatedat = sysutcdatetime()
                             where equipment_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@branch", equipment.BranchId), Param("@name", equipment.EquipmentName),
            Param("@category", equipment.Category), Param("@serial", equipment.SerialNumber),
            Param("@purchase", equipment.PurchaseDate), Param("@price", equipment.PurchasePrice),
            Param("@warranty", equipment.WarrantyUntil), Param("@location", equipment.Location),
            Param("@condition", equipment.ConditionStatus), Param("@lastMaint", equipment.LastMaintenanceDate),
            Param("@manufacturer", equipment.Manufacturer), Param("@active", equipment.IsActive),
            Param("@id", equipment.EquipmentId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update equipment set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where equipment_id = @id", Param("@id", id)) > 0;

    private static Equipment Map(SqlDataReader r) => new()
    {
        EquipmentId = GetInt(r, "equipment_id"),
        BranchId = GetInt(r, "branch_id"),
        BranchName = GetNullableString(r, "branch_name"),
        EquipmentName = GetString(r, "equipment_name"),
        Category = GetString(r, "category"),
        SerialNumber = GetNullableString(r, "serial_number"),
        PurchaseDate = GetNullableDateTime(r, "purchase_date"),
        PurchasePrice = GetNullableDecimal(r, "purchase_price"),
        WarrantyUntil = GetNullableDateTime(r, "warranty_until"),
        Location = GetNullableString(r, "location"),
        ConditionStatus = GetString(r, "condition_status"),
        LastMaintenanceDate = GetNullableDateTime(r, "last_maintenance_date"),
        Manufacturer = GetNullableString(r, "manufacturer"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class EquipmentMaintenanceRepository : RepositoryBase
{
    public EquipmentMaintenanceRepository(DatabaseHelper db) : base(db) { }

    public List<EquipmentMaintenance> GetAll(string? search = null)
    {
        var sql = @"select em.maintenance_id, em.equipment_id, e.equipment_name, em.maintenance_date, em.maintenance_type,
                           em.performed_by, t.first_name + ' ' + t.last_name as trainer_name, em.cost, em.description,
                           em.next_maintenance_date, em.status, em.vendor_name, em.isactive, em.isdeleted
                    from equipment_maintenance em
                    inner join equipment e on em.equipment_id = e.equipment_id
                    left join trainers t on em.performed_by = t.trainer_id
                    where em.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (e.equipment_name like @search or em.maintenance_type like @search or em.vendor_name like @search)";

        sql += " order by em.maintenance_date desc";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<EquipmentMaintenance>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public EquipmentMaintenance? GetById(int id)
    {
        const string sql = @"select em.maintenance_id, em.equipment_id, e.equipment_name, em.maintenance_date, em.maintenance_type,
                                    em.performed_by, t.first_name + ' ' + t.last_name as trainer_name, em.cost, em.description,
                                    em.next_maintenance_date, em.status, em.vendor_name, em.isactive, em.isdeleted
                             from equipment_maintenance em
                             inner join equipment e on em.equipment_id = e.equipment_id
                             left join trainers t on em.performed_by = t.trainer_id
                             where em.maintenance_id = @id and em.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(EquipmentMaintenance maintenance)
    {
        const string sql = @"insert into equipment_maintenance (equipment_id, maintenance_date, maintenance_type, performed_by,
                             cost, description, next_maintenance_date, status, vendor_name, isactive)
                             values (@equipment, @date, @type, @performed, @cost, @desc, @next, @status, @vendor, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@equipment", maintenance.EquipmentId), Param("@date", maintenance.MaintenanceDate),
            Param("@type", maintenance.MaintenanceType), Param("@performed", maintenance.PerformedBy),
            Param("@cost", maintenance.Cost), Param("@desc", maintenance.Description),
            Param("@next", maintenance.NextMaintenanceDate), Param("@status", maintenance.Status),
            Param("@vendor", maintenance.VendorName), Param("@active", maintenance.IsActive)));
    }

    public bool Update(EquipmentMaintenance maintenance)
    {
        const string sql = @"update equipment_maintenance set equipment_id = @equipment, maintenance_date = @date,
                             maintenance_type = @type, performed_by = @performed, cost = @cost, description = @desc,
                             next_maintenance_date = @next, status = @status, vendor_name = @vendor, isactive = @active,
                             updatedat = sysutcdatetime()
                             where maintenance_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@equipment", maintenance.EquipmentId), Param("@date", maintenance.MaintenanceDate),
            Param("@type", maintenance.MaintenanceType), Param("@performed", maintenance.PerformedBy),
            Param("@cost", maintenance.Cost), Param("@desc", maintenance.Description),
            Param("@next", maintenance.NextMaintenanceDate), Param("@status", maintenance.Status),
            Param("@vendor", maintenance.VendorName), Param("@active", maintenance.IsActive),
            Param("@id", maintenance.MaintenanceId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update equipment_maintenance set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where maintenance_id = @id", Param("@id", id)) > 0;

    private static EquipmentMaintenance Map(SqlDataReader r) => new()
    {
        MaintenanceId = GetInt(r, "maintenance_id"),
        EquipmentId = GetInt(r, "equipment_id"),
        EquipmentName = GetNullableString(r, "equipment_name"),
        MaintenanceDate = GetDateTime(r, "maintenance_date"),
        MaintenanceType = GetString(r, "maintenance_type"),
        PerformedBy = GetNullableInt(r, "performed_by"),
        TrainerName = GetNullableString(r, "trainer_name"),
        Cost = GetDecimal(r, "cost"),
        Description = GetNullableString(r, "description"),
        NextMaintenanceDate = GetNullableDateTime(r, "next_maintenance_date"),
        Status = GetString(r, "status"),
        VendorName = GetNullableString(r, "vendor_name"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class PaymentRepository : RepositoryBase
{
    public PaymentRepository(DatabaseHelper db) : base(db) { }

    public List<Payment> GetAll(string? search = null)
    {
        var sql = @"select p.payment_id, p.member_id, m.first_name + ' ' + m.last_name as member_name, p.membership_id,
                           p.payment_date, p.amount, p.payment_method, p.transaction_reference, p.status, p.processed_by,
                           p.notes, p.isactive, p.isdeleted
                    from payments p
                    inner join members m on p.member_id = m.member_id
                    where p.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (m.first_name like @search or m.last_name like @search or p.transaction_reference like @search)";

        sql += " order by p.payment_date desc";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<Payment>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Payment? GetById(int id)
    {
        const string sql = @"select p.payment_id, p.member_id, m.first_name + ' ' + m.last_name as member_name, p.membership_id,
                                    p.payment_date, p.amount, p.payment_method, p.transaction_reference, p.status, p.processed_by,
                                    p.notes, p.isactive, p.isdeleted
                             from payments p
                             inner join members m on p.member_id = m.member_id
                             where p.payment_id = @id and p.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(Payment payment)
    {
        const string sql = @"insert into payments (member_id, membership_id, payment_date, amount, payment_method,
                             transaction_reference, status, processed_by, notes, isactive)
                             values (@member, @membership, @date, @amount, @method, @ref, @status, @processed, @notes, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@member", payment.MemberId), Param("@membership", payment.MembershipId),
            Param("@date", payment.PaymentDate), Param("@amount", payment.Amount),
            Param("@method", payment.PaymentMethod), Param("@ref", payment.TransactionReference),
            Param("@status", payment.Status), Param("@processed", payment.ProcessedBy),
            Param("@notes", payment.Notes), Param("@active", payment.IsActive)));
    }

    public int ProcessViaStoredProcedure(Payment payment)
    {
        using var connection = Db.CreateConnection();
        using var command = new SqlCommand("sp_process_payment", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@member_id", payment.MemberId);
        command.Parameters.AddWithValue("@membership_id", (object?)payment.MembershipId ?? DBNull.Value);
        command.Parameters.AddWithValue("@amount", payment.Amount);
        command.Parameters.AddWithValue("@payment_method", payment.PaymentMethod);
        command.Parameters.AddWithValue("@processed_by", (object?)payment.ProcessedBy ?? DBNull.Value);
        command.Parameters.AddWithValue("@transaction_reference", (object?)payment.TransactionReference ?? DBNull.Value);
        var output = new SqlParameter("@payment_id", SqlDbType.Int) { Direction = ParameterDirection.Output };
        command.Parameters.Add(output);
        connection.Open();
        command.ExecuteNonQuery();
        return (int)output.Value;
    }

    public bool Update(Payment payment)
    {
        const string sql = @"update payments set member_id = @member, membership_id = @membership, payment_date = @date,
                             amount = @amount, payment_method = @method, transaction_reference = @ref, status = @status,
                             processed_by = @processed, notes = @notes, isactive = @active, updatedat = sysutcdatetime()
                             where payment_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@member", payment.MemberId), Param("@membership", payment.MembershipId),
            Param("@date", payment.PaymentDate), Param("@amount", payment.Amount),
            Param("@method", payment.PaymentMethod), Param("@ref", payment.TransactionReference),
            Param("@status", payment.Status), Param("@processed", payment.ProcessedBy),
            Param("@notes", payment.Notes), Param("@active", payment.IsActive),
            Param("@id", payment.PaymentId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update payments set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where payment_id = @id", Param("@id", id)) > 0;

    private static Payment Map(SqlDataReader r) => new()
    {
        PaymentId = GetInt(r, "payment_id"),
        MemberId = GetInt(r, "member_id"),
        MemberName = GetNullableString(r, "member_name"),
        MembershipId = GetNullableInt(r, "membership_id"),
        PaymentDate = GetDateTime(r, "payment_date"),
        Amount = GetDecimal(r, "amount"),
        PaymentMethod = GetString(r, "payment_method"),
        TransactionReference = GetNullableString(r, "transaction_reference"),
        Status = GetString(r, "status"),
        ProcessedBy = GetNullableInt(r, "processed_by"),
        Notes = GetNullableString(r, "notes"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class AuditLogRepository : RepositoryBase
{
    public AuditLogRepository(DatabaseHelper db) : base(db) { }

    public List<AuditLogEntry> GetAll(string? search = null)
    {
        var sql = @"select audit_id, table_name, record_id, action_type, old_values, new_values, changed_by, changed_at
                    from audit_log where 1 = 1";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (table_name like @search or action_type like @search or changed_by like @search)";

        sql += " order by changed_at desc";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<AuditLogEntry>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public AuditLogEntry? GetById(long id)
    {
        const string sql = @"select audit_id, table_name, record_id, action_type, old_values, new_values, changed_by, changed_at
                             from audit_log where audit_id = @id";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public long Create(AuditLogEntry entry)
    {
        const string sql = @"insert into audit_log (table_name, record_id, action_type, old_values, new_values, changed_by, changed_at)
                             values (@table, @record, @action, @old, @new, @changedBy, @changedAt);
                             select scope_identity();";
        return Convert.ToInt64(Db.ExecuteScalar(sql,
            Param("@table", entry.TableName), Param("@record", entry.RecordId),
            Param("@action", entry.ActionType), Param("@old", entry.OldValues),
            Param("@new", entry.NewValues), Param("@changedBy", entry.ChangedBy),
            Param("@changedAt", entry.ChangedAt)));
    }

    public bool Update(AuditLogEntry entry)
    {
        const string sql = @"update audit_log set table_name = @table, record_id = @record, action_type = @action,
                             old_values = @old, new_values = @new, changed_by = @changedBy, changed_at = @changedAt
                             where audit_id = @id";
        return Db.ExecuteNonQuery(sql,
            Param("@table", entry.TableName), Param("@record", entry.RecordId),
            Param("@action", entry.ActionType), Param("@old", entry.OldValues),
            Param("@new", entry.NewValues), Param("@changedBy", entry.ChangedBy),
            Param("@changedAt", entry.ChangedAt), Param("@id", entry.AuditId)) > 0;
    }

    public bool SoftDelete(long id) =>
        Db.ExecuteNonQuery("delete from audit_log where audit_id = @id", Param("@id", id)) > 0;

    private static AuditLogEntry Map(SqlDataReader r) => new()
    {
        AuditId = GetLong(r, "audit_id"),
        TableName = GetString(r, "table_name"),
        RecordId = GetInt(r, "record_id"),
        ActionType = GetString(r, "action_type"),
        OldValues = GetNullableString(r, "old_values"),
        NewValues = GetNullableString(r, "new_values"),
        ChangedBy = GetNullableString(r, "changed_by"),
        ChangedAt = GetDateTime(r, "changed_at")
    };
}
