using WakeNetServer.Server.Services;
using Xunit;

namespace WakeNetServer.Tests.Services;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_And_Verify_Works()
    {
        var password = "P@ssw0rd!";
        var hash = PasswordHasher.Hash(password);

        Assert.True(PasswordHasher.Verify(password, hash));
        Assert.False(PasswordHasher.Verify("wrong", hash));
    }
}

