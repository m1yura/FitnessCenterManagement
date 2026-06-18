/*
    autoservice database - complete deployment script
    domain: auto repair shop management
    run this script in sql server management studio or sqlcmd
*/

use master;
go

if db_id('autoservice_db') is not null
begin
    alter database autoservice_db set single_user with rollback immediate;
    drop database autoservice_db;
end
go

create database autoservice_db;
go

use autoservice_db;
go

/* ============================================================
   table definitions (16 related entities)
   ============================================================ */

create table roles (
    role_id int identity(1,1) not null,
    role_name nvarchar(50) not null,
    role_description nvarchar(255) null,
    permission_level int not null default 1,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_roles primary key (role_id),
    constraint uq_roles_name unique (role_name)
);
go

create table users (
    user_id int identity(1,1) not null,
    username nvarchar(50) not null,
    password_hash nvarchar(256) not null,
    email nvarchar(100) not null,
    role_id int not null,
    last_login datetime2 null,
    login_attempts int not null default 0,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_users primary key (user_id),
    constraint uq_users_username unique (username),
    constraint fk_users_roles foreign key (role_id) references roles(role_id)
);
go

create table customers (
    customer_id int identity(1,1) not null,
    first_name nvarchar(50) not null,
    last_name nvarchar(50) not null,
    phone nvarchar(20) not null,
    email nvarchar(100) null,
    address nvarchar(255) null,
    city nvarchar(100) null,
    postal_code nvarchar(20) null,
    loyalty_points int not null default 0,
    preferred_contact nvarchar(20) null,
    notes nvarchar(500) null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_customers primary key (customer_id)
);
go

create table vehicles (
    vehicle_id int identity(1,1) not null,
    customer_id int not null,
    make nvarchar(50) not null,
    model nvarchar(50) not null,
    year int not null,
    vin nvarchar(17) not null,
    license_plate nvarchar(20) not null,
    color nvarchar(30) null,
    mileage int not null default 0,
    engine_type nvarchar(50) null,
    fuel_type nvarchar(30) null,
    last_service_date date null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_vehicles primary key (vehicle_id),
    constraint uq_vehicles_vin unique (vin),
    constraint fk_vehicles_customers foreign key (customer_id) references customers(customer_id)
);
go

create table employees (
    employee_id int identity(1,1) not null,
    first_name nvarchar(50) not null,
    last_name nvarchar(50) not null,
    phone nvarchar(20) not null,
    email nvarchar(100) not null,
    position nvarchar(50) not null,
    hire_date date not null,
    hourly_rate decimal(10,2) not null default 0,
    specialization nvarchar(100) null,
    certification_level nvarchar(50) null,
    emergency_contact nvarchar(100) null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_employees primary key (employee_id)
);
go

create table service_categories (
    category_id int identity(1,1) not null,
    category_name nvarchar(100) not null,
    description nvarchar(255) null,
    display_order int not null default 0,
    icon_name nvarchar(50) null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_service_categories primary key (category_id),
    constraint uq_service_categories_name unique (category_name)
);
go

create table services (
    service_id int identity(1,1) not null,
    category_id int not null,
    service_name nvarchar(100) not null,
    description nvarchar(255) null,
    base_price decimal(10,2) not null,
    estimated_duration_minutes int not null default 60,
    skill_level_required nvarchar(30) null,
    warranty_days int not null default 30,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_services primary key (service_id),
    constraint fk_services_categories foreign key (category_id) references service_categories(category_id)
);
go

create table suppliers (
    supplier_id int identity(1,1) not null,
    supplier_name nvarchar(100) not null,
    contact_person nvarchar(100) null,
    phone nvarchar(20) not null,
    email nvarchar(100) null,
    address nvarchar(255) null,
    payment_terms nvarchar(50) null,
    rating decimal(3,2) null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_suppliers primary key (supplier_id)
);
go

create table parts (
    part_id int identity(1,1) not null,
    supplier_id int not null,
    part_number nvarchar(50) not null,
    part_name nvarchar(100) not null,
    description nvarchar(255) null,
    unit_price decimal(10,2) not null,
    quantity_in_stock int not null default 0,
    reorder_level int not null default 5,
    warehouse_location nvarchar(50) null,
    weight_kg decimal(8,3) null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_parts primary key (part_id),
    constraint uq_parts_number unique (part_number),
    constraint fk_parts_suppliers foreign key (supplier_id) references suppliers(supplier_id)
);
go

create table appointments (
    appointment_id int identity(1,1) not null,
    customer_id int not null,
    vehicle_id int not null,
    employee_id int null,
    appointment_date datetime2 not null,
    duration_minutes int not null default 60,
    status nvarchar(30) not null default 'scheduled',
    reason nvarchar(255) null,
    notes nvarchar(500) null,
    reminder_sent bit not null default 0,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_appointments primary key (appointment_id),
    constraint fk_appointments_customers foreign key (customer_id) references customers(customer_id),
    constraint fk_appointments_vehicles foreign key (vehicle_id) references vehicles(vehicle_id),
    constraint fk_appointments_employees foreign key (employee_id) references employees(employee_id)
);
go

create table work_orders (
    work_order_id int identity(1,1) not null,
    customer_id int not null,
    vehicle_id int not null,
    employee_id int null,
    appointment_id int null,
    order_number nvarchar(20) not null,
    status nvarchar(30) not null default 'open',
    priority nvarchar(20) not null default 'normal',
    opened_at datetime2 not null default sysutcdatetime(),
    completed_at datetime2 null,
    total_amount decimal(12,2) not null default 0,
    discount_percent decimal(5,2) not null default 0,
    diagnosis_notes nvarchar(1000) null,
    customer_complaint nvarchar(500) null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_work_orders primary key (work_order_id),
    constraint uq_work_orders_number unique (order_number),
    constraint fk_work_orders_customers foreign key (customer_id) references customers(customer_id),
    constraint fk_work_orders_vehicles foreign key (vehicle_id) references vehicles(vehicle_id),
    constraint fk_work_orders_employees foreign key (employee_id) references employees(employee_id),
    constraint fk_work_orders_appointments foreign key (appointment_id) references appointments(appointment_id)
);
go

create table work_order_services (
    work_order_service_id int identity(1,1) not null,
    work_order_id int not null,
    service_id int not null,
    quantity int not null default 1,
    unit_price decimal(10,2) not null,
    discount_amount decimal(10,2) not null default 0,
    line_total decimal(12,2) not null,
    performed_by int null,
    notes nvarchar(255) null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_work_order_services primary key (work_order_service_id),
    constraint fk_wos_work_orders foreign key (work_order_id) references work_orders(work_order_id),
    constraint fk_wos_services foreign key (service_id) references services(service_id),
    constraint fk_wos_employees foreign key (performed_by) references employees(employee_id)
);
go

create table work_order_parts (
    work_order_part_id int identity(1,1) not null,
    work_order_id int not null,
    part_id int not null,
    quantity int not null default 1,
    unit_price decimal(10,2) not null,
    line_total decimal(12,2) not null,
    installed_by int null,
    warranty_until date null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_work_order_parts primary key (work_order_part_id),
    constraint fk_wop_work_orders foreign key (work_order_id) references work_orders(work_order_id),
    constraint fk_wop_parts foreign key (part_id) references parts(part_id),
    constraint fk_wop_employees foreign key (installed_by) references employees(employee_id)
);
go

create table payments (
    payment_id int identity(1,1) not null,
    work_order_id int not null,
    payment_date datetime2 not null default sysutcdatetime(),
    amount decimal(12,2) not null,
    payment_method nvarchar(30) not null,
    transaction_reference nvarchar(100) null,
    status nvarchar(30) not null default 'completed',
    processed_by int null,
    notes nvarchar(255) null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_payments primary key (payment_id),
    constraint fk_payments_work_orders foreign key (work_order_id) references work_orders(work_order_id),
    constraint fk_payments_employees foreign key (processed_by) references employees(employee_id)
);
go

