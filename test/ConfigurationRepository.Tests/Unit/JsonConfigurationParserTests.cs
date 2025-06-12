using System.Text;

namespace ConfigurationRepository.Tests.Unit;

[Parallelizable]
[TestFixture]
internal class JsonConfigurationParserTests
{
    [Test]
    public void Json_Should_Be_Parsed_As_Expected()
    {
        const string input = """
        {
          "Logging": {
            "LogLevel": {
              "Default": "Debug",
              "Microsoft": "Warning",
              "Microsoft.Hosting.Lifetime": "Information"
            }
          },
          "AllowedHosts": "*"
        }
        """;
        // Arrange
        JsonConfigurationParser parser = new JsonConfigurationParser();
        byte[] byteArray = Encoding.UTF8.GetBytes(input);
        var sream = new MemoryStream(byteArray);

        // Act
        var dict = parser.Parse(sream);

        // Assert
        Assert.That(dict["Logging:LogLevel:Microsoft.Hosting.Lifetime"], Is.EqualTo("Information"));
    }
}
