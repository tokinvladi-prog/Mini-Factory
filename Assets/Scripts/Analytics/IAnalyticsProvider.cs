using System.Collections.Generic;

public interface IAnalyticsProvider
{
    string Name { get; }
    void Send(string eventName, IReadOnlyDictionary<string, object> parameters);
    void Flush();
}