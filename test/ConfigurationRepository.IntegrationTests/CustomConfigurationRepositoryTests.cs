using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.Tests.Integrational;

[Parallelizable]
[TestFixture]
public class CustomConfigurationRepositoryTests
{
    private InMemoryDictionaryRepository DictionaryRepository { get; set; }

    private InMemoryJsonRepository JsonRepository { get; set; }

    [OneTimeSetUp]
    public void Setup()
    {
        DictionaryRepository = new InMemoryDictionaryRepository(
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["Key1"] = "Value1",
                ["Key2"] = "Value2"
            });
        JsonRepository = new InMemoryJsonRepository(
            """
            {
              "Logging": {
                "LogLevel": {
                  "Default": "Information",
                  "Microsoft.AspNetCore": "Warning"
                }
              }
            }
            """);
    }

    [Test]
    public void Dictionary_Repository_Configuration_Should_Returns_Correct_Values()
    {
        var configuration = new ConfigurationBuilder()
            .AddDictionaryRepository(DictionaryRepository)
            .Build();

        Assert.That(configuration["Key1"], Is.EqualTo("Value1"));
    }

    [Test]
    public void Json_Repository_Configuration_Should_Returns_Correct_Values()
    {
        var configuration = new ConfigurationBuilder()
            .AddParsableRepository(JsonRepository)
            .Build();

        Assert.That(configuration["logging:LogLevel:Default"], Is.EqualTo("Information"));
    }

    private sealed class InMemoryDictionaryRepository(IDictionary<string, string?> configuration) : IRepository
    {
        public TData GetConfiguration<TData>() =>
            (TData)configuration;
    }

    private sealed class InMemoryJsonRepository(string jsonConfig) : IRepository
    {
        public TData GetConfiguration<TData>()
        {
            return (TData)Convert.ChangeType(jsonConfig, typeof(TData));
        }
    }
}
