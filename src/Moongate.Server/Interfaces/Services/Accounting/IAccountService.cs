using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;

namespace Moongate.Server.Interfaces.Services.Accounting;

public interface IAccountService
{
    Task CreateAccountAsync(string username, string password, AccountType accountType = AccountType.Regular);

    Task DeleteAccountAsync(Serial accountId);

    Task<bool> CheckAccountExistsAsync(string username);

    Task<UOAccountEntity?> LoginAsync(string username, string password);

    Task<UOAccountEntity?> GetAccountAsync(Serial accountId);
}
