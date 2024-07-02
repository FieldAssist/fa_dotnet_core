// Copyright (c) FieldAssist. All Rights Reserved.

using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using FA.Loki.Models;
using Polly;

namespace FA.Loki.Services;

public class LokiService : IDisposable
{
    private readonly string Prefix = "🐛 LokiService";
    private bool _isProcessing;

    private readonly HttpClient _httpClient;
    private readonly string _lokiEndpoint;
    private readonly ConcurrentQueue<dynamic> _logQueue;
    private readonly int _batchSize;
    private bool _disposed;
    private readonly string _projectName;
    private readonly string _serviceName;
    private readonly string? _version;
    private readonly TimeSpan _flushInterval;
    private Timer? _timer;

    public LokiService(string lokiEndpoint, TimeSpan flushInterval, int batchSize, string projectName,
        string serviceName, string? version = null, string? prefix = null)
    {
        _httpClient = new HttpClient();
        _lokiEndpoint = lokiEndpoint;
        _flushInterval = flushInterval;

        _logQueue = new ConcurrentQueue<dynamic>();
        _batchSize = batchSize;

        _projectName = projectName;
        _serviceName = serviceName;
        _version = version;
        _isProcessing = false;

        if (!string.IsNullOrEmpty(prefix))
        {
            Prefix = prefix;
        }
    }

    public void Log<T>(T lokiEntry) where T : LokiEntry
    {
        if (string.IsNullOrEmpty(_lokiEndpoint))
        {
            Console.WriteLine($"{Prefix}: Loki endpoint not configured. Skipping log.");
            return;
        }

        _timer ??= new Timer(FlushLogs<T>, null, _flushInterval, _flushInterval);
        _logQueue.Enqueue(lokiEntry);
    }

    private async void FlushLogs<T>(object? state) where T : LokiEntry
    {
        try
        {
            if (_isProcessing)
            {
                return;
            }

            if (_logQueue.Count == 0)
            {
                Console.WriteLine(
                    $"{Prefix}: Not enough logs to flush! Count: {_logQueue.Count}. Skipping flush.");
                return;
            }

            Console.WriteLine($"{Prefix}: Total logs: {_logQueue.Count}");

            _isProcessing = true;

            Console.WriteLine($"{Prefix}: Flushing logs to Loki...");
            var logsToSend = new List<T>();
            while (_logQueue.TryDequeue(out var log))
            {
                logsToSend.Add(log);
                if (logsToSend.Count >= _batchSize)
                {
                    break;
                }
            }

            Console.WriteLine($"{Prefix}: Logs to send: {logsToSend.Count}");

            if (logsToSend.Count <= 0)
            {
                Console.WriteLine($"{Prefix}: No logs to flush!");
                return;
            }

            var logEntries = new List<T>();

            foreach (var entry in logsToSend)
            {
                logEntries.Add(entry);
            }

            var logStream = new
            {
                streams =
                    logEntries.ConvertAll(entry =>
                    {
                        var labelsDict = new Dictionary<string, string?>
                        {
                            { "service", _serviceName },
                            { "project", _projectName },
                            { "version", _version ?? string.Empty },
                        };

                        labelsDict.Add("level", entry.Level);

                        foreach (var kvp in entry.GetLabels())
                        {
                            labelsDict.Add(kvp.Key, kvp.Value ?? string.Empty);
                        }

                        var stkTrace = string.Empty;
                        if (entry is ErrorLogEntry errorLogEntry)
                        {
                            stkTrace = errorLogEntry.StackTrace;
                        }

                        return new
                        {
                            stream = labelsDict,
                            values = new[]
                            {
                                new[]
                                {
                                    entry.Timestamp, entry.Message + (!string.IsNullOrEmpty(stkTrace)
                                        ? "\n" + stkTrace
                                        : string.Empty)
                                }
                            }
                        };
                    })
            };

            Console.WriteLine($"{Prefix}: Sending {logsToSend.Count} logs...");
            var bodyString = JsonSerializer.Serialize(logStream);
            var jsonContent =
                new StringContent(bodyString, Encoding.UTF8, "application/json");
            await Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(10, retryAttempt =>
                {
                    Console.WriteLine($"{Prefix}: Retry attempt: {retryAttempt}");
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                }).ExecuteAsync(async () =>
                {
                    try
                    {
                        var response = await _httpClient.PostAsync($"{_lokiEndpoint}/loki/api/v1/push", jsonContent);
                        if (!response.IsSuccessStatusCode)
                        {
                            var text = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"{Prefix}: {text}");
                        }

                        response.EnsureSuccessStatusCode();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"{Prefix}: Error in flushing logs: {ex}");
                        throw;
                    }
                });

            Console.WriteLine($"{Prefix}: Sent {logsToSend.Count} logs to Loki");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"{Prefix}: Error: {e}");
            // throw; // do not throw, to prevent stopping the execution of caller
        }
        finally
        {
            _isProcessing = false;
        }
    }

    public virtual void Dispose()
    {
        if (!_disposed)
        {
            _timer.Dispose();
            _disposed = true;
        }
    }
}