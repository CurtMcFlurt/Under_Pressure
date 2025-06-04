using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneObjectShuffler : MonoBehaviour
{
    public string tagToReplace = "Replaceable";
    public GameObject replacementPrefabA;
    public GameObject replacementPrefabB;
    public float positionTolerance = 0.1f; // Helps round positions to avoid float drift
    public float spacing = 1.5f; // How far apart objects are placed in the scene
    public bool includeDiagonals = true;

#if UNITY_EDITOR
    [ContextMenu("Replace 1/10 with A and Surrounding Replaceables with B")]
    public void ReplaceOneTenthWithAAndSurroundWithB()
    {
        if (replacementPrefabA == null || replacementPrefabB == null)
        {
            Debug.LogError("Both replacement prefabs must be assigned.");
            return;
        }

        GameObject[] allReplaceables = GameObject.FindGameObjectsWithTag(tagToReplace);
        int total = allReplaceables.Length;

        if (total < 10)
        {
            Debug.LogWarning("Not enough replaceable objects to select 1/10.");
            return;
        }

        // Position lookup for quick neighbor search
        Dictionary<Vector3, GameObject> positionLookup = new Dictionary<Vector3, GameObject>();
        foreach (GameObject obj in allReplaceables)
        {
            Vector3 roundedPos = RoundVector(obj.transform.position, positionTolerance);
            if (!positionLookup.ContainsKey(roundedPos))
                positionLookup.Add(roundedPos, obj);
        }

        // Shuffle objects
        List<GameObject> shuffled = new List<GameObject>(allReplaceables);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int j = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        int countToReplace = total / 10;
        HashSet<Vector3> alreadyReplaced = new HashSet<Vector3>();
        List<Vector3> neighborOffsets = GetNeighborOffsets();

        for (int i = 0; i < countToReplace; i++)
        {
            GameObject original = shuffled[i];
            Vector3 basePos = RoundVector(original.transform.position, positionTolerance);
            Quaternion rot = original.transform.rotation;

            // Replace with A
            Undo.RegisterFullObjectHierarchyUndo(original, "Replace A");
            Undo.DestroyObjectImmediate(original);
            GameObject aObj = (GameObject)PrefabUtility.InstantiatePrefab(replacementPrefabA);
            aObj.transform.position = basePos;
            aObj.transform.rotation = rot;
            Undo.RegisterCreatedObjectUndo(aObj, "Create A");

            alreadyReplaced.Add(basePos);

            // Replace neighbors (only if they are existing "Replaceable" objects)
            foreach (Vector3 offset in neighborOffsets)
            {
                Vector3 neighborPos = RoundVector(basePos + offset * spacing, positionTolerance);
                if (positionLookup.TryGetValue(neighborPos, out GameObject neighbor))
                {
                    if (neighbor == null || alreadyReplaced.Contains(neighborPos))
                        continue;

                    Quaternion neighborRot = neighbor.transform.rotation;

                    Undo.RegisterFullObjectHierarchyUndo(neighbor, "Replace B");
                    Undo.DestroyObjectImmediate(neighbor);
                    GameObject bObj = (GameObject)PrefabUtility.InstantiatePrefab(replacementPrefabB);
                    bObj.transform.position = neighborPos;
                    bObj.transform.rotation = neighborRot;
                    Undo.RegisterCreatedObjectUndo(bObj, "Create B");

                    alreadyReplaced.Add(neighborPos);
                }
            }
        }

        Debug.Log($"Replaced {countToReplace} of {total} objects with A and surrounded them with B.");
    }
#endif

    // ✅ These are required outside of UNITY_EDITOR to avoid Play Mode errors
    private Vector3 RoundVector(Vector3 v, float tolerance)
    {
        return new Vector3(
            Mathf.Round(v.x / tolerance) * tolerance,
            Mathf.Round(v.y / tolerance) * tolerance,
            Mathf.Round(v.z / tolerance) * tolerance
        );
    }

    private List<Vector3> GetNeighborOffsets()
    {
        List<Vector3> offsets = new List<Vector3>
        {
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back
        };

        if (includeDiagonals)
        {
            offsets.Add(Vector3.left + Vector3.forward);
            offsets.Add(Vector3.left + Vector3.back);
            offsets.Add(Vector3.right + Vector3.forward);
            offsets.Add(Vector3.right + Vector3.back);
        }

        return offsets;
    }
}
