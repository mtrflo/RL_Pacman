using MathNet.Numerics.Distributions;
using PMT;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
public class PendulumAgent : MonoBehaviour
{
    public Rigidbody rb;
    public Reinforce RLAlg => Reinforce.me;
    //public DQNAgent RLAlg => DQNAgent.me;

    public float delay = 1, startDelay;
    public float reward = 0.1f, terminateReward = -1f, scoreReward = 1, distanceReward = 0f;
    //public TimeController timeController;
    public int episodeCount, maxEpisodeCount;
    public Transform point;
    public float force;
    private Transition _Transition = new Transition();
    public bool greedy = false;
    static int oid;
    public float epsilon;
    private void Awake()
    {
        startDelay = delay;
        //timeController = TimeController.me;
        prev_state = new List<float>();
        current_state = new List<float>();

        //timeController.ChangeVarsByTimeScale += ChangeVars;
        epsilon = Mathf.Lerp(0, 1, oid / (float)MultiEnviroment.me.count);
        oid++;
    }
    bool addReward = false;
    private void Start()
    {
        transitions = new List<Transition>();
        Started();
    }

    private void Started()
    {
        StartCoroutine(ActionMaker());
    }
    int action;
    List<float> prev_state, current_state;
    WaitForSecondsRealtime wfsr;
    IEnumerator ActionMaker()
    {
        yield return new WaitForSeconds(Random.Range(0, 2f));
        wfsr = new WaitForSecondsRealtime(delay);
        while (true)
        {
            yield return wfsr;
            ChooseAction();
        }

    }
    float maxrew = -10000;
    float lastAngle = 0;
    
    void ChooseAction()
    {
        current_state.Clear();
        
        Vector3 toPo = (point.position - transform.position).normalized;
        float angle = Vector3.Angle(toPo, Vector3.up);
        float sign = MathF.Sign(point.position.x - transform.position.x);
        if(sign > 0)
        {
            angle = 180 + (180 - angle);
        }
        angle = angle * Mathf.Deg2Rad;
        //print("angle : " + angle);
        //print("angle : " + angle);
        
        
        AddObservation(Mathf.Sin(angle));
        AddObservation(Mathf.Cos(angle));
        //AddObservation(Mathf.Sign(rb.angularVelocity.magnitude));
        float angvel = rb.angularVelocity.magnitude * Mathf.Sign(rb.angularVelocity.z);
        AddObservation(angvel);


        if (prev_state.Count == 0)
            Utils.CopyTo(current_state, prev_state);


        //print("Mathf.Cos(angle) : " + Mathf.Cos(angle));
        float s_reward = Mathf.Cos(angle);
        //s_reward = Mathf.Abs(rb.angularVelocity.magnitude);
        //r = -(theta2 + 0.1 * theta_dt2 + 0.001 * torque2)
        //float torque = action == 1 ? -force : force;// Mathf.Lerp(-force, force, action / 10f);
        //float s_reward = -(Mathf.Pow(angle, 2) + 0.1f * Mathf.Pow((lastAngle - angle), 2) + 0.001f * Mathf.Pow(torque, 2)) + 1;
        lastAngle = angle;

        //print("reward : " + s_reward);
        _Transition.Set(prev_state.ToArray(), action, current_state.ToArray(), s_reward);
        Utils.CopyTo(current_state, prev_state);
        action = greedy ? RLAlg.ChooseAction(prev_state.ToArray()) : RLAlg.SampleAction(prev_state.ToArray());
        //action = RLAlg.SelectAction(current_state.ToArray(), epsilon);
        MakeAction(action);
        if (!greedy)
            Learn();
        //RLAlg.Learn(this,_Transition);
        episodeCount++;
        //if (maxrew < s_reward)
        //{
        //    maxrew = s_reward;
        //    print("maxrew : " + maxrew);
        //    RLAlg.ReplaceTarget();
        //}
    }

    List<Transition> transitions;
    int tCounter;
    bool trs = false;
    void Learn()
    {
        transitions.Add(_Transition);
        tCounter++;

        if (tCounter == RLAlg.trajectoryLength)
        {
            RLAlg.Learn(transitions.ToArray());

            tCounter = 0;
            trs = true;
        }
        if (trs)
            transitions.RemoveAt(0);
    }

    void MakeAction(int action)
    {
        float torque = force;
        if (action == 1)
            torque = -force;

        rb.AddTorque(0, 0, torque);
    }

    public void AddObservation(float observation)
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
        //timeController.ChangeVarsByTimeScale -= ChangeVars;

    }
}
