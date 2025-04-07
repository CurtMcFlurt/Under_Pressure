using UnityEngine;

public class EnsureSetupIsDone : MonoBehaviour
{
    public GameObject deeper;
    public HexagonalWeight currentWeight;
    

    // Update is called once per frame
    void Update()
    {
        if(currentWeight.walkableHexagons.Count != 0)
        {
            deeper.SetActive(true);
                }
    }
}
