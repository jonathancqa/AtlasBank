using System.Net;
using Xunit;

namespace AtlasBank.IntegrationTests;

[Collection("api")]
public class HealthTests(ApiFactory factory)
{
    [Fact]
    public async Task Health_Returns200()
    {
        var res = await factory.CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}