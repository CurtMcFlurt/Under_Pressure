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

    public NetworkVariable<bool> inUse = new(
      false,
       NetworkVariableReadPermission.Everyone, // default is Everyone
      NetworkVariableWritePermission.Owner
  );

    public NetworkVariable<bool> foundLost = new(
     false,
       NetworkVariableReadPermission.Everyone, // default is Everyone
      NetworkVariableWritePermission.Owner
    );

    public NetworkVariable<bool> puzzleSolved = new(
        false,
       NetworkVariableReadPermission.Everyone, // default is Everyone
      NetworkVariableWritePermission.Owner
    );


    public StartPuzzleInteract puzzleInt;

    private bool notWon;

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
                SocketInteracting sock = child2.GetComponent<SocketInteracting>();
                if (sock != null && !mySocks.Contains(sock))
                {
                    mySocks.Add(sock);
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner)
        {
            puzzleInt.taken = inUse.Value;
        }
        else if (!puzzleInt.taken)
        {
            inUse.Value = false;
        }

        foreach (var cog in myCogs)
        {
            if (!myCollider.bounds.Contains(cog.transform.position))
            {
                cog.Reset();
                Debug.Log("Resetting cog position");
            }
        }

        int totalSolved = 0;
        foreach (var sock in mySocks)
        {
            if (sock.Correct) totalSolved++;
        }

        if (totalSolved >= mySocks.Count && totalSolved != 0 && IsOwner)
        {
            puzzleSolved.Value = true;
            Debug.Log("Puzzle solved!");
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

    [Rpc(SendTo.Server)]
    public void RequestPuzzleStartRpc(ulong playerId, bool lostCogFound)
    {
        NetworkObject.ChangeOwnership(playerId);
        inUse.Value = true;
        foundLost.Value = lostCogFound;
    }
}
