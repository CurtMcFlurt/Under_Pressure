using Unity.Netcode;
using UnityEngine;

public class PlayerWeightSyncer : NetworkBehaviour
{
    [Header("Weight Settings")]
    public WeightChangers walkSound;

    [Header("Sound Values")]
    public float regularSound = 1f;
    public int regularRange = 1;
    public float extraRunningSound = 0.5f;
    public int extraRunningRange = 1;

    public NetworkVariable<bool> isMoving = new(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isRunning = new(writePerm: NetworkVariableWritePermission.Owner);

    public void SetMovementState(bool moving, bool running)
    {
        if (IsOwner)
        {
            isMoving.Value = moving;
            isRunning.Value = running;
        }
    }

    private void FixedUpdate()
    {
        if (!IsSpawned || walkSound == null) return;

      
            if (isMoving.Value)
            {
                if (isRunning.Value)
                    CreateSoundRun();
                else
                    CreateSound();
            }
        
    }

    private void CreateSound()
    {
        Debug.Log("Sound Norm");
        walkSound.myHeat.sound = regularSound;
        walkSound.range = regularRange;
        walkSound.enabled = true;
    }

    private void CreateSoundRun()
    {
        Debug.Log("Sound Run");
        walkSound.myHeat.sound = regularSound + extraRunningSound;
        walkSound.range = regularRange + extraRunningRange;
        walkSound.enabled = true;
    }
}
