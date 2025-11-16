namespace BetterBetterTeleporter.Networking;

public interface ISyncable
{
    void Sync();
    void Broadcast();
    void BeginListening();
    void StopListening(bool resetToLocalConfig = true);
}
