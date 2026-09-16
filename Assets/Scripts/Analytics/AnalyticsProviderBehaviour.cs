using System.Collections.Generic;
using UnityEngine;

public abstract class AnalyticsProviderBehaviour : MonoBehaviour, IAnalyticsProvider
{
    public abstract string Name { get; }
    public abstract void Send(string eventName, IReadOnlyDictionary<string, object> parameters);
    public virtual void Flush() { }
}