create table part_supplies (
    supply_id int identity(1,1) not null,
    supplier_id int not null,
    part_id int not null,
    supply_date date not null,
    quantity int not null,
    unit_cost decimal(10,2) not null,
    total_cost decimal(12,2) not null,
    invoice_number nvarchar(50) null,
    received_by int null,
    status nvarchar(30) not null default 'received',
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_part_supplies primary key (supply_id),
    constraint fk_supplies_suppliers foreign key (supplier_id) references suppliers(supplier_id),
    constraint fk_supplies_parts foreign key (part_id) references parts(part_id),
    constraint fk_supplies_employees foreign key (received_by) references employees(employee_id)
);
go

create table audit_log (
    audit_id bigint identity(1,1) not null,
    table_name nvarchar(100) not null,
    record_id int not null,
    action_type nvarchar(20) not null,
    old_values nvarchar(max) null,
    new_values nvarchar(max) null,
    changed_by nvarchar(100) null,
    changed_at datetime2 not null default sysutcdatetime(),
    ip_address nvarchar(45) null,
    constraint pk_audit_log primary key (audit_id)
);
go

/* ============================================================
   indexes
   ============================================================ */

create index ix_users_role_id on users(role_id);
create index ix_vehicles_customer_id on vehicles(customer_id);
create index ix_services_category_id on services(category_id);
create index ix_parts_supplier_id on parts(supplier_id);
create index ix_appointments_date on appointments(appointment_date);
create index ix_work_orders_status on work_orders(status);
create index ix_work_orders_customer_id on work_orders(customer_id);
create index ix_payments_work_order_id on payments(work_order_id);
create index ix_audit_log_table on audit_log(table_name, record_id);
go

/* ============================================================
   views
   ============================================================ */

create view vw_active_work_orders as
select
    wo.work_order_id,
    wo.order_number,
    wo.status,
    wo.priority,
    wo.opened_at,
    wo.total_amount,
    c.first_name + ' ' + c.last_name as customer_name,
    c.phone as customer_phone,
    v.make + ' ' + v.model as vehicle_info,
    v.license_plate,
    e.first_name + ' ' + e.last_name as mechanic_name
from work_orders wo
inner join customers c on wo.customer_id = c.customer_id
inner join vehicles v on wo.vehicle_id = v.vehicle_id
left join employees e on wo.employee_id = e.employee_id
where wo.isdeleted = 0 and wo.isactive = 1;
go

create view vw_customer_vehicles as
select
    c.customer_id,
    c.first_name + ' ' + c.last_name as customer_name,
    c.phone,
    c.email,
    c.loyalty_points,
    v.vehicle_id,
    v.make,
    v.model,
    v.year,
    v.license_plate,
    v.mileage,
    v.last_service_date
from customers c
inner join vehicles v on c.customer_id = v.customer_id
where c.isdeleted = 0 and v.isdeleted = 0;
go

create view vw_employee_workload as
select
    e.employee_id,
    e.first_name + ' ' + e.last_name as employee_name,
    e.position,
    e.specialization,
    count(distinct wo.work_order_id) as open_work_orders,
    count(distinct a.appointment_id) as upcoming_appointments
from employees e
left join work_orders wo on e.employee_id = wo.employee_id
    and wo.status in ('open', 'in_progress') and wo.isdeleted = 0
left join appointments a on e.employee_id = a.employee_id
    and a.appointment_date >= sysutcdatetime() and a.status = 'scheduled' and a.isdeleted = 0
where e.isdeleted = 0 and e.isactive = 1
group by e.employee_id, e.first_name, e.last_name, e.position, e.specialization;
go

create view vw_inventory_status as
select
    p.part_id,
    p.part_number,
    p.part_name,
    p.quantity_in_stock,
    p.reorder_level,
    p.unit_price,
    s.supplier_name,
    case
        when p.quantity_in_stock = 0 then 'out_of_stock'
        when p.quantity_in_stock <= p.reorder_level then 'low_stock'
        else 'in_stock'
    end as stock_status
from parts p
inner join suppliers s on p.supplier_id = s.supplier_id
where p.isdeleted = 0 and p.isactive = 1;
go

create view vw_revenue_summary as
select
    cast(p.payment_date as date) as payment_day,
    p.payment_method,
    count(*) as payment_count,
    sum(p.amount) as total_revenue
from payments p
where p.isdeleted = 0 and p.status = 'completed'
group by cast(p.payment_date as date), p.payment_method;
go

/* ============================================================
   audit helper procedure
   ============================================================ */

create procedure sp_write_audit_log
    @table_name nvarchar(100),
    @record_id int,
    @action_type nvarchar(20),
    @old_values nvarchar(max) = null,
    @new_values nvarchar(max) = null,
    @changed_by nvarchar(100) = 'system'
as
begin
    set nocount on;
    insert into audit_log (table_name, record_id, action_type, old_values, new_values, changed_by)
    values (@table_name, @record_id, @action_type, @old_values, @new_values, @changed_by);
end;
go

/* ============================================================
   triggers - business rules, audit, logging
   ============================================================ */

create trigger trg_work_orders_audit
on work_orders
after insert, update, delete
as
begin
    set nocount on;

    if exists (select 1 from inserted) and exists (select 1 from deleted)
    begin
        insert into audit_log (table_name, record_id, action_type, old_values, new_values, changed_by)
        select 'work_orders', i.work_order_id, 'update',
            'status=' + d.status + ';total=' + cast(d.total_amount as nvarchar(20)),
            'status=' + i.status + ';total=' + cast(i.total_amount as nvarchar(20)),
            'trigger'
        from inserted i
        inner join deleted d on i.work_order_id = d.work_order_id
        where i.status <> d.status or i.total_amount <> d.total_amount;
    end

    if exists (select 1 from inserted) and not exists (select 1 from deleted)
    begin
        insert into audit_log (table_name, record_id, action_type, new_values, changed_by)
        select 'work_orders', work_order_id, 'insert',
            'order=' + order_number + ';status=' + status, 'trigger'
        from inserted;
    end

    if exists (select 1 from deleted) and not exists (select 1 from inserted)
    begin
        insert into audit_log (table_name, record_id, action_type, old_values, changed_by)
        select 'work_orders', work_order_id, 'delete',
            'order=' + order_number, 'trigger'
        from deleted;
    end
end;
go

create trigger trg_payments_audit
on payments
after insert, update
as
begin
    set nocount on;
    insert into audit_log (table_name, record_id, action_type, new_values, changed_by)
    select 'payments', payment_id, 'insert',
        'amount=' + cast(amount as nvarchar(20)) + ';method=' + payment_method, 'trigger'
    from inserted;
end;
go

create trigger trg_work_orders_status_check
on work_orders
instead of update
as
begin
    set nocount on;

    if exists (
        select 1 from inserted i
        inner join deleted d on i.work_order_id = d.work_order_id
        where i.status = 'completed' and d.status <> 'completed'
    )
    begin
        if exists (
            select 1 from inserted i
            left join (
                select work_order_id, sum(amount) as paid
                from payments
                where isdeleted = 0 and status = 'completed'
                group by work_order_id
            ) p on i.work_order_id = p.work_order_id
            where i.status = 'completed'
              and isnull(p.paid, 0) < i.total_amount
        )
        begin
            raiserror('cannot complete work order: payment is insufficient.', 16, 1);
            return;
        end
    end

    update wo
    set
        wo.customer_id = i.customer_id,
        wo.vehicle_id = i.vehicle_id,
        wo.employee_id = i.employee_id,
        wo.appointment_id = i.appointment_id,
        wo.order_number = i.order_number,
        wo.status = i.status,
        wo.priority = i.priority,
        wo.opened_at = i.opened_at,
        wo.completed_at = case when i.status = 'completed' then sysutcdatetime() else i.completed_at end,
        wo.total_amount = i.total_amount,
        wo.discount_percent = i.discount_percent,
        wo.diagnosis_notes = i.diagnosis_notes,
        wo.customer_complaint = i.customer_complaint,
        wo.isactive = i.isactive,
        wo.isdeleted = i.isdeleted,
        wo.updatedat = sysutcdatetime()
    from work_orders wo
    inner join inserted i on wo.work_order_id = i.work_order_id;
