
using System.Text;

namespace ConfigurationRepository.Tests.Unit;

internal class JsonConfigurationParserTests
{
    [TestCase("""
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
        """)]
    public void Json_ShouldBe_ParsedAsExpected(string input)
    {
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
