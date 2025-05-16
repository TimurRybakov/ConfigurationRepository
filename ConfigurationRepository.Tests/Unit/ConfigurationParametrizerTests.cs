
using Microsoft.Extensions.Configuration;
using ParametrizedConfiguration;

namespace ConfigurationRepository.Tests.Unit;

[TestFixture]
internal class ConfigurationParametrizerTests
{
    [Test]
    public void ConfigurationParametrizer_Should_ReplaceParametersPlaceholders()
    {
        Dictionary<string, string?> configuration = new()
        {
            { "Key", "value" },
            { "parameter", "%Key%" }
        };

        var config = new ParametrizedConfigurationBuilder()
            .AddInMemoryCollection(configuration)
            .Build();

        Assert.That(config["parameter"], Is.EqualTo("value"));
    }

    [Test]
    public void ConfigurationParametrizer_Should_ReloadChanges()
    {
        Dictionary<string, string?> configuration = new()
        {
            { "Key", "value" },
            { "parameter", "%Key%" }
        };

        var config = new ParametrizedConfigurationBuilder()
            .AddInMemoryCollection(configuration)
            .Build();

        config["Key"] = "changed";

        Assert.That(config["parameter"], Is.EqualTo("changed"));
    }

    [Test]
    public void ConfigurationParametrizer_Should_FailOnParameterCyclicDeps()
    {
        Dictionary<string, string?> configuration = new()
        {
            { "Key", "%parameter%" },
            { "parameter", "%Key%" }
        };

        var configBuilder = new ParametrizedConfigurationBuilder()
            .AddInMemoryCollection(configuration);

        Assert.Throws<CyclicDependencyException>(() => configBuilder.Build());
    }
}
