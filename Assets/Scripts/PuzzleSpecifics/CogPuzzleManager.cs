using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CogPuzzleManager : MonoBehaviour
{
    public List<Cog> myCogs = new List<Cog>();
    public Collider myCollider;

    void OnEnable()
    {
        myCollider = GetComponent<Collider>();
        myCogs.Clear();

        foreach (Transform child in transform)
        {
            Cog cog = child.GetComponent<Cog>();
            if (cog != null)
            {
                myCogs.Add(cog);
            }
        }
    }

    void FixedUpdate()
    {
        foreach (var cog in myCogs)
        {
            if (!myCollider.bounds.Contains(cog.transform.position))
            {
                cog.Reset();
                Debug.Log("getMyCog");
            }
        }
    }
}
