using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public struct DeepWalkerMood
{
    public float anger;
    public float hunger;
    public float drowsy;
    public float alertness;

    public DeepWalkerMood(float anger=0,float hunger=0,float drowsy=0,float alertness=0)
    {
        this.alertness = alertness;
        this.drowsy = drowsy;
        this.hunger = hunger;
        this.anger = anger;
    }
}
public class DeepWalkerLogic : MonoBehaviour
{
    public HexagonalWeight WeightMap;
    public DeepWalkerMood mood;

    public AnimationCurve Roaming;
    public AnimationCurve tracking;
    public AnimationCurve hunting;
    public AnimationCurve sleeping;
    public AnimationCurve eating;
    public float updateTimer;
    public ScriptableBehaviour activeLogic;
    [Range(0, 1)]
    public float VictimPositionCertainty = 0;
    public int areaSizes = 3;
    public int hearingRange = 7;
   
    private Dictionary<Vector3, HexCell> hexMap = new();
    private Dictionary<Vector3, HexCell> inHexRange = new();
    private Dictionary<Vector3, HexCell> outHexRange = new();
    private HexCell myHex;
    private HexCell optimalSafety;
    private HexCell optimalFood;
    private HexCell optimalScouting;
    private HexCell probableVictimPosition;
    private float timeFood=-1;
    private float timeSafety=-1;
    private HexCell loudestReactHex;
    void OnEnable()
    {
        if (WeightMap == null)
            WeightMap = Object.FindObjectsByType<HexagonalWeight>(FindObjectsSortMode.None)[0];

        mood = new DeepWalkerMood(0, 0, 0, 0);
        loudestReactHex=new HexCell();
        loudestReactHex.weight.sound = 1;
        AwakenTheBeast(WeightMap.walkableHexagons);
    }

    void Update()
    {
        UpdateVision();
        if (updateSafety && timeSafety <= 0)
        {
            updateSafety = false;
            timeSafety = updateTimer;
            FindOptimalHex(0);
        }
        else timeSafety -= Time.deltaTime;
        if (updateFood && timeFood <= 0)
        {
            updateFood = false;
            timeFood = updateTimer;
            FindOptimalHex(1);
        }
        else timeFood -= Time.deltaTime;

        if (reactToSound)
        {
            Debug.Log("looking for loud");
            probableVictimPosition = FindSoundOrigin(loudestReactHex);
            loudestReactHex = new HexCell();
        }
        DecideLogic();
        
        activeLogic.Behave(this);
    }

    public void AwakenTheBeast(Dictionary<Vector3, HexCell> mapMemory)
    {
        hexMap = mapMemory;
        myHex = HexMath.NearestHex(transform.position, mapMemory.Values.ToList(), WeightMap.cellSize);

        optimalSafety = FindOptimalHex(0);
        optimalFood = FindOptimalHex(1);
        optimalScouting = FindOptimalHex(2);
    }


    public HexCell FindSoundOrigin(HexCell soundStartFound)
    {
        // Initialize loudest with the starting hex
        loudestReactHex = soundStartFound;
        float loudestValue = soundStartFound.weight.sound;

        // BFS-style search
        Queue<HexCell> openSet = new Queue<HexCell>();
        HashSet<Vector3> visited = new HashSet<Vector3>();

        openSet.Enqueue(soundStartFound);
        visited.Add(soundStartFound.hexCoords);

        while (openSet.Count > 0)
        {
            HexCell current = openSet.Dequeue();

            // Check if this is the new loudest
            if (current.weight.sound > loudestValue)
            {
                loudestValue = current.weight.sound;
                loudestReactHex = current;
            }

            // Check neighbors
            foreach (Vector3 direction in HexMath.cubeDirectionVectors)
            {
                Vector3 neighborCoords = current.hexCoords + direction;

                if (visited.Contains(neighborCoords)) continue;

                if (WeightMap.walkableHexagons.TryGetValue(neighborCoords, out HexCell neighbor))
                {
                    if (neighbor.weight.sound >= current.weight.sound)
                    {
                        openSet.Enqueue(neighbor);
                        visited.Add(neighborCoords);
                    }
                }
            }
        }
        return loudestReactHex;
      
    }


