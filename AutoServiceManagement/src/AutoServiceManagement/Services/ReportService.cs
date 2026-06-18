using AutoServiceManagement.Data;
using AutoServiceManagement.Models;
using Microsoft.Data.SqlClient;

namespace AutoServiceManagement.Services;

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
    /// Active work orders summary by status.
    /// </summary>
    public List<ReportRow> GetWorkOrdersByStatus()
    {
        const string sql = @"select status, count(*) as cnt, sum(total_amount) as total
                             from vw_active_work_orders
                             group by status
                             order by cnt desc";

        using var reader = _db.ExecuteReader(sql);
        var rows = new List<ReportRow>();
        while (reader.Read())
        {
            rows.Add(new ReportRow
            {
                Label = reader.GetString(0),
                NumericValue = reader.GetDecimal(2),
                Value = $"{reader.GetInt32(1)} orders, total {reader.GetDecimal(2):N2}"
            });
        }

        return rows;
    }

    /// <summary>
    /// Inventory status report from view.
    /// </summary>
    public List<ReportRow> GetInventoryStatusSummary()
    {
        const string sql = @"select stock_status, count(*) as cnt
                             from vw_inventory_status
                             group by stock_status
                             order by stock_status";

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
    /// Employee workload report.
    /// </summary>
    public List<ReportRow> GetEmployeeWorkload()
    {
        const string sql = @"select employee_name, open_work_orders, upcoming_appointments
                             from vw_employee_workload
                             order by open_work_orders desc, upcoming_appointments desc";

        using var reader = _db.ExecuteReader(sql);
        var rows = new List<ReportRow>();
        while (reader.Read())
        {
            rows.Add(new ReportRow
            {
                Label = reader.GetString(0),
                Value = $"work orders: {reader.GetInt32(1)}, appointments: {reader.GetInt32(2)}"
            });
        }

        return rows;
    }

    /// <summary>
    /// Top customers by loyalty points and work order count.
    /// </summary>
    public List<ReportRow> GetTopCustomers(int top = 10)
    {
        const string sql = @"select top (@top) c.first_name + ' ' + c.last_name as customer_name,
                                    c.loyalty_points, count(wo.work_order_id) as order_count
                             from customers c
                             left join work_orders wo on c.customer_id = wo.customer_id and wo.isdeleted = 0
                             where c.isdeleted = 0
                             group by c.customer_id, c.first_name, c.last_name, c.loyalty_points
                             order by c.loyalty_points desc, order_count desc";

        using var reader = _db.ExecuteReader(sql, new SqlParameter("@top", top));
        var rows = new List<ReportRow>();
        while (reader.Read())
        {
            rows.Add(new ReportRow
            {
                Label = reader.GetString(0),
                Value = $"loyalty: {reader.GetInt32(1)}, orders: {reader.GetInt32(2)}"
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