end;
go

create trigger trg_parts_stock_check
on work_order_parts
after insert, update
as
begin
    set nocount on;

    if exists (
        select 1
        from inserted i
        inner join parts p on i.part_id = p.part_id
        where p.quantity_in_stock < i.quantity and i.isdeleted = 0
    )
    begin
        raiserror('insufficient stock for one or more parts.', 16, 1);
        rollback transaction;
        return;
    end

    update p
    set p.quantity_in_stock = p.quantity_in_stock - i.quantity,
        p.updatedat = sysutcdatetime()
    from parts p
    inner join inserted i on p.part_id = i.part_id
    where i.isdeleted = 0;
end;
go

create trigger trg_customers_soft_delete
on customers
after update
as
begin
    set nocount on;

    if update(isdeleted)
    begin
        insert into audit_log (table_name, record_id, action_type, old_values, new_values, changed_by)
        select 'customers', i.customer_id, 'soft_delete',
            'isdeleted=' + cast(d.isdeleted as nvarchar(1)),
            'isdeleted=' + cast(i.isdeleted as nvarchar(1)),
            'trigger'
        from inserted i
        inner join deleted d on i.customer_id = d.customer_id
        where i.isdeleted <> d.isdeleted;
    end
end;
go

/* ============================================================
   sequences
   ============================================================ */

create sequence seq_order_number as int start with 1001 increment by 1;
go

/* ============================================================
   stored procedures with transactions
   ============================================================ */

create procedure sp_create_work_order
    @customer_id int,
    @vehicle_id int,
    @employee_id int = null,
    @priority nvarchar(20) = 'normal',
    @customer_complaint nvarchar(500) = null,
    @work_order_id int output
as
begin
    set nocount on;
    set xact_abort on;

    begin try
        begin transaction;

        if not exists (select 1 from customers where customer_id = @customer_id and isdeleted = 0)
            throw 50001, 'customer not found or inactive.', 1;

        if not exists (select 1 from vehicles where vehicle_id = @vehicle_id and customer_id = @customer_id and isdeleted = 0)
            throw 50002, 'vehicle not found for this customer.', 1;

        declare @order_number nvarchar(20) = 'wo-' + format(sysutcdatetime(), 'yyyyMMdd') + '-' + cast(next value for seq_order_number as nvarchar(10));

        insert into work_orders (customer_id, vehicle_id, employee_id, order_number, priority, customer_complaint)
        values (@customer_id, @vehicle_id, @employee_id, @order_number, @priority, @customer_complaint);

        set @work_order_id = scope_identity();

        commit transaction;
    end try
    begin catch
        if @@trancount > 0 rollback transaction;
        throw;
    end catch
end;
go

create procedure sp_add_service_to_work_order
    @work_order_id int,
    @service_id int,
    @quantity int = 1,
    @performed_by int = null
as
begin
    set nocount on;
    set xact_abort on;

    begin try
        begin transaction;

        declare @unit_price decimal(10,2);
        declare @line_total decimal(12,2);

        select @unit_price = base_price from services where service_id = @service_id and isdeleted = 0;
        if @unit_price is null throw 50003, 'service not found.', 1;

        set @line_total = @unit_price * @quantity;

        insert into work_order_services (work_order_id, service_id, quantity, unit_price, line_total, performed_by)
        values (@work_order_id, @service_id, @quantity, @unit_price, @line_total, @performed_by);

        update work_orders
        set total_amount = (
                select isnull(sum(line_total), 0) from work_order_services where work_order_id = @work_order_id and isdeleted = 0
            ) + (
                select isnull(sum(line_total), 0) from work_order_parts where work_order_id = @work_order_id and isdeleted = 0
            ),
            updatedat = sysutcdatetime()
        where work_order_id = @work_order_id;

        commit transaction;
    end try
    begin catch
        if @@trancount > 0 rollback transaction;
        throw;
    end catch
end;
go

create procedure sp_process_payment
    @work_order_id int,
    @amount decimal(12,2),
    @payment_method nvarchar(30),
    @processed_by int = null,
    @transaction_reference nvarchar(100) = null,
    @payment_id int output
as
begin
    set nocount on;
    set xact_abort on;

    begin try
        begin transaction;

        declare @total decimal(12,2);
        declare @paid decimal(12,2);

        select @total = total_amount from work_orders where work_order_id = @work_order_id and isdeleted = 0;
        if @total is null throw 50004, 'work order not found.', 1;

        select @paid = isnull(sum(amount), 0) from payments
        where work_order_id = @work_order_id and isdeleted = 0 and status = 'completed';

        if @paid + @amount > @total
            throw 50005, 'payment exceeds work order total.', 1;

        insert into payments (work_order_id, amount, payment_method, processed_by, transaction_reference)
        values (@work_order_id, @amount, @payment_method, @processed_by, @transaction_reference);

        set @payment_id = scope_identity();

        if @paid + @amount = @total
        begin
            update work_orders
            set status = 'ready_for_pickup', updatedat = sysutcdatetime()
            where work_order_id = @work_order_id;
        end

        commit transaction;
    end try
    begin catch
        if @@trancount > 0 rollback transaction;
        throw;
    end catch
end;
go

create procedure sp_register_appointment
    @customer_id int,
    @vehicle_id int,
    @appointment_date datetime2,
    @employee_id int = null,
    @reason nvarchar(255) = null,
    @duration_minutes int = 60,
    @appointment_id int output
as
begin
    set nocount on;
    set xact_abort on;

    begin try
        begin transaction;

        if @appointment_date < sysutcdatetime()
            throw 50006, 'appointment date must be in the future.', 1;

        if exists (
            select 1 from appointments
            where employee_id = @employee_id
              and appointment_date between dateadd(minute, -30, @appointment_date)
                                       and dateadd(minute, @duration_minutes + 30, @appointment_date)
              and status = 'scheduled' and isdeleted = 0
        )
            throw 50007, 'employee already has an appointment at this time.', 1;

        insert into appointments (customer_id, vehicle_id, employee_id, appointment_date, duration_minutes, reason)
        values (@customer_id, @vehicle_id, @employee_id, @appointment_date, @duration_minutes, @reason);

        set @appointment_id = scope_identity();

        commit transaction;
    end try
    begin catch
        if @@trancount > 0 rollback transaction;
        throw;
    end catch
end;
go

create procedure sp_receive_part_supply
    @supplier_id int,
    @part_id int,
    @quantity int,
    @unit_cost decimal(10,2),
    @invoice_number nvarchar(50) = null,
    @received_by int = null,
    @supply_id int output
as
begin
    set nocount on;
    set xact_abort on;

    begin try
        begin transaction;

        declare @total_cost decimal(12,2) = @quantity * @unit_cost;

        insert into part_supplies (supplier_id, part_id, supply_date, quantity, unit_cost, total_cost, invoice_number, received_by)
        values (@supplier_id, @part_id, cast(sysutcdatetime() as date), @quantity, @unit_cost, @total_cost, @invoice_number, @received_by);

        set @supply_id = scope_identity();

        update parts
        set quantity_in_stock = quantity_in_stock + @quantity,
            unit_price = (@unit_cost + unit_price) / 2,
            updatedat = sysutcdatetime()
        where part_id = @part_id;

        commit transaction;
    end try
    begin catch
        if @@trancount > 0 rollback transaction;
        throw;
    end catch
end;
go

/* ============================================================
   database roles and permissions (grant/revoke)
   ============================================================ */

create role autoservice_admin_role;
create role autoservice_manager_role;
create role autoservice_mechanic_role;
create role autoservice_guest_role;
go

