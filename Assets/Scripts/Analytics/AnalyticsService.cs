using System;
using System.Collections.Generic;
using UnityEngine;

public interface IAnalyticsService
{
    void Track(string eventName, params (string key, object value)[] parameters);
    void Track(string eventName, IReadOnlyDictionary<string, object> parameters);
    void Flush();
}

public class AnalyticsService : MonoBehaviour, IAnalyticsService
{
    [SerializeField] private bool autoDiscoverProviders = true;
    [SerializeField] private bool verboseLogging = true;

    private readonly List<IAnalyticsProvider> _providers = new();
    private static readonly Dictionary<string, object> EmptyParams = new(0);

    public IReadOnlyList<IAnalyticsProvider> Providers => _providers;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (autoDiscoverProviders)
        {
            var found = GetComponentsInChildren<AnalyticsProviderBehaviour>(includeInactive: true);
            for (int i = 0; i < found.Length; i++)
                _providers.Add(found[i]);
        }

        if (verboseLogging)
            Debug.Log($"[Analytics] Initialized with {_providers.Count} provider(s): " +
                      string.Join(", ", _providers.ConvertAll(p => p.Name)));
    }

    public void Register(IAnalyticsProvider provider)
    {
        if (provider == null || _providers.Contains(provider)) return;
        _providers.Add(provider);
    }

    public void Track(string eventName, params (string key, object value)[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
        {
            Track(eventName, EmptyParams);
            return;
        }

        var dict = new Dictionary<string, object>(parameters.Length);
        for (int i = 0; i < parameters.Length; i++)
        {
            var (k, v) = parameters[i];
            if (!string.IsNullOrEmpty(k)) dict[k] = v;
        }
        Track(eventName, dict);
    }

    public void Track(string eventName, IReadOnlyDictionary<string, object> parameters)
    {
        if (string.IsNullOrEmpty(eventName)) return;
        parameters ??= EmptyParams;

        for (int i = 0; i < _providers.Count; i++)
        {
            try { _providers[i].Send(eventName, parameters); }
            catch (Exception e)
            {
                Debug.LogError($"[Analytics] Provider '{_providers[i].Name}' threw on " +
                               $"'{eventName}': {e}");
            }
        }
    }

    public void Flush()
    {
        for (int i = 0; i < _providers.Count; i++)
        {
            try { _providers[i].Flush(); }
            catch (Exception e)
            {
                Debug.LogError($"[Analytics] Provider '{_providers[i].Name}' flush threw: {e}");
            }
        }
    }
}
