using UnityEngine;

public class VoiceCapture : MonoBehaviour
{
    public int sampleRate = 16000;
    public int recordingLength = 1; // 1 second buffer

    private AudioClip micClip;
    private string micName;
    private bool isRecording = false;

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            micName = Microphone.devices[0];
            micClip = Microphone.Start(micName, true, recordingLength, sampleRate);
            isRecording = true;
        }
    }

    public float[] GetAudioData()
    {
        if (!isRecording) return null;

        int micPosition = Microphone.GetPosition(micName);
        float[] data = new float[micClip.samples * micClip.channels];
        micClip.GetData(data, 0);

        return data;
    }
}
