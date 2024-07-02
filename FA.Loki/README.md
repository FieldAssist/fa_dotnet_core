# Loki

Official FA Loki library containing multiple utility libraries

## 💻 Installation

1. Clone the repo or add it as submodule
2. Add it in your project

```shell
dotnet add Loki/Loki.csproj
```

## ⭐ Features

Following Loki Logging are supported:

- Error Logging
- API Logging
- Slow Query Logging

#### ❔ Usage

Using cache in any project is now super easy

1. Update Dependencies.cs

```csharp

// Loki configuration
SetupLoki(services, config);
```

Example implementation 1

```csharp
private static void SetupLoki(IServiceCollection services, IConfiguration config)
{
    Console.WriteLine("ℹ️ Setting up Loki Service...");
    services.AddSingleton(
        sp =>
                {
                    const int BatchSize = 50;
                    const string ProjectName = "GT";
                    const string ServiceName = "gtappapi";
                    var version = GetVersion() ?? "Unknown";
                    // var baseUrl = config.GetValue<string>("AppSettings:LokiUrl") ?? "http://localhost:3100";
                    var baseUrl = config.GetValue<string>("AppSettings:LokiUrl") ?? "";

                    var lokiService = new LokiService(
                        baseUrl,
                        TimeSpan.FromSeconds(1),
                        BatchSize,
                        ProjectName,
                        ServiceName,
                        version);

                    return new LogService(lokiService);
                });
}
```

2. Inject as DI in your service
3. Use it directly

For API Logs: 

```csharp
_logService.LogApi(logEntry);
```

For Exception Logs: 

```csharp
logService.LogException(lokiEntry);
```

## API Reference

Every Loki model extends from base model `LokiEntry`. 
Three models are available: `LokiErrorEntry`, `LokiApiEntry` and `LokiSlowQueryEntry` out of box.

Some common labels are set while flushing logs like `project`, `service` and `version`.

Rest are provided via `LokiEntry -> GetLabels()` method.

Note- Prefer not to have labels with high cardinality.

## 👍 Contribution
1. Fork it
2. Create your feature branch (git checkout -b my-new-feature)
3. Commit your changes (git commit -m 'Add some feature')
4. Push to the branch (git push origin my-new-feature)
5. Create new Pull Request

## Author

Made with ❤️ by [Ayush P Gupta (@apgapg)](https://github.com/apgapg)
