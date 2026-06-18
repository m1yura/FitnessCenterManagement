using AutoServiceManagement.Data;
using AutoServiceManagement.Models;
using Microsoft.Data.SqlClient;

namespace AutoServiceManagement.Repositories;

public class AppointmentRepository : RepositoryBase
{
    public AppointmentRepository(DatabaseHelper db) : base(db) { }

    public List<Appointment> GetAll(string? search = null, string? status = null)
    {
        var sql = @"select a.appointment_id, a.customer_id, c.first_name + ' ' + c.last_name as customer_name,
                           a.vehicle_id, v.make + ' ' + v.model as vehicle_info, a.employee_id,
                           e.first_name + ' ' + e.last_name as employee_name, a.appointment_date, a.duration_minutes,
                           a.status, a.reason, a.notes, a.reminder_sent, a.isactive, a.isdeleted
                    from appointments a
                    inner join customers c on a.customer_id = c.customer_id
                    inner join vehicles v on a.vehicle_id = v.vehicle_id
                    left join employees e on a.employee_id = e.employee_id
                    where a.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(status)) sql += " and a.status = @status";
        if (!string.IsNullOrWhiteSpace(search)) sql += " and (c.first_name like @search or c.last_name like @search or a.reason like @search)";
        sql += " order by a.appointment_date desc";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"), Param("@status", status));
        var list = new List<Appointment>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Appointment? GetById(int id)
    {
        const string sql = @"select a.appointment_id, a.customer_id, c.first_name + ' ' + c.last_name as customer_name,
                                    a.vehicle_id, v.make + ' ' + v.model as vehicle_info, a.employee_id,
                                    e.first_name + ' ' + e.last_name as employee_name, a.appointment_date, a.duration_minutes,
                                    a.status, a.reason, a.notes, a.reminder_sent, a.isactive, a.isdeleted
                             from appointments a
                             inner join customers c on a.customer_id = c.customer_id
                             inner join vehicles v on a.vehicle_id = v.vehicle_id
                             left join employees e on a.employee_id = e.employee_id
                             where a.appointment_id = @id and a.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(Appointment a)
    {
        const string sql = @"insert into appointments (customer_id, vehicle_id, employee_id, appointment_date, duration_minutes,
                             status, reason, notes, reminder_sent, isactive)
                             values (@cid, @vid, @eid, @date, @dur, @status, @reason, @notes, @reminder, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@cid", a.CustomerId), Param("@vid", a.VehicleId), Param("@eid", a.EmployeeId),
            Param("@date", a.AppointmentDate), Param("@dur", a.DurationMinutes), Param("@status", a.Status),
            Param("@reason", a.Reason), Param("@notes", a.Notes), Param("@reminder", a.ReminderSent),
            Param("@active", a.IsActive)));
    }

    public bool Update(Appointment a)
    {
        const string sql = @"update appointments set customer_id = @cid, vehicle_id = @vid, employee_id = @eid,
                             appointment_date = @date, duration_minutes = @dur, status = @status, reason = @reason,
                             notes = @notes, reminder_sent = @reminder, isactive = @active, updatedat = sysutcdatetime()
                             where appointment_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@cid", a.CustomerId), Param("@vid", a.VehicleId), Param("@eid", a.EmployeeId),
            Param("@date", a.AppointmentDate), Param("@dur", a.DurationMinutes), Param("@status", a.Status),
            Param("@reason", a.Reason), Param("@notes", a.Notes), Param("@reminder", a.ReminderSent),
            Param("@active", a.IsActive), Param("@id", a.AppointmentId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update appointments set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where appointment_id = @id", Param("@id", id)) > 0;

    public int CreateViaStoredProcedure(Appointment a)
    {
        using var connection = Db.CreateConnection();
        using var command = new SqlCommand("sp_register_appointment", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@customer_id", a.CustomerId);
        command.Parameters.AddWithValue("@vehicle_id", a.VehicleId);
        command.Parameters.AddWithValue("@appointment_date", a.AppointmentDate);
        command.Parameters.AddWithValue("@employee_id", (object?)a.EmployeeId ?? DBNull.Value);
        command.Parameters.AddWithValue("@reason", (object?)a.Reason ?? DBNull.Value);
        command.Parameters.AddWithValue("@duration_minutes", a.DurationMinutes);
        var output = new SqlParameter("@appointment_id", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        command.Parameters.Add(output);
        connection.Open();
        command.ExecuteNonQuery();
        return (int)output.Value;
    }

    private static Appointment Map(SqlDataReader r) => new()
    {
        AppointmentId = GetInt(r, "appointment_id"),
        CustomerId = GetInt(r, "customer_id"),
        CustomerName = GetNullableString(r, "customer_name"),
        VehicleId = GetInt(r, "vehicle_id"),
        VehicleInfo = GetNullableString(r, "vehicle_info"),
        EmployeeId = GetNullableInt(r, "employee_id"),
        EmployeeName = GetNullableString(r, "employee_name"),
        AppointmentDate = GetDateTime(r, "appointment_date"),
        DurationMinutes = GetInt(r, "duration_minutes"),
        Status = GetString(r, "status"),
        Reason = GetNullableString(r, "reason"),
        Notes = GetNullableString(r, "notes"),
        ReminderSent = GetBool(r, "reminder_sent"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class WorkOrderRepository : RepositoryBase
{
    public WorkOrderRepository(DatabaseHelper db) : base(db) { }

    public List<WorkOrder> GetAll(string? search = null, string? status = null)
    {
        var sql = @"select wo.work_order_id, wo.customer_id, c.first_name + ' ' + c.last_name as customer_name,
                           wo.vehicle_id, v.make + ' ' + v.model as vehicle_info, wo.employee_id,
                           e.first_name + ' ' + e.last_name as employee_name, wo.appointment_id, wo.order_number,
                           wo.status, wo.priority, wo.opened_at, wo.completed_at, wo.total_amount, wo.discount_percent,
                           wo.diagnosis_notes, wo.customer_complaint, wo.isactive, wo.isdeleted
                    from work_orders wo
                    inner join customers c on wo.customer_id = c.customer_id
                    inner join vehicles v on wo.vehicle_id = v.vehicle_id
                    left join employees e on wo.employee_id = e.employee_id
                    where wo.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(status)) sql += " and wo.status = @status";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " and (wo.order_number like @search or c.first_name like @search or c.last_name like @search)";
        sql += " order by wo.opened_at desc";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"), Param("@status", status));
        var list = new List<WorkOrder>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public WorkOrder? GetById(int id)
    {
        const string sql = @"select wo.work_order_id, wo.customer_id, c.first_name + ' ' + c.last_name as customer_name,
                                    wo.vehicle_id, v.make + ' ' + v.model as vehicle_info, wo.employee_id,
                                    e.first_name + ' ' + e.last_name as employee_name, wo.appointment_id, wo.order_number,
                                    wo.status, wo.priority, wo.opened_at, wo.completed_at, wo.total_amount, wo.discount_percent,
                                    wo.diagnosis_notes, wo.customer_complaint, wo.isactive, wo.isdeleted
                             from work_orders wo
                             inner join customers c on wo.customer_id = c.customer_id
                             inner join vehicles v on wo.vehicle_id = v.vehicle_id
                             left join employees e on wo.employee_id = e.employee_id
                             where wo.work_order_id = @id and wo.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(WorkOrder wo)
    {
        const string sql = @"insert into work_orders (customer_id, vehicle_id, employee_id, appointment_id, order_number,
                             status, priority, opened_at, total_amount, discount_percent, diagnosis_notes, customer_complaint, isactive)
                             values (@cid, @vid, @eid, @aid, @num, @status, @priority, @opened, @total, @discount, @diag, @complaint, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@cid", wo.CustomerId), Param("@vid", wo.VehicleId), Param("@eid", wo.EmployeeId),
            Param("@aid", wo.AppointmentId), Param("@num", wo.OrderNumber), Param("@status", wo.Status),
            Param("@priority", wo.Priority), Param("@opened", wo.OpenedAt), Param("@total", wo.TotalAmount),
            Param("@discount", wo.DiscountPercent), Param("@diag", wo.DiagnosisNotes),
            Param("@complaint", wo.CustomerComplaint), Param("@active", wo.IsActive)));
    }

    public int CreateViaStoredProcedure(WorkOrder wo)
    {
        using var connection = Db.CreateConnection();
        using var command = new SqlCommand("sp_create_work_order", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@customer_id", wo.CustomerId);
        command.Parameters.AddWithValue("@vehicle_id", wo.VehicleId);
        command.Parameters.AddWithValue("@employee_id", (object?)wo.EmployeeId ?? DBNull.Value);
        command.Parameters.AddWithValue("@priority", wo.Priority);
        command.Parameters.AddWithValue("@customer_complaint", (object?)wo.CustomerComplaint ?? DBNull.Value);
        var output = new SqlParameter("@work_order_id", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        command.Parameters.Add(output);
        connection.Open();
        command.ExecuteNonQuery();
        return (int)output.Value;
    }

    public bool Update(WorkOrder wo)
    {
        const string sql = @"update work_orders set customer_id = @cid, vehicle_id = @vid, employee_id = @eid,
                             appointment_id = @aid, order_number = @num, status = @status, priority = @priority,
                             opened_at = @opened, completed_at = @completed, total_amount = @total, discount_percent = @discount,
                             diagnosis_notes = @diag, customer_complaint = @complaint, isactive = @active, updatedat = sysutcdatetime()
                             where work_order_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@cid", wo.CustomerId), Param("@vid", wo.VehicleId), Param("@eid", wo.EmployeeId),
            Param("@aid", wo.AppointmentId), Param("@num", wo.OrderNumber), Param("@status", wo.Status),
            Param("@priority", wo.Priority), Param("@opened", wo.OpenedAt), Param("@completed", wo.CompletedAt),
            Param("@total", wo.TotalAmount), Param("@discount", wo.DiscountPercent),
            Param("@diag", wo.DiagnosisNotes), Param("@complaint", wo.CustomerComplaint),
            Param("@active", wo.IsActive), Param("@id", wo.WorkOrderId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update work_orders set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where work_order_id = @id", Param("@id", id)) > 0;

    private static WorkOrder Map(SqlDataReader r) => new()
    {
        WorkOrderId = GetInt(r, "work_order_id"),
        CustomerId = GetInt(r, "customer_id"),
        CustomerName = GetNullableString(r, "customer_name"),
        VehicleId = GetInt(r, "vehicle_id"),
        VehicleInfo = GetNullableString(r, "vehicle_info"),
        EmployeeId = GetNullableInt(r, "employee_id"),
        EmployeeName = GetNullableString(r, "employee_name"),
        AppointmentId = GetNullableInt(r, "appointment_id"),
        OrderNumber = GetString(r, "order_number"),
        Status = GetString(r, "status"),
        Priority = GetString(r, "priority"),
        OpenedAt = GetDateTime(r, "opened_at"),
        CompletedAt = GetNullableDateTime(r, "completed_at"),
        TotalAmount = GetDecimal(r, "total_amount"),
        DiscountPercent = GetDecimal(r, "discount_percent"),
        DiagnosisNotes = GetNullableString(r, "diagnosis_notes"),
        CustomerComplaint = GetNullableString(r, "customer_complaint"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class WorkOrderServiceRepository : RepositoryBase
{
    public WorkOrderServiceRepository(DatabaseHelper db) : base(db) { }

    public List<WorkOrderService> GetAll(int? workOrderId = null)
    {
        var sql = @"select wos.work_order_service_id, wos.work_order_id, wos.service_id, s.service_name,
                           wos.quantity, wos.unit_price, wos.discount_amount, wos.line_total, wos.performed_by,
                           wos.notes, wos.isactive, wos.isdeleted
                    from work_order_services wos
                    inner join services s on wos.service_id = s.service_id
                    where wos.isdeleted = 0";
        if (workOrderId.HasValue) sql += " and wos.work_order_id = @woId";
        sql += " order by wos.work_order_service_id";
        using var reader = Db.ExecuteReader(sql, Param("@woId", workOrderId));
        var list = new List<WorkOrderService>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public WorkOrderService? GetById(int id)
    {
        const string sql = @"select wos.work_order_service_id, wos.work_order_id, wos.service_id, s.service_name,
                                    wos.quantity, wos.unit_price, wos.discount_amount, wos.line_total, wos.performed_by,
                                    wos.notes, wos.isactive, wos.isdeleted
                             from work_order_services wos
                             inner join services s on wos.service_id = s.service_id
                             where wos.work_order_service_id = @id and wos.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(WorkOrderService item)
    {
        const string sql = @"insert into work_order_services (work_order_id, service_id, quantity, unit_price, discount_amount,
                             line_total, performed_by, notes, isactive)
                             values (@wo, @sid, @qty, @price, @discount, @total, @performed, @notes, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@wo", item.WorkOrderId), Param("@sid", item.ServiceId), Param("@qty", item.Quantity),
            Param("@price", item.UnitPrice), Param("@discount", item.DiscountAmount), Param("@total", item.LineTotal),
            Param("@performed", item.PerformedBy), Param("@notes", item.Notes), Param("@active", item.IsActive)));
    }

    public bool Update(WorkOrderService item)
    {
        const string sql = @"update work_order_services set work_order_id = @wo, service_id = @sid, quantity = @qty,
                             unit_price = @price, discount_amount = @discount, line_total = @total, performed_by = @performed,
                             notes = @notes, isactive = @active, updatedat = sysutcdatetime()
                             where work_order_service_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@wo", item.WorkOrderId), Param("@sid", item.ServiceId), Param("@qty", item.Quantity),
            Param("@price", item.UnitPrice), Param("@discount", item.DiscountAmount), Param("@total", item.LineTotal),
            Param("@performed", item.PerformedBy), Param("@notes", item.Notes), Param("@active", item.IsActive),
            Param("@id", item.WorkOrderServiceId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update work_order_services set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where work_order_service_id = @id", Param("@id", id)) > 0;

    private static WorkOrderService Map(SqlDataReader r) => new()
    {
        WorkOrderServiceId = GetInt(r, "work_order_service_id"),
        WorkOrderId = GetInt(r, "work_order_id"),
        ServiceId = GetInt(r, "service_id"),
        ServiceName = GetNullableString(r, "service_name"),
        Quantity = GetInt(r, "quantity"),
        UnitPrice = GetDecimal(r, "unit_price"),
        DiscountAmount = GetDecimal(r, "discount_amount"),
        LineTotal = GetDecimal(r, "line_total"),
        PerformedBy = GetNullableInt(r, "performed_by"),
        Notes = GetNullableString(r, "notes"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class WorkOrderPartRepository : RepositoryBase
{
    public WorkOrderPartRepository(DatabaseHelper db) : base(db) { }

    public List<WorkOrderPart> GetAll(int? workOrderId = null)
    {
        var sql = @"select wop.work_order_part_id, wop.work_order_id, wop.part_id, p.part_name,
                           wop.quantity, wop.unit_price, wop.line_total, wop.installed_by, wop.warranty_until,
                           wop.isactive, wop.isdeleted
                    from work_order_parts wop
                    inner join parts p on wop.part_id = p.part_id
                    where wop.isdeleted = 0";
        if (workOrderId.HasValue) sql += " and wop.work_order_id = @woId";
        using var reader = Db.ExecuteReader(sql, Param("@woId", workOrderId));
        var list = new List<WorkOrderPart>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public WorkOrderPart? GetById(int id)
    {
        const string sql = @"select wop.work_order_part_id, wop.work_order_id, wop.part_id, p.part_name,
                                    wop.quantity, wop.unit_price, wop.line_total, wop.installed_by, wop.warranty_until,
                                    wop.isactive, wop.isdeleted
                             from work_order_parts wop
                             inner join parts p on wop.part_id = p.part_id
                             where wop.work_order_part_id = @id and wop.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(WorkOrderPart item)
    {
        const string sql = @"insert into work_order_parts (work_order_id, part_id, quantity, unit_price, line_total,
                             installed_by, warranty_until, isactive)
                             values (@wo, @pid, @qty, @price, @total, @installed, @warranty, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@wo", item.WorkOrderId), Param("@pid", item.PartId), Param("@qty", item.Quantity),
            Param("@price", item.UnitPrice), Param("@total", item.LineTotal),
            Param("@installed", item.InstalledBy), Param("@warranty", item.WarrantyUntil),
            Param("@active", item.IsActive)));
    }

    public bool Update(WorkOrderPart item)
    {
        const string sql = @"update work_order_parts set work_order_id = @wo, part_id = @pid, quantity = @qty,
                             unit_price = @price, line_total = @total, installed_by = @installed, warranty_until = @warranty,
                             isactive = @active, updatedat = sysutcdatetime()
                             where work_order_part_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@wo", item.WorkOrderId), Param("@pid", item.PartId), Param("@qty", item.Quantity),
            Param("@price", item.UnitPrice), Param("@total", item.LineTotal),
            Param("@installed", item.InstalledBy), Param("@warranty", item.WarrantyUntil),
            Param("@active", item.IsActive), Param("@id", item.WorkOrderPartId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update work_order_parts set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where work_order_part_id = @id", Param("@id", id)) > 0;

    private static WorkOrderPart Map(SqlDataReader r) => new()
    {
        WorkOrderPartId = GetInt(r, "work_order_part_id"),
        WorkOrderId = GetInt(r, "work_order_id"),
        PartId = GetInt(r, "part_id"),
        PartName = GetNullableString(r, "part_name"),
        Quantity = GetInt(r, "quantity"),
        UnitPrice = GetDecimal(r, "unit_price"),
        LineTotal = GetDecimal(r, "line_total"),
        InstalledBy = GetNullableInt(r, "installed_by"),
        WarrantyUntil = GetNullableDateTime(r, "warranty_until"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class PaymentRepository : RepositoryBase
{
    public PaymentRepository(DatabaseHelper db) : base(db) { }

    public List<Payment> GetAll(string? search = null, int? workOrderId = null)
    {
        var sql = @"select p.payment_id, p.work_order_id, wo.order_number, p.payment_date, p.amount, p.payment_method,
                           p.transaction_reference, p.status, p.processed_by, p.notes, p.isactive, p.isdeleted
                    from payments p
                    inner join work_orders wo on p.work_order_id = wo.work_order_id
                    where p.isdeleted = 0";
        if (workOrderId.HasValue) sql += " and p.work_order_id = @woId";
        if (!string.IsNullOrWhiteSpace(search)) sql += " and (wo.order_number like @search or p.transaction_reference like @search)";
        sql += " order by p.payment_date desc";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"), Param("@woId", workOrderId));
        var list = new List<Payment>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Payment? GetById(int id)
    {
        const string sql = @"select p.payment_id, p.work_order_id, wo.order_number, p.payment_date, p.amount, p.payment_method,
                                    p.transaction_reference, p.status, p.processed_by, p.notes, p.isactive, p.isdeleted
                             from payments p
                             inner join work_orders wo on p.work_order_id = wo.work_order_id
                             where p.payment_id = @id and p.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(Payment payment)
    {
        const string sql = @"insert into payments (work_order_id, payment_date, amount, payment_method, transaction_reference,
                             status, processed_by, notes, isactive)
                             values (@wo, @date, @amount, @method, @ref, @status, @processed, @notes, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@wo", payment.WorkOrderId), Param("@date", payment.PaymentDate), Param("@amount", payment.Amount),
            Param("@method", payment.PaymentMethod), Param("@ref", payment.TransactionReference),
            Param("@status", payment.Status), Param("@processed", payment.ProcessedBy),
            Param("@notes", payment.Notes), Param("@active", payment.IsActive)));
    }

    public int CreateViaStoredProcedure(Payment payment)
    {
        using var connection = Db.CreateConnection();
        using var command = new SqlCommand("sp_process_payment", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@work_order_id", payment.WorkOrderId);
        command.Parameters.AddWithValue("@amount", payment.Amount);
        command.Parameters.AddWithValue("@payment_method", payment.PaymentMethod);
        command.Parameters.AddWithValue("@processed_by", (object?)payment.ProcessedBy ?? DBNull.Value);
        command.Parameters.AddWithValue("@transaction_reference", (object?)payment.TransactionReference ?? DBNull.Value);
        var output = new SqlParameter("@payment_id", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        command.Parameters.Add(output);
        connection.Open();
        command.ExecuteNonQuery();
        return (int)output.Value;
    }

    public bool Update(Payment payment)
    {
        const string sql = @"update payments set work_order_id = @wo, payment_date = @date, amount = @amount,
                             payment_method = @method, transaction_reference = @ref, status = @status,
                             processed_by = @processed, notes = @notes, isactive = @active, updatedat = sysutcdatetime()
                             where payment_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@wo", payment.WorkOrderId), Param("@date", payment.PaymentDate), Param("@amount", payment.Amount),
            Param("@method", payment.PaymentMethod), Param("@ref", payment.TransactionReference),
            Param("@status", payment.Status), Param("@processed", payment.ProcessedBy),
            Param("@notes", payment.Notes), Param("@active", payment.IsActive), Param("@id", payment.PaymentId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update payments set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where payment_id = @id", Param("@id", id)) > 0;

    private static Payment Map(SqlDataReader r) => new()
    {
        PaymentId = GetInt(r, "payment_id"),
        WorkOrderId = GetInt(r, "work_order_id"),
        OrderNumber = GetNullableString(r, "order_number"),
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

public class PartSupplyRepository : RepositoryBase
{
    public PartSupplyRepository(DatabaseHelper db) : base(db) { }

    public List<PartSupply> GetAll(string? search = null)
    {
        var sql = @"select ps.supply_id, ps.supplier_id, s.supplier_name, ps.part_id, p.part_name, ps.supply_date,
                           ps.quantity, ps.unit_cost, ps.total_cost, ps.invoice_number, ps.received_by, ps.status,
                           ps.isactive, ps.isdeleted
                    from part_supplies ps
                    inner join suppliers s on ps.supplier_id = s.supplier_id
                    inner join parts p on ps.part_id = p.part_id
                    where ps.isdeleted = 0";
        if (!string.IsNullOrWhiteSpace(search)) sql += " and (ps.invoice_number like @search or p.part_name like @search)";
        sql += " order by ps.supply_date desc";
        using var reader = Db.ExecuteReader(sql, Param("@search", $"%{search}%"));
        var list = new List<PartSupply>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public PartSupply? GetById(int id)
    {
        const string sql = @"select ps.supply_id, ps.supplier_id, s.supplier_name, ps.part_id, p.part_name, ps.supply_date,
                                    ps.quantity, ps.unit_cost, ps.total_cost, ps.invoice_number, ps.received_by, ps.status,
                                    ps.isactive, ps.isdeleted
                             from part_supplies ps
                             inner join suppliers s on ps.supplier_id = s.supplier_id
                             inner join parts p on ps.part_id = p.part_id
                             where ps.supply_id = @id and ps.isdeleted = 0";
        using var reader = Db.ExecuteReader(sql, Param("@id", id));
        return reader.Read() ? Map(reader) : null;
    }

    public int Create(PartSupply supply)
    {
        const string sql = @"insert into part_supplies (supplier_id, part_id, supply_date, quantity, unit_cost, total_cost,
                             invoice_number, received_by, status, isactive)
                             values (@sid, @pid, @date, @qty, @cost, @total, @invoice, @received, @status, @active);
                             select scope_identity();";
        return Convert.ToInt32(Db.ExecuteScalar(sql,
            Param("@sid", supply.SupplierId), Param("@pid", supply.PartId), Param("@date", supply.SupplyDate),
            Param("@qty", supply.Quantity), Param("@cost", supply.UnitCost), Param("@total", supply.TotalCost),
            Param("@invoice", supply.InvoiceNumber), Param("@received", supply.ReceivedBy),
            Param("@status", supply.Status), Param("@active", supply.IsActive)));
    }

    public bool Update(PartSupply supply)
    {
        const string sql = @"update part_supplies set supplier_id = @sid, part_id = @pid, supply_date = @date,
                             quantity = @qty, unit_cost = @cost, total_cost = @total, invoice_number = @invoice,
                             received_by = @received, status = @status, isactive = @active, updatedat = sysutcdatetime()
                             where supply_id = @id and isdeleted = 0";
        return Db.ExecuteNonQuery(sql,
            Param("@sid", supply.SupplierId), Param("@pid", supply.PartId), Param("@date", supply.SupplyDate),
            Param("@qty", supply.Quantity), Param("@cost", supply.UnitCost), Param("@total", supply.TotalCost),
            Param("@invoice", supply.InvoiceNumber), Param("@received", supply.ReceivedBy),
            Param("@status", supply.Status), Param("@active", supply.IsActive), Param("@id", supply.SupplyId)) > 0;
    }

    public bool SoftDelete(int id) =>
        Db.ExecuteNonQuery("update part_supplies set isdeleted = 1, isactive = 0, updatedat = sysutcdatetime() where supply_id = @id", Param("@id", id)) > 0;

    private static PartSupply Map(SqlDataReader r) => new()
    {
        SupplyId = GetInt(r, "supply_id"),
        SupplierId = GetInt(r, "supplier_id"),
        SupplierName = GetNullableString(r, "supplier_name"),
        PartId = GetInt(r, "part_id"),
        PartName = GetNullableString(r, "part_name"),
        SupplyDate = GetDateTime(r, "supply_date"),
        Quantity = GetInt(r, "quantity"),
        UnitCost = GetDecimal(r, "unit_cost"),
        TotalCost = GetDecimal(r, "total_cost"),
        InvoiceNumber = GetNullableString(r, "invoice_number"),
        ReceivedBy = GetNullableInt(r, "received_by"),
        Status = GetString(r, "status"),
        IsActive = GetBool(r, "isactive"),
        IsDeleted = GetBool(r, "isdeleted")
    };
}

public class AuditLogRepository : RepositoryBase
{
    public AuditLogRepository(DatabaseHelper db) : base(db) { }

    public List<AuditLogEntry> GetAll(string? tableName = null, int? recordId = null)
    {
        var sql = @"select audit_id, table_name, record_id, action_type, old_values, new_values, changed_by, changed_at
                    from audit_log where 1 = 1";
        if (!string.IsNullOrWhiteSpace(tableName)) sql += " and table_name = @table";
        if (recordId.HasValue) sql += " and record_id = @recordId";
        sql += " order by changed_at desc";
        using var reader = Db.ExecuteReader(sql, Param("@table", tableName), Param("@recordId", recordId));
        var list = new List<AuditLogEntry>();
        while (reader.Read())
        {
            list.Add(new AuditLogEntry
            {
                AuditId = GetLong(reader, "audit_id"),
                TableName = GetString(reader, "table_name"),
                RecordId = GetInt(reader, "record_id"),
                ActionType = GetString(reader, "action_type"),
                OldValues = GetNullableString(reader, "old_values"),
                NewValues = GetNullableString(reader, "new_values"),
                ChangedBy = GetNullableString(reader, "changed_by"),
                ChangedAt = GetDateTime(reader, "changed_at")
            });
        }

        return list;
    }
}
