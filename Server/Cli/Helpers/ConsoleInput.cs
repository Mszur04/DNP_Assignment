namespace Cli.Helpers;

/// <summary>
/// Small helper for reading and validating user input from the console,
/// so the individual views don't have to repeat parsing/validation logic.
/// </summary>
public static class ConsoleInput
{
    public static string ReadNonEmptyString(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Trim();
            }

            Console.WriteLine("Input cannot be empty. Please try again.");
        }
    }

    /// <summary>
    /// Reads a string that may be left blank (used for "leave blank to keep the current value" updates).
    /// </summary>
    public static string? ReadOptionalString(string prompt)
    {
        Console.Write($"{prompt}: ");
        string? input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? null : input.Trim();
    }

    public static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int value))
            {
                return value;
            }

            Console.WriteLine("Please enter a valid whole number.");
        }
    }

    /// <summary>
    /// Reads an int, but allows leaving the input blank, in which case null is returned.
    /// </summary>
    public static int? ReadOptionalInt(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (int.TryParse(input, out int value))
            {
                return value;
            }

            Console.WriteLine("Please enter a valid whole number, or leave blank.");
        }
    }

    public static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        try
        {
            Console.ReadKey(intercept: true);
        }
        catch (InvalidOperationException)
        {
            // Console input is redirected (e.g. piped input); fall back to reading a line instead.
            Console.ReadLine();
        }
    }
}
