# OpenRouter API in ASP.NET Core

Example ASP.NET Core Web API project that integrates with [OpenRouter](https://openrouter.ai/) using the official [OpenAI NuGet package](https://www.nuget.org/packages/OpenAI).

This is the companion code for the blog post: [How to Use OpenRouter API in ASP.NET Core Web API](https://makarchie.com/posts/openrouter-in-dotnet/)

## Quick Start

1. Clone the repository
2. Set your OpenRouter API key in `AspNetCore/appsettings.json`
3. Run the project:

```bash
cd AspNetCore
dotnet run
```

4. Send a request:

```bash
curl -X POST http://localhost:5193/solve \
  -H "Content-Type: application/json" \
  -d '{"problem": "Solve equation x^2-1=0"}'
```

## Tech Stack

- .NET 10 (compatible with .NET 8/9)
- ASP.NET Core Minimal API
- [OpenAI](https://www.nuget.org/packages/OpenAI) v2.8.0
