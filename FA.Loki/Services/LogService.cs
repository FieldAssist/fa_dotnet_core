// Copyright (c) FieldAssist. All Rights Reserved.

using FA.Loki.Models;

namespace FA.Loki.Services;

public class LogService
{
    private readonly LokiService _lokiService;

    public LogService(LokiService lokiService)
    {
        _lokiService = lokiService;
    }

    public void LogSlowQuery(string database, string queryText, double queryTimeMs, string requestGuid, string host)
    {
        _lokiService.Log(new SlowQueryLogEntry(category: "SlowQuery", database: database,
            message: queryText,
            queryTime: queryTimeMs, requestId: requestGuid, host: host));
    }

    public void LogException(ErrorLogEntry entry)
    {
        _lokiService.Log(entry);
    }

    public void LogApi(ApiLogEntry entry)
    {
        _lokiService.Log(entry);
    }
}
