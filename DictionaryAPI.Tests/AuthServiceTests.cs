using DictionaryAPI.Models.DTOs.Auth;
using DictionaryAPI.Models.Entities;
using DictionaryAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;
using Xunit.Abstractions;

public class AuthServiceTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly IConfiguration _config;
    private readonly AuthService _authService;
    private readonly ITestOutputHelper _output;

    public AuthServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _userManagerMock = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<AppUser>>(),
            Array.Empty<IUserValidator<AppUser>>(),
            Array.Empty<IPasswordValidator<AppUser>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<AppUser>>>());


        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super_secret_key_min_32_chars_here!",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:DurationInMinutes"] = "15",
                ["Jwt:DurationInDays"] = "7"
            })
            .Build();

        _authService = new AuthService(_userManagerMock.Object, _config);
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ValidData_ReturnsTokens()
    {
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(new List<string> { "User" });

        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _authService.RegisterAsync(new RegisterDto
        {
            Username = "testuser",
            Email = "test@mail.com",
            Password = "Password1!"
        });

        _output.WriteLine($"Result: {result?.Success}");
        _output.WriteLine($"Tokens: {result?.Tokens}");


        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.Tokens!.Token));
    }

    [Fact]
    public async Task RegisterAsync_IdentityFails_ReturnsErrors()
    {
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password is too weak" }));

        var result = await _authService.RegisterAsync(new RegisterDto
        {
            Username = "testuser",
            Email = "test@mail.com",
            Password = "weak"
        });

        _output.WriteLine($"Result: {result?.Success}");
        _output.WriteLine($"Errors: {string.Join(", ", result.Errors!)}");

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.Tokens);
        Assert.Contains("Password is too weak", result.Errors!);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        var user = new AppUser { Id = "1", Email = "test@mail.com", UserName = "test@mail.com" };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync("test@mail.com"))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, "Password1!"))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _authService.LoginAsync(new LoginDto
        {
            Email = "test@mail.com",
            Password = "Password1!"
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsNull()
    {
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        var result = await _authService.LoginAsync(new LoginDto
        {
            Email = "noexiste@mail.com",
            Password = "Password1!"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        var user = new AppUser { Id = "1", Email = "test@mail.com" };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync("test@mail.com"))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, "wrongpassword"))
            .ReturnsAsync(false);

        var result = await _authService.LoginAsync(new LoginDto
        {
            Email = "test@mail.com",
            Password = "wrongpassword"
        });

        Assert.Null(result);
    }

    // ── Refresh Token ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshTokenAsync_ValidTokens_ReturnsNewTokens()
    {
        var expiredToken = GenerateExpiredTokenForTest("1");

        var user = new AppUser
        {
            Id = "1",
            Email = "test@mail.com",
            RefreshToken = "valid-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync("1"))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _authService.RefreshTokenAsync(new RefreshTokenDto
        {
            Token = expiredToken,
            RefreshToken = "valid-refresh-token"
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task RefreshTokenAsync_ExpiredRefreshToken_ReturnsNull()
    {
        var expiredToken = GenerateExpiredTokenForTest("1");

        var user = new AppUser
        {
            Id = "1",
            Email = "test@mail.com",
            RefreshToken = "valid-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1) // ← ya expiró
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync("1"))
            .ReturnsAsync(user);

        var result = await _authService.RefreshTokenAsync(new RefreshTokenDto
        {
            Token = expiredToken,
            RefreshToken = "valid-refresh-token"
        });

        Assert.Null(result);
    }

    // ── Revoke ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RevokeTokenAsync_ExistingUser_ClearsRefreshToken()
    {
        var user = new AppUser
        {
            Id = "1",
            RefreshToken = "some-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync("1"))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        await _authService.RevokeTokenAsync("1");

        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiryTime);
        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private string GenerateExpiredTokenForTest(string userId)
    {
        var secret = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key no configurado");

        var issuer = _config["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer no configurado");

        var audience = _config["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience no configurado");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(-5),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}