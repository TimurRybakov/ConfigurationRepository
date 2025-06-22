using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// Configuration parametrizer logic is here.
/// </summary>
public static class Parametrizer
{
    /// <summary>
    /// Parametrizes input <paramref name="config"/>.
    /// <code>
    /// IConfiguration config:               returned dictionary:
    /// {                                    {
    ///     { "param1", "1+%param2%" },          { "param1", "1+2+3" },
    ///     { "param2", "2+%param3%" },  -->     { "param2", "2+3" },
    ///     { "param3", "3" }                    { "param3", "3" }
    /// };                                   };
    /// </code>
    /// </summary>
    /// <param name="config">A configuration for parametrization.</param>
    /// <param name="parameterPlaceholderOpening">The string that indicates a start of parameter placeholder.</param>
    /// <param name="parameterPlaceholderClosing">The string that indicates an end of parameter placeholder.</param>
    /// <returns></returns>
    public static IDictionary<string, string?> Parametrize(
        this IConfiguration config,
        string parameterPlaceholderOpening = "%",
        string parameterPlaceholderClosing = "%")
    {
        var values = config.AsEnumerable().ToDictionary(
            x => x.Key,
            x => x.Value,
            StringComparer.OrdinalIgnoreCase);

        var graph = values.ToDictionary(
            kvp => kvp.Key,
            kvp => ScanForPlaceholders(kvp.Value, parameterPlaceholderOpening, parameterPlaceholderClosing)
                .Select(x => x.Key).ToList(),
            StringComparer.OrdinalIgnoreCase);

        var ordered = TopologicalSort(graph);

        // Ordered parameter value substitution
        foreach (var key in ordered)
        {
            ref var value = ref CollectionsMarshal.GetValueRefOrNullRef(values, key);
            Debug.Assert(!Unsafe.IsNullRef(ref value));
            value = ReplacePlaceholders(values, value, parameterPlaceholderOpening, parameterPlaceholderClosing);
        }

        return values;
    }

    private readonly struct Marker(int start, int end, string key)
    {
        public int Start { get; } = start;

        public int End { get; } = end;

        public string Key { get; } = key;
    }

    private static IEnumerable<Marker> ScanForPlaceholders(
        string? value,
        string parameterPlaceholderOpening,
        string parameterPlaceholderClosing)
    {
        if (string.IsNullOrEmpty(value))
            yield break;

        for (int i = 0; ;)
        {
            int open = value.IndexOf(parameterPlaceholderOpening, i, StringComparison.Ordinal);
            if (open == -1) yield break; // Parameter placeholder opening not found

            int close = value.IndexOf(parameterPlaceholderClosing, open + parameterPlaceholderOpening.Length, StringComparison.Ordinal);
            if (close == -1) yield break; // Parameter placeholder closing not found

            var key = value.Substring(open + parameterPlaceholderOpening.Length, close - open - parameterPlaceholderOpening.Length);
            yield return new Marker(open, close + parameterPlaceholderClosing.Length, key);

            i = close + parameterPlaceholderClosing.Length;
        }
    }

    private static string? ReplacePlaceholders(
        IDictionary<string, string?> map,
        string? value,
        string parameterPlaceholderOpening,
        string parameterPlaceholderClosing)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var sb = new StringBuilder(value.Length);
        int incrementalStart = 0;

        foreach (var marker in ScanForPlaceholders(value, parameterPlaceholderOpening, parameterPlaceholderClosing))
        {
            // Substitute parameter value
            sb.Append(value, incrementalStart, marker.Start - incrementalStart);

            if (!map.TryGetValue(marker.Key, out var repl) || repl is null)
                throw new InvalidOperationException($"Undefined parameter value '{marker.Key}'.");

            repl = ReplacePlaceholders(map, repl, parameterPlaceholderOpening, parameterPlaceholderClosing);

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
        var states = new Dictionary<string, NodeState>(graph.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var node in graph.Keys)
            Dfs(node);

        ordered.Reverse();
        return ordered;

        void Dfs(string node)
        {
            ref var stateRef = ref CollectionsMarshal.GetValueRefOrAddDefault(states, node, out bool exists);

            if (exists)
            {
                if (stateRef == NodeState.Visiting)
                    throw new CyclicDependencyException($"Cyclic dependency detected on parameter '{node}'.");

                if (stateRef == NodeState.Visited)
                    return;
            }

            stateRef = NodeState.Visiting;

            if (!graph.TryGetValue(node, out var children))
                throw new InvalidOperationException($"Undefined parameter '{node}'.");

            foreach (var child in children)
            {
                Dfs(child);
            }

            stateRef = NodeState.Visited;
            ordered.Add(node);
        }
    }

    private enum NodeState : Byte
    {
        Visiting,
        Visited
    }
}

