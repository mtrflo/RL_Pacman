using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public float timeScale = 1.0f;
    private float fixedTimestep = 0.02f;

    public Action<float> ChangeVarsByTimeScale;
    private void Awake()
    {
        ChangeVarsByTimeScale = Change;
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
}
