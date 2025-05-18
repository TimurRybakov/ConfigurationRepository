
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

        var config = new ParametrizedConfigurationBuilder()
            .AddInMemoryCollection(configuration)
            .Build();

        Assert.That(config["parameter"], Is.EqualTo("value"));
    }

    [Test]
    public void Configuration_Parametrizer_Should_Reload_Changes()
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
    public void Configuration_Parametrizer_Should_Fail_On_Parameter_Cyclic_Dependencies()
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


    [Test]
    public void Configuration_Parametrizer_Should_Fail_On_Undefined_Parameters()
    {
        Dictionary<string, string?> configuration = new()
        {
            { "Key", "%parameter not defined%" },
        };

        var configBuilder = new ParametrizedConfigurationBuilder()
            .AddInMemoryCollection(configuration);

        Assert.Throws<InvalidOperationException>(() => configBuilder.Build());
    }
}
