using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    // A single callback that will be called at some delay 
    private class TimerEntry
    {
        public Action callback;
        public float addedTime;
        public float delay;
    }

    private static TimerManager instance;


    // List of functions to call after some delay
    private List<TimerEntry> timedFunctions = new List<TimerEntry>();


    public TimerManager()
    {
        instance = this;
    }

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    { 
        // call timed functions at end of frame
        for(int i = timedFunctions.Count - 1; i >= 0; i--)
        {
            TimerEntry timedFunction = timedFunctions[i];
            if(Time.time >= timedFunction.addedTime + timedFunction.delay)
            {
                timedFunction.callback();
                timedFunctions.RemoveAt(i);
            }
        }
    }

    
    /// <summary>
    /// Set a timed function call. The input action willbe called after the input delay time
    /// </summary>
    public static void SetTimer(Action callback, float delay)
    {
        if(instance != null)
        {
            TimerEntry entry = new TimerEntry();
            entry.addedTime = Time.time;
            entry.callback = callback;
            entry.delay = delay;

            instance.timedFunctions.Add(entry);
        }
    }
}
