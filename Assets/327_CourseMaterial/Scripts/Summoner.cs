//using System.Diagnostics;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEditor.SearchService;
using UnityEngine;

public class Summoner : MonoBehaviour
{
    public GameObject rock;
    public GameObject paper;
    public GameObject scissors;

    public Material red;
    public Material blue;

    public int entityLim = 100;

    public int entityCount;
    public float cooldown = 2f;
    private bool canSummon;
    private float timer = 0f;
    private SummonBehavior summonBehavior;

    public struct Summon
    {             
        public GameObject obj;
        public int type;
        public int side;
        public float mass;
        public Vector3 velocity;
        public Vector3 acceleration;
    }
    public List<Summon> summons; 
    private GameObject[] prefabs;

    private void Start()
    {
        summonBehavior = GetComponent<SummonBehavior>();
        summons = new List<Summon>{};
        prefabs = new GameObject[4];
        entityCount = 0;
        canSummon = true;

        prefabs[1] = Resources.Load<GameObject>("prefabs/Rock");
        prefabs[2] = Resources.Load<GameObject>("prefabs/Paper");
        prefabs[3] = Resources.Load<GameObject>("prefabs/Scissors");
    }

    void FixedUpdate()
    {
        //Debug.Log("eount " + entityCount);
        timer += Time.deltaTime;

        if (!canSummon && timer >= cooldown)
        {
            canSummon = true;
            timer = 0f;
        }
    }

    //parameters take in side (enemy 0 or player 1) and summon type (rock 1,paper 2,scissors 3)
    //only summon if object limit is not reached
    //keep track of cooldown, flip canSummon to false until cooldown expires
    //create new object inside array of all summons

    public void Create(int side, int type)
    {
        //don't summon if battle has started
        // if (summonBehavior.start)
        // {
        //     return;
        // }

        if (entityCount >= 100)
        {
            Debug.Log("Entity limit reached!");
            return;
        } 

        if (side == 1 && !canSummon)
        {
           // Debug.Log("Wait for Cooldown!");
            return;
        }
        Summon currSummon = new Summon();
        GameObject obj = Instantiate(prefabs[type]);

        if (side == 0)
        {
            obj.transform.position = new Vector3(Random.Range(-6.8f, 6.8f), 0, Random.Range(17.5f, 12.7f));
            obj.GetComponent<Renderer>().material = red;
            currSummon.velocity = 5f * new Vector3(0,0,-1);

        } else
        {
            obj.transform.position = new Vector3(Random.Range(-5.5f, 5.8f), 0, Random.Range(2.5f, 5f));
            obj.GetComponent<Renderer>().material = blue;
            currSummon.velocity = 5f * new Vector3(0,0,1);
        }
        
        currSummon.type = type;
        currSummon.side = side;
        currSummon.obj = obj;
        currSummon.mass = 1;
        

     
        summons.Add(currSummon);
        
        //Debug.Log("Created summon type " + type + " on side " + side);
        entityCount++;

        if (side == 1)
        {
            canSummon = false;
        }
    }

}

