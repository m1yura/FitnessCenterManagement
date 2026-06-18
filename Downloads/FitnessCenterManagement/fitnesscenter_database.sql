/*
    fitness center database - complete deployment script
    domain: fitness center management
    run this script in sql server management studio or sqlcmd
*/

use master;
go

if db_id('fitnesscenter_db') is not null
begin
    alter database fitnesscenter_db set single_user with rollback immediate;
    drop database fitnesscenter_db;
end
go

create database fitnesscenter_db;
go

use fitnesscenter_db;
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

create table branches (
    branch_id int identity(1,1) not null,
    branch_name nvarchar(100) not null,
    address nvarchar(255) not null,
    city nvarchar(100) not null,
    phone nvarchar(20) not null,
    email nvarchar(100) null,
    opening_hours nvarchar(100) null,
    capacity int not null default 100,
    manager_name nvarchar(100) null,
    parking_spaces int not null default 0,
    has_pool bit not null default 0,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_branches primary key (branch_id),
    constraint uq_branches_name unique (branch_name)
);
go

create table membership_types (
    membership_type_id int identity(1,1) not null,
    type_name nvarchar(100) not null,
    description nvarchar(255) null,
    duration_days int not null,
    price decimal(10,2) not null,
    access_level nvarchar(30) not null default 'standard',
    max_classes_per_month int not null default 8,
    includes_personal_training bit not null default 0,
    guest_passes int not null default 0,
    freeze_days_allowed int not null default 14,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_membership_types primary key (membership_type_id),
    constraint uq_membership_types_name unique (type_name)
);
go

create table members (
    member_id int identity(1,1) not null,
    branch_id int not null,
    first_name nvarchar(50) not null,
    last_name nvarchar(50) not null,
    phone nvarchar(20) not null,
    email nvarchar(100) null,
    date_of_birth date null,
    gender nvarchar(10) null,
    address nvarchar(255) null,
    emergency_contact nvarchar(100) null,
    health_notes nvarchar(500) null,
    referral_source nvarchar(50) null,
    fitness_goal nvarchar(100) null,
    loyalty_points int not null default 0,
    join_date date not null default cast(sysutcdatetime() as date),
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_members primary key (member_id),
    constraint fk_members_branches foreign key (branch_id) references branches(branch_id)
);
go

create table memberships (
    membership_id int identity(1,1) not null,
    member_id int not null,
    membership_type_id int not null,
    start_date date not null,
    end_date date not null,
    status nvarchar(30) not null default 'active',
    auto_renew bit not null default 0,
    discount_percent decimal(5,2) not null default 0,
    freeze_start date null,
    freeze_end date null,
    notes nvarchar(255) null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_memberships primary key (membership_id),
    constraint fk_memberships_members foreign key (member_id) references members(member_id),
    constraint fk_memberships_types foreign key (membership_type_id) references membership_types(membership_type_id)
);
go

create table trainers (
    trainer_id int identity(1,1) not null,
    branch_id int not null,
    first_name nvarchar(50) not null,
    last_name nvarchar(50) not null,
    phone nvarchar(20) not null,
    email nvarchar(100) not null,
    hire_date date not null,
    hourly_rate decimal(10,2) not null default 0,
    employment_type nvarchar(30) not null default 'full_time',
    certification_level nvarchar(50) null,
    certification_expiry date null,
    bio nvarchar(500) null,
    rating decimal(3,2) null,
    max_clients_per_day int not null default 8,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_trainers primary key (trainer_id),
    constraint fk_trainers_branches foreign key (branch_id) references branches(branch_id)
);
go

create table specializations (
    specialization_id int identity(1,1) not null,
    specialization_name nvarchar(100) not null,
    description nvarchar(255) null,
    category nvarchar(50) null,
    difficulty_level nvarchar(20) null,
    required_certification nvarchar(100) null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_specializations primary key (specialization_id),
    constraint uq_specializations_name unique (specialization_name)
);
go

create table trainer_specializations (
    trainer_specialization_id int identity(1,1) not null,
    trainer_id int not null,
    specialization_id int not null,
    certified_date date not null,
    certification_number nvarchar(50) null,
    expiry_date date null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_trainer_specializations primary key (trainer_specialization_id),
    constraint fk_ts_trainers foreign key (trainer_id) references trainers(trainer_id),
    constraint fk_ts_specializations foreign key (specialization_id) references specializations(specialization_id)
);
go

create table fitness_classes (
    class_id int identity(1,1) not null,
    branch_id int not null,
    class_name nvarchar(100) not null,
    description nvarchar(255) null,
    category nvarchar(50) not null,
    difficulty_level nvarchar(20) not null default 'beginner',
    max_capacity int not null default 20,
    duration_minutes int not null default 60,
    room_name nvarchar(50) null,
    equipment_required nvarchar(255) null,
    calories_burn_estimate int null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_fitness_classes primary key (class_id),
    constraint fk_classes_branches foreign key (branch_id) references branches(branch_id)
);
go

create table class_schedules (
    schedule_id int identity(1,1) not null,
    class_id int not null,
    trainer_id int not null,
    start_time datetime2 not null,
    end_time datetime2 not null,
    status nvarchar(30) not null default 'scheduled',
    current_enrollment int not null default 0,
    room_override nvarchar(50) null,
    notes nvarchar(255) null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_class_schedules primary key (schedule_id),
    constraint fk_schedules_classes foreign key (class_id) references fitness_classes(class_id),
    constraint fk_schedules_trainers foreign key (trainer_id) references trainers(trainer_id)
);
go

create table class_enrollments (
    enrollment_id int identity(1,1) not null,
    schedule_id int not null,
    member_id int not null,
    enrollment_date datetime2 not null default sysutcdatetime(),
    status nvarchar(30) not null default 'enrolled',
    check_in_time datetime2 null,
    cancellation_reason nvarchar(255) null,
    notes nvarchar(255) null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_class_enrollments primary key (enrollment_id),
    constraint fk_enrollments_schedules foreign key (schedule_id) references class_schedules(schedule_id),
    constraint fk_enrollments_members foreign key (member_id) references members(member_id)
);
go

create table equipment (
    equipment_id int identity(1,1) not null,
    branch_id int not null,
    equipment_name nvarchar(100) not null,
    category nvarchar(50) not null,
    serial_number nvarchar(50) null,
    purchase_date date null,
    purchase_price decimal(10,2) null,
    warranty_until date null,
    location nvarchar(50) null,
    condition_status nvarchar(30) not null default 'good',
    last_maintenance_date date null,
    manufacturer nvarchar(100) null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_equipment primary key (equipment_id),
    constraint uq_equipment_serial unique (serial_number)
);
go

create table equipment_maintenance (
    maintenance_id int identity(1,1) not null,
    equipment_id int not null,
    maintenance_date date not null,
    maintenance_type nvarchar(50) not null,
    performed_by int null,
    cost decimal(10,2) not null default 0,
    description nvarchar(500) null,
    next_maintenance_date date null,
    status nvarchar(30) not null default 'completed',
    vendor_name nvarchar(100) null,
    isactive bit not null default 1,
    isdeleted bit not null default 0,
    createdat datetime2 not null default sysutcdatetime(),
    updatedat datetime2 null,
    constraint pk_equipment_maintenance primary key (maintenance_id),
    constraint fk_maintenance_equipment foreign key (equipment_id) references equipment(equipment_id),
    constraint fk_maintenance_trainers foreign key (performed_by) references trainers(trainer_id)
);
go

