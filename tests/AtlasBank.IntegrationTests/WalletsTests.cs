using System.Net;
using System.Net.Http.Json;

namespace AtlasBank.IntegrationTests;

[Collection("api")]
public class WalletsTests(ApiFactory factory)
{
    [Fact]
    public async Task Deposit_ValidAmount_IncreasesBalance()
    {
        var u = await WalletData.CreateWalletUserAsync(factory);

        await Check.Success(await WalletData.DepositAsync(u, 100m));

        Assert.Equal(100m, await WalletData.GetBalanceAsync(u.Client, u.WalletId));
    }

    [Fact]
    public async Task Withdraw_SufficientBalance_DecreasesBalance()
    {
        var u = await WalletData.CreateWalletUserAsync(factory);
        await Check.Success(await WalletData.DepositAsync(u, 100m));

        await Check.Success(await WalletData.WithdrawAsync(u, 30m));

        Assert.Equal(70m, await WalletData.GetBalanceAsync(u.Client, u.WalletId));
    }

    [Fact]
    public async Task Withdraw_InsufficientBalance_Returns400_AndKeepsBalance()
    {
        var u = await WalletData.CreateWalletUserAsync(factory);
        await Check.Success(await WalletData.DepositAsync(u, 50m));

        var res = await WalletData.WithdrawAsync(u, 500m);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal(50m, await WalletData.GetBalanceAsync(u.Client, u.WalletId));
    }

    [Fact]
    public async Task Transfer_BetweenAccounts_MovesMoney()
    {
        var a = await WalletData.CreateWalletUserAsync(factory);
        var b = await WalletData.CreateWalletUserAsync(factory);
        await Check.Success(await WalletData.DepositAsync(a, 100m));

        await Check.Success(await WalletData.TransferAsync(a, b.WalletId, 40m));

        Assert.Equal(60m, await WalletData.GetBalanceAsync(a.Client, a.WalletId));
        Assert.Equal(40m, await WalletData.GetBalanceAsync(b.Client, b.WalletId));
    }

    [Fact]
    public async Task Deposit_SameIdempotencyKeyTwice_AppliesOnlyOnce()
    {
        var u = await WalletData.CreateWalletUserAsync(factory);
        var key = Guid.NewGuid().ToString();

        await WalletData.DepositAsync(u, 100m, key);
        await WalletData.DepositAsync(u, 100m, key); // pode devolver 200 ou 400; o que importa é o saldo

        Assert.Equal(100m, await WalletData.GetBalanceAsync(u.Client, u.WalletId));
    }

    [Fact]
    public async Task Deposit_SameKeyOnDifferentWallets_BothApply()
    {
        var a = await WalletData.CreateWalletUserAsync(factory);
        var b = await WalletData.CreateWalletUserAsync(factory);
        var key = Guid.NewGuid().ToString();

        await Check.Success(await WalletData.DepositAsync(a, 100m, key));
        await Check.Success(await WalletData.DepositAsync(b, 100m, key));

        Assert.Equal(100m, await WalletData.GetBalanceAsync(a.Client, a.WalletId));
        Assert.Equal(100m, await WalletData.GetBalanceAsync(b.Client, b.WalletId));
    }

    [Fact]
    public async Task GetBalance_OfAnotherUsersWallet_Returns404()
    {
        var owner = await WalletData.CreateWalletUserAsync(factory);
        var intruder = await TestData.CreateAuthenticatedUserAsync(factory);

        var res = await intruder.Client.GetAsync($"/api/Wallets/{owner.WalletId}/balance");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task GetBalance_WithoutToken_Returns401()
    {
        var res = await factory.CreateClient().GetAsync($"/api/Wallets/{Guid.NewGuid()}/balance");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task CreateWallet_IgnoresAccountIdInBody_AndDoesNotBlockTheVictim()
    {
        var victim = await TestData.CreateAuthenticatedUserAsync(factory);
        var attacker = await TestData.CreateAuthenticatedUserAsync(factory);

        // o atacante tenta criar a carteira da vítima; o servidor deve usar o token dele
        var attackRes = await attacker.Client.PostAsJsonAsync("/api/Wallets",
            new { accountId = victim.AccountId, currency = "BRL" });
        Assert.Equal(HttpStatusCode.Created, attackRes.StatusCode);
        var attackerWalletId = (await attackRes.Content.ReadFromJsonAsync<ApiResponse<Guid>>())!.Data;

        // a vítima ainda consegue criar a própria carteira
        var victimRes = await victim.Client.PostAsJsonAsync("/api/Wallets", new { currency = "BRL" });
        Assert.Equal(HttpStatusCode.Created, victimRes.StatusCode);

        // a carteira criada pelo atacante pertence ao atacante, e a vítima não a enxerga
        var peek = await victim.Client.GetAsync($"/api/Wallets/{attackerWalletId}/balance");
        Assert.Equal(HttpStatusCode.NotFound, peek.StatusCode);
    }
}
