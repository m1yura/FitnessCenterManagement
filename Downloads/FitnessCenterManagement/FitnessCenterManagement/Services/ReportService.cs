using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.Data.SqlClient;

namespace FitnessCenterManagement.Services;

/// <summary>
/// Analytics and reporting service using database views and aggregate queries.
/// </summary>
public class ReportService
{
    private readonly DatabaseHelper _db;

    public ReportService(DatabaseHelper db)
    {
        _db = db;
    }

    /// <summary>
    /// Revenue report grouped by payment method.
    /// </summary>
    public List<ReportRow> GetRevenueByPaymentMethod()
    {
        const string sql = @"select payment_method, sum(total_revenue) as total_revenue, sum(payment_count) as payment_count
                             from vw_revenue_summary
                             group by payment_method
                             order by total_revenue desc";

        using var reader = _db.ExecuteReader(sql);
        var rows = new List<ReportRow>();
        while (reader.Read())
        {
            rows.Add(new ReportRow
            {
                Label = reader.GetString(0),
                NumericValue = reader.GetDecimal(1),
                Value = $"{reader.GetDecimal(1):N2} ({reader.GetInt32(2)} payments)"
            });
        }

        return rows;
    }

    /// <summary>
    /// Active memberships summary by status.
    /// </summary>
    public List<ReportRow> GetMembershipsByStatus()
    {
        const string sql = @"select status, count(*) as cnt
                             from vw_active_memberships
                             group by status
                             order by cnt desc";

        using var reader = _db.ExecuteReader(sql);
        var rows = new List<ReportRow>();
        while (reader.Read())
        {
            rows.Add(new ReportRow
            {
                Label = reader.GetString(0),
                Value = reader.GetInt32(1).ToString(),
                NumericValue = reader.GetInt32(1)
            });
        }

        return rows;
    }

    /// <summary>
    /// Class schedule overview by category.
    /// </summary>
    public List<ReportRow> GetClassesByCategory()
    {
        const string sql = @"select category, count(*) as cnt, sum(current_enrollment) as total_enrolled
                             from vw_class_schedule_overview
                             where status = 'scheduled'
                             group by category
                             order by cnt desc";

        using var reader = _db.ExecuteReader(sql);
        var rows = new List<ReportRow>();
        while (reader.Read())
        {
            rows.Add(new ReportRow
            {
                Label = reader.GetString(0),
                Value = $"{reader.GetInt32(1)} classes, {reader.GetInt32(2)} enrolled"
            });
        }

        return rows;
    }

    /// <summary>
    /// Trainer workload report.
    /// </summary>
    public List<ReportRow> GetTrainerWorkload()
    {
        const string sql = @"select trainer_name, upcoming_classes, total_enrollments
                             from vw_trainer_workload
                             order by upcoming_classes desc, total_enrollments desc";

        using var reader = _db.ExecuteReader(sql);
        var rows = new List<ReportRow>();
        while (reader.Read())
        {
            rows.Add(new ReportRow
            {
                Label = reader.GetString(0),
                Value = $"classes: {reader.GetInt32(1)}, enrollments: {reader.GetInt32(2)}"
            });
        }

        return rows;
    }

    /// <summary>
    /// Equipment maintenance status summary.
    /// </summary>
    public List<ReportRow> GetEquipmentStatusSummary()
    {
        const string sql = @"select maintenance_status, count(*) as cnt
                             from vw_equipment_status
                             group by maintenance_status
                             order by maintenance_status";

        using var reader = _db.ExecuteReader(sql);
        var rows = new List<ReportRow>();
        while (reader.Read())
        {
            rows.Add(new ReportRow
            {
                Label = reader.GetString(0),
                Value = reader.GetInt32(1).ToString(),
                NumericValue = reader.GetInt32(1)
            });
        }

        return rows;
    }

    /// <summary>
    /// Top members by loyalty points.
    /// </summary>
    public List<ReportRow> GetTopMembers(int top = 10)
    {
        const string sql = @"select top (@top) m.first_name + ' ' + m.last_name as member_name,
                                    m.loyalty_points, count(ms.membership_id) as membership_count
                             from members m
                             left join memberships ms on m.member_id = ms.member_id and ms.isdeleted = 0
                             where m.isdeleted = 0
                             group by m.member_id, m.first_name, m.last_name, m.loyalty_points
                             order by m.loyalty_points desc, membership_count desc";

        using var reader = _db.ExecuteReader(sql, new SqlParameter("@top", top));
        var rows = new List<ReportRow>();
        while (reader.Read())
        {
            rows.Add(new ReportRow
            {
                Label = reader.GetString(0),
                Value = $"loyalty: {reader.GetInt32(1)}, memberships: {reader.GetInt32(2)}"
            });
        }

        return rows;
    }

    /// <summary>
    /// Monthly revenue trend.
    /// </summary>
    public List<ReportRow> GetMonthlyRevenue()
    {
        const string sql = @"select format(payment_day, 'yyyy-MM') as month_key, sum(total_revenue) as revenue
                             from vw_revenue_summary
                             group by format(payment_day, 'yyyy-MM')
                             order by month_key";

        using var reader = _db.ExecuteReader(sql);
        var rows = new List<ReportRow>();
        while (reader.Read())
        {
            rows.Add(new ReportRow
            {
                Label = reader.GetString(0),
                NumericValue = reader.GetDecimal(1),
                Value = reader.GetDecimal(1).ToString("N2")
            });
        }

        return rows;
    }
}
