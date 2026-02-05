using Dragonwright.Seeder;
using Dragonwright.Seeder.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("appsettings.local.json", optional: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

// Parse command line arguments
var cliArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();
var options = ParseOptions(cliArgs);

if (options.ShowHelp)
{
    ShowHelp();
    return 0;
}

// Get connection string
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? "Host=localhost;Database=dragonwright;Username=postgres;Password=postgres";

// Setup DbContext
var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseNpgsql(connectionString);

await using var context = new AppDbContext(optionsBuilder.Options);

// Ensure database exists
Console.WriteLine("Connecting to database...");
await context.Database.EnsureCreatedAsync();

// Get SRD path
var srdPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".srd");
if (!Directory.Exists(srdPath))
{
    srdPath = Path.Combine(Directory.GetCurrentDirectory(), ".srd");
}
if (!Directory.Exists(srdPath))
{
    // Try relative to solution root
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Dragonwright.sln")))
    {
        dir = dir.Parent;
    }
    if (dir != null)
    {
        srdPath = Path.Combine(dir.FullName, ".srd");
    }
}

if (!Directory.Exists(srdPath))
{
    Console.WriteLine($"Error: SRD directory not found. Looked in: {srdPath}");
    return 1;
}

Console.WriteLine($"Using SRD data from: {srdPath}");

// Create and run seeder
var fileReader = new SrdFileReader(srdPath);
var seeder = new SrdSeeder(context, fileReader);

try
{
    await seeder.SeedAsync(options);
    Console.WriteLine("\nSeeding completed successfully!");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"\nError during seeding: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    return 1;
}

static SeederOptions ParseOptions(string[] arguments)
{
    var options = new SeederOptions();

    for (int i = 0; i < arguments.Length; i++)
    {
        switch (arguments[i].ToLowerInvariant())
        {
            case "--help":
            case "-h":
                options.ShowHelp = true;
                break;
            case "--2014":
                options.Seed2014 = true;
                break;
            case "--2024":
                options.Seed2024 = true;
                break;
            case "--all":
                options.Seed2014 = true;
                options.Seed2024 = true;
                break;
            case "--clean":
                options.Clean = true;
                break;
            case "--entity":
                if (i + 1 < arguments.Length)
                {
                    options.EntityFilter = arguments[++i].ToLowerInvariant();
                }
                break;
        }
    }

    // Default to all if nothing specified
    if (!options.Seed2014 && !options.Seed2024)
    {
        options.Seed2014 = true;
        options.Seed2024 = true;
    }

    return options;
}

static void ShowHelp()
{
    Console.WriteLine("""
        Dragonwright SRD Seeder

        Usage: dotnet run --project Dragonwright.Seeder -- [options]

        Options:
          --2014          Seed only 2014 content
          --2024          Seed only 2024 content
          --all           Seed both 2014 and 2024 content (default)
          --entity <type> Seed only specific entity type:
                          languages, items, spells, classes, races, backgrounds, feats
          --clean         Delete existing SRD content before seeding (use with caution)
          --help, -h      Show this help message

        Examples:
          dotnet run --project Dragonwright.Seeder
          dotnet run --project Dragonwright.Seeder -- --2014
          dotnet run --project Dragonwright.Seeder -- --entity languages
          dotnet run --project Dragonwright.Seeder -- --clean --all
        """);
}
