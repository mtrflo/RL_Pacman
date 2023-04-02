using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoRL;
using PMT;
using UnityEngine.SceneManagement;

public class FlappyBirdAgent : MonoBehaviour
{
    public static int birdsCount = 0;
    public GameMain gameMain;
    public BirdControl birdControl;
    //public DQNAgent dQNAgent => DQNAgent.me;
    public SRLAgent rLAgent => SRLAgent.me;

    private float startDelay;
    public float delay;

    public float reward = 0.1f, terminateReward = -1f, scoreReward = 1;
    public static int maxEpisodeCount;
    public int episodeCount;
    public int transitionCount;

    public TimeController timeController;

    private Transition _Transition = new Transition();
    private Rigidbody2D rb;
    private void Awake()
    {
        timeController = TimeController.me;
        birdsCount++;
        prev_state = new List<double>();
        current_state = new List<double>();

        startDelay = delay;
        timeController.ChangeVarsByTimeScale += ChangeVars;
    }
    bool addReward = false;
    private void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        rb = GetComponent<Rigidbody2D>();
        gameMain.OnGameStarted += Started;
        birdControl.OnDie += ChooseAction;
        birdControl.OnPipePassed += () => addReward = true;
        if (birdControl.inGame)
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
        yield return new WaitWhile(()=> PipeMove.lastPipe == null);
        BirdControl.pipe = PipeMove.lastPipe;
        wfsr = new WaitForSecondsRealtime(delay);
        while (birdControl.inGame || birdControl.dead)
        {
            yield return wfsr;
            ChooseAction();
            if (birdControl.dead)
                break;
        }
        
    }
    void ChooseAction()
    {
        current_state.Clear();
        //double[] distances = GetRayDistances();
        //foreach (var distance in distances) 
        //    AddObservation(distance);
        
        AddObservation(transform.position.y);// bird y pos
        
        AddObservation(Mathf.Abs(Mathf.Abs(BirdControl.pipe.topPoint.position.x) - Mathf.Abs(transform.position.x)));// pipe distance 
        AddObservation(Mathf.Abs(Mathf.Abs(BirdControl.pipe.topPoint.position.y) - Mathf.Abs(transform.position.y)));// top point distance 
        if(BirdControl.pipe.bottomPoint)
            AddObservation(Mathf.Abs(Mathf.Abs(BirdControl.pipe.bottomPoint.position.y) - Mathf.Abs(transform.position.y)));// bottom point distance 
        
        AddObservation(rb.velocity.y);

        if (prev_state.Count == 0)
            Utils.CopyTo(current_state, prev_state);

        float s_reward = reward;
        if (birdControl.dead)
            s_reward = terminateReward;
        if (addReward)
        {
            addReward = false;
            s_reward = scoreReward;
        }

        //print("action : " + action);
        print("reward : " + s_reward);
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
        
        if (birdControl.dead)
            Restart();
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
        Destroy(gameObject);

        //ResetAgent();
        birdsCount--;
        if (birdsCount <= 0)
        {
            birdsCount = 0;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Destroy(gameObject);
        }
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
