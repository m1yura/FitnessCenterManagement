using Microsoft.Extensions.Configuration;

namespace FitnessCenterManagement.Configuration;

/// <summary>
/// Loads application configuration from appsettings.json.
/// </summary>
public static class AppSettings
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    public static string ConnectionString =>
        Configuration.GetConnectionString("FitnessCenterDb")
        ?? throw new InvalidOperationException("Connection string 'FitnessCenterDb' is not configured.");

    public static int PageSize =>
        int.TryParse(Configuration["AppSettings:PageSize"], out var size) ? size : 20;

    public static string DateFormat =>
        Configuration["AppSettings:DateFormat"] ?? "yyyy-MM-dd HH:mm";
}
