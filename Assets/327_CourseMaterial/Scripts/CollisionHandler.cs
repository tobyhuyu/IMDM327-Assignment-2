using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    private Summoner summoner;
    void Awake()
    {
        summoner = FindFirstObjectByType<Summoner>();
    }

    void OnCollisionEnter(Collision collision)
    {
        int selfIndex = summoner.summons.FindIndex(summon => summon.obj == gameObject);
        int otherIndex = summoner.summons.FindIndex(summon => summon.obj == collision.gameObject);

        //summons weren't found
        if (selfIndex == -1 || otherIndex == -1)
        {
            return;
        }

        if (summoner.summons[selfIndex].side == summoner.summons[otherIndex].side)
        {
            return;
        }

        int selfType = summoner.summons[selfIndex].type;
        int otherType = summoner.summons[otherIndex].type;
        
    
        if (selfType == otherType)
        {
            return;
        }
        if (selfType == 1)
        {
            if (otherType == 2)
            {
                Destruct(this.gameObject);
            }
            if (otherType == 3)
            {
                Destruct(collision.gameObject);
            }
        }
        else if (selfType == 2)
        {
            if (otherType == 1)
            {
                Destruct(collision.gameObject);
            }
            if (otherType == 3)
            {
                Destruct(this.gameObject);
            }
        }
        else 
        {
            if (otherType == 1)
            {
                Destruct(this.gameObject);
            }
            if (otherType == 2)
            {
                Destruct(collision.gameObject);
            }
        }
    }
     void Destruct(GameObject victim)
    {
        int index = summoner.summons.FindIndex(summon => summon.obj == victim);
        //already deleted
        if (index == -1)
            {
                return;
            }
        
        Summoner.Summon toDestroy = summoner.summons[index];

        summoner.summons.RemoveAt(index);
        summoner.entityCount--;

        Destroy(toDestroy.obj);
    }
}