-- admin: full access
grant select, insert, update, delete on schema::dbo to autoservice_admin_role;
grant execute on schema::dbo to autoservice_admin_role;
grant alter on schema::dbo to autoservice_admin_role;

-- manager: operational access without user/role management
grant select, insert, update on customers to autoservice_manager_role;
grant select, insert, update on vehicles to autoservice_manager_role;
grant select, insert, update on employees to autoservice_manager_role;
grant select on roles to autoservice_manager_role;
grant select on users to autoservice_manager_role;
grant select on service_categories to autoservice_manager_role;
grant select on services to autoservice_manager_role;
grant select, insert, update on suppliers to autoservice_manager_role;
grant select, insert, update on parts to autoservice_manager_role;
grant select, insert, update on appointments to autoservice_manager_role;
grant select, insert, update on work_orders to autoservice_manager_role;
grant select, insert, update on work_order_services to autoservice_manager_role;
grant select, insert, update on work_order_parts to autoservice_manager_role;
grant select, insert, update on payments to autoservice_manager_role;
grant select, insert on part_supplies to autoservice_manager_role;
grant select on audit_log to autoservice_manager_role;
grant select on vw_active_work_orders to autoservice_manager_role;
grant select on vw_customer_vehicles to autoservice_manager_role;
grant select on vw_employee_workload to autoservice_manager_role;
grant select on vw_inventory_status to autoservice_manager_role;
grant select on vw_revenue_summary to autoservice_manager_role;
grant execute on sp_create_work_order to autoservice_manager_role;
grant execute on sp_add_service_to_work_order to autoservice_manager_role;
grant execute on sp_process_payment to autoservice_manager_role;
grant execute on sp_register_appointment to autoservice_manager_role;
grant execute on sp_receive_part_supply to autoservice_manager_role;
revoke delete on users from autoservice_manager_role;
revoke delete on roles from autoservice_manager_role;

-- mechanic: work orders and appointments
grant select on customers to autoservice_mechanic_role;
grant select on vehicles to autoservice_mechanic_role;
grant select on employees to autoservice_mechanic_role;
grant select on services to autoservice_mechanic_role;
grant select on parts to autoservice_mechanic_role;
grant select, update on work_orders to autoservice_mechanic_role;
grant select, insert, update on work_order_services to autoservice_mechanic_role;
grant select, insert on work_order_parts to autoservice_mechanic_role;
grant select, insert, update on appointments to autoservice_mechanic_role;
grant select on vw_active_work_orders to autoservice_mechanic_role;
grant select on vw_customer_vehicles to autoservice_mechanic_role;
grant select on vw_inventory_status to autoservice_mechanic_role;
grant execute on sp_add_service_to_work_order to autoservice_mechanic_role;
revoke delete on work_orders from autoservice_mechanic_role;
revoke insert, update, delete on payments from autoservice_mechanic_role;

-- guest: read-only views
grant select on vw_active_work_orders to autoservice_guest_role;
grant select on vw_customer_vehicles to autoservice_guest_role;
grant select on vw_inventory_status to autoservice_guest_role;
grant select on services to autoservice_guest_role;
grant select on service_categories to autoservice_guest_role;
revoke insert, update, delete on schema::dbo from autoservice_guest_role;
go

/* ============================================================
   test data
   ============================================================ */

insert into roles (role_name, role_description, permission_level) values
('administrator', 'full system access', 100),
('manager', 'shop manager with operational access', 75),
('mechanic', 'technician with work order access', 50),
('guest', 'read-only access', 10);
go

insert into users (username, password_hash, email, role_id) values
('admin', 'hash_admin_123', 'admin@autoservice.local', 1),
('manager1', 'hash_manager_123', 'manager@autoservice.local', 2),
('mechanic1', 'hash_mech_123', 'mech1@autoservice.local', 3),
('mechanic2', 'hash_mech_456', 'mech2@autoservice.local', 3),
('guest1', 'hash_guest_123', 'guest@autoservice.local', 4);
go

insert into customers (first_name, last_name, phone, email, address, city, postal_code, loyalty_points, preferred_contact, notes) values
('ivan', 'petrov', '+7-900-111-0001', 'ivan.petrov@mail.ru', 'ul. lenina 10', 'moscow', '101000', 120, 'phone', 'regular customer'),
('elena', 'smirnova', '+7-900-111-0002', 'elena.s@mail.ru', 'pr. mira 25', 'moscow', '129110', 80, 'email', null),
('dmitry', 'volkov', '+7-900-111-0003', 'd.volkov@mail.ru', 'ul. pushkina 5', 'spb', '190000', 200, 'phone', 'vip client'),
('anna', 'kuznetsova', '+7-900-111-0004', 'anna.k@mail.ru', 'ul. gagarina 12', 'kazan', '420000', 45, 'sms', null),
('sergey', 'orlov', '+7-900-111-0005', 's.orlov@mail.ru', 'ul. sovetskaya 8', 'novosibirsk', '630000', 30, 'phone', null),
('maria', 'fedorova', '+7-900-111-0006', 'm.fedorova@mail.ru', 'ul. kirova 3', 'ekaterinburg', '620000', 150, 'email', 'fleet owner'),
('alexey', 'morozov', '+7-900-111-0007', 'a.morozov@mail.ru', 'ul. centralnaya 44', 'samara', '443000', 60, 'phone', null),
('olga', 'nikolaeva', '+7-900-111-0008', 'o.nikolaeva@mail.ru', 'ul. sadovaya 19', 'rostov', '344000', 90, 'email', null),
('pavel', 'sokolov', '+7-900-111-0009', 'p.sokolov@mail.ru', 'ul. zelenaya 7', 'voronezh', '394000', 25, 'phone', null),
('tatyana', 'lebedeva', '+7-900-111-0010', 't.lebedeva@mail.ru', 'ul. nadezhdy 2', 'krasnodar', '350000', 110, 'sms', null),
('nikolay', 'egorov', '+7-900-111-0011', 'n.egorov@mail.ru', 'ul. shkolnaya 15', 'ufa', '450000', 70, 'phone', null),
('svetlana', 'popova', '+7-900-111-0012', 's.popova@mail.ru', 'ul. lesnaya 33', 'perm', '614000', 55, 'email', null),
('andrey', 'vasiliev', '+7-900-111-0013', 'a.vasiliev@mail.ru', 'ul. rechnaya 6', 'volgograd', '400000', 40, 'phone', null),
('irina', 'pavlova', '+7-900-111-0014', 'i.pavlova@mail.ru', 'ul. solnechnaya 21', 'omsk', '644000', 95, 'email', null),
('mikhail', 'semenov', '+7-900-111-0015', 'm.semenov@mail.ru', 'ul. polevaya 9', 'chelyabinsk', '454000', 35, 'phone', null),
('yulia', 'golubeva', '+7-900-111-0016', 'y.golubeva@mail.ru', 'ul. tsvetochnaya 4', 'tula', '300000', 65, 'sms', null),
('viktor', 'komarov', '+7-900-111-0017', 'v.komarov@mail.ru', 'ul. zheleznaya 11', 'barnaul', '656000', 50, 'phone', null),
('nadezhda', 'belova', '+7-900-111-0018', 'n.belova@mail.ru', 'ul. yuzhnaya 28', 'irkutsk', '664000', 85, 'email', null),
('roman', 'tarasov', '+7-900-111-0019', 'r.tarasov@mail.ru', 'ul. severnaya 17', 'vladivostok', '690000', 20, 'phone', null),
('ekaterina', 'ryabova', '+7-900-111-0020', 'e.ryabova@mail.ru', 'ul. ozernaya 13', 'yaroslavl', '150000', 75, 'email', null),
('konstantin', 'zaitsev', '+7-900-111-0021', 'k.zaitsev@mail.ru', 'ul. gorodskaya 30', 'tver', '170000', 42, 'phone', null),
('larisa', 'medvedeva', '+7-900-111-0022', 'l.medvedeva@mail.ru', 'ul. parkovaya 5', 'lipetsk', '398000', 58, 'sms', null),
('oleg', 'frolov', '+7-900-111-0023', 'o.frolov@mail.ru', 'ul. fabrichnaya 22', 'penza', '440000', 33, 'phone', null),
('galina', 'antonova', '+7-900-111-0024', 'g.antonova@mail.ru', 'ul. kolhoznaya 14', 'kursk', '305000', 48, 'email', null),
('denis', 'markov', '+7-900-111-0025', 'd.markov@mail.ru', 'ul. promyshlennaya 9', 'tolyatti', '445000', 62, 'phone', null);
go

