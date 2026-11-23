using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

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

public record WalletUser(HttpClient Client, Guid AccountId, Guid WalletId);

public static class Check
{
    public static async Task Success(HttpResponseMessage res)
    {
        if (!res.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException(
                $"HTTP {(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");
    }
}

public static class WalletData
{
    public static async Task<WalletUser> CreateWalletUserAsync(ApiFactory factory)
    {
        var user = await TestData.CreateAuthenticatedUserAsync(factory);
        var res = await user.Client.PostAsJsonAsync("/api/Wallets", new { accountId = user.AccountId });
        await Check.Success(res);
        var body = await res.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        return new WalletUser(user.Client, user.AccountId, body!.Data);
    }

    private static Task<HttpResponseMessage> PostWithKeyAsync(
        HttpClient client, string url, object body, string? key)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        req.Headers.Add("Idempotency-Key", key ?? Guid.NewGuid().ToString());
        return client.SendAsync(req);
    }

    public static Task<HttpResponseMessage> DepositAsync(WalletUser u, decimal amount, string? key = null)
        => PostWithKeyAsync(u.Client, $"/api/Wallets/{u.WalletId}/deposit",
            new { amount, currency = "BRL" }, key);

    public static Task<HttpResponseMessage> WithdrawAsync(WalletUser u, decimal amount, string? key = null)
        => PostWithKeyAsync(u.Client, $"/api/Wallets/{u.WalletId}/withdraw",
            new { amount, currency = "BRL" }, key);

    public static Task<HttpResponseMessage> TransferAsync(
        WalletUser from, Guid destinationWalletId, decimal amount, string? key = null)
        => PostWithKeyAsync(from.Client, $"/api/Wallets/{from.WalletId}/transfer",
            new { destinationWalletId, amount, currency = "BRL" }, key);

    public static async Task<decimal> GetBalanceAsync(HttpClient client, Guid walletId)
    {
        var res = await client.GetAsync($"/api/Wallets/{walletId}/balance");
        await Check.Success(res);
        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
        var data = json.GetProperty("data");
        foreach (var p in data.EnumerateObject())
            if (p.Value.ValueKind == JsonValueKind.Number)
                return p.Value.GetDecimal();
        throw new InvalidOperationException($"Nenhum campo numérico em data: {data}");
    }
}