    public HexCell FindOptimalHex(int type)
    {
        HexCell optimalHex = myHex;
        float bestValue = 0;

        foreach (var hex in hexMap.Values)
        {
            float weightSum = 0;

            foreach (var subHex in hexMap.Values)
            {
                if (HexMath.HexDistance(hex.hexCoords, subHex.hexCoords) > areaSizes) continue;

                switch (type)
                {
                    case 0: weightSum += subHex.weight.safety; break;
                    case 1: weightSum += subHex.weight.food; break;
                    case 2: weightSum += subHex.timeSinceChecked; break;
                }
            }

            if (weightSum > bestValue)
            {
                optimalHex = hex;
                bestValue = weightSum;
            }
        }

        return optimalHex;
    }

    private void DecideLogic()
    {
        if (compareCurves(Roaming, tracking, mood.alertness))
        {
            if (compareCurves(tracking, hunting, mood.anger))
            {
                // hunting behavior
            }
            else
            {
                // tracking behavior
            }
            return;
        }

        if (compareCurves(Roaming, eating, mood.hunger)) 
        { 
            
            return;
        
        }
        if (compareCurves(Roaming, sleeping, mood.drowsy)) { 
            
            return; 
        
        
        
        }

        // Roaming behavior
    }
  
    public bool compareCurves(AnimationCurve neutral, AnimationCurve superceding, float value)
    {
        return neutral.Evaluate(value) > superceding.Evaluate(value);
    }
    private bool updateFood, updateSafety, reactToSound;
    public void UpdateVision()
    {
        myHex = HexMath.NearestHex(transform.position, hexMap.Values.ToList(), WeightMap.cellSize);
        inHexRange.Clear();
        outHexRange.Clear();

        foreach (var kvp in hexMap)
        {
            Vector3 key = kvp.Key;
            HexCell cell = kvp.Value;

            int dist = HexMath.HexDistance(myHex.hexCoords, cell.hexCoords);

            if (dist <= hearingRange)
            {
                cell.timeSinceChecked = 0;
              
                if (cell.weight.food != WeightMap.walkableHexagons[kvp.Key].weight.food) { updateFood = true; timeFood = 1; }
                if (cell.weight.safety != WeightMap.walkableHexagons[kvp.Key].weight.safety){ updateSafety = true; timeSafety = 1;}
                if (cell.weight.sound != WeightMap.walkableHexagons[kvp.Key].weight.sound){ reactToSound = true; if (cell.weight.sound > loudestReactHex.weight.sound) loudestReactHex = cell;  }

            inHexRange.Add(kvp.Key,cell);
            }
            else
            {
                cell.timeSinceChecked += Time.deltaTime;
                if (inHexRange.ContainsKey(kvp.Key)) inHexRange.Remove(kvp.Key);
                outHexRange.Add(kvp.Key,cell);
            }
        }
    }

    // Mood modifiers
    public void AngerInfluence(float angerChange) => mood.anger = Mathf.Clamp01(mood.anger + angerChange);
    public void AlertnessInfluence(float alertChange) => mood.alertness = Mathf.Clamp01(mood.alertness + alertChange);
    public void HungerInfluence(float hungerChange) => mood.hunger = Mathf.Clamp01(mood.hunger + hungerChange);
    public void DrowsyInfluence(float drowsyChange) => mood.drowsy = Mathf.Clamp01(mood.drowsy + drowsyChange);

    public void OnDrawGizmosSelected()
    {
        if (probableVictimPosition.weight.sound > 4)
        {
            Gizmos.DrawCube(HexMath.Axial2World(probableVictimPosition,WeightMap.cellSize), new Vector3(1, 2, 1));
        }
    }
}