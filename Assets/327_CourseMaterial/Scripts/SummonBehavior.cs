using UnityEngine;

public class SummonBehavior : MonoBehaviour
{
    void Start()
    {
        Summoner summoner = GetComponent<Summoner>();
    }

    void Update()
    {
        //loop through all objects in summoner.summons for boid behavior
        //check for collision
    }

    void Kill(int self, int other)
    {
        //check self type vs other type, if self can kill, use destroy() on other
    }

    void Destruct()
    {
        
    }
}
