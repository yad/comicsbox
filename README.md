# ComicsBox

ASP.NET Core MVC Application for managing and downloading comic book PDFs.

## Features

- Browse comic categories (e.g., Mangas, Comics DC) stored in configured paths
- View series within each category
- View and download PDF files

## Prerequisites

- .NET 10 SDK installed.
- Comic files organized in configured paths, e.g., X:\eBooks\Mangas\SeriesName\*.pdf and X:\eBooks\Comics DC\SeriesName\*.pdf

## Configuration

Edit appsettings.json to add or modify categories:

```json
"BookCategories": [
  {
    "Name": "Mangas",
    "Path": "X:\\eBooks\\Mangas"
  },
  {
    "Name": "Comics DC",
    "Path": "X:\\eBooks\\Comics DC"
  }
]
```

## How to Run

For development with hot reload: `dotnet watch run`

Alternatively:
1. Open the project in Visual Studio Code.
2. To build: Run the "build" task (Ctrl+Shift+P > Tasks: Run Task > build).
3. To run: Execute `dotnet run` in the terminal or use the debugger.

The application will start on https://localhost:5001 (or similar).

## Project Structure

- Controllers/: MVC controllers
- Models/: Data models
- Views/: Razor views
- wwwroot/: Static files
- appsettings.json: Configuration (includes MangaPath)