insert into vehicles (customer_id, make, model, year, vin, license_plate, color, mileage, engine_type, fuel_type, last_service_date) values
(1, 'toyota', 'camry', 2019, '1HGBH41JXMN109186', 'a123bc77', 'silver', 65000, '2.5l', 'gasoline', '2025-11-15'),
(1, 'honda', 'civic', 2017, '2HGFC2F59HH123456', 'b456de77', 'black', 98000, '1.8l', 'gasoline', '2025-08-20'),
(2, 'bmw', 'x5', 2021, '5UXCR6C05M9D12345', 'c789fg99', 'white', 32000, '3.0l', 'diesel', '2026-01-10'),
(3, 'mercedes', 'e-class', 2020, 'WDDZF4JB0LA123456', 'd012hi77', 'blue', 45000, '2.0l', 'gasoline', '2025-12-05'),
(4, 'kia', 'rio', 2018, 'KNADM4A36J6123456', 'e345jk77', 'red', 72000, '1.6l', 'gasoline', '2025-09-30'),
(5, 'hyundai', 'solaris', 2019, 'KMHD35LE5JU123456', 'f678lm77', 'gray', 55000, '1.6l', 'gasoline', null),
(6, 'ford', 'focus', 2016, '1FADP3F20GL123456', 'g901no77', 'green', 110000, '1.6l', 'gasoline', '2025-07-12'),
(6, 'ford', 'transit', 2020, '1FTBR1XM5LKA12345', 'h234pq77', 'white', 85000, '2.0l', 'diesel', '2026-02-01'),
(7, 'volkswagen', 'passat', 2018, '1VWBP7A35JC123456', 'i567rs77', 'black', 78000, '2.0l', 'diesel', '2025-10-18'),
(8, 'skoda', 'octavia', 2021, 'TMBJJ7NE4M0123456', 'j890tu77', 'silver', 28000, '1.4l', 'gasoline', '2026-03-01'),
(9, 'nissan', 'qashqai', 2019, 'SJNFBAJ11U2123456', 'k123vw77', 'orange', 60000, '2.0l', 'gasoline', null),
(10, 'mazda', 'cx-5', 2020, 'JM3KFBDM5L0123456', 'l456xy77', 'red', 41000, '2.5l', 'gasoline', '2025-11-28'),
(11, 'renault', 'duster', 2017, 'VF1HJD40267812345', 'm789za77', 'brown', 95000, '1.6l', 'gasoline', '2025-06-15'),
(12, 'chevrolet', 'cruze', 2018, '1G1BE5SM7J7123456', 'n012bc99', 'blue', 82000, '1.4l', 'gasoline', null),
(13, 'lada', 'vesta', 2022, 'XTA219040N0123456', 'o345de99', 'white', 15000, '1.6l', 'gasoline', '2026-01-20'),
(14, 'toyota', 'rav4', 2021, '2T3P1RFV8MC123456', 'p678fg99', 'gray', 35000, '2.5l', 'hybrid', '2026-02-15'),
(15, 'audi', 'a4', 2019, 'WAUZZZF49KA123456', 'q901hi99', 'black', 52000, '2.0l', 'gasoline', '2025-12-22'),
(16, 'subaru', 'forester', 2020, 'JF2SKADC5LH123456', 'r234jk99', 'green', 48000, '2.5l', 'gasoline', null),
(17, 'lexus', 'rx350', 2021, '2T2BZMCA5MC123456', 's567lm99', 'pearl', 22000, '3.5l', 'gasoline', '2026-03-10'),
(18, 'mitsubishi', 'outlander', 2018, 'JA4AD3A36JZ123456', 't890no99', 'silver', 70000, '2.4l', 'gasoline', '2025-08-08'),
(19, 'peugeot', '3008', 2019, 'VF3MRHPY0KS123456', 'u123pq99', 'blue', 58000, '1.6l', 'diesel', null),
(20, 'volvo', 'xc60', 2020, 'YV4A22PK1L1123456', 'v456rs99', 'white', 39000, '2.0l', 'hybrid', '2026-01-05'),
(21, 'toyota', 'corolla', 2018, '5YFBURHE5JP123456', 'w789tu99', 'black', 88000, '1.6l', 'gasoline', '2025-09-12'),
(22, 'honda', 'accord', 2019, '1HGCV1F16KA123456', 'x012vw99', 'silver', 61000, '2.0l', 'gasoline', null),
(23, 'kia', 'sportage', 2021, 'KNDPM3AC5M7123456', 'y345xy99', 'red', 30000, '2.0l', 'gasoline', '2026-02-28'),
(24, 'bmw', '320i', 2018, 'WBA8E9G50JNU12345', 'z678za99', 'gray', 74000, '2.0l', 'gasoline', '2025-10-30'),
(25, 'mercedes', 'c-class', 2020, 'WDDWF4JB0FR123456', 'a901bc177', 'black', 43000, '1.5l', 'gasoline', '2026-03-05'),
(3, 'porsche', 'cayenne', 2022, 'WP1AA2A59NDA12345', 'b234de177', 'white', 12000, '3.0l', 'gasoline', '2026-01-15'),
(6, 'ford', 'mustang', 2019, '1FA6P8TH5K5123456', 'c567fg177', 'yellow', 35000, '5.0l', 'gasoline', '2025-11-01'),
(10, 'mazda', '3', 2018, '3MZBN1V37JM123456', 'd890hi177', 'blue', 67000, '2.0l', 'gasoline', null),
(15, 'audi', 'q5', 2021, 'WA1CNAFY5M2123456', 'e123jk177', 'gray', 25000, '2.0l', 'gasoline', '2026-02-20');
go

insert into employees (first_name, last_name, phone, email, position, hire_date, hourly_rate, specialization, certification_level, emergency_contact) values
('alexander', 'ivanov', '+7-901-200-0001', 'a.ivanov@autoservice.local', 'master mechanic', '2018-03-15', 850.00, 'engine repair', 'level 3', 'wife: +7-901-200-0101'),
('boris', 'sidorov', '+7-901-200-0002', 'b.sidorov@autoservice.local', 'mechanic', '2019-06-01', 650.00, 'brake systems', 'level 2', 'brother: +7-901-200-0102'),
('victor', 'kozlov', '+7-901-200-0003', 'v.kozlov@autoservice.local', 'mechanic', '2020-01-10', 650.00, 'electrical', 'level 2', 'mother: +7-901-200-0103'),
('grigory', 'novikov', '+7-901-200-0004', 'g.novikov@autoservice.local', 'diagnostician', '2017-09-20', 900.00, 'computer diagnostics', 'level 3', 'wife: +7-901-200-0104'),
('maxim', 'baranov', '+7-901-200-0005', 'm.baranov@autoservice.local', 'manager', '2016-05-01', 1200.00, 'operations', 'level 3', 'spouse: +7-901-200-0105'),
('timur', 'safin', '+7-901-200-0006', 't.safin@autoservice.local', 'mechanic', '2021-04-12', 600.00, 'suspension', 'level 1', 'father: +7-901-200-0106'),
('artem', 'gromov', '+7-901-200-0007', 'a.gromov@autoservice.local', 'mechanic', '2022-08-01', 600.00, 'transmission', 'level 2', 'wife: +7-901-200-0107'),
('ilya', 'chernov', '+7-901-200-0008', 'i.chernov@autoservice.local', 'receptionist', '2023-02-15', 450.00, 'customer service', 'level 1', 'sister: +7-901-200-0108');
go

