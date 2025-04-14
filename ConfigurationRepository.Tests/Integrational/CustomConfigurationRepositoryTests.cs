
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.Tests;

public class CustomConfigurationRepositoryTests
{
    private InMemoryRepository Repository { get; set; }

    [OneTimeSetUp]
    public void Setup()
    {
        Repository = new InMemoryRepository(
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["Key1"] = "Value1",
                ["Key2"] = "Value2"
            });
    }

    [Test]
    public void InMemoryRepositoryChanges_Should_UpdateConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddRepository(Repository, source =>
            {
                //source.UseRepositoryChangesNotifier();
            })
            .Build();

        Assert.DoesNotThrow(() => _ = Repository.GetConfiguration()["key1"], "Repository keys should be read ignoring case!");
        Repository.SetConfiguration("key1", "changed value1");

        Assert.That(configuration["Key1"], Is.EqualTo("changed value1"));
    }

    private sealed class InMemoryRepository(IDictionary<string, string?> configuration) : IRepository
    {
        public void SetConfiguration(string key, string? value)
        {
            configuration[key] = value;
        }

        public IDictionary<string, string?> GetConfiguration()
        {
            return configuration;
        }
    }
}
