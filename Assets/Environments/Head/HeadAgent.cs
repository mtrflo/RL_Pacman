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
    public Transform point;
    public float randomRot = 10;
    public float epsilon;

    private void Awake()
    {
        startDelay = delay;
        rb.transform.eulerAngles += new Vector3(Random.Range(-randomRot, randomRot), 0, Random.Range(-randomRot, randomRot));
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
    float RotT(float er) => er > 270 ? 360 - er : er;
    void ChooseAction()
    {
        current_state.Clear();


        AddObservation(rb.transform.rotation.x);
        AddObservation(rb.transform.rotation.y);
        AddObservation(rb.transform.rotation.z);
        AddObservation(rb.transform.rotation.w);
        AddObservation(ballRB.velocity.x);
        AddObservation(ballRB.velocity.z);
        AddObservation(ballRB.transform.localPosition.x);
        AddObservation(ballRB.transform.localPosition.z);



        if (prev_state.Count == 0)
            Utils.CopyTo(current_state, prev_state);

        Vector3 center = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 ballCenter = new Vector3(ballRB.transform.position.x, 0, ballRB.transform.position.z);
        float distance = Vector3.Distance(point.position, ballRB.transform.position);
        float vel = ballRB.velocity.magnitude;
        //print("distance : " + distance);
        float s_reward = reward - 0.1f * distance - 2 * vel;



        _Transition.Set(prev_state.ToArray(), action, current_state.ToArray(), s_reward);
        Utils.CopyTo(current_state, prev_state);

        if (ballRB.transform.position.y < 0.5f)
        {
            s_reward = terminateReward;
            _Transition.isDone = true;
        }


        action = rLAgent.SelectAction(prev_state.ToArray(), epsilon);
        //if (Input.GetKey(KeyCode.UpArrow))
        //    action = 1;
        //if (Input.GetKey(KeyCode.DownArrow))
        //    action = 0;
        //if (Input.GetKey(KeyCode.LeftArrow))
        //    action = 2;
        //if (Input.GetKey(KeyCode.RightArrow))
        //    action = 3;
        MakeAction(action);

        rLAgent.Learn(_Transition);

        episodeCount++;
        if (maxrew < s_reward)
        {
            maxrew = s_reward;
            //print("maxrew : " + maxrew);
            rLAgent.ReplaceTarget();
        }

        print("reward : " + s_reward);

        if (ballRB.transform.position.y < 0.5f)
            Restart();
    }
    void MakeAction(int action)
    {
        // 0  1  2  3
        // -x x -z  z
        float x = 0, z = 0;
        switch (action)
        {
            case 0: x = -force; break;
            case 1: x = force; break;
            case 2: z = force; break;
            case 3: z = -force; break;
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
