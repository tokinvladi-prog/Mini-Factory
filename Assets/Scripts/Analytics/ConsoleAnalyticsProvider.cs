using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public class ConsoleAnalyticsProvider : AnalyticsProviderBehaviour
{
    [SerializeField] private bool includeTimestamp = true;
    [SerializeField] private Color logColor = new(0.55f, 0.85f, 1f);

    public override string Name => "console";

    private readonly StringBuilder _sb = new(256);

    public override void Send(string eventName, IReadOnlyDictionary<string, object> parameters)
    {
        _sb.Clear();
        _sb.Append("[Analytics] ").Append(eventName);

        if (parameters != null && parameters.Count > 0)
        {
            _sb.Append(" { ");
            bool first = true;
            foreach (var kv in parameters)
            {
                if (!first) _sb.Append(", ");
                first = false;
                _sb.Append(kv.Key).Append('=').Append(Format(kv.Value));
            }
            _sb.Append(" }");
        }

        if (includeTimestamp)
            _sb.Append(" @ ").Append(Time.realtimeSinceStartup.ToString("F2", CultureInfo.InvariantCulture)).Append('s');

        Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(logColor)}>{_sb}</color>");
    }

    private static string Format(object v) => v switch
    {
        null => "null",
        double d => d.ToString("F2", CultureInfo.InvariantCulture),
        float f => f.ToString("F2", CultureInfo.InvariantCulture),
        bool b => b ? "true" : "false",
        _ => v.ToString()
    };
}
