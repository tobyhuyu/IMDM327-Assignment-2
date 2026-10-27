using UnityEngine;

public class Barrier : MonoBehaviour
{
    public float gracePD = 30f;
    public bool startTrigger = false;
    private float timer = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= gracePD)
        {
            gameObject.SetActive(false);
            startTrigger = true;
            Debug.Log("The barrier shattered.");
        }
    }
}
