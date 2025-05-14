using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
[System.Serializable]
public enum TargetType { Objective, Player, Unknown, Beast }
[System.Serializable]
public class CompassTargetData
{
    public Transform targetTransform;
    public Sprite icon;
    public TargetType type;
}
public class CompassHandler : MonoBehaviour
{
    [System.Serializable]
    public class CompassTarget
    {
        public Transform target;
        public Image marker;
        public Sprite icon; // Unique icon for this target
    }

    public RawImage compassImage;
    public Transform player;
    public RectTransform compassRect;
    public GameObject markerPrefab;
    public float maxVisibleAngle = 90f;

    private List<CompassTarget> targets = new List<CompassTarget>();

    void Update()
    {
        if (player == null) return;

        // Rotate compass background based on player rotation
        compassImage.uvRect = new Rect(player.eulerAngles.y / 360f, 0f, 1f, 1f);

        // Update marker positions
        foreach (var ct in targets)
        {
            if (ct.target == null || ct.marker == null) continue;

            Vector3 direction = ct.target.position - player.position;
            float angleToTarget = Vector3.SignedAngle(player.forward, direction, Vector3.up);

            if (Mathf.Abs(angleToTarget) > maxVisibleAngle)
            {
                ct.marker.gameObject.SetActive(false);
                continue;
            }

            ct.marker.gameObject.SetActive(true);

            float compassWidth = compassRect.rect.width;
            float normalizedAngle = angleToTarget / maxVisibleAngle;
            float xPos = (compassWidth / 2f) * normalizedAngle;

            ct.marker.rectTransform.anchoredPosition = new Vector2(xPos, 0f);
        }
    }

    public void AddTarget(Transform targetTransform, Sprite icon)
    {
        if (markerPrefab == null || compassRect == null)
        {
            Debug.LogWarning("CompassHandler: MarkerPrefab or CompassRect not set.");
            return;
        }

        GameObject markerObj = Instantiate(markerPrefab, compassRect);
        Image markerImage = markerObj.GetComponent<Image>();
        markerImage.sprite = icon;
        markerImage.enabled = true;

        CompassTarget ct = new CompassTarget
        {
            target = targetTransform,
            marker = markerImage,
            icon = icon
        };

        targets.Add(ct);
    }

    public void RemoveTarget(Transform targetTransform)
    {
        CompassTarget ct = targets.Find(t => t.target == targetTransform);
        if (ct != null)
        {
            if (ct.marker != null)
                Destroy(ct.marker.gameObject);

            targets.Remove(ct);
        }
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

    public void UnPackData(Component sender, object data)
    {
        // Check if the incoming data is of type CompassTargetData
        if (data is CompassTargetData targetData)
        {
            if (targetData.targetTransform == null || targetData.icon == null)
            {
                Debug.LogWarning("Invalid CompassTargetData: Missing transform or icon.");
                return;
            }

            // Add the target to the compass
            AddTarget(targetData.targetTransform, targetData.icon);

            Debug.Log($"Compass target added: {targetData.targetTransform.name}");
        }
        else
        {
            Debug.LogWarning("UnPackData received an invalid type. Expected CompassTargetData.");
        }
    }


}
