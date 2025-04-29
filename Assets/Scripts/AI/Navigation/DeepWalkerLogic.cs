using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
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
    private ActiveBehaviour oldBehaviour;
    private BehaviourHandler behaveHandle;
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
    public int areaSizes = 2;
    public int hearingRange = 7;
    public Vector3 currentTarget;
    public float minimumSound = 1.5f;
    public Dictionary<Vector3, HexCell> hexMap = new();
    public Dictionary<Vector3, HexCell> inHexRange = new();
    public Dictionary<Vector3, HexCell> outHexRange = new();
    public HexCell myHex;
    public HexCell optimalSafety;
    public HexCell optimalFood;
    public HexCell optimalRoaming;
    public HexCell optimalTracking;
    public HexCell currentHexTarget;
    public HexCell probableVictimPosition;
    private HexCell loudestReactHex;
    private HexCell oldTrackingHex;
    public GameObject TrackingObject;
    private bool updateFood, updateSafety, reactToSound;
    public bool readyForSleep;
    public bool readyForEating;
    public bool debugRoamFinder;
    public float restTime=0;
    public float losetime = 3;
    public float minimumWaitToRecalculate=3;
    public AttackOrbHandler myAttack;
    private float recalculateTime;
    public bool neutral = true;
    private float timeLost=0;
    private float maxDistance = 0f;
    public float feedTime = 0;
    public float sleepTime = 0;
    void OnEnable()
    {
        if (WeightMap == null)WeightMap = Object.FindObjectsByType<HexagonalWeight>(FindObjectsSortMode.None)[0];
        if (behaveHandle == null) behaveHandle = GetComponent<BehaviourHandler>();
        if(pathfinder==null)pathfinder = GetComponent<Agent_findPath>();
        mood = new DeepWalkerMood(0, 0, 0, 0);
        loudestReactHex=new HexCell();
        loudestReactHex.weight.sound = 1;
        AwakenTheBeast(WeightMap.walkableHexagons);
        foreach (var hex in hexMap.Values)
        {
            float dist = (VectorFix.returnVector3With0Y(HexMath.Axial2World(hex, WeightMap.cellSize)) -
                          VectorFix.returnVector3With0Y(HexMath.Axial2World(myHex, WeightMap.cellSize))).magnitude;
            if (dist > maxDistance) maxDistance = dist;
        }
        oldTrackingHex = myHex;
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
        optimalRoaming = FindOptimalHex(2);
    }


    private float roamingBreak=5;
    private float roamingBreakCounter;
    private float timeSinceLastSound = 0;
    void Update()
    {
        UpdateVision();
     
        if (TrackingObject != null && myBehaviour==ActiveBehaviour.hunting)
        {
            huntThePerson();
            Debug.Log("Hunting");
            return;
        }
        UpdateMoodTicks();
        //reacting to sound happens when the AI isnt activly hunting a person
        if (reactToSound)
        {
            Debug.Log("looking for loud");
            reactToSound = false;
            probableVictimPosition = FindSoundOrigin(loudestReactHex);
            AlertnessInfluence(Time.deltaTime * 20);
            timeSinceLastSound = 0;
            
            loudestReactHex = new HexCell();
        }
     
        
        DecideLogic();
        LogicBasedAction();




        if(readyForEating && myHex.hexCoords == optimalFood.hexCoords)
        {
            TimeToEat();
        }  
        if(readyForSleep && myHex.hexCoords == optimalSafety.hexCoords)
        {
            TimeToSleep();
        }
    }

    private void FixedUpdate()
    {
        var keys = hexMap.Keys.ToList(); // Copy keys to avoid modifying collection while iterating
        foreach (var key in keys)
        {
            HexCell hex = hexMap[key];
            if (inHexRange.Keys.Contains(key))
            {

                hex.weight.soundMemory = Mathf.Lerp(hex.weight.soundMemory, 1f, 3*Time.fixedDeltaTime);
            }
            else hex.weight.soundMemory = Mathf.Lerp(hex.weight.soundMemory, 1f, .5f * Time.fixedDeltaTime);

            hexMap[key] = hex; // Store modified back
        }
        timeSinceLastSound += Time.fixedDeltaTime;
        if (recalculateTime > 0) recalculateTime -= Time.fixedDeltaTime*4;
    }
    private void LogicBasedAction()
    {
        if (myBehaviour == ActiveBehaviour.tracking)
        {

            if (timeSinceLastSound < 5)
            {
               
                    GuessPlayerPosition(probableVictimPosition);
               
                currentHexTarget = probableVictimPosition;

            }else currentHexTarget = FindOptimalHex(3);
        }
        if (myBehaviour != oldBehaviour || myHex.hexCoords == currentHexTarget.hexCoords && myBehaviour == ActiveBehaviour.roaming)
        {
            activeLogic = behaveHandle.TimeToChange(myBehaviour);
            oldBehaviour = myBehaviour;
            activeLogic.Behave(this);

        }
        if (myBehaviour == ActiveBehaviour.scouting)
        {

            currentHexTarget = probableVictimPosition;
        }
        if (currentTarget != currentHexTarget.hexCoords || HexMath.NearestHex(pathfinder.goal.transform.position, hexMap.Values.ToList(), WeightMap.cellSize).hexCoords != currentHexTarget.hexCoords)
        {
            updateGoal(HexMath.Axial2World(currentHexTarget, WeightMap.cellSize));
            currentTarget = currentHexTarget.hexCoords;
        }
  
     
        if (debugRoamFinder || roamingBreakCounter > roamingBreak)
        {
            debugRoamFinder = false;
            optimalRoaming = FindOptimalHex(2);
            currentHexTarget = optimalRoaming;
            updateGoal(HexMath.Axial2World(currentHexTarget, WeightMap.cellSize));
        }

    }
    private void UpdateMoodTicks()
    {
        if (mood.drowsy < 1)
        {
            DrowsyInfluence((Time.deltaTime * activeLogic.tickrateDrowsy)/100);
        }
        else mood.drowsy = 1;
        if (mood.hunger < 1)
        {
            HungerInfluence((Time.deltaTime * activeLogic.tickrateHunger)/100);
        }
        else mood.hunger = 1;
      
        if (mood.alertness > .5f && mood.anger<1)
        {
            AngerInfluence(Time.deltaTime / 10);

        }

        if (mood.alertness > 0 && inHexRange.Keys.Contains(probableVictimPosition.hexCoords))
        {
            mood.alertness -= Time.deltaTime / 10;
        }
       
        

    }
    
    private void TimeToEat()
    {
        feedTime -= Time.deltaTime;
        if (feedTime <= 0)
        {
            mood.hunger = 0;
            readyForEating = false;
        }
    }
    private void TimeToSleep()
    {
        sleepTime -= Time.deltaTime;
        if (sleepTime <= 0)
        {
            mood.drowsy = 0;
            readyForSleep = false;
        }
    }
    private void huntThePerson()
    {
        var hex = HexMath.NearestHex(TrackingObject.transform.position, hexMap.Values.ToList(), WeightMap.cellSize);
    

        if (myAttack.KilledPlayer)
        {
            TrackingObject = null; timeLost = 0;
            VictimPositionCertainty = 0;
            Debug.Log("Caught");
            mood.anger = 0;
            return;
            
        }

        if (HexMath.HexDistance(hex.hexCoords, myHex.hexCoords) > hearingRange)
        {
            timeLost += Time.deltaTime;
        }
        else timeLost = 0;
        if (timeLost >= losetime) { TrackingObject = null; timeLost = 0; Debug.Log("Lost");VictimPositionCertainty = 0; }

        if (hex.hexCoords != oldTrackingHex.hexCoords || pathfinder.curentSpeed < 3)
        {
            if (recalculateTime <= 0)
            {
                updateGoal(TrackingObject.transform.position);
            }
        }

        Debug.Log(hex.hexCoords + " " + HexMath.HexDistance(hex.hexCoords, myHex.hexCoords) + " hex distances"+ oldTrackingHex.hexCoords+"old positions ");
        oldTrackingHex = hex;
    }

    private Vector3 previousGoal;

    public void updateGoal(Vector3 position)
    {
        if (position == previousGoal || recalculateTime>0) return;
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
        previousGoal = position;
        recalculateTime = minimumWaitToRecalculate;
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

                if (WeightMap.fullMapHexagons.TryGetValue(neighborCoords, out HexCell neighbor))
                {
                    if (neighbor.weight.sound >= loudestReactHex.weight.sound)
                    {
                        openSet.Enqueue(neighbor);
                        visited.Add(neighborCoords);
                    }
                }
            }
        }
        // Boost sound memory of the detected loudest hex
        loudestReactHex.weight.soundMemory = 10;

        // Update the value in memory and vision range
        hexMap[loudestReactHex.hexCoords] = loudestReactHex;
        inHexRange[loudestReactHex.hexCoords] = loudestReactHex;

        // Update neighboring hexes
        foreach (var offset in HexMath.cubeDirectionVectors)
        {
            Vector3 neighborCoords = loudestReactHex.hexCoords + offset;

            if (hexMap.TryGetValue(neighborCoords, out HexCell memoryNeighbor) &&
                WeightMap.walkableHexagons.TryGetValue(neighborCoords, out HexCell liveNeighbor))
            {

                memoryNeighbor.weight.soundMemory = Mathf.Max(5, memoryNeighbor.weight.soundMemory); // Slightly lower influence for neighbors
               
                HexMath.UpdateWeights(ref memoryNeighbor, liveNeighbor);

                hexMap[neighborCoords] = memoryNeighbor;
                inHexRange[neighborCoords] = memoryNeighbor;
            }
        }
        return loudestReactHex;
      
    }


    public HexCell FindOptimalHex(int type)
    {
        HexCell optimalHex = myHex;
        float bestValue = float.MinValue;

        foreach (var hex in hexMap.Values)
        {
            float weightSum = 0;
            int count = 0;

            foreach (var subHex in hexMap.Values)
            {
                if (HexMath.HexDistance(hex.hexCoords, subHex.hexCoords) > areaSizes)
                    continue;

                switch (type)
                {
                    case 0:
                        weightSum += subHex.weight.safety;
                        break;
                    case 1:
                        weightSum += subHex.weight.food;
                        break;
                    case 2:
                        weightSum += subHex.timeSinceChecked;
                        count++;
                        break;
                    case 3:
                        weightSum += subHex.weight.soundMemory;
                        break;
                }
            }

            if (type == 2)
            {
                
                float distance = (VectorFix.returnVector3With0Y(HexMath.Axial2World(hex, WeightMap.cellSize)) -
                                  VectorFix.returnVector3With0Y(HexMath.Axial2World(myHex, WeightMap.cellSize))).magnitude;

                float normalizedDistance = maxDistance > 0 ? distance / maxDistance : 0;
                weightSum += normalizedDistance/20; // Encourages exploration
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
        return neutral.Evaluate(value) < superceding.Evaluate(value);
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
                HexCell liveCell = WeightMap.fullMapHexagons[key];

            

                if (
                    cell.weight.sound < liveCell.weight.sound && // it's new
                   // liveCell.weight.sound > loudestReactHex.weight.sound && // louder than anything before
                    liveCell.weight.sound > minimumSound && // above threshold
                    key != loudestReactHex.hexCoords // not the same hex already reacting to
                )
                {
                    reactToSound = true;
                    loudestReactHex = liveCell;
                }
                liveCell.timeSinceChecked = 0;
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
    public void GuessPlayerPosition(HexCell probablePlayerPosition)
    {
        if (inHexRange.Values.Contains(probablePlayerPosition))
        {
           foreach(var phex in WeightMap.playerWeights)
            {
                if (phex.myHex.hexCoords == probablePlayerPosition.hexCoords)
                {
                    TrackingObject = phex.transform.parent.gameObject;
                    VictimPositionCertainty += .25f;
                }
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
            Gizmos.DrawCube(HexMath.Axial2World(probableVictimPosition,WeightMap.cellSize), new Vector3(.125f, .125f, .125f));
        }
      
        foreach(var hex in inHexRange)
        {
            Gizmos.color = new Color(hex.Value.weight.food/10, hex.Value.weight.safety/10, hex.Value.weight.sound / 10);
            Gizmos.DrawCube(HexMath.Axial2World(hex.Value,WeightMap.cellSize), new Vector3(.5f, .5f, .5f));
        }

        foreach (var hex in outHexRange)
        {
            Gizmos.color = new Color(hex.Value.weight.food / 10, hex.Value.weight.soundMemory / 10, hex.Value.weight.sound / 10);
            Gizmos.DrawSphere(HexMath.Axial2World(hex.Value, WeightMap.cellSize)+Vector3.up*hex.Value.timeSinceChecked, .25f);
        }
        try
        {
            Gizmos.color = Color.green;

            Gizmos.DrawSphere(HexMath.Axial2World(optimalSafety, WeightMap.cellSize) + Vector3.up * 3, .5f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(HexMath.Axial2World(optimalFood, WeightMap.cellSize) + Vector3.up * 3, .5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(HexMath.Axial2World(optimalRoaming, WeightMap.cellSize) + Vector3.up * 3, .5f);
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(HexMath.Axial2World(optimalTracking, WeightMap.cellSize) + Vector3.up * 3, .5f);


        } catch 
        { 
        
        
        }


    }
}