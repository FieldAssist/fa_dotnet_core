// Copyright (c) FieldAssist. All Rights Reserved.

namespace FA.Loki.Models;

public class SlowQueryLogEntry : LokiEntry
{
    private string Category { get; set; }
    private string Database { get; set; }
    private string RequestId { get; set; }
    private string? Host { get; set; }

    private double? QueryTime { get; set; }
    // public long? CompanyId { get; set; }
    // public long? UserId { get; set; }

    public SlowQueryLogEntry(string message,
        string category, string database, double queryTime, string requestId, string? host) : base("info", message)
    {
        Category = category;
        Database = database;
        QueryTime = queryTime;
        RequestId = requestId;
        Host = host;
    }

    public override Dictionary<string, string?> GetLabels()
    {
        var dict = new Dictionary<string, string?>();
        dict["database"] = Database;
        dict["category"] = Category;
        dict["query_time"] = QueryTime.ToString();
        dict["request_id"] = RequestId;
        dict["host"] = Host;
        return dict;
    }
}
