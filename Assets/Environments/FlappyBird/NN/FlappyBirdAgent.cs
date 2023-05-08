using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoRL;
using PMT;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class FlappyBirdAgent : MonoBehaviour
{
    
    public static int birdsCount = 0;
    public GameMain gameMain;
    public BirdControl birdControl;
    //public DQNAgent dQNAgent => DQNAgent.me;
    public DQNAgent rLAgent => DQNAgent.me;

    private float startDelay;
    public float delay;

    public float reward = 0.1f, terminateReward = -1f, scoreReward = 1, distanceReward = 0f;
    public int distanceRewardCount = 50;
    public static int maxEpisodeCount;
    public int episodeCount;
    public int replaceTargetCount;
    public static int totalEpisodeCount;

    //public TimeController timeController;
    public float epsilon;
    public BirdEnv env;
    public PipeSpawner pipeSpawner;
    private Transition _Transition = new Transition();
    private Rigidbody2D rb;
    private void Awake()
    {
        //timeController = TimeController.me;
        birdsCount++;
        prev_state = new List<double>();
        current_state = new List<double>();

        startDelay = delay;
        //timeController.ChangeVarsByTimeScale += ChangeVars;
        
    }
    bool addReward = false;
    private void Start()
    {
        startPos = birdControl.transform.position;
        startRot = birdControl.transform.rotation;
        rb = birdControl.GetComponent<Rigidbody2D>();
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
        yield return new WaitWhile(()=> pipeSpawner.lastPipe == null);
        
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
        double[] distances = GetRayDistances();
        //foreach (var distance in distances) 
        //    AddObservation(distance);
        Vector3 birdPos = birdControl.transform.position;

        //AddObservation(birdControl.transform.localPosition.y);// bird y pos
        //AddObservation(dis);// pipe distance 
        //AddObservation(pipeSpawner.lastPipe.transform.localPosition.y);
        //AddObservation(Mathf.Abs(pipeSpawner.lastPipe.topPoint_l.position.y) - Mathf.Abs(birdPos.y) ) ;// top point distance 
        //AddObservation(Mathf.Abs(pipeSpawner.lastPipe.bottomPoint.position.y) - Mathf.Abs(birdPos.y) );// bottom point distance 
        //AddObservation(pipeSpawner.lastPipe.bottomPoint.position.y);// bottom point height
        //AddObservation(distances[0]);
        //AddObservation(distances[1]);
        //float leftSign = Mathf.Sign(pipeSpawner.lastPipe.bottomPoint_l.position.x - birdPos.x);
        AddObservation(Vector3.Distance(birdPos, pipeSpawner.lastPipe.bottomPoint_l.position));
        AddObservation(Vector3.Distance(birdPos, pipeSpawner.lastPipe.bottomPoint_r.position));
        AddObservation(Vector3.Distance(birdPos, pipeSpawner.lastPipe.topPoint_l.position));
        AddObservation(Vector3.Distance(birdPos, pipeSpawner.lastPipe.topPoint_r.position));
        AddObservation(distances[0]);
        AddObservation(distances[1]);
        AddObservation(distances[2]);
        AddObservation(distances[3]);
        

        AddObservation(rb.velocity.y);

        //print(current_state.ToCommaSeparatedString());
        if (prev_state.Count == 0)
            Utils.CopyTo(current_state, prev_state);

        float s_reward = reward;
        
        
            
        if (addReward)
        {
            addReward = false;
            s_reward = scoreReward;
        }
        if (birdControl.dead)
        {
            s_reward = terminateReward;
            if (maxEpisodeCount < episodeCount)
            {
                maxEpisodeCount = episodeCount;
                Scoring.me.NewScore(maxEpisodeCount);

                
                print("maxEpisodeCount : " + maxEpisodeCount);
            }
        }
        //print("action : " + action);
        //print("reward : " + s_reward);
        _Transition.Set(prev_state.ToArray(), action, current_state.ToArray(), s_reward, birdControl.dead);
        Utils.CopyTo(current_state, prev_state);
        action = rLAgent.SelectAction(prev_state.ToArray(), epsilon);
        MakeAction(action);
        rLAgent.Learn(_Transition);
        episodeCount++;
        totalEpisodeCount++;
        //if (maxEpisodeCount < episodeCount)
        //{
        //    maxEpisodeCount = episodeCount;
        //    print("maxTimeStep : " + maxEpisodeCount);
        //}
        //if (totalEpisodeCount % replaceTargetCount == 0)
        //{
        //    rLAgent.ReplaceTarget();
        //    print("replace");
        //}
        if (totalEpisodeCount % distanceRewardCount == 0)
        {
            distanceReward += 0.01f;
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
        env.Restart();
        Destroy(gameObject);

        //ResetAgent();
        /*
        birdsCount--;
        if (birdsCount <= 0)
        {
            birdsCount = 0;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Destroy(gameObject);
        }
        */
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
        birdControl.transform.position = startPos;
        birdControl.transform.rotation = startRot;

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
        //timeController.ChangeVarsByTimeScale -= ChangeVars;

    }
}
