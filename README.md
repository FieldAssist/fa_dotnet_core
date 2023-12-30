# FA .NET Core

Official FA .NET core library containing multiple utility libraries

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
            () => _unifyDbRepository.GetCompanyPositionUserDetails(companyId));
```

## 👍 Contribution
1. Fork it
2. Create your feature branch (git checkout -b my-new-feature)
3. Commit your changes (git commit -m 'Add some feature')
4. Push to the branch (git push origin my-new-feature)
5. Create new Pull Request

## Author

Made with ❤️ by [Ayush P Gupta (@apgapg)](https://github.com/apgapg)
