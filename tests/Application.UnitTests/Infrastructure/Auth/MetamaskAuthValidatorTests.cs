using IdentityService.Infrastructure.Auth;
using NUnit.Framework;
using Shouldly;

namespace IdentityService.Application.UnitTests.Infrastructure.Auth;

[TestFixture]
public class MetamaskAuthValidatorTests
{
    [Test]
    public void IsValidAddress_ShouldRejectInvalidAddressFormats()
    {
        MetamaskAuthValidator.IsValidAddress(null).ShouldBeFalse();
        MetamaskAuthValidator.IsValidAddress(string.Empty).ShouldBeFalse();
        MetamaskAuthValidator.IsValidAddress("0x123").ShouldBeFalse();
        MetamaskAuthValidator.IsValidAddress("not-an-address").ShouldBeFalse();
    }

    [Test]
    public void IsValidAddress_ShouldAcceptValidAddressFormat()
    {
        const string address = "0xAbCdEf1234567890AbCdEf1234567890AbCdEf12";

        MetamaskAuthValidator.IsValidAddress(address).ShouldBeTrue();
    }

    [Test]
    public void NormalizeAddress_ShouldTrimAndLowercase()
    {
        const string address = " 0xAbCdEf1234567890AbCdEf1234567890AbCdEf12 ";

        MetamaskAuthValidator.NormalizeAddress(address)
            .ShouldBe("0xabcdef1234567890abcdef1234567890abcdef12");
    }

    [Test]
    public void IsValidSignature_ShouldRejectInvalidSignatureFormat()
    {
        MetamaskAuthValidator.IsValidSignature(null).ShouldBeFalse();
        MetamaskAuthValidator.IsValidSignature(string.Empty).ShouldBeFalse();
        MetamaskAuthValidator.IsValidSignature("abc").ShouldBeFalse();
        MetamaskAuthValidator.IsValidSignature("0x123").ShouldBeFalse();
    }

    [Test]
    public void IsValidSignature_ShouldAcceptValidSignatureFormat()
    {
        var signature = "0x" + new string('a', 130);

        MetamaskAuthValidator.IsValidSignature(signature).ShouldBeTrue();
    }
}
