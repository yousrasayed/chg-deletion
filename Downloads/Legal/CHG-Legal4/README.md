# CHG-Legal

A Razor Pages web application in this workspace for the Cleopatra Hospitals legal portal.

## Known technologies

- .NET 8 (target framework)
- ASP.NET Core Razor Pages
- C#
- HTML, CSS
- Bootstrap 5.3 (RTL stylesheet referenced in views)
- jQuery 3.7.1
- jQuery Validate / jQuery Unobtrusive Validation
- Font Awesome (icons)
- Visual Studio 2026 (development environment used in this workspace)

Note: The list above reflects technologies observed in the repository (views and project targets). If you use additional libraries (Entity Framework Core, third-party packages, authentication providers, etc.) add them here.

## Prerequisites

- .NET 8 SDK installed: https://dotnet.microsoft.com
- Visual Studio 2026 (recommended) or VS Code with C# extensions

## Build and run

From the repository root or the project folder that contains the `.csproj` file:

1. Restore and build

   ```powershell
   dotnet restore
   dotnet build
   ```

2. Run the app

   ```powershell
   dotnet run --project ./projects/CHG-Legal/CHG-Legal.csproj
   ```

Open the URL shown in the console (usually `https://localhost:5001` or similar).

Alternatively, open the solution in Visual Studio and run with IIS Express or Kestrel.

## Configuration

- Application configuration is typically in `appsettings.json` in the project folder. Add connection strings or secrets as needed.
- For environment-specific settings, use `appsettings.Development.json`, environment variables or Secret Manager.

## Common troubleshooting

- CS0246: `The type or namespace name 'LoginViewModel' could not be found`
  - Ensure a `LoginViewModel` class exists and is compiled by the project. Example:

    ```csharp
    // File: Models/LoginViewModel.cs
    using System.ComponentModel.DataAnnotations;

    namespace CHG_Legal.ViewModels
    {
        public class LoginViewModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }
    }
    ```

  - Reference the full type in the Razor view or add the namespace to `_ViewImports.cshtml`:

    ```razor
    @using CHG_Legal.ViewModels
    @model LoginViewModel
    ```

  - If the view model is in another project, add a project reference to the web project and import the namespace.

- If static assets (CSS/JS) are not loading, verify CDN links in the views or ensure local files exist in `wwwroot`.

## Project structure (example)

- `projects/CHG-Legal/` - web project
- `projects/CHG-Legal/Views/` - Razor views
- `projects/CHG-Legal/wwwroot/` - static assets

Adjust paths to match your repository layout.

## Contributing

- Use branches for features and fixes.
- Follow repository coding conventions.

## License

Add your project license here (for example, MIT).