namespace FitnessCenterManagement.Models;

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

public class Branch
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? OpeningHours { get; set; }
    public int Capacity { get; set; }
    public string? ManagerName { get; set; }
    public int ParkingSpaces { get; set; }
    public bool HasPool { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class MembershipType
{
    public int MembershipTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public string AccessLevel { get; set; } = "standard";
    public int MaxClassesPerMonth { get; set; }
    public bool IncludesPersonalTraining { get; set; }
    public int GuestPasses { get; set; }
    public int FreezeDaysAllowed { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class Member
{
    public int MemberId { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? HealthNotes { get; set; }
    public string? ReferralSource { get; set; }
    public string? FitnessGoal { get; set; }
    public int LoyaltyPoints { get; set; }
    public DateTime JoinDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class Membership
{
    public int MembershipId { get; set; }
    public int MemberId { get; set; }
    public string? MemberName { get; set; }
    public int MembershipTypeId { get; set; }
    public string? TypeName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "active";
    public bool AutoRenew { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTime? FreezeStart { get; set; }
    public DateTime? FreezeEnd { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class Trainer
{
    public int TrainerId { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public decimal HourlyRate { get; set; }
    public string EmploymentType { get; set; } = "full_time";
    public string? CertificationLevel { get; set; }
    public DateTime? CertificationExpiry { get; set; }
    public string? Bio { get; set; }
    public decimal? Rating { get; set; }
    public int MaxClientsPerDay { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class Specialization
{
    public int SpecializationId { get; set; }
    public string SpecializationName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? DifficultyLevel { get; set; }
    public string? RequiredCertification { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class TrainerSpecialization
{
    public int TrainerSpecializationId { get; set; }
    public int TrainerId { get; set; }
    public string? TrainerName { get; set; }
    public int SpecializationId { get; set; }
    public string? SpecializationName { get; set; }
    public DateTime CertifiedDate { get; set; }
    public string? CertificationNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class FitnessClass
{
    public int ClassId { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = "beginner";
    public int MaxCapacity { get; set; }
    public int DurationMinutes { get; set; }
    public string? RoomName { get; set; }
    public string? EquipmentRequired { get; set; }
    public int? CaloriesBurnEstimate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class ClassSchedule
{
    public int ScheduleId { get; set; }
    public int ClassId { get; set; }
    public string? ClassName { get; set; }
    public int TrainerId { get; set; }
    public string? TrainerName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = "scheduled";
    public int CurrentEnrollment { get; set; }
    public string? RoomOverride { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class ClassEnrollment
{
    public int EnrollmentId { get; set; }
    public int ScheduleId { get; set; }
    public string? ClassName { get; set; }
    public int MemberId { get; set; }
    public string? MemberName { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public string Status { get; set; } = "enrolled";
    public DateTime? CheckInTime { get; set; }
    public string? CancellationReason { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class Equipment
{
    public int EquipmentId { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime? WarrantyUntil { get; set; }
    public string? Location { get; set; }
    public string ConditionStatus { get; set; } = "good";
    public DateTime? LastMaintenanceDate { get; set; }
    public string? Manufacturer { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class EquipmentMaintenance
{
    public int MaintenanceId { get; set; }
    public int EquipmentId { get; set; }
    public string? EquipmentName { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public int? PerformedBy { get; set; }
    public string? TrainerName { get; set; }
    public decimal Cost { get; set; }
    public string? Description { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string Status { get; set; } = "completed";
    public string? VendorName { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}

public class Payment
{
    public int PaymentId { get; set; }
    public int MemberId { get; set; }
    public string? MemberName { get; set; }
    public int? MembershipId { get; set; }
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
