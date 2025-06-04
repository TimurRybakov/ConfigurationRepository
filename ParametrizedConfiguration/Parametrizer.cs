using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace ParametrizedConfiguration;

public static class Parametrizer
{
    public static IDictionary<string, string?> Parametrize(this IConfiguration config)
    {
        var valueDictionary = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in config.AsEnumerable())
        {
            valueDictionary.Add(kvp.Key, kvp.Value);
        }

        var dependencyGraph = BuildDependencyGraph(valueDictionary);

        var sortedKeys = TopologicalSort(dependencyGraph);

        foreach (var key in sortedKeys)
        {
            var value = valueDictionary[key];
            valueDictionary[key] = ParametrizeValue(valueDictionary, value);
        }

        return valueDictionary;
    }

    private static Dictionary<string, List<string>?> BuildDependencyGraph(IDictionary<string, string?> input)
    {
        var graph = new Dictionary<string, List<string>?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in input)
        {
            if (kvp.Value is not null)
            {
                var dependencies = ExtractDependencies(kvp.Value);
                graph[kvp.Key] = dependencies;
            }
        }
        return graph;
    }

    private static List<string>? ExtractDependencies(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        List<string>? dependencies = null;
        int startIndex = 0;
        while ((startIndex = value.IndexOf('%', startIndex)) != -1)
        {
            var endIndex = value.IndexOf('%', startIndex + 1);
            if (endIndex == -1)
                break;

            var key = value.Substring(startIndex + 1, endIndex - startIndex - 1);

            if (dependencies is null)
                dependencies = [];

            dependencies.Add(key);
            startIndex = endIndex + 1;
        }

        return dependencies;
    }

    private static List<string> TopologicalSort(Dictionary<string, List<string>?> graph)
    {
        var sorted = new List<string>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var recStack = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in graph.Keys)
        {
            DFS(node);
        }

        sorted.Reverse();
        return sorted;

        void DFS(string node)
        {
            if (!recStack.Add(node))
            {
                throw new CyclicDependencyException($"Cyclic dependency detected on parameter '{node}'.");
            }

            if (!visited.Add(node))
                return;

            ref var nodes = ref CollectionsMarshal.GetValueRefOrNullRef(graph, node);

            if (Unsafe.IsNullRef(ref nodes))
            {
                throw new InvalidOperationException($"Undefined parameter value '{node}'.");
            }

            for (var i = 0; i < (nodes?.Count ?? 0); i++)
            {
                var neighbor = nodes![i];
                DFS(neighbor);
            }

            recStack.Remove(node);
            sorted.Add(node);
        }
    }

    private static string? ParametrizeValue(IDictionary<string, string?> input, string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        int startIndex = 0;
        while ((startIndex = value.IndexOf('%', startIndex)) != -1)
        {
            var endIndex = value.IndexOf('%', startIndex + 1);
            if (endIndex == -1) break;

            var key = value.Substring(startIndex + 1, endIndex - startIndex - 1);
            if (!input.TryGetValue(key, out var replacement) || replacement is null)
            {
                throw new InvalidOperationException($"Undefined parameter value '{key}'.");
            }

            value = value.Substring(0, startIndex) + replacement + value.Substring(endIndex + 1);
            startIndex += replacement.Length;
        }

        return value;
    }
}
