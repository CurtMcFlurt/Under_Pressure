using UnityEngine;

public class CompassTargetSender : MonoBehaviour
{
    [Header("Compass Marker Settings")]
    public Sprite iconSprite;

    [Header("Event Channel")]
    public GameEvent compassTargetEvent; // Your custom GameEvent asset

    private void OnEnable()
    {
        SendToCompass();
    }

    public void SendToCompass()
    {
        if (iconSprite == null)
        {
            Debug.LogWarning($"{name}: No icon assigned for compass marker.");
            return;
        }

        if (compassTargetEvent == null)
        {
            Debug.LogError($"{name}: No GameEvent assigned.");
            return;
        }

        CompassTargetData data = new CompassTargetData
        {
            targetTransform = this.transform,
            icon = iconSprite
        };

        compassTargetEvent.Raise(this, data);
        Debug.Log($"{name}: Compass target event sent.");
    }
}
