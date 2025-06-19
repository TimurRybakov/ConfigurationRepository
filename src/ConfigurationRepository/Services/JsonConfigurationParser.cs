using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// Standard json configuration parser. Works same as one for json configuration files.
/// </summary>
public sealed class JsonConfigurationParser : IConfigurationParser
{
    private readonly Dictionary<string, string?> _data = new (StringComparer.OrdinalIgnoreCase);
    private readonly Stack<string> _paths = new ();

    /// <inheritdoc/>
    public IDictionary<string, string?> Parse(Stream input)
    {
        if (_data.Count > 0)
        {
            _data.Clear();
            _paths.Clear();
        }

        try
        {
            var jsonDocumentOptions = new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true,
            };

            using (var reader = new StreamReader(input))
            using (JsonDocument doc = JsonDocument.Parse(reader.ReadToEnd(), jsonDocumentOptions))
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException(
                        $"Top-level JSON element must be an object. Instead, '{doc.RootElement.ValueKind}' was found.");
                }

                VisitObjectElement(doc.RootElement);
            }

            return _data;
        }
        catch (JsonException e)
        {
            throw new FormatException("Could not parse the JSON data", e);
        }
    }

    private void VisitObjectElement(JsonElement element)
    {
        var isEmpty = true;

        foreach (JsonProperty property in element.EnumerateObject())
        {
            isEmpty = false;
            EnterContext(property.Name);
            VisitValue(property.Value);
            ExitContext();
        }

        SetNullIfElementIsEmpty(isEmpty);
    }

    private void VisitArrayElement(JsonElement element)
    {
        int index = 0;

        foreach (JsonElement arrayElement in element.EnumerateArray())
        {
            EnterContext(index.ToString());
            VisitValue(arrayElement);
            ExitContext();
            index++;
        }

        SetNullIfElementIsEmpty(isEmpty: index == 0);
    }

    private void SetNullIfElementIsEmpty(bool isEmpty)
    {
        if (isEmpty && _paths.Count > 0)
        {
            _data[_paths.Peek()] = null;
        }
    }

    private void VisitValue(JsonElement value)
    {
        Debug.Assert(_paths.Count > 0);

        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                VisitObjectElement(value);
                break;

            case JsonValueKind.Array:
                VisitArrayElement(value);
                break;

            case JsonValueKind.Number:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                string key = _paths.Peek();
                if (_data.ContainsKey(key))
                {
                    throw new FormatException($"A duplicate key '{key}' was found.");
                }
                _data[key] = value.ToString();
                break;

            default:
                throw new FormatException($"Unsupported JSON token '{value.ValueKind}' was found");
        }
    }

    private void EnterContext(string context) =>
        _paths.Push(_paths.Count > 0 ?
            _paths.Peek() + ConfigurationPath.KeyDelimiter + context :
            context);

    private void ExitContext() => _paths.Pop();
}
