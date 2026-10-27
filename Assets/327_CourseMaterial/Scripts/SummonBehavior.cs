using UnityEngine;

public class SummonBehavior : MonoBehaviour
{
    private const float G = 500f;
    private Summoner summoner;
    public float maxVel = 5f;
    public GameObject barrier;
    public bool start;

    void Start()
    {
        summoner = GetComponent<Summoner>();
        start = barrier.GetComponent<Barrier>().startTrigger;
    }

    void Update()
    {
        // if (start)
        // {

        for (int i = 0; i < summoner.entityCount; i++)
        {

            Summoner.Summon curr = summoner.summons[i];
            curr.acceleration = Vector3.zero;

            for (int j = 0; j < summoner.entityCount; j++)
            {
                if (j != i)
                {
                    //gravity
                    Vector3 distance = curr.obj.transform.position - summoner.summons[j].obj.transform.position;
                    Vector3 gravity = CalculateGravity(distance, curr.mass, summoner.summons[j].mass);

                    //check side
                    if (curr.side != summoner.summons[j].side)
                    {
                        //check type
                        //chase takes priority over avoid

                        //if i is rock and j is scissors, i is paper and j is rock, or i is scissors and j is paper
                        if ((curr.type == 1 && summoner.summons[j].type == 3) || 
                        (curr.type == 2 && summoner.summons[j].type == 1) ||
                        (curr.type == 3 && summoner.summons[j].type == 2))
                        {
                            //chase j
                            curr.acceleration -= gravity / curr.mass;
                        }
                        //else, if i is rock and j is paper, i is paper and j is scissors, or i is scissors and j is rock
                        else if ((curr.type == 1 && summoner.summons[j].type == 2) || 
                        (curr.type == 2 && summoner.summons[j].type == 3) ||
                        (curr.type == 3 && summoner.summons[j].type == 1))
                        {
                            //avoid j
                            curr.acceleration += gravity / curr.mass;
                        }
                    }

                    //check for collision
                    
                }
            }
            summoner.summons[i] = curr;
        }

        for (int i = 0; i < summoner.entityCount; i++)
        {
            Summoner.Summon curr = summoner.summons[i];
            //velocity (cannot go above maxVel)
            curr.velocity += curr.acceleration * Time.deltaTime;
            curr.velocity = Vector3.ClampMagnitude(curr.velocity, maxVel);

            summoner.summons[i] = curr;
        }

        for (int i = 0; i < summoner.entityCount; i++)
        {
            Summoner.Summon curr = summoner.summons[i];
            //acceleration
            curr.obj.transform.position += summoner.summons[i].velocity * Time.deltaTime;
            summoner.summons[i] = curr;
        }
        //}
    }

    void Destruct(int i)
    {
        Summoner.Summon toDestroy = summoner.summons[i];
        //remove from list
        summoner.summons.RemoveAt(i);

        //destroy object
        GameObject.Destroy(toDestroy.obj);
        summoner.entityCount--;
    }

    private Vector3 CalculateGravity(Vector3 distanceVector, float m1, float m2)
    {
        //check for a min distance to avoid dividing by 0
        float disSquared = distanceVector.sqrMagnitude;

        if (disSquared < 0.01f)
        {
            return Vector3.zero;
        }

        return G * m1 * m2 / disSquared * distanceVector.normalized;
    }
}
