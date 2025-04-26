using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

public class ParsableConfigurationRepositorySource : ConfigurationRepositorySource
{
    /// <summary>
    /// Parser to parse data from database.
    /// </summary>
    public IConfigurationParser? ConfigurationParser { get; set; }

    /// <inheritdoc/>
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        ConfigurationParser ??= builder.GetParsableConfigurationParser();
        RetrievalStrategy ??= new ParsableRetrievalStrategy(ConfigurationParser);

        return base.Build(builder);
    }
}
