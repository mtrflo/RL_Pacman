using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using MonoRL;
using Unity.MLAgents;
using PMT;
using MonoRL;
public class FlappyBirdAgent : MonoBehaviour
{
    public static int birdsCount = 0;
    public GameMain gameMain;
    public BirdControl birdControl;
    //public DQNAgent dQNAgent => DQNAgent.me;
    public SRLAgent rLAgent => SRLAgent.me;
    
    private float startDelay;
    public float delay;

    public float reward = 0.1f, terminateReward = -1f;
    public static int maxEpisodeCount;
    public int episodeCount;
    public int transitionCount;
    
    public TimeController timeController;

    private Transition _Transition = new Transition();
    private Rigidbody2D rb;
    private void Awake()
    {
        birdsCount++;
        prev_state = new List<double>();
        current_state = new List<double>();

        startDelay = delay;
        timeController.ChangeVarsByTimeScale += (ts) => { delay = startDelay / ts; };
    }
    private void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        rb = GetComponent<Rigidbody2D>();
        gameMain.OnGameStarted += Started;
        birdControl.OnDie += ChooseAction;
        if (birdControl.inGame)
            Started();
    }

    private void Started()
    {
        StartCoroutine(ActionMaker());
    }
    int action;
    List<double> prev_state, current_state;
    IEnumerator ActionMaker()
    {
        //WaitForSecondsRealtime wfsr = new WaitForSecondsRealtime(delay);
        while (birdControl.inGame)
        {
            ChooseAction();
            yield return new WaitForSecondsRealtime(delay);
            if (birdControl.dead)
                break;
        }
        if (birdControl.dead)
            Restart();
    }
    void ChooseAction()
    {
        current_state.Clear();
        double[] distances = GetRayDistances();
        foreach (var distance in distances) 
            AddObservation(distance);
        
        AddObservation(rb.velocity.y);
        
        if (prev_state.Count == 0)
            Utils.CopyTo(current_state, prev_state);

        float s_reward = reward;
        if (birdControl.dead)
            s_reward = terminateReward;
        //print("action : " + action);
        //print("reward : " + reward);
        _Transition.Set(prev_state.ToArray(), action, current_state.ToArray(), s_reward, birdControl.dead);
        Utils.CopyTo(current_state, prev_state);
        action = rLAgent.SelectAction(prev_state.ToArray());
        MakeAction(action);
        rLAgent.Learn(_Transition);
        episodeCount++;
        if (maxEpisodeCount < episodeCount)
        {
            maxEpisodeCount = episodeCount;
            print("maxTimeStep : " + maxEpisodeCount);
            rLAgent.ReplaceTarget();
        }
    }

    public Transform[] rayPoints;
    double[] GetRayDistances()
    {
        double[] distances = new double[rayPoints.Length];

        for (int i = 0; i < distances.Length; i++)
            distances[i] = GetRayLength(rayPoints[i]);
        return distances;
    }

    private double GetRayLength(Transform point)
    {
        double dstns = -1;
        RaycastHit2D hit = Physics2D.Raycast(point.transform.position, point.right, 10, ~LayerMask.GetMask("bird"));
        if (hit.collider != null)
        {
            dstns = hit.distance;
        }
        if (dstns == -1)
            print("-111");
        return dstns;
    }

    void MakeAction(int action)
    {
        if (action == 1)
            birdControl.JumpUp();
    }

    void Restart()
    {
        ResetAgent();
        //birdsCount--;
        //if (birdsCount <= 0)
        //{
        //    birdsCount = 0;
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //}
    }
    Vector3 startPos;
    Quaternion startRot;
    public void ResetAgent()
    {
        rb.velocity = Vector2.zero;
        rb.gravityScale = 1;
        prev_state.Clear();
        current_state.Clear();
        episodeCount = 0;
        transform.position = startPos;
        transform.rotation = startRot;

        birdControl.ResetComponent();


        StartCoroutine(ActionMaker());
    }

    public void AddObservation(double observation)
    {
        current_state.Add(observation);
    }

}
