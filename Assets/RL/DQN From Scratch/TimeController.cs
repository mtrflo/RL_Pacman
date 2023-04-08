using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public static TimeController me;
    [Range(1,12)]
    public float timeScale = 1.0f;
    private float fixedTimestep = 0.02f;

    public Action<float> ChangeVarsByTimeScale;
    private void Awake()
    {
        if (me != null)
        {
            Destroy(gameObject);
            return;
        }
        me = this;
        ChangeVarsByTimeScale = Change;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        ChangeVarsByTimeScale.Invoke(timeScale);
    }

    void Change(float ts)
    {
        Time.timeScale = ts;
        Time.fixedDeltaTime = fixedTimestep / timeScale;
    }
    
    private void OnValidate()
    {
        if(Application.isPlaying)
        {
            ChangeVarsByTimeScale?.Invoke(timeScale);
        }
    }
    private void OnLevelWasLoaded(int level)
    {
        if(me==this)
            me.ChangeVarsByTimeScale.Invoke(timeScale);
    }
}
