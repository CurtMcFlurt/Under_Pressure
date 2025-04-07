using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    const int threadGroupSize = 1024;

    public BoidSettings settings;
    public ComputeShader compute;
    public HashSet<GameObject> targets= new HashSet<GameObject>();

    public List <Boids> boids;
    public Collider boundCollider;
    [System.Obsolete]
    void Start()
    {
        var boidies = FindObjectsOfType<Boids>();
        List<Boids> tempboids = boids.ToList();
        foreach (Boids b in boids)
        {
           
                b.Initialize(settings, null);
             

        }

     

        if(boundCollider != null)
        {
            boundCollider=GetComponent<Collider>();
        }
    }
    void Update()
    {
     

       
        if (boids != null)
        {

            int numBoids = boids.Count;
            var boidData = new BoidData[numBoids];

            for (int i = 0; i < boids.Count; i++)
            {
                boidData[i].position = boids[i].position;
                boidData[i].direction = boids[i].forward;
              
            }

            var boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
            boidBuffer.SetData(boidData);

            compute.SetBuffer(0, "boids", boidBuffer);
            compute.SetInt("numBoids", boids.Count);
            compute.SetFloat("viewRadius", settings.perceptionRadius);
            compute.SetFloat("avoidRadius", settings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);
            compute.Dispatch(0, threadGroups, 1, 1);

            boidBuffer.GetData(boidData);

            for (int i = 0; i < boids.Count; i++)
            {
                boids[i].avgFlockHeading = boidData[i].flockHeading;
                boids[i].centreOfFlockmates = boidData[i].flockCentre;
                boids[i].avgAvoidanceHeading = boidData[i].avoidanceHeading;
                boids[i].numPerceivedFlockmates = boidData[i].numFlockmates;
                if(targets.Count == 0)
                {
                    boids[i].target = null;
                }
                else
                {
                    foreach (var v in targets)
                    {

                        if (boids[i].target == null)
                        {
                            boids[i].target = v.transform;

                        }
                        else if ((boids[i].target.position - boids[i].position).magnitude < (boids[i].target.position-v.transform.position).magnitude)
                        {
                            boids[i].target = v.transform;
                        }
                    }
                }
                if(boundCollider != null)
                {
                    if (!boundCollider.bounds.Contains(boids[i].position))
                    {
                        boids[i].TeleportBoid(transform.position);
                        Debug.Log("escape");
                    }
                }

                    
                boids[i].UpdateBoid();
            }

            boidBuffer.Release();
        }
    }
    public struct BoidData
    {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public static int Size
        {
            get
            {
                return sizeof(float) * 3 * 5 + sizeof(int);
            }
        }
    }

    
}
