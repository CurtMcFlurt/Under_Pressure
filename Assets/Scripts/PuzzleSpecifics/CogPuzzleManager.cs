using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CogPuzzleManager : MonoBehaviour
{
    public List<Cog> myCogs = new List<Cog>();
    public GameObject cogParent;
    public Collider myCollider;
    public GameObject sockParent;
    public List<SocketInteracting> mySocks = new List<SocketInteracting>();
    public GameEvent winEvent;
    public string sendString;
    void OnEnable()
    {
        myCollider = GetComponent<Collider>();
        myCogs.Clear();

        foreach (Transform child in cogParent.transform)
        {
            Cog cog = child.GetComponent<Cog>();
            if (cog != null&& !myCogs.Contains(cog))
            {
                myCogs.Add(cog);
            }
        }
        foreach (Transform child in sockParent.transform)
        {
            foreach (Transform child2 in child)
            {


                SocketInteracting cog = child2.GetComponent<SocketInteracting>();
                if (cog != null && !mySocks.Contains(cog))
                {
                    mySocks.Add(cog);
                }
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
        int totalSolved = 0;
        for (int i = 0; i < mySocks.Count; i++)
        {
            if (mySocks[i].Correct) totalSolved++;
        }

        if(totalSolved>=mySocks.Count && totalSolved != 0)
        {
            SolvedPuzzle();
        }
    }


    public void SolvedPuzzle()
    {
        Debug.Log("Solved:" + sendString);
        winEvent.Raise(this, sendString);
    }
}
