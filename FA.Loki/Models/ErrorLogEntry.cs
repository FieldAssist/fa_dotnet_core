// Copyright (c) FieldAssist. All Rights Reserved.

namespace FA.Loki.Models;

public class ErrorLogEntry : LokiEntry
{
    private string RequestUrl { get; set; }
    private string AuthToken { get; set; }
    private string UserId { get; set; }
    private string CompanyId { get; set; }
    private string HttpMethod { get; set; }
    private int StatusCode { get; set; }
    private long TotalTimeMs { get; set; }
    private string FullRequestUrl { get; set; }
    private string Category { get; set; }
    internal string StackTrace { get; set; }

    public ErrorLogEntry(string message,
        string requestUrl,
        string authToken,
        string userId,
        string companyId,
        string httpMethod,
        int statusCode,
        long totalTimeMs,
        string fullRequestUrl, string stackTrace) : base("error", message)
    {
        Category = "Exception";
        RequestUrl = requestUrl;
        AuthToken = authToken;
        UserId = userId;
        CompanyId = companyId;
        HttpMethod = httpMethod;
        StatusCode = statusCode;
        TotalTimeMs = totalTimeMs;
        FullRequestUrl = fullRequestUrl;
        StackTrace = stackTrace;
    }

    public override Dictionary<string, string?> GetLabels()
    {
        var dict = new Dictionary<string, string?>();
        dict["category"] = Category;
        dict["request_url"] = RequestUrl;
        dict["auth_token"] = AuthToken;
        dict["user_id"] = UserId;
        dict["company_id"] = CompanyId;
        dict["http_method"] = HttpMethod;
        dict["status_code"] = StatusCode.ToString();
        dict["total_time_ms"] = TotalTimeMs.ToString();
        dict["full_request_url"] = FullRequestUrl;
        return dict;
    }
}
