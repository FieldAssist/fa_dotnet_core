// Copyright (c) FieldAssist. All Rights Reserved.

namespace FA.Loki.Models;

public abstract class LokiEntry
{
    public string Timestamp { get; set; }
    public string Message { get; set; }
    public string Level { get; set; }

    protected LokiEntry(string logLevel, string message)
    {
        Level = logLevel;
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() + "000000";
        Message = message;
    }

    public abstract Dictionary<string, string?> GetLabels();
}

//public class ExtendedLokiEntry : LokiEntry
// {
//     public string? ExtraField { get; set; }
//
//     public ExtendedLokiEntry(LogLevel logLevel, string message, string? extraField = null, long? companyId = null, long? userId = null,
//         string? category = null, string? database = null)
//         : base(logLevel, message, companyId, userId, category, database)
//     {
//         ExtraField = extraField;
//     }
//
//     public override Dictionary<string, string?> ToDictionary()
//     {
//         var dict = base.ToDictionary();
//         dict[nameof(ExtraField)] = ExtraField;
//         return dict;
//     }
// }
//
