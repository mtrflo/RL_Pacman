using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TimeBasedInvoke : MonoBehaviour
{
    public MultiEnviroment multiEnviromewnt;
    public DQNAgent dqn;
    public List<TimeEvent> TimeEvents;
    List<BirdEnv> birdEnvs = new List<BirdEnv>();

    private void Start()
    {
        StartCoroutine(TiemEventTick());
    }
    private IEnumerator EveryMinutTick()
    {
        int minut = 0;
        while (true)
        {
            yield return new WaitWhile(() => Time.timeSinceLevelLoad < (minut * 60f));
            minut++;
        }
    }
    private IEnumerator TiemEventTick()
    {
        int timesID = 0;
        while (timesID < TimeEvents.Count)
        {
            yield return new WaitWhile(() => Time.timeSinceLevelLoad < (TimeEvents[timesID].time * 60f));

            TimeEvents[timesID].unityEvent.Invoke();
            timesID++;
        }
    }


    public void DestroyMinScoreAgents(int count)
    {
        birdEnvs.RemoveAll(i => i == null);
        if (birdEnvs.Count == 0)
        {
            foreach (Transform item in multiEnviromewnt.ins_parent)
            {
                birdEnvs.Add(item.GetComponent<BirdEnv>());
            }
        }
        if (birdEnvs.Count <= count)
            return;
        birdEnvs.Sort((x, y) => x.currentBird.birdControl.scoreMgr.currentScore.CompareTo(y.currentBird.birdControl.scoreMgr.currentScore));
        IEnumerator IEDestroy()
        {
            for (int i = 0; i < count; i++)
            {
                Destroy(birdEnvs[i].gameObject);
                yield return null;
            }
            multiEnviromewnt.count -= count;
        }
        StartCoroutine(IEDestroy());
    }

    public void SetLR(float lr)
    {
        dqn.network.LearningRate = lr;
    }
}
[Serializable]
public class TimeEvent
{
    public float time;//in minutes
    [Space]
    public UnityEvent unityEvent;
}