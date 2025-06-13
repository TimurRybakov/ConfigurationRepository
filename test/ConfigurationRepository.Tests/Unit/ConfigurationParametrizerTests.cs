using Microsoft.Extensions.Configuration;
using ParametrizedConfiguration;

namespace ConfigurationRepository.Tests.Unit;

[TestFixture]
internal class ConfigurationParametrizerTests
{
    [Test]
    public void Configuration_Parametrizer_Should_Replace_Placeholders_With_Parameters_Values()
    {
        Dictionary<string, string?> configuration = new()
        {
            { "Key", "value" },
            { "parameter", "%Key%" }
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configuration)
            .WithParametrization()
            .Build();

        Assert.That(config["parameter"], Is.EqualTo("value"));
    }

    [Test]
    public void Configuration_Parametrizer_Should_Support_Nested_Parametrization()
    {
        Dictionary<string, string?> configuration = new()
        {
            { "param1", "1+%param2%" },
            { "param2", "2+%param3%" },
            { "param3", "3" }
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configuration)
            .WithParametrization()
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(config["param1"], Is.EqualTo("1+2+3"));
            Assert.That(config["param2"], Is.EqualTo("2+3"));
            Assert.That(config["param3"], Is.EqualTo("3"));
        });
    }

    [Test]
    public void Configuration_Parametrizer_Should_Reload_Changes()
    {
        Dictionary<string, string?> configuration = new()
        {
            { "Key", "value" },
            { "parameter", "%Key%" }
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configuration)
            .WithParametrization()
            .Build();

        config["Key"] = "changed";

        Assert.That(config["parameter"], Is.EqualTo("changed"));
    }

    [Test]
    public void Configuration_Parametrizer_Should_Fail_On_Parameter_Cyclic_Dependencies()
    {
        Dictionary<string, string?> configuration = new()
        {
            { "Key", "%parameter%" },
            { "parameter", "%Key%" }
        };

        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(configuration)
            .WithParametrization();

        Assert.Throws<CyclicDependencyException>(() => configBuilder.Build());
    }


    [Test]
    public void Configuration_Parametrizer_Should_Fail_On_Undefined_Parameters()
    {
        Dictionary<string, string?> configuration = new()
        {
            { "Key", "%parameter not defined%" },
        };

        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(configuration)
            .WithParametrization();

        Assert.Throws<InvalidOperationException>(() => configBuilder.Build());
    }

    [Test]
    public void Configuration_Parametrizer_Should_Fail_On_Null_Parameters()
    {
        Dictionary<string, string?> configuration = new()
        {
            { "null parameter", null },
            { "Key", "%null parameter%" },
        };

        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(configuration)
            .WithParametrization();

        Assert.Throws<InvalidOperationException>(() => configBuilder.Build());
    }

    [Test]
    public void Providers()
    {
        Dictionary<string, string?> configuration = new()
        {
            { "Logging:LogLevel:Default", "Information" },
            { "Logging:LogLevel:Microsoft.AspNetCore", "Waring" }
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configuration)
            .Build();

        var provider = config.Providers.First();
        var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        ProviderDataToDictionary(provider, dict);

        CollectionAssert.AreEqual(configuration, dict);
    }


    /// <summary>
    /// Builds dictionary from <see cref="IConfigurationProvider"/> provider.
    /// </summary>
    /// <param name="provider">Configuration provider.</param>
    /// <param name="resultDictionary">Resulting dictionary.</param>
    /// <param name="currentPath">Current path (i.e., "Logging:LogLevel").</param>
    private static void ProviderDataToDictionary(IConfigurationProvider provider, Dictionary<string, string?> resultDictionary, string? currentPath = null)
    {
        // If it is not a root path, then add value
        if (!string.IsNullOrEmpty(currentPath))
        {
            if (provider.TryGet(currentPath, out var value))
            {
                resultDictionary[currentPath] = value;
            }
        }

        // Recursively process all child keys
        IEnumerable<string> childKeys = provider.GetChildKeys(Enumerable.Empty<string>(), currentPath);

        foreach (string childKey in childKeys)
        {
            // Combine current path with child key to get next path
            string fullChildPath = currentPath is null ? childKey : ConfigurationPath.Combine(currentPath, childKey);

            ProviderDataToDictionary(provider, resultDictionary, fullChildPath);
        }
    }
}
