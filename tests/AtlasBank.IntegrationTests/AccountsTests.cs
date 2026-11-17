using System.Net;
using System.Net.Http.Json;

namespace AtlasBank.IntegrationTests;

[Collection("api")]
public class AccountsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateAccount_ValidData_Returns201()
    {
        var res = await _client.PostAsJsonAsync("/api/Accounts", new
        {
            fullName = "Usuario Teste",
            email = $"{Guid.NewGuid():N}@teste.com",
            document = Cpf.Generate(),
            password = TestData.Password
        });

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        Assert.True(body!.Success);
        Assert.NotEqual(Guid.Empty, body.Data);
    }

    [Fact]
    public async Task CreateAccount_DuplicateEmail_Returns400()
    {
        var (email, _) = await TestData.CreateAccountAsync(_client);

        var res = await _client.PostAsJsonAsync("/api/Accounts", new
        {
            fullName = "Outro Usuario",
            email,
            document = Cpf.Generate(),
            password = TestData.Password
        });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_InvalidEmail_ReturnsBadRequest()
    {
        var res = await _client.PostAsJsonAsync("/api/Accounts", new
        {
            fullName = "Usuario Teste",
            email = "invalido",
            document = Cpf.Generate(),
            password = TestData.Password
        });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var (email, _) = await TestData.CreateAccountAsync(_client);
        var token = await TestData.LoginAsync(_client, email);
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Login_WrongPassword_Returns400()
    {
        var (email, _) = await TestData.CreateAccountAsync(_client);

        var res = await _client.PostAsJsonAsync("/api/Accounts/login", new { email, password = "errada" });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}