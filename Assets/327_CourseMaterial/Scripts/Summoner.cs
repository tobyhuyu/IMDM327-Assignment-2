//using System.Diagnostics;
using UnityEditor.Build.Content;
using UnityEditor.SearchService;
using UnityEngine;

public class Summoner : MonoBehaviour
{
    public GameObject rock;
    public GameObject paper;
    public GameObject scissors;
    public int entityLim = 100;

    private int entityCount;
    public float cooldown = 2f;
    private bool canSummon;
    private float timer = 0f;

    public struct Summon
    {             
        public GameObject obj;
        public int type;
        public int side;
    }
    public Summon[] summons; 
    private GameObject[] prefabs;

    private void Start()
    {
        summons = new Summon[entityLim];
        prefabs = new GameObject[4];
        entityCount = 0;
        canSummon = true;

        prefabs[1] = Resources.Load<GameObject>("prefabs/Rock");
        prefabs[2] = Resources.Load<GameObject>("prefabs/Paper");
        prefabs[3] = Resources.Load<GameObject>("prefabs/Scissors");
    }

    void FixedUpdate()
    {
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

        GameObject summoned = Instantiate(prefabs[type]);

        if (side == 0)
        {
            summoned.transform.position = new Vector3(Random.Range(-6.8f, 6.8f), 0, Random.Range(17.5f, 12.7f));
            //change color to red

        } else
        {
            summoned.transform.position = new Vector3(Random.Range(-5.5f, 5.8f), 0, Random.Range(2.5f, 5f));
            //change color to blue
        }
        
        summons[entityCount].type = type;
        summons[entityCount].side = side;
        summons[entityCount].obj = summoned;
        
        Debug.Log("Created summon type " + type + " on side " + side);
        entityCount++;

        if (side == 1)
        {
            canSummon = false;
        }
    }

}

