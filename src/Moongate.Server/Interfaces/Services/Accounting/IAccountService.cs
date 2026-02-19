using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Types;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Server.Interfaces.Services.Accounting;

/// <summary>
/// Defines account management and authentication operations.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Creates a new account with the provided credentials and account type.
    /// </summary>
    /// <param name="username">Account username.</param>
    /// <param name="password">Plain text password to hash and store.</param>
    /// <param name="accountType">Account role/type.</param>
    Task CreateAccountAsync(string username, string password, AccountType accountType = AccountType.Regular);

    /// <summary>
    /// Deletes an account by identifier.
    /// </summary>
    /// <param name="accountId">Account serial identifier.</param>
    Task DeleteAccountAsync(Serial accountId);

    /// <summary>
    /// Checks whether an account with the given username exists.
    /// </summary>
    /// <param name="username">Account username.</param>
    /// <returns><see langword="true"/> if the account exists; otherwise <see langword="false"/>.</returns>
    Task<bool> CheckAccountExistsAsync(string username);

    /// <summary>
    /// Attempts to authenticate an account with username and password.
    /// </summary>
    /// <param name="username">Account username.</param>
    /// <param name="password">Plain text password.</param>
    /// <returns>The account when authentication succeeds; otherwise <see langword="null"/>.</returns>
    Task<UOAccountEntity?> LoginAsync(string username, string password);

    /// <summary>
    /// Gets an account by identifier.
    /// </summary>
    /// <param name="accountId">Account serial identifier.</param>
    /// <returns>The account when found; otherwise <see langword="null"/>.</returns>
    Task<UOAccountEntity?> GetAccountAsync(Serial accountId);
}
