using UnityEngine;

public class SummonBehavior : MonoBehaviour
{
    private const float G = 500f;
    private Summoner summoner;
    public float maxVel = 2f;
    public GameObject barrier;
    public float start;
    private float minX = -6.5f;
    private float maxX = 6.5f;
    private float minZ = 3f;
    private float maxZ = 16f;
    public float timer = 0f;
    //public float movementStrength = 0.001f;

    void Start()
    {
        summoner = GetComponent<Summoner>();
        start = barrier.GetComponent<Barrier>().gracePD;
    }

    void Update()
    {

        timer += Time.deltaTime;

        if (timer >= start)
        {
        for (int i = 0; i < summoner.entityCount; i++)
        {

            Summoner.Summon curr = summoner.summons[i];
            curr.acceleration = Vector3.zero;

            for (int j = 0; j < summoner.entityCount; j++)
            {
                if (j != i)
                {
                    if (curr.side != summoner.summons[j].side)
                    {
                    //gravity
                    Vector3 distance = curr.obj.transform.position - summoner.summons[j].obj.transform.position;
                    Vector3 gravity = CalculateGravity(distance, curr.mass, summoner.summons[j].mass);

                    //check side
                    
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
            curr.obj.transform.position += curr.velocity * Time.deltaTime;
            Vector3 pos = curr.obj.transform.position;
            
            //bouncy
            if (pos.x <= minX || pos.x >= maxX)
            {
                curr.velocity.x *= -1f;
            }
            if (pos.z <= minZ || pos.z >= maxZ)
            {
                curr.velocity.z *= -1f;
            }

            //restrict objs to platform
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
                
            curr.obj.transform.position = pos;
            summoner.summons[i] = curr;
        }
        }
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
