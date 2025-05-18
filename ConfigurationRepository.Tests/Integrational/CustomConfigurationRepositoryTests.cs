
using Microsoft.Extensions.Configuration;
using ParametrizedConfiguration;

namespace ConfigurationRepository.Tests.Integrational;

[Parallelizable]
[TestFixture]
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
    public void InMemory_Repository_Changes_Should_Update_Configuration()
    {
        var configuration = new ConfigurationBuilder()
            .AddRepository(Repository, source => source.RetrievalStrategy = DictionaryRetrievalStrategy.Instance)
            .Build();

        TestDelegate getValue = () => _ = Repository.GetConfiguration<IDictionary<string, string?>>()["key1"];
        Assert.DoesNotThrow(getValue, "Repository keys should be read ignoring case!");
        Repository.SetConfiguration("key1", "changed value1");

        Assert.That(configuration["Key1"], Is.EqualTo("changed value1"));
    }

    private sealed class InMemoryRepository(IDictionary<string, string?> configuration) : IRepository
    {
        public void SetConfiguration(string key, string? value)
        {
            configuration[key] = value;
        }

        public TData GetConfiguration<TData>() =>
            (TData)GetConfiguration();

        private IDictionary<string, string?> GetConfiguration()
        {
            return configuration;
        }
    }
}
