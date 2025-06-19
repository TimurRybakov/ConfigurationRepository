using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// This repository configuration source uses parsable retrieval strategy.
/// </summary>
public class ParsableConfigurationRepositorySource : ConfigurationRepositorySource
{
    /// <summary>
    /// Parser selector function, thart returns parser to parse data from a source.
    /// </summary>
    public Func<IConfigurationParser>? GetConfigurationParser { get; set; }

    /// <inheritdoc/>
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        GetConfigurationParser ??= builder.GetConfigurationParserFactory() ?? (() => new JsonConfigurationParser());
        RetrievalStrategy ??= new ParsableRetrievalStrategy(GetConfigurationParser);

        return base.Build(builder);
    }
}
