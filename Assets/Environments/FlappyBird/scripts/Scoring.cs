using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoring : MonoBehaviour
{
    public static Scoring me;
    public List<float> times;

    public float maxScore;
    private void Awake()
    {
        me = this;
    }
    private void Start()
    {
        StartCoroutine(Tick());
    }
    public void NewScore(float score)
    {
        if(maxScore < score)
            maxScore = score;
    }

    private IEnumerator Tick()
    {
        int timesID = 0;
        while (timesID < times.Count)
        {
            yield return new WaitWhile( ()=>Time.timeSinceLevelLoad < (times[timesID] * 60f) );
            print(times[timesID] + " MaxScore : " + maxScore);
            timesID++;
        }
    }
}
