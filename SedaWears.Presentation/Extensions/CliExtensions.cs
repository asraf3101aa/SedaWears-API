using System.CommandLine;
using SedaWears.Infrastructure.Persistence.Seeds;

namespace SedaWears.Presentation.Extensions;

public static class CliExtensions
{
    public static void SetupCliCommands(this RootCommand rootCommand, WebApplication app)
    {
        var createAdminCommand = new Command("create-admin", "Create an admin user");
        var emailOpt = new Option<string>("--email", "Admin email") { IsRequired = true };
        var passwordOpt = new Option<string>("--password", "Admin password") { IsRequired = true };
        var firstNameOpt = new Option<string>("--first-name", "Admin first name") { IsRequired = true };
        var lastNameOpt = new Option<string>("--last-name", "Admin last name") { IsRequired = true };

        createAdminCommand.AddOption(emailOpt);
        createAdminCommand.AddOption(passwordOpt);
        createAdminCommand.AddOption(firstNameOpt);
        createAdminCommand.AddOption(lastNameOpt);

        createAdminCommand.SetHandler(async (email, password, firstName, lastName) =>
        {
            Console.WriteLine("--- Create Admin User ---");
            email = Prompt("Email", email, v => !string.IsNullOrWhiteSpace(v) && v.Contains("@"));
            firstName = Prompt("First Name", firstName, v => !string.IsNullOrWhiteSpace(v));
            lastName = Prompt("Last Name", lastName, v => !string.IsNullOrWhiteSpace(v));
            password = Prompt("Password", password, v => !string.IsNullOrWhiteSpace(v), isPassword: true);

            await AdminSeeder.CreateAdminAsync(app.Services, email!, password!, firstName!, lastName!);
        }, emailOpt, passwordOpt, firstNameOpt, lastNameOpt);

        var seedRolesCommand = new Command("seed-roles", "Seed initial roles");
        seedRolesCommand.SetHandler(async () =>
        {
            Console.WriteLine("--- Seeding Roles ---");
            await RoleSeeder.SeedRolesAsync(app.Services);
        });

        rootCommand.AddCommand(createAdminCommand);
        rootCommand.AddCommand(seedRolesCommand);
    }

    private static string Prompt(string label, string? defaultValue, Func<string, bool> validator, bool isPassword = false)
    {
        if (!string.IsNullOrEmpty(defaultValue) && validator(defaultValue)) return defaultValue;
        while (true)
        {
            Console.Write($"{label}: ");
            string? input;
            if (isPassword)
            {
                input = ReadPassword();
                Console.WriteLine();
            }
            else input = Console.ReadLine();

            if (input != null && validator(input)) return input;
            Console.WriteLine($"Invalid {label}. Please try again.");
        }
    }

    private static string ReadPassword()
    {
        var pass = string.Empty;
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                pass += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
            {
                pass = pass[..^1];
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);
        return pass;
    }
}
