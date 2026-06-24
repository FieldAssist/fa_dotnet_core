# FA Cache

Official FA Cache library

## 💻 Installation

1. Clone the repo or add it as submodule
2. Add it in your project

```shell
dotnet add FA.Cache/FA.Cache.csproj
```

## ⭐ Features

### Cache

Provides two popular cache:

- Memory Cache
- Redis Cache

Following implementation of above cache are provided:

- `MemoryCacheProvider`: Use .NET inbuilt memory cache.
- `RedisCacheProvider`: Redis cache provider for both read write combined. Use when only 1 redis server is there.
- `RedisReadWriteCacheProvider`: Provides separate read write connections. Use when master, replicas are different.

#### ❔ Usage

Using cache in any project is now super easy

1. Update Dependencies.cs

```csharp
   // Cache
   ConfigUtils.SetupCache(serviceProvider, configuration);
```

Example implementation 1

```csharp
   public static void SetupCache(IServiceCollection serviceProvider, IConfiguration configuration)
    {
        var redisCacheConnectionString = configuration.GetConnectionString("RedisCache");
        serviceProvider.AddMemoryCache();
        if (redisCacheConnectionString != null)
        {
            serviceProvider.AddSingleton<ICacheProvider>(s => new RedisCacheProvider(redisCacheConnectionString));
            Console.WriteLine("\u2705 Cache: Redis cache setup successful");
        }
        else
        {
            serviceProvider.AddSingleton<ICacheProvider, MemoryCacheProvider>();
            Console.WriteLine("\u2705 Cache: Memory cache setup successful");
        }

        var cacheProvider = (ICacheProvider)serviceProvider.BuildServiceProvider().GetService(typeof(ICacheProvider));
        serviceProvider.AddSingleton(s => new CacheHelper(cacheProvider));
    }
```

Example Implementation 2

```csharp
 private static void SetupCache(IServiceCollection serviceProvider, IConfiguration configuration)
        {
            // add for safety
            serviceProvider.AddMemoryCache();

            var redisReadCacheConnectionString = configuration.GetConnectionString("RedisReadCache");
            var redisWriteCacheConnectionString = configuration.GetConnectionString("RedisWriteCache");
            if (redisReadCacheConnectionString != null && redisWriteCacheConnectionString != null)
            {
                serviceProvider.AddSingleton<ICacheProvider>(s => new RedisReadWriteCacheProvider(
                    redisReadOnlyConnectionString: redisReadCacheConnectionString,
                    redisWriteOnlyConnectionString: redisWriteCacheConnectionString
                    ));
                Console.WriteLine("\u2705 Cache: Redis cache setup successful");
            }
            else
            {
                serviceProvider.AddSingleton<ICacheProvider, MemoryCacheProvider>();
                Console.WriteLine("\u2705 Cache: Memory cache setup successful");
            }

            var cacheProvider = (ICacheProvider)serviceProvider.BuildServiceProvider().GetService(typeof(ICacheProvider));
            serviceProvider.AddSingleton(s => new CacheHelper(cacheProvider!));
        }
```

2. Add CacheHelper as dependency as class constructor parameter in respective service.
3. Use it directly

API Reference:

```csharp
        public async Task<T> GetResult<T>(string cacheKey, TimeSpan expiresIn, Func<Task<T>> fetchDataFunc)
```

- `cacheKey`: Unique corresponding key identifier
- `expiresIn`: Time to expire the cache value
- `fetchDataFunc`: callback to get data if cache not found

Example Usage: 

```csharp
        var positionsList = await _cacheHelper.GetResult(
            CacheKeys.GetPositionDetails(companyId), TimeSpan.FromHours(1),
            async () => await _unifyDbRepository.GetCompanyPositionUserDetails(companyId));
```

### Distributed Locks

Provides distributed locking to prevent race conditions across multiple service instances (e.g., in Kubernetes).

Compatible with **Redis** and **Dragonfly DB**.

Following implementations are provided:

- `RedisDistributedLockProvider`: Uses Redis/Dragonfly `SET NX PX` and Lua scripts for atomic lock operations.
- `MemoryDistributedLockProvider`: Thread-safe in-memory fallback for local development.

#### ❔ Setup

1. Update Dependencies.cs

Example with Redis:

```csharp
   public static void SetupDistributedLock(IServiceCollection serviceProvider, IConfiguration configuration)
   {
       var redisCacheConnectionString = configuration.GetConnectionString("RedisCache");
       if (redisCacheConnectionString != null)
       {
           // Option 1: Reuse the IDatabase from an existing RedisCacheProvider
           var cacheProvider = serviceProvider.BuildServiceProvider()
               .GetRequiredService<ICacheProvider>() as RedisCacheProvider;
           serviceProvider.AddSingleton<IDistributedLockProvider>(s =>
               new RedisDistributedLockProvider(
                   s.GetRequiredService<ILogger<RedisDistributedLockProvider>>(),
                   cacheProvider!.Database));
       }
       else
       {
           serviceProvider.AddSingleton<IDistributedLockProvider, MemoryDistributedLockProvider>();
       }
   }
```

Example with Read/Write Redis:

```csharp
   public static void SetupDistributedLock(IServiceCollection serviceProvider, IConfiguration configuration)
   {
       var redisWriteConnectionString = configuration.GetConnectionString("RedisWriteCache");
       if (redisWriteConnectionString != null)
       {
           var cacheProvider = serviceProvider.BuildServiceProvider()
               .GetRequiredService<ICacheProvider>() as RedisReadWriteCacheProvider;
           serviceProvider.AddSingleton<IDistributedLockProvider>(s =>
               new RedisDistributedLockProvider(
                   s.GetRequiredService<ILogger<RedisDistributedLockProvider>>(),
                   cacheProvider!.Database));
       }
       else
       {
           serviceProvider.AddSingleton<IDistributedLockProvider, MemoryDistributedLockProvider>();
       }
   }
```

2. Add `IDistributedLockProvider` as a dependency in your service constructor.

#### ❔ Usage

**Simple usage with `await using` (recommended):**

```csharp
   await using var lockHandle = await _lockProvider.TryAcquireAsync(
       lockKey: $"lock:order:{orderId}",
       expiresIn: TimeSpan.FromSeconds(30));

   if (lockHandle is null)
   {
       // Could not acquire the lock
       throw new InvalidOperationException("Resource is busy.");
   }

   // Critical section — only one instance runs this at a time
   await ProcessOrderAsync(orderId);
   // Lock is automatically released when lockHandle is disposed
```

**With wait and retry:**

```csharp
   await using var lockHandle = await _lockProvider.TryAcquireAsync(
       lockKey: $"lock:report:{reportId}",
       expiresIn: TimeSpan.FromMinutes(2),
       waitTime: TimeSpan.FromSeconds(10),   // Wait up to 10s for the lock
       retryInterval: TimeSpan.FromMilliseconds(200));  // Retry every 200ms

   if (lockHandle is null)
   {
       _logger.LogWarning("Timeout waiting for lock on report {ReportId}", reportId);
       return;
   }

   await GenerateReportAsync(reportId);
```

**Low-level acquire/release:**

```csharp
   var lockValue = Guid.NewGuid().ToString();
   var acquired = await _lockProvider.AcquireAsync(
       "lock:my-resource", lockValue, TimeSpan.FromSeconds(30));

   if (acquired)
   {
       try
       {
           // Critical section
       }
       finally
       {
           await _lockProvider.ReleaseAsync("lock:my-resource", lockValue);
       }
   }
```

## Author

Made with ❤️ by [Ayush P Gupta (@apgapg)](https://github.com/apgapg)

