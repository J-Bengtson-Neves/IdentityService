using IdentityService.Domain.Exceptions;

namespace IdentityService.Domain.ValueObjects;

public class LightningPubKey : ValueObject
{
    public string Value { get; private set; }

    private LightningPubKey() { } // EF Core

    private LightningPubKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 66)
            throw new InvalidLightningPubKeyException(value);

        Value = value;
    }

    public static LightningPubKey From(string value)
    {
        return new LightningPubKey(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
