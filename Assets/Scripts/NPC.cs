using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    private float tickTimer;
    private float tickTimerMax;
    private int tick;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        tickTimer += Time.deltaTime;
        while (tickTimer >= tickTimerMax)
        {
            tickTimer -= tickTimerMax;
            tick++;
            OnTick();
        }
    }

    private void OnTick()
    {
        // current position -> current position + 1/4 * next tile position
    }
}
