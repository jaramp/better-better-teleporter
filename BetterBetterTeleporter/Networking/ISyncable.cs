using Unity.Netcode;

namespace BetterBetterTeleporter.Networking;

public interface ISyncable
{
    int GetSize();
    void ResetValue();
    void SetFromReader(FastBufferReader reader);
    void WriteToWriter(FastBufferWriter writer);
}
