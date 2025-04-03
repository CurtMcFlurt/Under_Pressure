using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Hexagon_HeatmapManager : MonoBehaviour
{
    public HexagonalWeight hexWeighter;
    public List<GameObject> staticWeightChangers = new List<GameObject>();
   
    void Start()
    {
        if (hexWeighter == null)hexWeighter=GetComponent<HexagonalWeight>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 Axial2World(HexCell con)
    {
        Vector3 centre = new Vector3();
        centre.x = hexWeighter.cellSize * (1.5f * con.hexCoords.x);
        centre.z = hexWeighter.cellSize * ((Mathf.Sqrt(3) / 2) * con.hexCoords.x + Mathf.Sqrt(3) * con.hexCoords.y);
        centre.y = con.height;
        return centre;
    }
}
