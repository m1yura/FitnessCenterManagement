using FitnessCenterManagement.UI;

namespace FitnessCenterManagement;

/// <summary>
/// Entry point for the Fitness Center Management console application.
/// </summary>
internal static class Program
{
    private static void Main()
    {
        try
        {
            var menu = new MenuManager();
            menu.Run();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
