using Moongate.Persistence.Data.Internal;
using Moongate.Persistence.Data.Persistence;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Types;

namespace Moongate.Persistence.Services.Persistence;

/// <summary>
/// Coordinates repositories plus snapshot/journal load-save lifecycle.
/// </summary>
public sealed class PersistenceUnitOfWork : IPersistenceUnitOfWork
{
    private readonly BinaryJournalService _journalService;
    private readonly MemoryPackSnapshotService _snapshotService;
    private readonly PersistenceStateStore _stateStore = new();

    public PersistenceUnitOfWork(PersistenceOptions options)
    {
        _snapshotService = new(options.SnapshotFilePath);
        _journalService = new(options.JournalFilePath);

        Accounts = new AccountRepository(_stateStore, _journalService);
        Mobiles = new MobileRepository(_stateStore, _journalService);
        Items = new ItemRepository(_stateStore, _journalService);
    }

    public IAccountRepository Accounts { get; }

    public IItemRepository Items { get; }

    public IMobileRepository Mobiles { get; }

    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await _snapshotService.LoadAsync(cancellationToken);

        lock (_stateStore.SyncRoot)
        {
            _stateStore.AccountsById.Clear();
            _stateStore.AccountNameIndex.Clear();
            _stateStore.MobilesById.Clear();
            _stateStore.ItemsById.Clear();
            _stateStore.LastSequenceId = 0;

            if (snapshot is not null)
            {
                for (var i = 0; i < snapshot.Accounts.Length; i++)
                {
                    var account = SnapshotMapper.ToAccountEntity(snapshot.Accounts[i]);
                    _stateStore.AccountsById[account.Id] = account;
                    _stateStore.AccountNameIndex[account.Username] = account.Id;
                }

                for (var i = 0; i < snapshot.Mobiles.Length; i++)
                {
                    var mobile = SnapshotMapper.ToMobileEntity(snapshot.Mobiles[i]);
                    _stateStore.MobilesById[mobile.Id] = mobile;
                }

                for (var i = 0; i < snapshot.Items.Length; i++)
                {
                    var item = SnapshotMapper.ToItemEntity(snapshot.Items[i]);
                    _stateStore.ItemsById[item.Id] = item;
                }

                _stateStore.LastSequenceId = snapshot.LastSequenceId;
            }
        }

        var journalEntries = await _journalService.ReadAllAsync(cancellationToken);

        lock (_stateStore.SyncRoot)
        {
            foreach (var entry in journalEntries.OrderBy(e => e.SequenceId))
            {
                ApplyEntry(entry);

                if (entry.SequenceId > _stateStore.LastSequenceId)
                {
                    _stateStore.LastSequenceId = entry.SequenceId;
                }
            }
        }
    }

    public async ValueTask SaveSnapshotAsync(CancellationToken cancellationToken = default)
    {
        WorldSnapshot snapshot;

        lock (_stateStore.SyncRoot)
        {
            snapshot = new()
            {
                Version = 1,
                CreatedUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                LastSequenceId = _stateStore.LastSequenceId,
                Accounts = [.. _stateStore.AccountsById.Values.Select(SnapshotMapper.ToAccountSnapshot)],
                Mobiles = [.. _stateStore.MobilesById.Values.Select(SnapshotMapper.ToMobileSnapshot)],
                Items = [.. _stateStore.ItemsById.Values.Select(SnapshotMapper.ToItemSnapshot)]
            };
        }

        await _snapshotService.SaveAsync(snapshot, cancellationToken);
        await _journalService.ResetAsync(cancellationToken);
    }

    private void ApplyEntry(JournalEntry entry)
    {
        switch (entry.OperationType)
        {
            case PersistenceOperationType.UpsertAccount:
                {
                    var account = JournalPayloadCodec.DecodeAccount(entry.Payload);
                    _stateStore.AccountsById[account.Id] = account;
                    _stateStore.AccountNameIndex[account.Username] = account.Id;

                    break;
                }
            case PersistenceOperationType.RemoveAccount:
                {
                    var id = JournalPayloadCodec.DecodeSerial(entry.Payload);

                    if (_stateStore.AccountsById.Remove(id, out var account))
                    {
                        _stateStore.AccountNameIndex.Remove(account.Username);
                    }

                    break;
                }
            case PersistenceOperationType.UpsertMobile:
                {
                    var mobile = JournalPayloadCodec.DecodeMobile(entry.Payload);
                    _stateStore.MobilesById[mobile.Id] = mobile;

                    break;
                }
            case PersistenceOperationType.RemoveMobile:
                {
                    var id = JournalPayloadCodec.DecodeSerial(entry.Payload);
                    _stateStore.MobilesById.Remove(id);

                    break;
                }
            case PersistenceOperationType.UpsertItem:
                {
                    var item = JournalPayloadCodec.DecodeItem(entry.Payload);
                    _stateStore.ItemsById[item.Id] = item;

                    break;
                }
            case PersistenceOperationType.RemoveItem:
                {
                    var id = JournalPayloadCodec.DecodeSerial(entry.Payload);
                    _stateStore.ItemsById.Remove(id);

                    break;
                }
        }
    }
}
