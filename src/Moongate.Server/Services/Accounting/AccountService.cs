using Moongate.Core.Utils;
using Moongate.Server.Interfaces.Services.Accounting;
using Moongate.Server.Interfaces.Services.Persistence;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;
using Serilog;

namespace Moongate.Server.Services.Accounting;

public class AccountService : IAccountService
{
    private readonly ILogger _logger = Log.ForContext<AccountService>();

    private readonly IPersistenceService _persistenceService;

    public AccountService(IPersistenceService persistenceService)
        => _persistenceService = persistenceService;

    public async Task<bool> CheckAccountExistsAsync(string username)
    {
        var exists = await _persistenceService.UnitOfWork.Accounts.ExistsAsync(a => a.Username == username);

        _logger.Information("Checked existence of account with username {Username}: {Exists}", username, exists);

        return exists;
    }

    public async Task CreateAccountAsync(string username, string password, AccountType accountType = AccountType.Regular)
    {
        var account = new UOAccountEntity
        {
            Id = _persistenceService.UnitOfWork.AllocateNextAccountId(),
            AccountType = accountType,
            Username = username,
            PasswordHash = HashUtils.HashPassword(password),
            IsLocked = false
        };

        var result = await _persistenceService.UnitOfWork.Accounts.AddAsync(account);

        if (result)
        {
            _logger.Information("Created account with username {Username}", username);
        }
        else
        {
            _logger.Error("Failed to create account with username {Username}", username);
        }
    }

    public async Task DeleteAccountAsync(Serial accountId)
    {
        var result = await _persistenceService.UnitOfWork.Accounts.RemoveAsync(accountId);

        if (result)
        {
            _logger.Information("Deleted account with ID {AccountId}", accountId);
        }
        else
        {
            _logger.Error("Failed to delete account with ID {AccountId}", accountId);
        }
    }

    public async Task<UOAccountEntity?> GetAccountAsync(Serial accountId)
    {
        var account = await _persistenceService.UnitOfWork.Accounts.GetByIdAsync(accountId);

        if (account != null)
        {
            _logger.Information("Retrieved account with ID {AccountId}", accountId);
        }
        else
        {
            _logger.Warning("No account found with ID {AccountId}", accountId);
        }

        return account;
    }

    public async Task<UOAccountEntity?> LoginAsync(string username, string password)
    {
        var account = await _persistenceService.UnitOfWork.Accounts.GetByUsernameAsync(username);

        if (account == null)
        {
            _logger.Warning("Login attempt failed for non-existent username {Username}", username);

            return null;
        }

        if (!HashUtils.VerifyPassword(password, account.PasswordHash))
        {
            _logger.Warning("Login attempt failed for username {Username} due to incorrect password", username);

            return null;
        }

        if (account.IsLocked)
        {
            _logger.Warning("Login attempt failed for locked account with username {Username}", username);

            return null;
        }

        account.LastLoginUtc = DateTime.UtcNow;
        await _persistenceService.UnitOfWork.Accounts.UpsertAsync(account);

        _logger.Information("Successful login for account with username {Username}", username);

        return account;
    }
}
