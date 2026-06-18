namespace AutoServiceManagement.Models;

public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? RoleDescription { get; set; }
    public int PermissionLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public DateTime? LastLogin { get; set; }
    public int LoginAttempts { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class Customer
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public int LoyaltyPoints { get; set; }
    public string? PreferredContact { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class Vehicle
{
    public int VehicleId { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int Mileage { get; set; }
    public string? EngineType { get; set; }
    public string? FuelType { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class Employee
{
    public int EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public decimal HourlyRate { get; set; }
    public string? Specialization { get; set; }
    public string? CertificationLevel { get; set; }
    public string? EmergencyContact { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class ServiceCategory
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string? IconName { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class ServiceItem
{
    public int ServiceId { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public string? SkillLevelRequired { get; set; }
    public int WarrantyDays { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class Supplier
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? Rating { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class Part
{
    public int PartId { get; set; }
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int QuantityInStock { get; set; }
    public int ReorderLevel { get; set; }
    public string? WarehouseLocation { get; set; }
    public decimal? WeightKg { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class Appointment
{
    public int AppointmentId { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int VehicleId { get; set; }
    public string? VehicleInfo { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = "scheduled";
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public bool ReminderSent { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class WorkOrder
{
    public int WorkOrderId { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int VehicleId { get; set; }
    public string? VehicleInfo { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int? AppointmentId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "open";
    public string Priority { get; set; } = "normal";
    public DateTime OpenedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public string? DiagnosisNotes { get; set; }
    public string? CustomerComplaint { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class WorkOrderService
{
    public int WorkOrderServiceId { get; set; }
    public int WorkOrderId { get; set; }
    public int ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
    public int? PerformedBy { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class WorkOrderPart
{
    public int WorkOrderPartId { get; set; }
    public int WorkOrderId { get; set; }
    public int PartId { get; set; }
    public string? PartName { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public int? InstalledBy { get; set; }
    public DateTime? WarrantyUntil { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class Payment
{
    public int PaymentId { get; set; }
    public int WorkOrderId { get; set; }
    public string? OrderNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public string Status { get; set; } = "completed";
    public int? ProcessedBy { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class PartSupply
{
    public int SupplyId { get; set; }
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int PartId { get; set; }
    public string? PartName { get; set; }
    public DateTime SupplyDate { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? InvoiceNumber { get; set; }
    public int? ReceivedBy { get; set; }
    public string Status { get; set; } = "received";
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class AuditLogEntry
{
    public long AuditId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int RecordId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class ReportRow
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public decimal? NumericValue { get; set; }
}
