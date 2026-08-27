# Developer Guide

## Prerequisites

- .NET 10.0 SDK or later
- Visual Studio Code (or Visual Studio) with C# extension

## Building the Project

### Build from Command Line

```bash
dotnet build
```

This will compile the entire solution including the main API project and test projects.

### Build in Visual Studio Code

Use the integrated build task:
- Press `Ctrl+Shift+B` to run the build task, or
- Open the Command Palette (`Ctrl+Shift+P`) and select "Tasks: Run Build Task"

## Running Tests

### Run All Tests

```bash
dotnet test
```

## Development Workflow

### Watch Mode (Auto-rebuild and run on file changes)

```bash
dotnet watch run
```

Or use the VS Code watch task:
- Open the Command Palette and select "Tasks: Run Task"
- Choose "watch" to start the watch mode

This will automatically rebuild and restart the application when you save changes.

## Project Structure

```
src/
├── DotnetBaseApi/           # Main API project
│   ├── Controllers/         # API endpoint controllers
│   ├── Models/              # Data models
│   ├── Services/            # Business logic services
│   ├── Filters/             # Custom HTTP filters (e.g., ETag)
│   ├── Program.cs           # Application startup configuration
│   └── appsettings.json     # Configuration file

test/
└── DotnetBaseApi.Tests/     # Unit tests
    ├── Controllers/         # Controller tests
    └── Filters/             # Filter and utility tests
```

## Useful Commands

| Command | Description |
|---------|-------------|
| `dotnet clean` | Remove build artifacts |
| `dotnet restore` | Restore NuGet dependencies |
| `dotnet build --configuration Release` | Build in Release mode |
| `dotnet test --help` | View all test command options |

## Troubleshooting

### Build Fails
- Run `dotnet clean` to remove build artifacts
- Run `dotnet restore` to restore dependencies
- Check that .NET 10.0 SDK is installed: `dotnet --version`

### Tests Fail
- Ensure all dependencies are installed: `dotnet restore`
- Check test output for specific error messages
- Run tests with verbose logging: `dotnet test --verbosity detailed`