insert into service_categories (category_name, description, display_order, icon_name) values
('maintenance', 'regular maintenance services', 1, 'wrench'),
('diagnostics', 'vehicle diagnostic services', 2, 'scanner'),
('engine', 'engine repair and overhaul', 3, 'engine'),
('brakes', 'brake system services', 4, 'brake'),
('electrical', 'electrical system repair', 5, 'bolt'),
('bodywork', 'body repair and painting', 6, 'paint');
go

insert into services (category_id, service_name, description, base_price, estimated_duration_minutes, skill_level_required, warranty_days) values
(1, 'oil change', 'engine oil and filter replacement', 3500.00, 45, 'basic', 30),
(1, 'air filter replacement', 'replace engine air filter', 1200.00, 20, 'basic', 30),
(1, 'cabin filter replacement', 'replace cabin air filter', 1500.00, 25, 'basic', 30),
(1, 'tire rotation', 'rotate all four tires', 2000.00, 40, 'basic', 14),
(1, 'wheel alignment', 'four wheel alignment', 4500.00, 60, 'intermediate', 30),
(2, 'full diagnostic scan', 'complete obd-ii diagnostic', 3000.00, 60, 'intermediate', 7),
(2, 'engine diagnostics', 'detailed engine analysis', 5000.00, 90, 'advanced', 7),
(3, 'timing belt replacement', 'replace timing belt and tensioner', 18000.00, 300, 'advanced', 90),
(3, 'spark plug replacement', 'replace all spark plugs', 4500.00, 60, 'intermediate', 60),
(4, 'brake pad replacement front', 'replace front brake pads', 6000.00, 90, 'intermediate', 60),
(4, 'brake pad replacement rear', 'replace rear brake pads', 5500.00, 90, 'intermediate', 60),
(4, 'brake disc replacement', 'replace brake discs and pads', 12000.00, 120, 'advanced', 90),
(5, 'battery replacement', 'replace car battery', 2500.00, 30, 'basic', 365),
(5, 'alternator repair', 'repair or replace alternator', 9000.00, 180, 'advanced', 90),
(5, 'starter motor repair', 'repair or replace starter', 8000.00, 150, 'advanced', 90),
(6, 'dent removal', 'paintless dent removal', 5000.00, 120, 'intermediate', 30),
(6, 'bumper painting', 'repaint front or rear bumper', 15000.00, 480, 'advanced', 180),
(1, 'coolant flush', 'flush and replace coolant', 4000.00, 60, 'intermediate', 60),
(1, 'transmission fluid change', 'replace transmission fluid', 5500.00, 75, 'intermediate', 60),
(2, 'pre-purchase inspection', 'comprehensive vehicle inspection', 7000.00, 120, 'advanced', 0);
go

insert into suppliers (supplier_name, contact_person, phone, email, address, payment_terms, rating) values
('autoparts pro', 'sergey belov', '+7-495-100-0001', 'sales@autopartspro.ru', 'moscow, industrial zone 5', 'net 30', 4.80),
('euro parts supply', 'marina klimova', '+7-812-200-0002', 'info@europarts.ru', 'spb, port area 12', 'net 15', 4.50),
('fast brake co', 'igor demidov', '+7-495-100-0003', 'orders@fastbrake.ru', 'moscow, south district', 'prepaid', 4.70),
('electro auto', 'nina sorokina', '+7-343-300-0004', 'shop@electroauto.ru', 'ekaterinburg, tech park', 'net 30', 4.30),
('filter world', 'petr zhukov', '+7-495-100-0005', 'sales@filterworld.ru', 'moscow, north warehouse', 'net 45', 4.60),
('oil masters', 'diana rogozina', '+7-831-400-0006', 'supply@oilmasters.ru', 'nizhny novgorod', 'net 30', 4.90);
go

insert into parts (supplier_id, part_number, part_name, description, unit_price, quantity_in_stock, reorder_level, warehouse_location, weight_kg) values
(1, 'ap-oil-5w30-4l', 'engine oil 5w30 4l', 'synthetic engine oil', 2800.00, 50, 10, 'a-01-01', 3.600),
(1, 'ap-of-std', 'oil filter standard', 'universal oil filter', 450.00, 80, 15, 'a-01-02', 0.300),
(5, 'fw-af-001', 'air filter universal', 'engine air filter', 650.00, 60, 12, 'a-02-01', 0.400),
(5, 'fw-cf-002', 'cabin filter', 'cabin air filter', 800.00, 45, 10, 'a-02-02', 0.250),
(3, 'fb-pad-f-001', 'brake pads front', 'ceramic front brake pads', 3200.00, 30, 8, 'b-01-01', 1.200),
(3, 'fb-pad-r-001', 'brake pads rear', 'ceramic rear brake pads', 2800.00, 28, 8, 'b-01-02', 1.000),
(3, 'fb-disc-f-001', 'brake disc front', 'ventilated front disc', 4500.00, 16, 4, 'b-02-01', 5.500),
(4, 'ea-bat-60ah', 'battery 60ah', 'maintenance free battery', 6500.00, 20, 5, 'c-01-01', 15.000),
(4, 'ea-alt-001', 'alternator reman', 'remanufactured alternator', 12000.00, 8, 2, 'c-02-01', 6.000),
(1, 'ap-sp-001', 'spark plugs set', 'iridium spark plug set x4', 2400.00, 35, 8, 'a-03-01', 0.500),
(1, 'ap-tb-kit', 'timing belt kit', 'timing belt with tensioner', 8500.00, 12, 3, 'a-04-01', 2.000),
(6, 'om-cool-5l', 'coolant 5l', 'antifreeze coolant concentrate', 1800.00, 40, 10, 'd-01-01', 5.500),
(6, 'om-atf-1l', 'transmission fluid 1l', 'automatic transmission fluid', 950.00, 55, 12, 'd-01-02', 0.900),
(2, 'ep-wp-001', 'water pump', 'engine water pump', 5500.00, 10, 3, 'e-01-01', 1.800),
(2, 'ep-rad-001', 'radiator', 'aluminum radiator', 9800.00, 6, 2, 'e-02-01', 4.200),
(1, 'ap-belt-001', 'serpentine belt', 'drive belt', 1200.00, 25, 6, 'a-05-01', 0.350),
(4, 'ea-fuse-box', 'fuse box assembly', 'main fuse box', 3500.00, 8, 2, 'c-03-01', 0.800),
(3, 'fb-caliper-f', 'brake caliper front', 'front brake caliper', 7200.00, 6, 2, 'b-03-01', 3.500),
(5, 'fw-fuel-f', 'fuel filter', 'inline fuel filter', 900.00, 40, 10, 'a-06-01', 0.200),
(2, 'ep-shock-f', 'shock absorber front', 'front shock absorber', 4800.00, 14, 4, 'e-03-01', 3.000);
go

