using UnityEngine;

public class SocketInteracting : MonoBehaviour
{
    public Cog activeCog;
    public float CorrectSize;
    public bool Correct;
    // Update is called once per frame
    void Update()
    {
        if (activeCog != null)
        {
            if (Mathf.Abs(activeCog.Size - CorrectSize) < .15f)
            {
                Correct = true;
            }
            else Correct = false;
        }
        else Correct = false;
        
    }
}
