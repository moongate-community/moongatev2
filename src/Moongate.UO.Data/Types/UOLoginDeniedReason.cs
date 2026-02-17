namespace Moongate.UO.Data.Types;

public enum UOLoginDeniedReason : byte
{
    IncorrectNameOrPassword = 0x00,
    AccountAlreadyInUse = 0x01,
    AccountBlocked = 0x02,
    InvalidCredentials = 0x03,
    CommunicationProblem = 0x04,
    IgrConcurrencyLimitMet = 0x05,
    IgrTimeLimitMet = 0x06,
    GeneralIgrAuthenticationFailure = 0x07
}
