using Concentus;
using Concentus.Enums;
using Concentus.Structs;
using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class VoiceSenderBehaviour : NetworkBehaviour
{
    public int sampleRate = 16000;
    public int frameSize = 320;
    private AudioClip micClip;
    private float[] micBuffer;
    private short[] shortBuffer;
    private byte[] encodedBuffer;
    private OpusEncoder encoder;
    public string micDevice;
    private int lastMicPos;


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            StartCoroutine(InitMicWithRetry());
    }

    private IEnumerator InitMicWithRetry()
    {
        float timeout = 5f; // Wait up to 5 seconds
        float timer = 0f;

        while (Microphone.devices.Length == 0 && timer < timeout)
        {
            Debug.Log("⏳ Waiting for microphone device to become available...");
            timer += Time.deltaTime;
            yield return null;
        }

        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("❌ No microphone devices detected after timeout.");
            yield break;
        }

        micDevice = Microphone.devices[0];
        micClip = Microphone.Start(micDevice, true, 1, sampleRate);

        micBuffer = new float[frameSize];
        shortBuffer = new short[frameSize];
        encodedBuffer = new byte[1024];
        encoder = (OpusEncoder)OpusCodecFactory.CreateEncoder(sampleRate, 1, OpusApplication.OPUS_APPLICATION_VOIP);
        encoder.Bitrate = 128000;
        encoder.UseVBR = true;
        Debug.Log($"✅ Microphone initialized: {micDevice}");
    }

    private void Update()
    {
        if (!IsOwner || micClip == null || encoder == null) return;

        int micPos = Microphone.GetPosition(micDevice);
        int samplesAvailable = (micPos - lastMicPos + micClip.samples) % micClip.samples;

        if (samplesAvailable >= frameSize)
        {
            micClip.GetData(micBuffer, lastMicPos);
            for (int i = 0; i < frameSize; i++)
            {
                float sample = Mathf.Clamp(micBuffer[i], -1f, 1f);
                shortBuffer[i] = (short)(sample * short.MaxValue);
            }

            int encodedLength = encoder.Encode(shortBuffer, 0, frameSize, encodedBuffer, 0, encodedBuffer.Length);
            byte[] compressed = new byte[encodedLength];
            Array.Copy(encodedBuffer, compressed, encodedLength);


            TransmitVoiceRpc(compressed, NetworkManager.Singleton.LocalClientId);  // ✅ Correct call with sender ID

            lastMicPos = (lastMicPos + frameSize) % micClip.samples;
        }
    }
    [Rpc(SendTo.Everyone)]
    private void TransmitVoiceRpc(byte[] compressedData, ulong senderId)
    {
        if (NetworkManager.Singleton.LocalClientId == senderId)
            return; // 🚫 Don't process your own voice

        var receiver = VoiceReceiverBehaviour.GetReceiver(senderId);
        if (receiver != null)
        {
            receiver.ReceiveVoiceData(compressedData);
        }
    }


}