create table payments (
    payment_id int identity(1,1) not null,
    member_id int not null,
    membership_id int null,
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
    constraint fk_payments_members foreign key (member_id) references members(member_id),
    constraint fk_payments_memberships foreign key (membership_id) references memberships(membership_id),
    constraint fk_payments_users foreign key (processed_by) references users(user_id)
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
create index ix_members_branch_id on members(branch_id);
create index ix_memberships_member_id on memberships(member_id);
create index ix_trainers_branch_id on trainers(branch_id);
create index ix_class_schedules_start on class_schedules(start_time);
create index ix_class_enrollments_schedule on class_enrollments(schedule_id);
create index ix_equipment_branch_id on equipment(branch_id);
create index ix_payments_member_id on payments(member_id);
create index ix_audit_log_table on audit_log(table_name, record_id);
go

/* ============================================================
   views
   ============================================================ */

create view vw_active_memberships as
select
    ms.membership_id,
    ms.start_date,
    ms.end_date,
    ms.status,
    ms.auto_renew,
    mt.type_name,
    mt.price,
    mt.access_level,
    m.member_id,
    m.first_name + ' ' + m.last_name as member_name,
    m.phone as member_phone,
    b.branch_name
from memberships ms
inner join members m on ms.member_id = m.member_id
inner join membership_types mt on ms.membership_type_id = mt.membership_type_id
inner join branches b on m.branch_id = b.branch_id
where ms.isdeleted = 0 and ms.isactive = 1 and m.isdeleted = 0;
go

create view vw_class_schedule_overview as
select
    cs.schedule_id,
    fc.class_name,
    fc.category,
    fc.difficulty_level,
    fc.max_capacity,
    cs.start_time,
    cs.end_time,
    cs.status,
    cs.current_enrollment,
    t.first_name + ' ' + t.last_name as trainer_name,
    b.branch_name,
    fc.room_name
from class_schedules cs
inner join fitness_classes fc on cs.class_id = fc.class_id
inner join trainers t on cs.trainer_id = t.trainer_id
inner join branches b on fc.branch_id = b.branch_id
where cs.isdeleted = 0 and cs.isactive = 1;
go

create view vw_trainer_workload as
select
    t.trainer_id,
    t.first_name + ' ' + t.last_name as trainer_name,
    t.employment_type,
    t.rating,
    b.branch_name,
    count(distinct cs.schedule_id) as upcoming_classes,
    count(distinct ce.enrollment_id) as total_enrollments
from trainers t
inner join branches b on t.branch_id = b.branch_id
left join class_schedules cs on t.trainer_id = cs.trainer_id
    and cs.start_time >= sysutcdatetime() and cs.status = 'scheduled' and cs.isdeleted = 0
left join class_enrollments ce on cs.schedule_id = ce.schedule_id and ce.isdeleted = 0
where t.isdeleted = 0 and t.isactive = 1
group by t.trainer_id, t.first_name, t.last_name, t.employment_type, t.rating, b.branch_name;
go

create view vw_equipment_status as
select
    e.equipment_id,
    e.equipment_name,
    e.category,
    e.condition_status,
    e.location,
    e.last_maintenance_date,
    b.branch_name,
    case
        when e.condition_status = 'out_of_service' then 'critical'
        when e.last_maintenance_date is null or e.last_maintenance_date < dateadd(month, -6, cast(sysutcdatetime() as date)) then 'needs_maintenance'
        else 'operational'
    end as maintenance_status
from equipment e
inner join branches b on e.branch_id = b.branch_id
where e.isdeleted = 0 and e.isactive = 1;
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

create trigger trg_memberships_audit
on memberships
after insert, update, delete
as
begin
    set nocount on;

    if exists (select 1 from inserted) and exists (select 1 from deleted)
    begin
        insert into audit_log (table_name, record_id, action_type, old_values, new_values, changed_by)
        select 'memberships', i.membership_id, 'update',
            'status=' + d.status + ';end=' + cast(d.end_date as nvarchar(20)),
            'status=' + i.status + ';end=' + cast(i.end_date as nvarchar(20)),
            'trigger'
        from inserted i
        inner join deleted d on i.membership_id = d.membership_id
        where i.status <> d.status or i.end_date <> d.end_date;
    end

    if exists (select 1 from inserted) and not exists (select 1 from deleted)
    begin
        insert into audit_log (table_name, record_id, action_type, new_values, changed_by)
        select 'memberships', membership_id, 'insert',
            'member=' + cast(member_id as nvarchar(10)) + ';status=' + status, 'trigger'
        from inserted;
    end

    if exists (select 1 from deleted) and not exists (select 1 from inserted)
    begin
        insert into audit_log (table_name, record_id, action_type, old_values, changed_by)
        select 'memberships', membership_id, 'delete',
            'member=' + cast(member_id as nvarchar(10)), 'trigger'
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

create trigger trg_class_enrollments_capacity
on class_enrollments
after insert, update
as
begin
    set nocount on;

    if exists (
        select 1
        from inserted i
        inner join class_schedules cs on i.schedule_id = cs.schedule_id
        inner join fitness_classes fc on cs.class_id = fc.class_id
        where i.isdeleted = 0 and i.status = 'enrolled'
          and cs.current_enrollment > fc.max_capacity
    )
    begin
        raiserror('class capacity exceeded.', 16, 1);
        rollback transaction;
        return;
    end

    update cs
    set cs.current_enrollment = (
            select count(*) from class_enrollments
            where schedule_id = cs.schedule_id and isdeleted = 0 and status = 'enrolled'
        ),
        cs.updatedat = sysutcdatetime()
    from class_schedules cs
    inner join inserted i on cs.schedule_id = i.schedule_id;
end;
go

create trigger trg_members_soft_delete
on members
after update
as
begin
    set nocount on;

    if update(isdeleted)
    begin
        insert into audit_log (table_name, record_id, action_type, old_values, new_values, changed_by)
        select 'members', i.member_id, 'soft_delete',
            'isdeleted=' + cast(d.isdeleted as nvarchar(1)),
            'isdeleted=' + cast(i.isdeleted as nvarchar(1)),
            'trigger'
        from inserted i
        inner join deleted d on i.member_id = d.member_id
        where i.isdeleted <> d.isdeleted;
    end
end;
go

/* ============================================================
   sequences
   ============================================================ */

create sequence seq_membership_number as int start with 1001 increment by 1;
go

/* ============================================================
   stored procedures with transactions
   ============================================================ */

create procedure sp_register_member
    @branch_id int,
    @first_name nvarchar(50),
    @last_name nvarchar(50),
    @phone nvarchar(20),
    @email nvarchar(100) = null,
    @membership_type_id int = null,
    @member_id int output
as
begin
    set nocount on;
    set xact_abort on;

    begin try
        begin transaction;

        if not exists (select 1 from branches where branch_id = @branch_id and isdeleted = 0)
            throw 50001, 'branch not found or inactive.', 1;

        insert into members (branch_id, first_name, last_name, phone, email)
        values (@branch_id, @first_name, @last_name, @phone, @email);

        set @member_id = scope_identity();

        if @membership_type_id is not null
        begin
            declare @duration int;
            declare @price decimal(10,2);
            select @duration = duration_days, @price = price
            from membership_types where membership_type_id = @membership_type_id and isdeleted = 0;

            if @duration is null throw 50002, 'membership type not found.', 1;

            insert into memberships (member_id, membership_type_id, start_date, end_date, status)
            values (@member_id, @membership_type_id, cast(sysutcdatetime() as date),
                    dateadd(day, @duration, cast(sysutcdatetime() as date)), 'active');
        end

        commit transaction;
    end try
    begin catch
        if @@trancount > 0 rollback transaction;
        throw;
    end catch
end;
go

create procedure sp_create_membership
    @member_id int,
    @membership_type_id int,
    @auto_renew bit = 0,
    @discount_percent decimal(5,2) = 0,
    @membership_id int output
as
begin
    set nocount on;
    set xact_abort on;

    begin try
        begin transaction;

        if not exists (select 1 from members where member_id = @member_id and isdeleted = 0)
            throw 50003, 'member not found or inactive.', 1;

        if exists (select 1 from memberships where member_id = @member_id and status = 'active' and isdeleted = 0)
            throw 50004, 'member already has an active membership.', 1;

        declare @duration int;
        select @duration = duration_days from membership_types
        where membership_type_id = @membership_type_id and isdeleted = 0;

        if @duration is null throw 50005, 'membership type not found.', 1;

        insert into memberships (member_id, membership_type_id, start_date, end_date, status, auto_renew, discount_percent)
        values (@member_id, @membership_type_id, cast(sysutcdatetime() as date),
                dateadd(day, @duration, cast(sysutcdatetime() as date)), 'active', @auto_renew, @discount_percent);

        set @membership_id = scope_identity();

        commit transaction;
    end try
    begin catch
        if @@trancount > 0 rollback transaction;
        throw;
    end catch
end;
go

create procedure sp_enroll_in_class
    @schedule_id int,
    @member_id int,
    @enrollment_id int output
as
begin
    set nocount on;
    set xact_abort on;

    begin try
        begin transaction;

        if not exists (
            select 1 from memberships ms
            inner join members m on ms.member_id = m.member_id
            where m.member_id = @member_id and ms.status = 'active' and ms.isdeleted = 0
              and ms.end_date >= cast(sysutcdatetime() as date)
        )
            throw 50006, 'member has no active membership.', 1;

        if not exists (select 1 from class_schedules where schedule_id = @schedule_id and isdeleted = 0 and status = 'scheduled')
            throw 50007, 'class schedule not found or not available.', 1;

        if exists (
            select 1 from class_enrollments
            where schedule_id = @schedule_id and member_id = @member_id and isdeleted = 0 and status = 'enrolled'
        )
            throw 50008, 'member already enrolled in this class.', 1;

        declare @max_cap int;
        declare @current int;
        select @max_cap = fc.max_capacity, @current = cs.current_enrollment
        from class_schedules cs
        inner join fitness_classes fc on cs.class_id = fc.class_id
        where cs.schedule_id = @schedule_id;

        if @current >= @max_cap
            throw 50009, 'class is full.', 1;

        insert into class_enrollments (schedule_id, member_id, status)
        values (@schedule_id, @member_id, 'enrolled');

        set @enrollment_id = scope_identity();

        update class_schedules
        set current_enrollment = current_enrollment + 1, updatedat = sysutcdatetime()
        where schedule_id = @schedule_id;

        commit transaction;
    end try
    begin catch
        if @@trancount > 0 rollback transaction;
        throw;
    end catch
end;
go

create procedure sp_process_payment
    @member_id int,
    @membership_id int = null,
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

        if not exists (select 1 from members where member_id = @member_id and isdeleted = 0)
            throw 50010, 'member not found.', 1;

        if @amount <= 0
            throw 50011, 'payment amount must be positive.', 1;

        insert into payments (member_id, membership_id, amount, payment_method, processed_by, transaction_reference)
        values (@member_id, @membership_id, @amount, @payment_method, @processed_by, @transaction_reference);

        set @payment_id = scope_identity();

        if @membership_id is not null
        begin
            update members
            set loyalty_points = loyalty_points + cast(@amount / 100 as int),
                updatedat = sysutcdatetime()
            where member_id = @member_id;
        end

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

create role fitness_admin_role;
create role fitness_manager_role;
create role fitness_trainer_role;
create role fitness_guest_role;
go

grant select, insert, update, delete on schema::dbo to fitness_admin_role;
grant execute on schema::dbo to fitness_admin_role;
grant alter on schema::dbo to fitness_admin_role;

grant select, insert, update on members to fitness_manager_role;
grant select, insert, update on memberships to fitness_manager_role;
grant select, insert, update on membership_types to fitness_manager_role;
grant select, insert, update on branches to fitness_manager_role;
grant select, insert, update on trainers to fitness_manager_role;
grant select on specializations to fitness_manager_role;
grant select, insert, update on trainer_specializations to fitness_manager_role;
grant select, insert, update on fitness_classes to fitness_manager_role;
grant select, insert, update on class_schedules to fitness_manager_role;
grant select, insert, update on class_enrollments to fitness_manager_role;
grant select, insert, update on equipment to fitness_manager_role;
grant select, insert, update on equipment_maintenance to fitness_manager_role;
grant select, insert, update on payments to fitness_manager_role;
grant select on roles to fitness_manager_role;
grant select on users to fitness_manager_role;
grant select on audit_log to fitness_manager_role;
grant select on vw_active_memberships to fitness_manager_role;
grant select on vw_class_schedule_overview to fitness_manager_role;
grant select on vw_trainer_workload to fitness_manager_role;
grant select on vw_equipment_status to fitness_manager_role;
grant select on vw_revenue_summary to fitness_manager_role;
grant execute on sp_register_member to fitness_manager_role;
grant execute on sp_create_membership to fitness_manager_role;
grant execute on sp_enroll_in_class to fitness_manager_role;
grant execute on sp_process_payment to fitness_manager_role;
revoke delete on users from fitness_manager_role;
revoke delete on roles from fitness_manager_role;

grant select on members to fitness_trainer_role;
grant select on memberships to fitness_trainer_role;
grant select on membership_types to fitness_trainer_role;
grant select on branches to fitness_trainer_role;
grant select on trainers to fitness_trainer_role;
grant select on specializations to fitness_trainer_role;
grant select on fitness_classes to fitness_trainer_role;
grant select, update on class_schedules to fitness_trainer_role;
grant select, insert, update on class_enrollments to fitness_trainer_role;
grant select on equipment to fitness_trainer_role;
grant select, insert on equipment_maintenance to fitness_trainer_role;
grant select on vw_class_schedule_overview to fitness_trainer_role;
grant select on vw_trainer_workload to fitness_trainer_role;
grant execute on sp_enroll_in_class to fitness_trainer_role;
revoke delete on class_schedules from fitness_trainer_role;
revoke insert, update, delete on payments from fitness_trainer_role;

grant select on vw_active_memberships to fitness_guest_role;
grant select on vw_class_schedule_overview to fitness_guest_role;
grant select on vw_equipment_status to fitness_guest_role;
grant select on membership_types to fitness_guest_role;
grant select on fitness_classes to fitness_guest_role;
grant select on branches to fitness_guest_role;
revoke insert, update, delete on schema::dbo from fitness_guest_role;
go

/* ============================================================
   test data
   ============================================================ */

insert into roles (role_name, role_description, permission_level) values
('administrator', 'full system access', 100),
('manager', 'branch manager with operational access', 75),
('trainer', 'fitness trainer with class access', 50),
('guest', 'read-only access', 10);
go

insert into users (username, password_hash, email, role_id) values
('admin', 'hash_admin_123', 'admin@fitnesscenter.local', 1),
('manager1', 'hash_manager_123', 'manager@fitnesscenter.local', 2),
('trainer1', 'hash_trainer_123', 'trainer1@fitnesscenter.local', 3),
('trainer2', 'hash_trainer_456', 'trainer2@fitnesscenter.local', 3),
('guest1', 'hash_guest_123', 'guest@fitnesscenter.local', 4);
go

insert into branches (branch_name, address, city, phone, email, opening_hours, capacity, manager_name, parking_spaces, has_pool) values
('fitlife central', 'ul. lenina 10', 'moscow', '+7-495-100-0001', 'central@fitlife.ru', '06:00-23:00', 500, 'anna ivanova', 50, 1),
('fitlife north', 'pr. mira 25', 'moscow', '+7-495-100-0002', 'north@fitlife.ru', '07:00-22:00', 300, 'sergey petrov', 30, 0),
('fitlife south', 'ul. pushkina 5', 'spb', '+7-812-200-0001', 'south@fitlife.ru', '06:00-23:00', 400, 'elena smirnova', 40, 1),
('fitlife east', 'ul. gagarina 12', 'kazan', '+7-843-300-0001', 'east@fitlife.ru', '07:00-22:00', 250, 'dmitry volkov', 25, 0),
('fitlife west', 'ul. sovetskaya 8', 'novosibirsk', '+7-383-400-0001', 'west@fitlife.ru', '06:00-23:00', 350, 'maria fedorova', 35, 1);
go

insert into membership_types (type_name, description, duration_days, price, access_level, max_classes_per_month, includes_personal_training, guest_passes, freeze_days_allowed) values
('basic monthly', 'gym access only', 30, 2500.00, 'basic', 4, 0, 0, 7),
('standard monthly', 'gym + group classes', 30, 4500.00, 'standard', 12, 0, 1, 14),
('premium monthly', 'full access + 2 pt sessions', 30, 7500.00, 'premium', 20, 1, 2, 21),
('annual basic', 'yearly basic plan', 365, 25000.00, 'basic', 4, 0, 0, 30),
('annual premium', 'yearly premium plan', 365, 75000.00, 'premium', 999, 1, 5, 60),
('student monthly', 'discounted student plan', 30, 2000.00, 'basic', 8, 0, 0, 7),
('family monthly', 'family plan up to 4 members', 30, 12000.00, 'standard', 16, 0, 3, 14),
('day pass', 'single day access', 1, 500.00, 'basic', 1, 0, 0, 0);
go

insert into members (branch_id, first_name, last_name, phone, email, date_of_birth, gender, address, emergency_contact, health_notes, referral_source, fitness_goal, loyalty_points, join_date) values
(1, 'ivan', 'petrov', '+7-900-111-0001', 'ivan.p@mail.ru', '1990-05-15', 'male', 'ul. lenina 10', 'maria petrova +7-900-111-9001', null, 'friend', 'weight loss', 120, '2024-01-10'),
(1, 'elena', 'smirnova', '+7-900-111-0002', 'elena.s@mail.ru', '1988-03-22', 'female', 'pr. mira 25', 'alex smirnov +7-900-111-9002', 'asthma', 'online', 'flexibility', 80, '2024-02-15'),
(2, 'dmitry', 'volkov', '+7-900-111-0003', 'd.volkov@mail.ru', '1985-11-08', 'male', 'ul. pushkina 5', 'olga volkova +7-900-111-9003', null, 'advertisement', 'muscle gain', 200, '2023-06-01'),
(1, 'anna', 'kuznetsova', '+7-900-111-0004', 'anna.k@mail.ru', '1995-07-30', 'female', 'ul. gagarina 12', 'sergey kuznetsov +7-900-111-9004', null, 'social media', 'general fitness', 45, '2024-03-20'),
(3, 'sergey', 'orlov', '+7-900-111-0005', 's.orlov@mail.ru', '1992-01-12', 'male', 'ul. sovetskaya 8', 'irina orlova +7-900-111-9005', 'knee injury history', 'walk-in', 'rehabilitation', 30, '2024-04-05'),
(2, 'maria', 'fedorova', '+7-900-111-0006', 'm.fedorova@mail.ru', '1987-09-18', 'female', 'ul. kirova 3', 'pavel fedorov +7-900-111-9006', null, 'corporate', 'endurance', 150, '2023-11-10'),
(1, 'alexey', 'morozov', '+7-900-111-0007', 'a.morozov@mail.ru', '1993-04-25', 'male', 'ul. centralnaya 44', 'nadezhda morozova +7-900-111-9007', null, 'friend', 'crossfit', 60, '2024-05-01'),
(4, 'olga', 'nikolaeva', '+7-900-111-0008', 'o.nikolaeva@mail.ru', '1991-12-03', 'female', 'ul. sadovaya 19', 'viktor nikolaev +7-900-111-9008', null, 'online', 'yoga', 90, '2024-01-25'),
(1, 'pavel', 'sokolov', '+7-900-111-0009', 'p.sokolov@mail.ru', '1989-08-14', 'male', 'ul. zelenaya 7', 'tatyana sokolova +7-900-111-9009', 'diabetes type 2', 'advertisement', 'weight loss', 25, '2024-06-10'),
(2, 'tatyana', 'lebedeva', '+7-900-111-0010', 't.lebedeva@mail.ru', '1994-02-28', 'female', 'ul. nadezhdy 2', 'andrey lebedev +7-900-111-9010', null, 'social media', 'toning', 110, '2023-09-15'),
(3, 'nikolay', 'egorov', '+7-900-111-0011', 'n.egorov@mail.ru', '1986-06-07', 'male', 'ul. shkolnaya 15', 'svetlana egorova +7-900-111-9011', null, 'walk-in', 'muscle gain', 70, '2024-02-28'),
(1, 'svetlana', 'popova', '+7-900-111-0012', 's.popova@mail.ru', '1996-10-19', 'female', 'ul. lesnaya 33', 'mikhail popov +7-900-111-9012', null, 'friend', 'general fitness', 55, '2024-07-01'),
(4, 'andrey', 'vasiliev', '+7-900-111-0013', 'a.vasiliev@mail.ru', '1991-03-11', 'male', 'ul. rechnaya 6', 'yulia vasilieva +7-900-111-9013', null, 'corporate', 'endurance', 40, '2024-03-15'),
(2, 'irina', 'pavlova', '+7-900-111-0014', 'i.pavlova@mail.ru', '1988-11-25', 'female', 'ul. solnechnaya 21', 'roman pavlov +7-900-111-9014', 'back pain', 'online', 'rehabilitation', 95, '2023-12-20'),
(5, 'mikhail', 'semenov', '+7-900-111-0015', 'm.semenov@mail.ru', '1990-07-04', 'male', 'ul. polevaya 9', 'ekaterina semenova +7-900-111-9015', null, 'advertisement', 'crossfit', 35, '2024-08-01'),
(1, 'yulia', 'golubeva', '+7-900-111-0016', 'y.golubeva@mail.ru', '1993-01-30', 'female', 'ul. tsvetochnaya 4', 'konstantin golubev +7-900-111-9016', null, 'social media', 'flexibility', 65, '2024-04-20'),
(3, 'viktor', 'komarov', '+7-900-111-0017', 'v.komarov@mail.ru', '1987-05-22', 'male', 'ul. zheleznaya 11', 'nadezhda komarova +7-900-111-9017', null, 'friend', 'muscle gain', 50, '2024-01-05'),
(2, 'nadezhda', 'belova', '+7-900-111-0018', 'n.belova@mail.ru', '1995-09-09', 'female', 'ul. yuzhnaya 28', 'roman belov +7-900-111-9018', null, 'walk-in', 'toning', 85, '2024-05-15'),
(4, 'roman', 'tarasov', '+7-900-111-0019', 'r.tarasov@mail.ru', '1992-12-17', 'male', 'ul. severnaya 17', 'ekaterina tarasova +7-900-111-9019', null, 'online', 'general fitness', 20, '2024-06-25'),
(5, 'ekaterina', 'ryabova', '+7-900-111-0020', 'e.ryabova@mail.ru', '1994-04-08', 'female', 'ul. ozernaya 13', 'konstantin ryabov +7-900-111-9020', null, 'corporate', 'yoga', 75, '2024-02-10'),
(1, 'konstantin', 'zaitsev', '+7-900-111-0021', 'k.zaitsev@mail.ru', '1989-08-30', 'male', 'ul. gorodskaya 30', 'yulia zaitseva +7-900-111-9021', null, 'advertisement', 'endurance', 42, '2024-07-15'),
(2, 'alina', 'frolova', '+7-900-111-0022', 'a.frolova@mail.ru', '1997-02-14', 'female', 'ul. parkovaya 5', 'dmitry frolov +7-900-111-9022', null, 'social media', 'weight loss', 38, '2024-08-20'),
(3, 'oleg', 'baranov', '+7-900-111-0023', 'o.baranov@mail.ru', '1984-10-05', 'male', 'ul. lesnaya 18', 'marina baranova +7-900-111-9023', 'hypertension', 'corporate', 'general fitness', 88, '2023-10-01'),
(1, 'marina', 'kozlova', '+7-900-111-0024', 'm.kozlova@mail.ru', '1991-06-21', 'female', 'ul. tsvetnaya 7', 'sergey kozlov +7-900-111-9024', null, 'friend', 'toning', 52, '2024-03-01'),
(4, 'artem', 'novikov', '+7-900-111-0025', 'a.novikov@mail.ru', '1993-11-11', 'male', 'ul. sportivnaya 2', 'olga novikova +7-900-111-9025', null, 'walk-in', 'crossfit', 33, '2024-09-01'),
(5, 'daria', 'sorokina', '+7-900-111-0026', 'd.sorokina@mail.ru', '1996-03-27', 'female', 'ul. molodezhnaya 14', 'ivan sorokin +7-900-111-9026', null, 'online', 'flexibility', 47, '2024-04-10'),
(1, 'maxim', 'vinogradov', '+7-900-111-0027', 'm.vinogradov@mail.ru', '1988-07-19', 'male', 'ul. nadezhdinskaya 9', 'elena vinogradova +7-900-111-9027', null, 'advertisement', 'muscle gain', 61, '2024-01-20'),
(2, 'polina', 'medvedeva', '+7-900-111-0028', 'p.medvedeva@mail.ru', '1995-12-02', 'female', 'ul. zarechnaya 3', 'alexey medvedev +7-900-111-9028', null, 'social media', 'yoga', 29, '2024-05-25'),
(3, 'kirill', 'borisov', '+7-900-111-0029', 'k.borisov@mail.ru', '1990-04-16', 'male', 'ul. stepnaya 11', 'anna borisova +7-900-111-9029', null, 'friend', 'endurance', 56, '2024-06-05'),
(1, 'veronika', 'andreeva', '+7-900-111-0030', 'v.andreeva@mail.ru', '1998-01-08', 'female', 'ul. rodnaya 6', 'pavel andreev +7-900-111-9030', null, 'walk-in', 'general fitness', 18, '2024-09-15');
go

insert into memberships (member_id, membership_type_id, start_date, end_date, status, auto_renew, discount_percent) values
(1, 2, '2024-01-10', '2024-02-09', 'expired', 0, 0),
(1, 2, '2024-02-10', '2025-02-09', 'active', 1, 0),
(2, 3, '2024-02-15', '2025-02-14', 'active', 1, 10),
(3, 5, '2023-06-01', '2024-05-31', 'expired', 0, 0),
(3, 5, '2024-06-01', '2025-05-31', 'active', 1, 5),
(4, 1, '2024-03-20', '2024-04-19', 'expired', 0, 0),
(4, 2, '2024-04-20', '2025-04-19', 'active', 0, 0),
(5, 2, '2024-04-05', '2025-04-04', 'active', 1, 0),
(6, 4, '2023-11-10', '2024-11-09', 'active', 1, 0),
(7, 2, '2024-05-01', '2024-05-31', 'expired', 0, 0),
(7, 3, '2024-06-01', '2025-05-31', 'active', 1, 0),
(8, 1, '2024-01-25', '2024-02-24', 'expired', 0, 0),
(8, 2, '2024-02-25', '2025-02-24', 'active', 0, 0),
(9, 6, '2024-06-10', '2024-07-09', 'expired', 0, 15),
(9, 6, '2024-07-10', '2025-07-09', 'active', 1, 15),
(10, 3, '2023-09-15', '2024-09-14', 'expired', 0, 0),
(10, 3, '2024-09-15', '2025-09-14', 'active', 1, 0),
(11, 2, '2024-02-28', '2025-02-27', 'active', 0, 0),
(12, 1, '2024-07-01', '2024-07-31', 'expired', 0, 0),
(12, 2, '2024-08-01', '2025-07-31', 'active', 1, 0),
(13, 2, '2024-03-15', '2025-03-14', 'active', 0, 0),
(14, 3, '2023-12-20', '2024-12-19', 'active', 1, 0),
(15, 1, '2024-08-01', '2024-08-31', 'expired', 0, 0),
(15, 2, '2024-09-01', '2025-08-31', 'active', 1, 0),
(16, 2, '2024-04-20', '2025-04-19', 'active', 0, 0),
(17, 4, '2024-01-05', '2025-01-04', 'active', 1, 0),
(18, 2, '2024-05-15', '2025-05-14', 'active', 0, 0),
(19, 8, '2024-06-25', '2024-06-26', 'expired', 0, 0),
(19, 1, '2024-06-27', '2024-07-26', 'expired', 0, 0),
(19, 2, '2024-07-27', '2025-07-26', 'active', 1, 0);
go

insert into trainers (branch_id, first_name, last_name, phone, email, hire_date, hourly_rate, employment_type, certification_level, certification_expiry, bio, rating, max_clients_per_day) values
(1, 'alexander', 'fitman', '+7-900-200-0001', 'a.fitman@fitlife.ru', '2020-03-01', 1500.00, 'full_time', 'master', '2026-03-01', 'crossfit and strength specialist', 4.90, 10),
(1, 'victoria', 'stretch', '+7-900-200-0002', 'v.stretch@fitlife.ru', '2021-06-15', 1200.00, 'full_time', 'advanced', '2025-06-15', 'yoga and pilates instructor', 4.85, 8),
(2, 'maxim', 'power', '+7-900-200-0003', 'm.power@fitlife.ru', '2019-01-10', 1400.00, 'full_time', 'master', '2025-12-01', 'bodybuilding coach', 4.75, 8),
(3, 'natalia', 'cardio', '+7-900-200-0004', 'n.cardio@fitlife.ru', '2022-02-20', 1100.00, 'part_time', 'intermediate', '2025-02-20', 'hiit and cardio expert', 4.80, 6),
(1, 'igor', 'martial', '+7-900-200-0005', 'i.martial@fitlife.ru', '2020-09-01', 1300.00, 'full_time', 'advanced', '2025-09-01', 'boxing and mma trainer', 4.70, 8),
(4, 'sophia', 'dance', '+7-900-200-0006', 's.dance@fitlife.ru', '2023-04-10', 1000.00, 'part_time', 'intermediate', '2025-04-10', 'zumba and dance fitness', 4.88, 6),
(2, 'denis', 'swim', '+7-900-200-0007', 'd.swim@fitlife.ru', '2021-11-01', 1250.00, 'full_time', 'advanced', '2025-11-01', 'swimming and aqua aerobics', 4.82, 7),
(5, 'kristina', 'wellness', '+7-900-200-0008', 'k.wellness@fitlife.ru', '2022-08-15', 1150.00, 'part_time', 'intermediate', '2025-08-15', 'wellness and meditation coach', 4.78, 5);
go

insert into specializations (specialization_name, description, category, difficulty_level, required_certification) values
('crossfit', 'high intensity functional training', 'strength', 'advanced', 'crossfit level 2'),
('yoga', 'flexibility and mindfulness', 'mind-body', 'beginner', 'yoga alliance 200h'),
('pilates', 'core strength and posture', 'mind-body', 'intermediate', 'pilates mat certification'),
('hiit', 'high intensity interval training', 'cardio', 'intermediate', 'ace group fitness'),
('boxing', 'combat sports training', 'martial arts', 'advanced', 'boxing coach license'),
('swimming', 'aquatic fitness', 'cardio', 'intermediate', 'lifeguard + swim coach'),
('bodybuilding', 'muscle hypertrophy training', 'strength', 'advanced', 'nasm cpt'),
('zumba', 'dance fitness', 'cardio', 'beginner', 'zumba basic 1'),
('personal training', 'one-on-one coaching', 'general', 'advanced', 'nasm cpt'),
('rehabilitation', 'injury recovery exercises', 'therapy', 'advanced', 'physical therapy aide');
go

insert into trainer_specializations (trainer_id, specialization_id, certified_date, certification_number, expiry_date) values
(1, 1, '2020-03-01', 'cf-l2-001', '2026-03-01'),
(1, 7, '2020-03-01', 'nasm-001', '2025-03-01'),
(2, 2, '2021-06-15', 'ya-200-002', '2025-06-15'),
(2, 3, '2021-06-15', 'pil-002', '2025-06-15'),
(3, 7, '2019-01-10', 'nasm-003', '2025-01-10'),
(3, 9, '2019-01-10', 'pt-003', '2025-01-10'),
(4, 4, '2022-02-20', 'ace-004', '2025-02-20'),
(5, 5, '2020-09-01', 'box-005', '2025-09-01'),
(6, 8, '2023-04-10', 'zmb-006', '2025-04-10'),
(7, 6, '2021-11-01', 'swim-007', '2025-11-01'),
(8, 2, '2022-08-15', 'ya-200-008', '2025-08-15'),
(8, 10, '2022-08-15', 'rehab-008', '2025-08-15');
go

insert into fitness_classes (branch_id, class_name, description, category, difficulty_level, max_capacity, duration_minutes, room_name, equipment_required, calories_burn_estimate) values
(1, 'morning crossfit', 'intensive functional workout', 'strength', 'advanced', 15, 60, 'studio a', 'barbell, kettlebell', 600),
(1, 'gentle yoga', 'relaxing yoga session', 'mind-body', 'beginner', 25, 60, 'studio b', 'yoga mat', 200),
(1, 'hiit blast', 'high intensity intervals', 'cardio', 'intermediate', 20, 45, 'studio a', 'none', 500),
(2, 'power lifting', 'strength training focus', 'strength', 'advanced', 12, 90, 'gym floor', 'barbell, rack', 700),
(2, 'aqua aerobics', 'pool workout', 'cardio', 'beginner', 20, 45, 'pool', 'pool noodles', 350),
(3, 'boxing basics', 'intro to boxing', 'martial arts', 'beginner', 16, 60, 'studio c', 'gloves, pads', 450),
(3, 'pilates core', 'core strengthening', 'mind-body', 'intermediate', 18, 55, 'studio a', 'pilates ball', 300),
(4, 'zumba party', 'dance fitness fun', 'cardio', 'beginner', 30, 60, 'studio b', 'none', 400),
(1, 'evening stretch', 'flexibility and recovery', 'mind-body', 'beginner', 20, 45, 'studio b', 'yoga mat', 150),
(5, 'meditation flow', 'mindfulness and breathing', 'mind-body', 'beginner', 15, 30, 'wellness room', 'cushion', 100);
go

insert into class_schedules (class_id, trainer_id, start_time, end_time, status, current_enrollment, notes) values
(1, 1, '2025-06-20 07:00:00', '2025-06-20 08:00:00', 'scheduled', 12, null),
(1, 1, '2025-06-21 07:00:00', '2025-06-21 08:00:00', 'scheduled', 10, null),
(2, 2, '2025-06-20 09:00:00', '2025-06-20 10:00:00', 'scheduled', 18, null),
(2, 2, '2025-06-22 09:00:00', '2025-06-22 10:00:00', 'scheduled', 15, null),
(3, 4, '2025-06-20 18:00:00', '2025-06-20 18:45:00', 'scheduled', 16, null),
(4, 3, '2025-06-20 10:00:00', '2025-06-20 11:30:00', 'scheduled', 8, null),
(5, 7, '2025-06-21 11:00:00', '2025-06-21 11:45:00', 'scheduled', 14, null),
(6, 5, '2025-06-20 17:00:00', '2025-06-20 18:00:00', 'scheduled', 10, null),
(7, 2, '2025-06-21 16:00:00', '2025-06-21 16:55:00', 'scheduled', 12, null),
(8, 6, '2025-06-20 19:00:00', '2025-06-20 20:00:00', 'scheduled', 22, null),
(9, 2, '2025-06-20 20:00:00', '2025-06-20 20:45:00', 'scheduled', 8, null),
(10, 8, '2025-06-22 08:00:00', '2025-06-22 08:30:00', 'scheduled', 6, null),
(3, 4, '2025-06-21 18:00:00', '2025-06-21 18:45:00', 'scheduled', 14, null),
(1, 1, '2025-06-22 07:00:00', '2025-06-22 08:00:00', 'scheduled', 11, null),
(6, 5, '2025-06-22 17:00:00', '2025-06-22 18:00:00', 'scheduled', 9, null),
(4, 3, '2025-06-21 10:00:00', '2025-06-21 11:30:00', 'scheduled', 7, null),
(8, 6, '2025-06-21 19:00:00', '2025-06-21 20:00:00', 'scheduled', 20, null),
(5, 7, '2025-06-22 11:00:00', '2025-06-22 11:45:00', 'scheduled', 11, null),
(7, 2, '2025-06-22 16:00:00', '2025-06-22 16:55:00', 'scheduled', 10, null),
(9, 2, '2025-06-21 20:00:00', '2025-06-21 20:45:00', 'scheduled', 7, null),
(10, 8, '2025-06-23 08:00:00', '2025-06-23 08:30:00', 'scheduled', 5, null),
(2, 2, '2025-06-23 09:00:00', '2025-06-23 10:00:00', 'scheduled', 14, null),
(3, 4, '2025-06-22 18:00:00', '2025-06-22 18:45:00', 'scheduled', 13, null),
(1, 1, '2025-06-23 07:00:00', '2025-06-23 08:00:00', 'scheduled', 9, null),
(6, 5, '2025-06-23 17:00:00', '2025-06-23 18:00:00', 'scheduled', 8, null),
(4, 3, '2025-06-22 10:00:00', '2025-06-22 11:30:00', 'completed', 10, 'completed session'),
(8, 6, '2025-06-19 19:00:00', '2025-06-19 20:00:00', 'completed', 25, 'full class'),
(2, 2, '2025-06-19 09:00:00', '2025-06-19 10:00:00', 'completed', 20, null),
(3, 4, '2025-06-19 18:00:00', '2025-06-19 18:45:00', 'cancelled', 0, 'trainer sick'),
(1, 1, '2025-06-19 07:00:00', '2025-06-19 08:00:00', 'completed', 14, null);
go

insert into class_enrollments (schedule_id, member_id, enrollment_date, status, check_in_time) values
(1, 1, '2025-06-18 10:00:00', 'enrolled', null),
(1, 7, '2025-06-18 11:00:00', 'enrolled', null),
(1, 15, '2025-06-18 12:00:00', 'enrolled', null),
(3, 2, '2025-06-18 09:00:00', 'enrolled', null),
(3, 8, '2025-06-18 10:00:00', 'enrolled', null),
(3, 16, '2025-06-18 11:00:00', 'enrolled', null),
(5, 3, '2025-06-18 14:00:00', 'enrolled', null),
(5, 11, '2025-06-18 15:00:00', 'enrolled', null),
(8, 5, '2025-06-18 16:00:00', 'enrolled', null),
(8, 19, '2025-06-18 17:00:00', 'enrolled', null),
(10, 4, '2025-06-18 18:00:00', 'enrolled', null),
(10, 10, '2025-06-18 19:00:00', 'enrolled', null),
(10, 18, '2025-06-18 20:00:00', 'enrolled', null),
(26, 1, '2025-06-17 10:00:00', 'attended', '2025-06-19 06:55:00'),
(26, 7, '2025-06-17 11:00:00', 'attended', '2025-06-19 06:58:00'),
(28, 2, '2025-06-17 09:00:00', 'attended', '2025-06-19 08:55:00'),
(28, 8, '2025-06-17 10:00:00', 'attended', '2025-06-19 08:57:00'),
(27, 4, '2025-06-17 18:00:00', 'attended', '2025-06-19 18:55:00'),
(27, 10, '2025-06-17 19:00:00', 'attended', '2025-06-19 18:58:00'),
(6, 3, '2025-06-18 08:00:00', 'enrolled', null),
(6, 17, '2025-06-18 09:00:00', 'enrolled', null),
(7, 6, '2025-06-18 10:00:00', 'enrolled', null),
(7, 14, '2025-06-18 11:00:00', 'enrolled', null),
(9, 12, '2025-06-18 12:00:00', 'enrolled', null),
(9, 20, '2025-06-18 13:00:00', 'enrolled', null),
(11, 9, '2025-06-18 14:00:00', 'enrolled', null),
(11, 13, '2025-06-18 15:00:00', 'enrolled', null),
(12, 21, '2025-06-18 16:00:00', 'enrolled', null),
(12, 25, '2025-06-18 17:00:00', 'enrolled', null),
(13, 22, '2025-06-18 18:00:00', 'enrolled', null),
(14, 23, '2025-06-18 19:00:00', 'enrolled', null);
go

insert into equipment (branch_id, equipment_name, category, serial_number, purchase_date, purchase_price, warranty_until, location, condition_status, last_maintenance_date, manufacturer) values
(1, 'treadmill pro x500', 'cardio', 'tm-x500-001', '2022-01-15', 350000.00, '2025-01-15', 'cardio zone', 'good', '2025-03-01', 'technogym'),
(1, 'treadmill pro x500', 'cardio', 'tm-x500-002', '2022-01-15', 350000.00, '2025-01-15', 'cardio zone', 'good', '2025-03-01', 'technogym'),
(1, 'leg press machine', 'strength', 'lp-200-001', '2021-06-01', 450000.00, '2024-06-01', 'strength zone', 'fair', '2024-12-01', 'hammer strength'),
(1, 'cable crossover', 'strength', 'cc-100-001', '2021-06-01', 280000.00, '2024-06-01', 'strength zone', 'good', '2025-01-15', 'life fitness'),
(2, 'rowing machine', 'cardio', 'rm-300-001', '2023-03-10', 180000.00, '2026-03-10', 'cardio zone', 'good', '2025-02-01', 'concept2'),
(2, 'smith machine', 'strength', 'sm-150-001', '2020-09-01', 320000.00, '2023-09-01', 'strength zone', 'fair', '2024-11-01', 'precor'),
(3, 'elliptical trainer', 'cardio', 'et-400-001', '2022-11-20', 220000.00, '2025-11-20', 'cardio zone', 'good', '2025-04-01', 'matrix'),
(3, 'boxing bag stand', 'martial arts', 'bb-050-001', '2023-01-05', 45000.00, '2026-01-05', 'studio c', 'good', '2025-01-01', 'everlast'),
(4, 'spin bike', 'cardio', 'sb-600-001', '2023-06-15', 95000.00, '2026-06-15', 'cycle studio', 'good', '2025-05-01', 'schwinn'),
(4, 'spin bike', 'cardio', 'sb-600-002', '2023-06-15', 95000.00, '2026-06-15', 'cycle studio', 'good', '2025-05-01', 'schwinn'),
(5, 'multi gym station', 'strength', 'mg-800-001', '2021-04-01', 550000.00, '2024-04-01', 'strength zone', 'out_of_service', '2024-08-01', 'technogym'),
(1, 'dumbbell set 5-50kg', 'free weights', 'db-set-001', '2020-01-01', 120000.00, null, 'free weights area', 'good', '2025-02-01', 'rogue'),
(2, 'pool lane divider', 'aquatic', 'pl-010-001', '2022-05-01', 35000.00, null, 'pool', 'good', '2024-10-01', 'arena'),
(3, 'yoga mat rack', 'accessories', 'ym-rack-001', '2023-02-01', 15000.00, null, 'studio a', 'good', null, 'manduka'),
(1, 'bench press station', 'strength', 'bp-100-001', '2021-08-01', 85000.00, '2024-08-01', 'strength zone', 'good', '2025-01-20', 'rogue');
go

insert into equipment_maintenance (equipment_id, maintenance_date, maintenance_type, performed_by, cost, description, next_maintenance_date, status, vendor_name) values
(1, '2025-03-01', 'preventive', 1, 5000.00, 'belt lubrication and calibration', '2025-09-01', 'completed', 'technogym service'),
(2, '2025-03-01', 'preventive', 1, 5000.00, 'belt lubrication and calibration', '2025-09-01', 'completed', 'technogym service'),
(3, '2024-12-01', 'repair', 3, 15000.00, 'hydraulic system repair', '2025-06-01', 'completed', 'hammer strength service'),
(6, '2024-11-01', 'preventive', 3, 8000.00, 'guide rail inspection', '2025-05-01', 'completed', 'precor service'),
(11, '2024-08-01', 'emergency', null, 45000.00, 'cable replacement needed', '2025-08-01', 'pending', 'technogym service'),
(7, '2025-04-01', 'preventive', 4, 3500.00, 'resistance calibration', '2025-10-01', 'completed', 'matrix service'),
(5, '2025-02-01', 'preventive', 3, 2000.00, 'chain and flywheel check', '2025-08-01', 'completed', null),
(12, '2025-02-01', 'inspection', 1, 0.00, 'weight accuracy check', '2025-08-01', 'completed', null),
(15, '2025-01-20', 'preventive', 1, 3000.00, 'pad replacement', '2025-07-20', 'completed', null),
(8, '2025-01-01', 'inspection', 5, 0.00, 'mounting stability check', '2025-07-01', 'completed', null);
go

insert into payments (member_id, membership_id, payment_date, amount, payment_method, transaction_reference, status, processed_by, notes) values
(1, 2, '2024-02-10 10:00:00', 4500.00, 'card', 'txn-001', 'completed', 2, 'standard monthly renewal'),
(2, 2, '2024-02-15 11:30:00', 6750.00, 'card', 'txn-002', 'completed', 2, 'premium with 10% discount'),
(3, 5, '2024-06-01 09:00:00', 71250.00, 'bank_transfer', 'txn-003', 'completed', 2, 'annual premium renewal'),
(4, 6, '2024-04-20 14:00:00', 4500.00, 'card', 'txn-004', 'completed', 2, null),
(5, 7, '2024-04-05 16:00:00', 4500.00, 'cash', 'txn-005', 'completed', 2, null),
(6, 8, '2023-11-10 10:00:00', 25000.00, 'bank_transfer', 'txn-006', 'completed', 2, 'annual basic'),
(7, 11, '2024-06-01 12:00:00', 7500.00, 'card', 'txn-007', 'completed', 2, 'premium upgrade'),
(8, 13, '2024-02-25 13:00:00', 4500.00, 'card', 'txn-008', 'completed', 2, null),
(9, 15, '2024-07-10 15:00:00', 1700.00, 'card', 'txn-009', 'completed', 2, 'student discount 15%'),
(10, 17, '2024-09-15 10:30:00', 7500.00, 'card', 'txn-010', 'completed', 2, null),
(11, 18, '2024-02-28 11:00:00', 4500.00, 'cash', 'txn-011', 'completed', 2, null),
(12, 20, '2024-08-01 09:30:00', 4500.00, 'card', 'txn-012', 'completed', 2, null),
(13, 21, '2024-03-15 14:30:00', 4500.00, 'card', 'txn-013', 'completed', 2, null),
(14, 22, '2023-12-20 16:00:00', 7500.00, 'bank_transfer', 'txn-014', 'completed', 2, null),
(15, 24, '2024-09-01 10:00:00', 4500.00, 'card', 'txn-015', 'completed', 2, null),
(16, 25, '2024-04-20 11:00:00', 4500.00, 'card', 'txn-016', 'completed', 2, null),
(17, 26, '2024-01-05 09:00:00', 25000.00, 'bank_transfer', 'txn-017', 'completed', 2, null),
(18, 27, '2024-05-15 13:00:00', 4500.00, 'card', 'txn-018', 'completed', 2, null),
(19, 30, '2024-07-27 15:00:00', 4500.00, 'card', 'txn-019', 'completed', 2, null),
(1, null, '2024-06-15 12:00:00', 1500.00, 'cash', 'txn-020', 'completed', 2, 'personal training session'),
(3, null, '2024-07-01 10:00:00', 3000.00, 'card', 'txn-021', 'completed', 2, 'pt package 2 sessions'),
(10, null, '2024-10-01 14:00:00', 500.00, 'cash', 'txn-022', 'completed', 2, 'protein shake bar'),
(6, null, '2024-03-01 11:00:00', 2000.00, 'card', 'txn-023', 'completed', 2, 'locker rental yearly'),
(14, null, '2024-06-01 16:00:00', 1500.00, 'card', 'txn-024', 'completed', 2, 'pt session'),
(2, null, '2024-08-01 09:00:00', 800.00, 'cash', 'txn-025', 'completed', 2, 'supplements purchase'),
(7, null, '2024-09-01 10:00:00', 1500.00, 'card', 'txn-026', 'completed', 2, 'pt session'),
(4, null, '2024-05-01 13:00:00', 500.00, 'cash', 'txn-027', 'completed', 2, 'day pass guest'),
(11, null, '2024-04-01 15:00:00', 1200.00, 'card', 'txn-028', 'completed', 2, 'fitness assessment'),
(20, null, '2024-03-01 11:30:00', 2000.00, 'card', 'txn-029', 'completed', 2, 'locker rental'),
(5, null, '2024-06-01 14:30:00', 1500.00, 'cash', 'txn-030', 'completed', 2, 'pt session');
go

print 'fitnesscenter_db deployed successfully.';
go