insert into appointments (customer_id, vehicle_id, employee_id, appointment_date, duration_minutes, status, reason, notes, reminder_sent) values
(1, 1, 1, '2026-06-20 09:00:00', 60, 'scheduled', 'oil change', 'regular maintenance', 1),
(2, 3, 4, '2026-06-20 11:00:00', 90, 'scheduled', 'diagnostics', 'check engine light', 1),
(3, 4, 1, '2026-06-21 10:00:00', 120, 'scheduled', 'brake inspection', null, 0),
(4, 5, 2, '2026-06-21 14:00:00', 60, 'scheduled', 'tire rotation', null, 0),
(5, 6, 3, '2026-06-22 09:30:00', 60, 'scheduled', 'battery check', null, 0),
(6, 7, 1, '2026-06-22 13:00:00', 90, 'scheduled', 'full service', 'fleet vehicle', 0),
(7, 9, 6, '2026-06-23 10:00:00', 60, 'scheduled', 'alignment', null, 0),
(8, 10, 2, '2026-06-23 15:00:00', 45, 'scheduled', 'oil change', null, 0),
(9, 11, 3, '2026-06-24 09:00:00', 60, 'scheduled', 'diagnostics', null, 0),
(10, 12, 7, '2026-06-24 11:30:00', 90, 'scheduled', 'suspension check', 'noise from front', 0),
(11, 13, 1, '2026-06-25 10:00:00', 60, 'scheduled', 'oil change', null, 0),
(12, 14, 2, '2026-06-25 14:00:00', 120, 'scheduled', 'brake service', null, 0),
(13, 15, 4, '2026-06-26 09:00:00', 90, 'scheduled', 'pre-purchase check', null, 0),
(14, 16, 6, '2026-06-26 13:00:00', 60, 'scheduled', 'maintenance', null, 0),
(15, 17, 3, '2026-06-27 10:00:00', 60, 'scheduled', 'electrical check', null, 0),
(1, 2, 1, '2026-06-28 09:00:00', 45, 'scheduled', 'filter replacement', null, 0),
(3, 26, 4, '2026-06-28 14:00:00', 120, 'scheduled', 'full diagnostic', 'vip client', 0),
(6, 8, 7, '2026-06-29 10:00:00', 180, 'scheduled', 'transmission service', 'fleet van', 0),
(10, 29, 2, '2026-06-29 15:00:00', 60, 'scheduled', 'brake check', null, 0),
(20, 22, 1, '2026-06-30 09:00:00', 90, 'scheduled', 'coolant flush', null, 0),
(2, 3, 4, '2026-07-01 11:00:00', 60, 'scheduled', 'follow-up diagnostic', null, 0),
(15, 30, 3, '2026-07-02 10:00:00', 120, 'scheduled', 'electrical repair', null, 0),
(22, 23, 6, '2026-07-03 09:30:00', 60, 'scheduled', 'oil change', null, 0),
(18, 20, 2, '2026-07-04 14:00:00', 90, 'scheduled', 'wheel alignment', null, 0),
(25, 25, 1, '2026-07-05 10:00:00', 60, 'scheduled', 'maintenance', null, 0);
go

insert into work_orders (customer_id, vehicle_id, employee_id, appointment_id, order_number, status, priority, opened_at, total_amount, discount_percent, diagnosis_notes, customer_complaint) values
(1, 1, 1, 1, 'wo-20260601-1001', 'completed', 'normal', '2026-06-01 09:00:00', 6950.00, 0, 'oil and filters replaced', 'scheduled maintenance'),
(2, 3, 4, null, 'wo-20260602-1002', 'in_progress', 'high', '2026-06-02 10:00:00', 8000.00, 5, 'diagnostic in progress', 'engine warning light'),
(3, 4, 1, null, 'wo-20260603-1003', 'open', 'normal', '2026-06-03 11:00:00', 12000.00, 0, null, 'grinding noise from brakes'),
(4, 5, 2, null, 'wo-20260604-1004', 'completed', 'normal', '2026-06-04 09:00:00', 2000.00, 0, 'tires rotated', 'regular service'),
(5, 6, 3, null, 'wo-20260605-1005', 'ready_for_pickup', 'normal', '2026-06-05 14:00:00', 9000.00, 0, 'alternator replaced', 'battery not charging'),
(6, 7, 1, null, 'wo-20260606-1006', 'in_progress', 'high', '2026-06-06 08:00:00', 15000.00, 10, 'timing belt job started', 'scheduled fleet maintenance'),
(7, 9, 6, null, 'wo-20260607-1007', 'open', 'normal', '2026-06-07 10:30:00', 4500.00, 0, null, 'car pulls to the left'),
(8, 10, 2, null, 'wo-20260608-1008', 'completed', 'normal', '2026-06-08 11:00:00', 5300.00, 0, 'oil change and air filter', 'maintenance'),
(9, 11, 3, null, 'wo-20260609-1009', 'open', 'low', '2026-06-09 09:00:00', 3000.00, 0, null, 'strange noise when starting'),
(10, 12, 7, null, 'wo-20260610-1010', 'in_progress', 'normal', '2026-06-10 13:00:00', 6000.00, 0, 'front brake pads worn', 'squeaking brakes'),
(11, 13, 1, null, 'wo-20260611-1011', 'completed', 'normal', '2026-06-11 10:00:00', 3500.00, 0, 'oil changed', 'regular visit'),
(12, 14, 2, null, 'wo-20260612-1012', 'open', 'high', '2026-06-12 15:00:00', 17500.00, 5, null, 'brake failure warning'),
(13, 15, 4, null, 'wo-20260613-1013', 'completed', 'normal', '2026-06-13 09:30:00', 7000.00, 0, 'pre-purchase report generated', 'buying used car'),
(14, 16, 6, null, 'wo-20260614-1014', 'ready_for_pickup', 'normal', '2026-06-14 11:00:00', 8500.00, 0, 'coolant flush done', 'overheating issue resolved'),
(15, 17, 3, null, 'wo-20260615-1015', 'in_progress', 'high', '2026-06-15 08:30:00', 14500.00, 0, 'starter motor faulty', 'car wont start intermittently'),
(16, 18, 2, null, 'wo-20260616-1016', 'open', 'normal', '2026-06-16 10:00:00', 4000.00, 0, null, 'cabin smells musty'),
(17, 19, 1, null, 'wo-20260617-1017', 'completed', 'normal', '2026-06-17 14:00:00', 5500.00, 0, 'transmission fluid changed', 'maintenance'),
(18, 20, 6, null, 'wo-20260618-1018', 'open', 'normal', '2026-06-18 09:00:00', 4500.00, 0, null, 'vibration at highway speed'),
(19, 21, 3, null, 'wo-20260619-1019', 'in_progress', 'low', '2026-06-19 11:00:00', 2500.00, 0, 'battery test failed', 'slow cranking'),
(20, 22, 4, null, 'wo-20260620-1020', 'open', 'normal', '2026-06-20 08:00:00', 5000.00, 0, null, 'full diagnostic requested'),
(3, 26, 4, null, 'wo-20260621-1021', 'in_progress', 'high', '2026-06-21 09:00:00', 22000.00, 15, 'complex electrical issue', 'multiple warning lights'),
(6, 8, 7, null, 'wo-20260622-1022', 'open', 'high', '2026-06-22 07:30:00', 18000.00, 10, null, 'transmission slipping'),
(1, 2, 1, null, 'wo-20260623-1023', 'completed', 'normal', '2026-06-23 10:00:00', 2700.00, 0, 'filters replaced', 'maintenance'),
(10, 29, 2, null, 'wo-20260624-1024', 'open', 'normal', '2026-06-24 12:00:00', 6000.00, 0, null, 'rear brake noise'),
(25, 25, 1, null, 'wo-20260625-1025', 'ready_for_pickup', 'normal', '2026-06-25 09:00:00', 10450.00, 5, 'spark plugs and oil done', 'scheduled service');
go

update work_orders set completed_at = '2026-06-01 11:00:00' where work_order_id = 1;
update work_orders set completed_at = '2026-06-04 10:30:00' where work_order_id = 4;
update work_orders set completed_at = '2026-06-08 12:30:00' where work_order_id = 8;
update work_orders set completed_at = '2026-06-11 11:30:00' where work_order_id = 11;
update work_orders set completed_at = '2026-06-13 12:00:00' where work_order_id = 13;
update work_orders set completed_at = '2026-06-17 15:30:00' where work_order_id = 17;
update work_orders set completed_at = '2026-06-23 11:30:00' where work_order_id = 23;
go

