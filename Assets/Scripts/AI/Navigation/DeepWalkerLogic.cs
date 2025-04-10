using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
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
    public ActiveBehaviour myBehaviour;
    public Agent_findPath pathfinder;
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
    public Vector3 currentTarget;
    public float minimumSound = 1.5f;
    public Dictionary<Vector3, HexCell> hexMap = new();
    public Dictionary<Vector3, HexCell> inHexRange = new();
    public Dictionary<Vector3, HexCell> outHexRange = new();
    public HexCell myHex;
    public HexCell optimalSafety;
    public HexCell optimalFood;
    public HexCell optimalScouting;
    private HexCell probableVictimPosition;
    private HexCell loudestReactHex;
    public GameObject TrackingObject;
    private bool updateFood, updateSafety, reactToSound;
    public bool readyForSleep;
    public bool readyForEating;
    public float restTime=0;
    public float losetime = 3;
    private float timeLost=0;
    
    void OnEnable()
    {
        if (WeightMap == null)WeightMap = Object.FindObjectsByType<HexagonalWeight>(FindObjectsSortMode.None)[0];
        if(pathfinder==null)pathfinder = GetComponent<Agent_findPath>();
        mood = new DeepWalkerMood(0, 0, 0, 0);
        loudestReactHex=new HexCell();
        loudestReactHex.weight.sound = 1;
        AwakenTheBeast(WeightMap.walkableHexagons);
    }
    private HexCell oldTrackingHex;
    void Update()
    {
        UpdateVision();

        if (TrackingObject != null)
        {
            var hex = HexMath.NearestHex(TrackingObject.transform.position, hexMap.Values.ToList(), WeightMap.cellSize);
            Debug.Log(hex.hexCoords + " "+ HexMath.HexDistance(hex.hexCoords, myHex.hexCoords) + " hex distances");
            
            if(myHex.hexCoords == hex.hexCoords || (transform.position-TrackingObject.transform.position).magnitude<3)
            {
                TrackingObject = null; timeLost = 0;
                Debug.Log("Caught");
            }

            if (HexMath.HexDistance(hex.hexCoords, myHex.hexCoords) > hearingRange)
            {
                timeLost += Time.deltaTime;
            }
            else timeLost = 0;
            if (timeLost >= losetime) { TrackingObject = null; timeLost = 0; Debug.Log("Lost"); }

            if (hex.hexCoords != oldTrackingHex.hexCoords)
            {
                updateGoal(TrackingObject.transform.position);

            }

            oldTrackingHex = hex;
            return;
        }
       
        if (reactToSound)
        {
            Debug.Log("looking for loud");
            reactToSound = false;
            probableVictimPosition = FindSoundOrigin(loudestReactHex);
            updateGoal(HexMath.Axial2World(probableVictimPosition, WeightMap.cellSize));
       
        }
        if (myHex.hexCoords==probableVictimPosition.hexCoords)
        {
            loudestReactHex = new HexCell();
        }
        currentTarget = probableVictimPosition.hexCoords;
       
        DecideLogic();
        
        activeLogic.Behave(this);
    }

    public void AwakenTheBeast(Dictionary<Vector3, HexCell> mapMemory)
    {
        hexMap = new Dictionary<Vector3, HexCell>();

        foreach (var kvp in mapMemory)
        {
            // Create a deep copy of the HexCell
            HexCell cellCopy = kvp.Value;
            cellCopy.weight = new HeatMapValues
            {
                food = kvp.Value.weight.food,
                sound = kvp.Value.weight.sound,
                safety = kvp.Value.weight.safety
            };

            // Reset any other values if needed
            cellCopy.timeSinceChecked = kvp.Value.timeSinceChecked;

            hexMap[kvp.Key] = cellCopy;
        }

        myHex = HexMath.NearestHex(transform.position, hexMap.Values.ToList(), WeightMap.cellSize);

        optimalSafety = FindOptimalHex(0);
        optimalFood = FindOptimalHex(1);
        optimalScouting = FindOptimalHex(2);
    }


    public void updateGoal(Vector3 position)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, WeightMap.cellSize, pathfinder.BezierLayers))
        {
            pathfinder.goal.transform.position = hit.position;
        }
        else if (NavMesh.SamplePosition(position, out hit, WeightMap.cellSize*3, pathfinder.BezierLayers))
        {

            pathfinder.goal.transform.position = hit.position;
        }

            pathfinder.RecalculatePath=true;
        //RaycastHit sphereHitRay;
        //if (pathfinder.Path.Count < 1) { pathfinder.RecalculatePath = true; return; }
        //if (Physics.SphereCast(pathfinder.Path[pathfinder.Path.Count - 1], transform.localScale.x, (pathfinder.Path[pathfinder.Path.Count - 1] - position).normalized,
        //    out sphereHitRay, (pathfinder.Path[pathfinder.Path.Count - 1] - position).magnitude, pathfinder.LineOfSightLayers))
        //{
        //    pathfinder.AddPointToCurve(position);
        //}
        //else pathfinder.RecalculatePath = true;
    }



    public HexCell FindSoundOrigin(HexCell soundStartFound)
    {
        // Initialize loudest with the starting hex
        Debug.Log("StartFoundOrigin");
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
                    if (neighbor.weight.sound >= loudestReactHex.weight.sound)
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
                    case 2: 
                        { 
                            weightSum += subHex.timeSinceChecked;
                            weightSum += ((HexMath.Axial2World(subHex, WeightMap.cellSize) + HexMath.Axial2World(myHex, WeightMap.cellSize) / 2).magnitude);
                            
                            break; 
                
                        
                        }
                }
                
            }

            if (weightSum > bestValue)
            {
                optimalHex = hex;
                bestValue = weightSum;
                Debug.Log("hex"+ hex.hexCoords);
             
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
                // tracking behavior
                //tracking means it might have found a player and is activly trying to find them
                myBehaviour = ActiveBehaviour.tracking;
                if (VictimPositionCertainty > .9f)
                {

                    myBehaviour = ActiveBehaviour.hunting;
                    //hunting behaviour
                    //hunting means locked in on a player
                }


            }
            else
            {

                myBehaviour = ActiveBehaviour.scouting;
                // scouting behavior
                // scouting means they heard a noise but isnt activly trying to kill a player
            }
            return;
        }

        if (compareCurves(Roaming, eating, mood.hunger)) 
        {

            myBehaviour = ActiveBehaviour.feeding;
            // goto place where food is
            return;
        
        }
        if (compareCurves(Roaming, sleeping, mood.drowsy)) {


            myBehaviour = ActiveBehaviour.sleeping;
            //goto place where its safe
            
            return; 
        
        
        
        }

        myBehaviour = ActiveBehaviour.roaming;
        // walking around aimlesly to the areas where it has not been for a while that is far away
        // Roaming behavior
    }
  
    public bool compareCurves(AnimationCurve neutral, AnimationCurve superceding, float value)
    {
        return neutral.Evaluate(value) > superceding.Evaluate(value);
    }

    public void UpdateVision()
    {
        myHex = HexMath.NearestHex(transform.position, hexMap.Values.ToList(), WeightMap.cellSize);
        inHexRange.Clear();
        outHexRange.Clear();

        foreach (var kvp in hexMap.ToList())
        {
            Vector3 key = kvp.Key;
            HexCell cell = kvp.Value;
            int dist = HexMath.HexDistance(myHex.hexCoords, cell.hexCoords);

            if (dist <= hearingRange)
            {
                cell.timeSinceChecked = 0;

                // Fetch the latest live value
                HexCell liveCell = WeightMap.walkableHexagons[key];

            

                if (
                    cell.weight.sound < liveCell.weight.sound && // it's new
                    liveCell.weight.sound > loudestReactHex.weight.sound && // louder than anything before
                    liveCell.weight.sound > minimumSound && // above threshold
                    key != loudestReactHex.hexCoords // not the same hex already reacting to
                )
                {
                    reactToSound = true;
                    loudestReactHex = liveCell;
                }

                // Now update the memory
                HexMath.UpdateWeights(ref cell, liveCell);
                hexMap[key] = cell;
                inHexRange[key] = cell;
            }
            else
            {
                cell.timeSinceChecked += Time.deltaTime;
                hexMap[key] = cell;
                if (inHexRange.ContainsKey(key)) inHexRange.Remove(key);
                outHexRange[key] = cell;
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
        Gizmos.color = Color.yellow;
        if (probableVictimPosition.weight.sound > 1)
        {
            Gizmos.DrawCube(HexMath.Axial2World(probableVictimPosition,WeightMap.cellSize), new Vector3(2, 2, 2));
        }
      
        foreach(var hex in inHexRange)
        {
            Gizmos.color = new Color(hex.Value.weight.food/10, hex.Value.weight.safety/10, hex.Value.weight.sound / 10);
            Gizmos.DrawCube(HexMath.Axial2World(hex.Value,WeightMap.cellSize), new Vector3(3,3,3));
        }

        foreach (var hex in outHexRange)
        {
            Gizmos.color = new Color(hex.Value.weight.food / 10, hex.Value.weight.safety / 10, hex.Value.weight.sound / 10);
            Gizmos.DrawSphere(HexMath.Axial2World(hex.Value, WeightMap.cellSize), 2.5f);
        }

    }
}