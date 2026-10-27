using UnityEngine;

public class ComputerBehavior : MonoBehaviour
{
    public float checkTime = 5f;
    private float summmonTimer = 0f;
    private float checkTimer = 0f;
    private Summoner summoner;
    private float summonTime;
    //private int thresh = 4;
    public GameObject mediapipe;

    void Start()
    {
        summonTime = 2f;
        //Random.Range(summoner.cooldown, summoner.cooldown+2f);
        summoner = mediapipe.GetComponent<Summoner>();   
    }

    void FixedUpdate()
    {   
        summmonTimer += Time.deltaTime;
        checkTimer += Time.deltaTime;

        if (summmonTimer >= summonTime)
        {
            TrySummon();
            summmonTimer = 0f;
        }

        //check if player is summoning a lot of one specific type, if yes then up the chance the computer will summon its counter 
        // if (checkTimer >= checkTime)
        // {
        //     int rCount = 0;
        //     int sCount = 0;
        //     int pCount = 0;

        //     foreach(Summoner.Summon obj in summoner.summons)
        //     {
        //         if (obj.side == 1)
        //         {
        //             if (obj.type == 1)
        //             {
        //                 rCount++;
        //             } 
        //             else if (obj.type == 2)
        //             {
        //                 pCount++;
        //             }
        //             else
        //             {
        //                 sCount++;
        //             }
        //         }
        //     }
        //     //Mathf.Max(rCount, sCount, pCount)

        // }

    }

    void TrySummon()
    {
        summoner.Create(0, Random.Range(1,4));
        summonTime = Random.Range(.25f, summoner.cooldown);
    }
}