insert into work_order_services (work_order_id, service_id, quantity, unit_price, discount_amount, line_total, performed_by, notes) values
(1, 1, 1, 3500.00, 0, 3500.00, 1, null),
(1, 2, 1, 1200.00, 0, 1200.00, 1, null),
(1, 3, 1, 1500.00, 0, 1500.00, 1, null),
(1, 4, 1, 750.00, 0, 750.00, 1, 'discounted rotation'),
(2, 6, 1, 3000.00, 0, 3000.00, 4, null),
(2, 7, 1, 5000.00, 0, 5000.00, 4, null),
(3, 10, 1, 6000.00, 0, 6000.00, 1, null),
(3, 11, 1, 5500.00, 0, 5500.00, 1, null),
(3, 4, 1, 500.00, 0, 500.00, 2, null),
(4, 4, 1, 2000.00, 0, 2000.00, 2, null),
(5, 14, 1, 9000.00, 0, 9000.00, 3, null),
(6, 8, 1, 18000.00, 2000.00, 16000.00, 1, 'fleet discount applied'),
(7, 5, 1, 4500.00, 0, 4500.00, 6, null),
(8, 1, 1, 3500.00, 0, 3500.00, 2, null),
(8, 2, 1, 1200.00, 0, 1200.00, 2, null),
(8, 4, 1, 600.00, 0, 600.00, 2, null),
(9, 6, 1, 3000.00, 0, 3000.00, 3, null),
(10, 10, 1, 6000.00, 0, 6000.00, 7, null),
(11, 1, 1, 3500.00, 0, 3500.00, 1, null),
(12, 12, 1, 12000.00, 0, 12000.00, 2, null),
(12, 10, 1, 6000.00, 500.00, 5500.00, 2, null),
(13, 20, 1, 7000.00, 0, 7000.00, 4, null),
(14, 18, 1, 4000.00, 0, 4000.00, 6, null),
(14, 1, 1, 3500.00, 0, 3500.00, 6, null),
(14, 3, 1, 1000.00, 0, 1000.00, 6, null),
(15, 15, 1, 8000.00, 0, 8000.00, 3, null),
(15, 6, 1, 3000.00, 0, 3000.00, 3, null),
(15, 13, 1, 2500.00, 0, 2500.00, 3, null),
(16, 3, 1, 1500.00, 0, 1500.00, 2, null),
(16, 2, 1, 1200.00, 0, 1200.00, 2, null),
(16, 4, 1, 1300.00, 0, 1300.00, 2, null);
go

insert into work_order_parts (work_order_id, part_id, quantity, unit_price, line_total, installed_by, warranty_until) values
(1, 1, 1, 2800.00, 2800.00, 1, '2026-07-01'),
(1, 2, 1, 450.00, 450.00, 1, '2026-07-01'),
(1, 3, 1, 650.00, 650.00, 1, '2026-07-01'),
(5, 8, 1, 6500.00, 6500.00, 3, '2027-06-05'),
(5, 9, 1, 2500.00, 2500.00, 3, '2027-06-05'),
(6, 11, 1, 8500.00, 8500.00, 1, '2026-09-06'),
(8, 1, 1, 2800.00, 2800.00, 2, '2026-07-08'),
(8, 3, 1, 650.00, 650.00, 2, '2026-07-08'),
(10, 5, 1, 3200.00, 3200.00, 7, '2026-08-10'),
(10, 6, 1, 2800.00, 2800.00, 7, '2026-08-10'),
(12, 7, 2, 4500.00, 9000.00, 2, '2026-09-12'),
(12, 5, 1, 3200.00, 3200.00, 2, '2026-09-12'),
(14, 12, 1, 1800.00, 1800.00, 6, '2026-08-14'),
(15, 10, 1, 2400.00, 2400.00, 3, '2026-08-15'),
(17, 13, 4, 950.00, 3800.00, 1, '2026-08-17'),
(19, 8, 1, 6500.00, 6500.00, 3, '2027-06-19'),
(21, 17, 1, 3500.00, 3500.00, 4, '2026-07-21'),
(21, 10, 1, 2400.00, 2400.00, 4, '2026-08-21'),
(23, 4, 1, 800.00, 800.00, 1, '2026-07-23'),
(23, 3, 1, 650.00, 650.00, 1, '2026-07-23'),
(25, 1, 1, 2800.00, 2800.00, 1, '2026-07-25'),
(25, 10, 1, 2400.00, 2400.00, 1, '2026-08-25'),
(25, 2, 1, 450.00, 450.00, 1, '2026-07-25');
go

insert into payments (work_order_id, payment_date, amount, payment_method, transaction_reference, status, processed_by, notes) values
(1, '2026-06-01 11:30:00', 6950.00, 'card', 'txn-001', 'completed', 8, null),
(4, '2026-06-04 10:45:00', 2000.00, 'cash', 'txn-002', 'completed', 8, null),
(5, '2026-06-05 16:00:00', 9000.00, 'card', 'txn-003', 'completed', 8, null),
(8, '2026-06-08 13:00:00', 5300.00, 'card', 'txn-004', 'completed', 8, null),
(11, '2026-06-11 12:00:00', 3500.00, 'cash', 'txn-005', 'completed', 8, null),
(13, '2026-06-13 12:30:00', 7000.00, 'card', 'txn-006', 'completed', 8, null),
(17, '2026-06-17 16:00:00', 5500.00, 'card', 'txn-007', 'completed', 8, null),
(23, '2026-06-23 12:00:00', 2700.00, 'cash', 'txn-008', 'completed', 8, null),
(14, '2026-06-14 15:00:00', 8500.00, 'card', 'txn-009', 'completed', 8, null),
(25, '2026-06-25 14:00:00', 10450.00, 'card', 'txn-010', 'completed', 8, null),
(2, '2026-06-02 12:00:00', 4000.00, 'card', 'txn-011', 'completed', 8, 'partial payment'),
(6, '2026-06-06 10:00:00', 7500.00, 'bank_transfer', 'txn-012', 'completed', 8, 'fleet partial'),
(15, '2026-06-15 12:00:00', 7000.00, 'card', 'txn-013', 'completed', 8, 'partial payment'),
(21, '2026-06-21 11:00:00', 10000.00, 'card', 'txn-014', 'completed', 8, 'partial vip payment');
go

insert into part_supplies (supplier_id, part_id, supply_date, quantity, unit_cost, total_cost, invoice_number, received_by, status) values
(1, 1, '2026-05-01', 30, 2500.00, 75000.00, 'inv-ap-001', 5, 'received'),
(1, 2, '2026-05-01', 50, 400.00, 20000.00, 'inv-ap-001', 5, 'received'),
(3, 5, '2026-05-05', 20, 2800.00, 56000.00, 'inv-fb-002', 5, 'received'),
(3, 6, '2026-05-05', 20, 2400.00, 48000.00, 'inv-fb-002', 5, 'received'),
(4, 8, '2026-05-10', 15, 5800.00, 87000.00, 'inv-ea-003', 5, 'received'),
(5, 3, '2026-05-12', 40, 550.00, 22000.00, 'inv-fw-004', 5, 'received'),
(5, 4, '2026-05-12', 35, 700.00, 24500.00, 'inv-fw-004', 5, 'received'),
(6, 12, '2026-05-15', 25, 1600.00, 40000.00, 'inv-om-005', 5, 'received'),
(6, 13, '2026-05-15', 40, 850.00, 34000.00, 'inv-om-005', 5, 'received'),
(2, 14, '2026-05-20', 10, 4800.00, 48000.00, 'inv-ep-006', 5, 'received'),
(1, 10, '2026-05-22', 25, 2100.00, 52500.00, 'inv-ap-007', 5, 'received'),
(1, 11, '2026-05-22', 10, 7800.00, 78000.00, 'inv-ap-007', 5, 'received'),
(3, 7, '2026-05-25', 8, 4000.00, 32000.00, 'inv-fb-008', 5, 'received'),
(4, 9, '2026-05-28', 5, 10500.00, 52500.00, 'inv-ea-009', 5, 'received'),
(2, 15, '2026-06-01', 4, 8800.00, 35200.00, 'inv-ep-010', 5, 'received');
go

print 'autoservice_db deployed successfully.';
print 'tables: 16 | views: 5 | triggers: 5 | procedures: 6 | roles: 4';
go
