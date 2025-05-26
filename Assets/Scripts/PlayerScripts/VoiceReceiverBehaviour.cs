using Concentus;
using Concentus.Structs;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VoiceReceiverBehaviour : NetworkBehaviour
{
    private OpusDecoder decoder;
    private short[] decodeBuffer;
    private float[] floatBuffer;

    public AudioSource audioSource;

    private static Dictionary<ulong, VoiceReceiverBehaviour> receivers = new();

    private Queue<float> playbackBuffer = new(); // Buffer PCM samples for playback
    private const int sampleRate = 16000;
    private const int channels = 1;

    private AudioClip playbackClip;
    private const int playbackClipLengthSeconds = 1; // 1-second clip buffer

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

        // Create a streaming AudioClip for continuous playback
        playbackClip = AudioClip.Create("VoicePlayback", sampleRate * playbackClipLengthSeconds, channels, sampleRate, true, OnAudioRead, OnAudioSetPosition);

        if (audioSource != null)
        {
            audioSource.clip = playbackClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        Debug.Log("VoiceReceiverBehaviour initialized with playback buffer");
    }

    // This is called on the audio thread by Unity to fill the audio buffer
    private void OnAudioRead(float[] data)
    {
        lock (bufferLock)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (playbackBuffer.Count > 0)
                    data[i] = playbackBuffer.Dequeue();
                else
                    data[i] = 0f; // Silence if no data buffered
            }
        }
    }

    private void OnAudioSetPosition(int newPosition)
    {
        // Reset playback buffer on position change
        lock (bufferLock)
        {
            playbackBuffer.Clear();
        }
    }

    public void ReceiveVoiceData(byte[] encoded)
    {
        if (decoder == null)
        {
            Debug.LogWarning("Decoder not initialized!");
            return;
        }

        int decodedSamples = decoder.Decode(encoded, 0, encoded.Length, decodeBuffer, 0, decodeBuffer.Length, false);
        for (int i = 0; i < decodedSamples; i++)
            floatBuffer[i] = decodeBuffer[i] / 32768f;

        lock (bufferLock)
        {
            // Enqueue decoded samples for playback
            foreach (var sample in floatBuffer.AsSpan(0, decodedSamples))
                playbackBuffer.Enqueue(sample);
        }

        Debug.Log($"Received voice data: {encoded.Length} bytes, decoded to {decodedSamples} samples. Buffer length: {playbackBuffer.Count}");
    }

    private void OnDestroy()
    {
        receivers.Remove(OwnerClientId);
    }
}
