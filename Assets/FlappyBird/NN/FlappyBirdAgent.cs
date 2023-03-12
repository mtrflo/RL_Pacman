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

public class FlappyBirdAgent : MonoBehaviour
{
    public static int birdsCount = 0;
    public GameMain gameMain;
    public BirdControl birdControl;
    //public DQNAgent dQNAgent => DQNAgent.me;
    public SRLAgent rLAgent => SRLAgent.me;

    public float delay;

    public float reward = 0.1f, terminateReward = -1f;
    public static int maxEpisodeCount;
    public int episodeCount;
    public int transitionCount;
    
    private Transition _Transition = new Transition();
    private Rigidbody2D rb;
    private void Awake()
    {
        birdsCount++;
        prev_state = new double[3];
        current_state = new double[3];
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
    double[] prev_state, current_state;
    IEnumerator ActionMaker()
    {
        WaitForSecondsRealtime wfsr = new WaitForSecondsRealtime(delay);
        while (birdControl.inGame)
        {
            ChooseAction();
            yield return wfsr;
            if (birdControl.dead)
                break;
        }
        if (birdControl.dead)
            Restart();
    }
    void ChooseAction()
    {
        current_state[0] = GetRayDistances()[0];
        current_state[1] = GetRayDistances()[1];
        current_state[2] = rb.velocity.y;
        float s_reward = reward;
        if (birdControl.dead)
            s_reward = terminateReward;
        //print("action : " + action);
        //print("reward : " + reward);
        _Transition.Set(prev_state, action, current_state, s_reward, birdControl.dead);
        current_state.CopyTo(prev_state, 0);
        
        action = rLAgent.SelectAction(prev_state);
        MakeAction(action);
        rLAgent.Learn(_Transition);
        episodeCount++;
        if (maxEpisodeCount < episodeCount)
        {
            maxEpisodeCount = episodeCount;
            print("maxEpisodeCount : " + maxEpisodeCount);
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
        prev_state = new double[3];
        current_state = new double[3];
        episodeCount = 0;
        transform.position = startPos;
        transform.rotation = startRot;

        birdControl.ResetComponent();


        StartCoroutine(ActionMaker());
    }
}
