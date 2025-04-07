using NUnit.Framework;
using System.Collections.Generic;
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
    //roaming and tracking are based on alertness, when sound happens nearby alertness goes up and behaviour moves into tracking
    public AnimationCurve Roaming;
    public AnimationCurve tracking;
    //tracking is measured next to hunting, when alertness goes up it stops roaming but if its gotten angry it starts hunting instead of tracking
    public AnimationCurve hunting;
    //actions (such as murder and running) take energy and hunger, while hunting it will never go to sleep but while roaming it will try and sleep when necesary
    public AnimationCurve sleeping;
    //where players die (and maybe preset positions) the deepstalker can eat, it wont sleep when too hungry and eating lowers anger levels (especially if its eating players)
    public AnimationCurve eating;


    public ScriptableBehaviour activeLogic;
    public float VictimPositionCertainty = 0;
    public int areaSizes=3;
    public int hearingSize = 7;
    private List<HexCell> hexMap = new List<HexCell>();
    private HexCell myHex;
    private HexCell optimalSafety;
    private HexCell optimalFood;
    private HexCell optimalScouting;
    private HexCell probableVictimPosition;

    
    void OnEnable()
    {
        if (WeightMap == null) WeightMap = Object.FindObjectsByType<HexagonalWeight>(FindObjectsSortMode.None)[0];
        mood = new DeepWalkerMood(0, 0, 0, 0);
        AwakenTheBeast(WeightMap.walkableHexagons);
    }
    void Update()
    {
        DecideLogic();
        activeLogic.Behave(this);

    }

    public void AwakenTheBeast(List<HexCell> mapMemory)
    {
        hexMap = mapMemory;
        myHex = HexMath.NearestHex(transform.position, mapMemory, WeightMap.cellSize);

        float safetyWeightValue = 0;
        float foodWeightValue = 0;
        float scoutWeightValue = 0;

        float bestSaf=0;
        float bestFoo=0;
        float bestSco=0;

        foreach (var hex in hexMap)
        {
            safetyWeightValue = 0;
            foodWeightValue = 0;
            scoutWeightValue = 0;
            foreach (var subHex in hexMap)
            {
                if (HexMath.HexDistance(hex.hexCoords, subHex.hexCoords) > areaSizes) continue;

                safetyWeightValue += subHex.weight.safety;
                foodWeightValue += subHex.weight.food;
                scoutWeightValue += subHex.weight.sound;
            }
            if (safetyWeightValue > bestSaf)
            {
                optimalSafety = hex;
                bestSaf = safetyWeightValue;
            } 
            if (foodWeightValue > bestFoo)
            {
                optimalFood = hex;
                bestFoo = foodWeightValue;
            } 
            if (scoutWeightValue > bestSco)
            {
                optimalScouting = hex;
                bestSco = scoutWeightValue;
            }
        }
    }


    public HexCell determineOptimalHex(int value)
    {
        HexCell optimalHex=myHex;
        float WeightValue = 0;
        float bestValue = 0;
        switch (value){
            case 0:
                {
                    //optimal safety
               
                    foreach (var hex in hexMap)
                    {
                        WeightValue = 0;
                        foreach (var subHex in hexMap)
                        {
                            if (HexMath.HexDistance(hex.hexCoords, subHex.hexCoords) > areaSizes) continue;

                            WeightValue += subHex.weight.safety;
                        }
                        if (WeightValue > bestValue)
                        {
                            optimalHex = hex;
                            bestValue = WeightValue;
                        }
                    }
                    break;
                }
            case 1:
                
                {

                    //optimal food

                    foreach (var hex in hexMap)
                    {
                        WeightValue = 0;
                        foreach (var subHex in hexMap)
                        {
                            if (HexMath.HexDistance(hex.hexCoords, subHex.hexCoords) > areaSizes) continue;

                            WeightValue += subHex.weight.food;
                        }
                        if (WeightValue > bestValue)
                        {
                            optimalHex = hex;
                            bestValue = WeightValue;
                        }
                    }
                    break;
                }
            case 2:
                {
                    foreach (var hex in hexMap)
                    {
                        WeightValue = 0;
                        foreach (var subHex in hexMap)
                        {
                            if (HexMath.HexDistance(hex.hexCoords, subHex.hexCoords) > areaSizes) continue;

                            WeightValue += subHex.timeSinceChecked;
                        }
                        if (WeightValue > bestValue)
                        {
                            optimalHex = hex;
                            bestValue = WeightValue;
                        }
                    }

                    break;
                }
            case 3:
                {

                    foreach (var hex in hexMap)
                    {
                        WeightValue = 0;
                        foreach (var subHex in hexMap)
                        {
                            if (HexMath.HexDistance(hex.hexCoords, subHex.hexCoords) > areaSizes) continue;

                            WeightValue += subHex.timeSinceChecked;
                        }
                        if (WeightValue > bestValue)
                        {
                            optimalHex = hex;
                            bestValue = WeightValue;
                        }
                    }

                    break;
                }

        }


        return optimalHex;
    }



    private void DecideLogic()
    {
        //if alerted do action
        if (compareCurves(Roaming, tracking, mood.alertness))
        {
            //if angry do action while alerted
            if(compareCurves(tracking, hunting, mood.anger))
            {

            }
            //if not angry do other action
            else
            {

            }
            //being alerted supercedes all other behaviour, if danger exists deepwalker wont do something else
            return;
        }
        //bellow is neutral behaviour aka how to move when not interacting with players

        if (compareCurves(Roaming, eating, mood.hunger))
        {
            //if hungry do action
            return;
        }
        if (compareCurves(Roaming, sleeping, mood.drowsy))
        {
            //if sleepy do action
            return;
        }
                   
        //lastly do action when needs are not that important (just roam smh)

    }


    public bool compareCurves(AnimationCurve neutral,AnimationCurve superceding,float value)
    {
        //if neutral is greater return true else return false
        return neutral.Evaluate(value) > superceding.Evaluate(value);
    }



    public void AngerInfluence(float angerChange)
    {
        mood.anger = Mathf.Clamp01(mood.anger + angerChange);
    }  
    public void AlertnessInfluence(float alertChange)
    {
        mood.alertness = Mathf.Clamp01(mood.alertness + alertChange);
    }
    public void HungerInfluence(float hungerChange)
    {
        mood.hunger = Mathf.Clamp01(mood.alertness + hungerChange);
    }  
    public void DrowsyInfluence(float drowsyChange)
    {
        mood.drowsy = Mathf.Clamp01(mood.alertness + drowsyChange);
    }

}
