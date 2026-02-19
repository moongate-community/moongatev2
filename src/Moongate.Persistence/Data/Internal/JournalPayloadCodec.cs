using MemoryPack;
using Moongate.Persistence.Data.Persistence;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Persistence.Data.Internal;

/// <summary>
/// Encodes and decodes operation payloads used by journal entries.
/// </summary>
internal static class JournalPayloadCodec
{
    public static UOAccountEntity DecodeAccount(byte[] payload)
        => SnapshotMapper.ToAccountEntity(MemoryPackSerializer.Deserialize<AccountSnapshot>(payload)!);

    public static UOItemEntity DecodeItem(byte[] payload)
        => SnapshotMapper.ToItemEntity(MemoryPackSerializer.Deserialize<ItemSnapshot>(payload)!);

    public static UOMobileEntity DecodeMobile(byte[] payload)
        => SnapshotMapper.ToMobileEntity(MemoryPackSerializer.Deserialize<MobileSnapshot>(payload)!);

    public static Serial DecodeSerial(byte[] payload)
        => (Serial)MemoryPackSerializer.Deserialize<uint>(payload);

    public static byte[] EncodeAccount(UOAccountEntity account)
        => MemoryPackSerializer.Serialize(SnapshotMapper.ToAccountSnapshot(account));

    public static byte[] EncodeItem(UOItemEntity item)
        => MemoryPackSerializer.Serialize(SnapshotMapper.ToItemSnapshot(item));

    public static byte[] EncodeMobile(UOMobileEntity mobile)
        => MemoryPackSerializer.Serialize(SnapshotMapper.ToMobileSnapshot(mobile));

    public static byte[] EncodeSerial(Serial id)
        => MemoryPackSerializer.Serialize((uint)id);
}
