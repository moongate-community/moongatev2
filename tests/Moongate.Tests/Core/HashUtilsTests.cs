using Moongate.Core.Utils;

namespace Moongate.Tests.Core;

public class HashUtilsTests
{
    [Test]
    public void HashPassword_WhenCalledTwice_ShouldProduceDifferentHashes()
    {
        const string password = "MySecurePassword123!";

        var firstHash = HashUtils.HashPassword(password);
        var secondHash = HashUtils.HashPassword(password);

        Assert.That(firstHash, Is.Not.EqualTo(secondHash));
    }

    [Test]
    public void HashPassword_WhenPasswordIsEmpty_ShouldThrowArgumentException()
    {
        Assert.That(() => HashUtils.HashPassword(string.Empty), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void VerifyPassword_WhenHashFormatIsInvalid_ShouldReturnFalse()
    {
        var isValid = HashUtils.VerifyPassword("password", "invalid-hash-format");

        Assert.That(isValid, Is.False);
    }

    [Test]
    public void VerifyPassword_WhenPasswordDoesNotMatch_ShouldReturnFalse()
    {
        var hash = HashUtils.HashPassword("MySecurePassword123!");

        var isValid = HashUtils.VerifyPassword("WrongPassword", hash);

        Assert.That(isValid, Is.False);
    }

    [Test]
    public void VerifyPassword_WhenPasswordMatches_ShouldReturnTrue()
    {
        const string password = "MySecurePassword123!";
        var hash = HashUtils.HashPassword(password);

        var isValid = HashUtils.VerifyPassword(password, hash);

        Assert.That(isValid, Is.True);
    }
}
