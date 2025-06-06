using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// This repository configuration source uses parsable retrieval strategy.
/// </summary>
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
