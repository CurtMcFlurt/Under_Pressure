using UnityEngine;

public class EnsureSetupIsDone : MonoBehaviour
{
    public GameObject deeper;
    public HexagonalWeight currentWeight;
    public float waitForSeconds = 1;
    private float seconds;
    // Update is called once per frame
    void Update()
    {
        if (currentWeight.walkableHexagons.Count != 0 && seconds > waitForSeconds)
        {
            deeper.SetActive(true);

        }
        else
        {
           seconds += Time.deltaTime;
            
        }
    }
}
