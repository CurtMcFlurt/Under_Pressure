using Concentus;
using Concentus.Structs;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using FMOD;
using FMODUnity;
using System.Runtime.InteropServices;
using Debug = UnityEngine.Debug;

public class VoiceReceiverBehaviour : NetworkBehaviour
{
    private OpusDecoder decoder;
    private short[] decodeBuffer;
    private float[] floatBuffer;

    private static Dictionary<ulong, VoiceReceiverBehaviour> receivers = new();

    private const int sampleRate = 16000;
    private const int channels = 1;

    private object bufferLock = new object();

    public static void Register(ulong clientId, VoiceReceiverBehaviour receiver)
    {
        receivers[clientId] = receiver;
    }

    public static VoiceReceiverBehaviour GetReceiver(ulong clientId)
    {
        receivers.TryGetValue(clientId, out var r);
        return r;
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            Register(OwnerClientId, this);
        }
    }

    void Awake()
    {
        decoder = (OpusDecoder)OpusCodecFactory.CreateDecoder(sampleRate, channels);
        decodeBuffer = new short[320];
        floatBuffer = new float[320];

        Debug.Log("VoiceReceiverBehaviour initialized with FMOD playback");
    }

    public void ReceiveVoiceData(byte[] encoded)
    {
        if (decoder == null)
        {
            Debug.LogWarning("Decoder not initialized!");
            return;
        }

        int decodedSamples = decoder.Decode(encoded, 0, encoded.Length, decodeBuffer, 0, decodeBuffer.Length, false);

        // Convert short[] to byte[] for FMOD
        byte[] pcmBytes = new byte[decodedSamples * sizeof(short)];
        Buffer.BlockCopy(decodeBuffer, 0, pcmBytes, 0, pcmBytes.Length);

        PlayFmodVoice(pcmBytes, decodedSamples);

      //  Debug.Log($"[VoiceReceiver] Received {encoded.Length} bytes, decoded to {decodedSamples} samples");
    }

    private void PlayFmodVoice(byte[] pcmData, int sampleCount)
    {
        FMOD.CREATESOUNDEXINFO exinfo = new FMOD.CREATESOUNDEXINFO();
        exinfo.cbsize = Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO));
        exinfo.length = (uint)pcmData.Length;
        exinfo.numchannels = channels;
        exinfo.defaultfrequency = sampleRate;
        exinfo.format = FMOD.SOUND_FORMAT.PCM16;

        FMOD.Sound sound;
        FMOD.RESULT result = RuntimeManager.CoreSystem.createSound(
            pcmData,
            FMOD.MODE.OPENMEMORY | FMOD.MODE.OPENRAW | FMOD.MODE.LOOP_OFF,
            ref exinfo,
            out sound
        );

        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError($"[FMOD] Failed to create sound: {result}");
            return;
        }
        FMOD.ChannelGroup nullGroup = new FMOD.ChannelGroup(IntPtr.Zero);
        result = RuntimeManager.CoreSystem.playSound(sound, nullGroup, false, out FMOD.Channel channel);
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError($"[FMOD] Failed to play sound: {result}");
            return;
        }

        // Optionally release the sound after playback
        sound.release();
    }

    private void OnDestroy()
    {
        receivers.Remove(OwnerClientId);
    }
}
