using PMT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumAgent : MonoBehaviour
{
    private Rigidbody2D rb;
    public SRLAgent rLAgent => SRLAgent.me;

    public float delay = 1, startDelay;
    public float reward = 0.1f, terminateReward = -1f, scoreReward = 1, distanceReward = 0f;
    public TimeController timeController;
    public int episodeCount, maxEpisodeCount;
    public Transform point;
    public float force;
    private Transition _Transition = new Transition();
    private void Awake()
    {
        startDelay = delay;
        timeController = TimeController.me;
        prev_state = new List<double>();
        current_state = new List<double>();

        timeController.ChangeVarsByTimeScale += ChangeVars;
    }
    bool addReward = false;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Started();
    }

    private void Started()
    {
        StartCoroutine(ActionMaker());
    }
    int action;
    List<double> prev_state, current_state;
    WaitForSecondsRealtime wfsr;
    IEnumerator ActionMaker()
    {
        wfsr = new WaitForSecondsRealtime(delay);
        while (true)
        {
            yield return wfsr;
            ChooseAction();
        }

    }
    float maxrew = -10000;
    float lastAngle = 180;
    void ChooseAction()
    {
        current_state.Clear();
        
        Vector3 toPo = (point.position - transform.position).normalized;
        float angle = Vector3.Angle(toPo, Vector3.up);
        angle = angle * MathF.Sign(point.position.x) * Mathf.Deg2Rad;
        print("angle : " + angle);
        
        
        AddObservation(Mathf.Sin(angle));
        AddObservation(Mathf.Cos(angle));
        AddObservation(rb.angularVelocity);


        if (prev_state.Count == 0)
            Utils.CopyTo(current_state, prev_state);

        

        float s_reward = Mathf.Cos(angle);
        lastAngle = angle;

        print("reward : " + s_reward);
        _Transition.Set(prev_state.ToArray(), action, current_state.ToArray(), s_reward);
        Utils.CopyTo(current_state, prev_state);
        action = rLAgent.SelectAction(prev_state.ToArray());
        MakeAction(action);
        rLAgent.Learn(_Transition);
        episodeCount++;
        if (maxrew < s_reward)
        {
            maxrew = s_reward;
            print("maxrew : " + maxrew);
            rLAgent.ReplaceTarget();
        }
    }

    void MakeAction(int action)
    {
        float torque = force;
        if (action == 1)
            torque = -force;
        
        rb.AddTorque(torque);
    }

    public void AddObservation(double observation)
    {
        current_state.Add(observation);
    }

    void ChangeVars(float ts)
    {
        delay = startDelay / ts;
        wfsr = new WaitForSecondsRealtime(delay);
    }
    private void OnDestroy()
    {
        timeController.ChangeVarsByTimeScale -= ChangeVars;

    }
}
