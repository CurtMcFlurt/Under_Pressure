using System.Collections.Generic;
using System;
using UnityEngine;

public class HexGridScript
{

    public int Size = 5;
    public float cellSize = 1;

    public GameObject TilePrefab;
    [NonSerialized]
    public float[,,] gridArray;

    public List<Vector3> coordinate = new List<Vector3>();
    public List<Vector3> hexvalue = new List<Vector3>();

    private float q, r, s;
    // Start is called before the first frame update

    public HexGridScript(int Size, float cellSize)
    {
        this.Size = Size;
        this.cellSize = cellSize;

        Hex2Draw(Size);
    }


    private void Hex2Draw(int i)
    {

        gridArray = new float[i, i, i];

        for (int q = -i; q <= i; q++)
        {
            for (int r = -i; r <= i; r++)
            {
                for (int s = -i; s <= i; s++)
                {
                    if (q + r + s == 0)
                    {

                        Vector3 currentHex = new Vector3(q, r, s);
                        currentHex = Axial2Pixel(currentHex);
                        coordinate.Add(currentHex);
                        hexvalue.Add(new Vector3(q, r, s));
                    }
                }
            }
        }
    }
    public Vector3 Axial2Pixel(Vector3 con)
    {
        Vector3 centre = new Vector3();
        centre.x = cellSize * (1.5f * con.x);
        centre.z = cellSize * ((Mathf.Sqrt(3) / 2) * con.x + Mathf.Sqrt(3) * con.y);
        centre.y = 0;
        return centre;
    }






    private float HexDistance(Vector3 start, Vector3 end)
    {
        Vector3 vec = HexSubtract(start, end);

        return (Mathf.Abs(vec.x) + Mathf.Abs(vec.y) + Mathf.Abs(vec.z)) / 2;
    }

    private Vector3 HexSubtract(Vector3 first, Vector3 second)
    {
        return (new Vector3(first.x - second.x, first.y - second.y, first.z - second.z));
    }



}