using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AtlasBank.IntegrationTests;

public record ApiResponse<T>(bool Success, string? Message, T? Data);

public record TestUser(HttpClient Client, Guid AccountId, string Email, string Password);

public static class Cpf
{
    public static string Generate()
    {
        var n = new int[11];
        for (var i = 0; i < 9; i++) n[i] = Random.Shared.Next(10);
        n[9] = Digit(n, 9);
        n[10] = Digit(n, 10);
        return string.Concat(n);
    }

    private static int Digit(int[] n, int len)
    {
        var sum = 0;
        for (var i = 0; i < len; i++) sum += n[i] * (len + 1 - i);
        var r = sum * 10 % 11;
        return r == 10 ? 0 : r;
    }
}

public static class TestData
{
    public const string Password = "Senha@123456";

    public static async Task<(string Email, Guid AccountId)> CreateAccountAsync(HttpClient client)
    {
        var email = $"{Guid.NewGuid():N}@teste.com";
        var res = await client.PostAsJsonAsync("/api/Accounts", new
        {
            fullName = "Usuario Teste",
            email,
            document = Cpf.Generate(),
            password = Password
        });
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        return (email, body!.Data);
    }

    public static async Task<string> LoginAsync(HttpClient client, string email, string password = Password)
    {
        var res = await client.PostAsJsonAsync("/api/Accounts/login", new { email, password });
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
        return body!.Data!;
    }

    public static async Task<TestUser> CreateAuthenticatedUserAsync(ApiFactory factory)
    {
        var client = factory.CreateClient();
        var (email, accountId) = await CreateAccountAsync(client);
        var token = await LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return new TestUser(client, accountId, email, Password);
    }
}