// Copyright (c) FieldAssist. All Rights Reserved.

using System.Text.Json;

namespace FA.Loki.Models;

public class ApiLogEntry : LokiEntry
{
    private string RequestUrl { get; set; }
    private string UserId { get; set; }
    private string CompanyId { get; set; }
    private string HttpMethod { get; set; }
    private int StatusCode { get; set; }
    private string Category { get; set; }

    // high cardinality labels
    private long TotalTimeMs { get; set; }
    private string AuthToken { get; set; }
    private string FullRequestUrl { get; set; }

    public ApiLogEntry(string message,
        string requestUrl,
        string authToken,
        string userId,
        string companyId,
        string httpMethod,
        int statusCode,
        long totalTimeMs,
        string fullRequestUrl) : base("info", message)
    {
        Category = "ApiLogs";
        RequestUrl = requestUrl;
        AuthToken = authToken;
        UserId = userId;
        CompanyId = companyId;
        HttpMethod = httpMethod;
        StatusCode = statusCode;
        TotalTimeMs = totalTimeMs;
        FullRequestUrl = fullRequestUrl;
    }

    public override Dictionary<string, string?> GetLabels()
    {
        var dict = new Dictionary<string, string?>();
        dict["category"] = Category;
        dict["request_url"] = RequestUrl;
        dict["user_id"] = UserId;
        dict["company_id"] = CompanyId;
        dict["http_method"] = HttpMethod;
        dict["status_code"] = StatusCode.ToString();
        // dict["auth_token"] = AuthToken;
        // dict["total_time_ms"] = TotalTimeMs.ToString();
        // dict["full_request_url"] = FullRequestUrl;
        return dict;
    }

    public static string GetMessageJsonString(string requestUrl, string authToken, string fullRequestUrl, string totalTimeMs)
    {
        var dict = new Dictionary<string, string>
        {
            { "request_url", requestUrl }, { "auth_token", authToken }, { "full_request_url", fullRequestUrl },{"total_time_ms", totalTimeMs}
        };
        return JsonSerializer.Serialize(dict);
    }
}
