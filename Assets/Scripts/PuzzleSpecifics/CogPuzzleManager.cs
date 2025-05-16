using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class CogPuzzleManager : NetworkBehaviour
{
    public List<Cog> myCogs = new List<Cog>();
    public GameObject cogParent;
    public Collider myCollider;
    public GameObject sockParent;
    public List<SocketInteracting> mySocks = new List<SocketInteracting>();
    public GameEvent winEvent;
    public string sendString;

    public NetworkVariable<bool> puzzleSolved = new NetworkVariable<bool>();

    private void OnEnable()
    {
        myCollider = GetComponent<Collider>();
        myCogs.Clear();

        foreach (Transform child in cogParent.transform)
        {
            Cog cog = child.GetComponent<Cog>();
            if (cog != null && !myCogs.Contains(cog))
            {
                myCogs.Add(cog);
            }
        }

        mySocks.Clear();
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

    private bool notWon;
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
            Debug.Log(totalSolved + " ammount solved");
        }

        if (totalSolved >= mySocks.Count && totalSolved != 0)
        {
            puzzleSolved.Value = true; // This will trigger OnPuzzleSolvedChanged on all clients
            Debug.Log("puzzle should be solved");
        }

        if (puzzleSolved.Value && !notWon)
        {
            SolvedPuzzle();
        }
        
        
    }

  

    public void SolvedPuzzle()
    {
        Debug.Log("Solved: " + sendString);
        winEvent.Raise(this, sendString);
        notWon = true;
    }
}
