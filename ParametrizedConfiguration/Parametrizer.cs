using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ParametrizedConfiguration;

public static class Parametrizer
{
    public static IDictionary<string, string?> Parametrize(this IConfiguration config)
    {
        var values = config.AsEnumerable().ToDictionary(
            x => x.Key,
            x => x.Value,
            StringComparer.OrdinalIgnoreCase);

        var graph = values.ToDictionary(
            kvp => kvp.Key,
            kvp => ScanForPlaceholders(kvp.Value).Select(x => x.Key).ToList(),
            StringComparer.OrdinalIgnoreCase);

        var ordered = TopologicalSort(graph);

        // Ordered parameter value substitution
        foreach (var key in ordered)
        {
            ref var value = ref CollectionsMarshal.GetValueRefOrNullRef(values, key);
            Debug.Assert(!Unsafe.IsNullRef(ref value));
            value = ReplacePlaceholders(values, value);
        }

        return values;
    }

    private readonly struct Marker
    {
        public Marker(int start, int end, string key)
        {
            Start = start;
            End = end;
            Key = key;
        }

        public int Start { get; }

        public int End { get; }

        public string Key { get; }
    }

    private static IEnumerable<Marker> ScanForPlaceholders(string? value)
    {
        if (string.IsNullOrEmpty(value))
            yield break;

        for (int i = 0; ;)
        {
            int open = value.IndexOf('%', i);
            if (open == -1) yield break; // Parameter placeholder opening '%' not found

            int close = value.IndexOf('%', open + 1);
            if (close == -1) yield break; // Parameter placeholder closing '%' not found

            var key = value.Substring(open + 1, close - open - 1);
            yield return new Marker(open, close + 1, key);

            i = close + 1;
        }
    }

    private static string? ReplacePlaceholders(
        IDictionary<string, string?> map, string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var sb = new StringBuilder(value.Length);
        int incrementalStart = 0;

        foreach (var marker in ScanForPlaceholders(value))
        {
            // Substitute parameter value
            sb.Append(value, incrementalStart, marker.Start - incrementalStart);

            if (!map.TryGetValue(marker.Key, out var repl) || repl is null)
                throw new InvalidOperationException($"Undefined parameter value '{marker.Key}'.");

            sb.Append(repl);
            incrementalStart = marker.End;
        }

        // Add tail
        sb.Append(value, incrementalStart, value.Length - incrementalStart);
        return sb.ToString();
    }

    private static List<string> TopologicalSort(IDictionary<string, List<string>> graph)
    {
        var ordered  = new List<string>(graph.Count);
        var visited  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in graph.Keys)
            Dfs(node);

        ordered.Reverse();
        return ordered;

        void Dfs(string node)
        {
            if (!visiting.Add(node))
                throw new CyclicDependencyException(
                    $"Cyclic dependency detected on parameter '{node}'.");

            if (!visited.Add(node))
                return;

            if (!graph.TryGetValue(node, out var children))
                throw new InvalidOperationException($"Undefined parameter '{node}'.");

            foreach (var child in children)
                Dfs(child);

            visiting.Remove(node);
            ordered.Add(node);
        }
    }
}
