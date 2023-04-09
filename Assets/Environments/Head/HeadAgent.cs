using PMT;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
public class HeadAgent : MonoBehaviour
{
    
    public SRLAgent rLAgent => SRLAgent.me;

    public float delay = 1, startDelay;
    public float reward = 0.1f, terminateReward = -1f, scoreReward = 1, distanceReward = 0f;
    //public TimeController timeController;
    public int episodeCount, maxEpisodeCount;
    public float force;
    private Transition _Transition = new Transition();
    
    
    public Rigidbody rb;
    public Rigidbody ballRB;
    public HeadEnv env;
    public float randomRot = 10;
    public float epsilon;
    
    private void Awake()
    {
        startDelay = delay;
        transform.eulerAngles += new Vector3(Random.Range(-randomRot, randomRot), 0, Random.Range(-randomRot, randomRot));
    }
    private void Start()
    {
        Started();
    }

    private void Started()
    {
        StartCoroutine(ActionMaker());
    }
    int action;
    List<double> prev_state = new List<double>(), current_state = new List<double>();
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

    void ChooseAction()
    {
        current_state.Clear();


        AddObservation(rb.transform.eulerAngles.x * Mathf.Deg2Rad);
        AddObservation(rb.transform.eulerAngles.z * Mathf.Deg2Rad);
        AddObservation(ballRB.velocity.x);
        AddObservation(ballRB.velocity.z);


        if (prev_state.Count == 0)
            Utils.CopyTo(current_state, prev_state);


        float s_reward = -ballRB.velocity.magnitude;
        

        
        _Transition.Set(prev_state.ToArray(), action, current_state.ToArray(), s_reward);
        Utils.CopyTo(current_state, prev_state);

        if (ballRB.transform.position.y < 0.3f)
        {
            s_reward = terminateReward;
            _Transition.isDone = true;
        }


        action = rLAgent.SelectAction(prev_state.ToArray(),epsilon);
        MakeAction(action);
        
        rLAgent.Learn(_Transition);
        
        episodeCount++;
        if (maxrew < s_reward)
        {
            maxrew = s_reward;
            print("maxrew : " + maxrew);
            rLAgent.ReplaceTarget();
        }
        
        print("reward : " + s_reward);

        if(_Transition.isDone)
            Restart();
    }
    void MakeAction(int action)
    {
        // 0  1  2  3
        // -x x -z  z
        float x = 0,z = 0;
        switch (action)
        { 
            case 0: x = -force;break;
            case 1: x = force;break;
            case 2: z = force;break;
            case 3: z = -force;break;
        }
        rb.AddTorque(x, 0, z);
    }

    public void AddObservation(double observation)
    {
        current_state.Add(observation);
    }
    private void Restart()
    {
        env.Restart();
        Destroy(gameObject);
    }
}
