namespace IdentityService.Domain.Exceptions;

public class InvalidLightningPubKeyException : Exception
{
    public InvalidLightningPubKeyException(string pubKey)
        : base($"The Lightning pubkey \"{pubKey}\" is invalid. It must be a 66-character hex string.")
    {
    }
}
