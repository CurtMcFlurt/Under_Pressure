using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Community.Discovery;
using UnityEngine;

public struct DiscoveryBroadcastData : INetworkSerializable
{
    public FixedString32Bytes ServerName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ServerName);
    }
}

public struct DiscoveryResponseData : INetworkSerializable
{
    public FixedString32Bytes ServerName;
    public FixedString32Bytes ServerAddress;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ServerName);
        serializer.SerializeValue(ref ServerAddress);
    }
}
