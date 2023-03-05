using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlappyBirdAgent : MonoBehaviour
{
    public static int birdsCount = 0;
    public GameMain gameMain;
    public BirdControl birdControl;
    public DQNAgent dQNAgent => DQNAgent.me;

    public float delay;

    public float reward = 0.1f, terminateReward = -1f;
    public static int maxEpisodeCount;
    public int episodeCount;
    public int transitionCount;
    public UdpSocket udpSocket => UdpSocket.me;
    Transition transition;
    private void Awake()
    {
        birdsCount++;
    }
    Rigidbody2D rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameMain.OnGameStarted += Started;
        birdControl.OnDie += ChooseAction;
        if (birdControl.inGame)
            Started();
        print("start");
    }

    private void Started()
    {
        StartCoroutine(ActionMaker());
    }
    int action;
    double[] state, state_ = new double[3];
    bool isWaiting = true;
    IEnumerator ActionMaker()
    {
        WaitForSeconds wfs = new WaitForSeconds(delay);
        WaitWhile wh = new WaitWhile(()=>isWaiting);
        while (birdControl.inGame || birdControl.dead)
        {
            ChooseAction();
            //yield return wh;
            yield return wfs;
            if (birdControl.dead)
                break;
        }
        if (birdControl.dead)
            Restart();
            //Invoke("Restart", Time.deltaTime * 3);
    }
    void ChooseAction()
    {
        print("ChooseAction");
        isWaiting = true;
        state_[0] = GetRayDistances()[0];
        state_[1] = GetRayDistances()[1];
        state_[2] = rb.velocity.y;
        if (state == null)
            state = state_;

        if (birdControl.dead)
            reward = terminateReward;
        print("action : " + action);
        print("reward : " + reward);
        //transition.Set(state, action, state_, reward, birdControl.dead);
        //print("json : " + JsonUtility.ToJson(transition));
        //CA2(udpSocket.SendAndGetData(JsonUtility.ToJson(transition)));
        dQNAgent.Learn(state,action,state_,reward, birdControl.dead);
        action = dQNAgent.ChooseAction(state);
        MakeAction(action);
        state = state_;

        episodeCount++;
        if (maxEpisodeCount < episodeCount)
        {
            maxEpisodeCount = episodeCount;
            print("maxEpisodeCount : " + maxEpisodeCount);
            dQNAgent.ReplaceTarget();
        }
    }
    private void CA2(string data)
    {
        print("data : " + data);
        action = dQNAgent.ChooseAction(state);
        MakeAction(action);
        state = state_;

        episodeCount++;
        if (maxEpisodeCount < episodeCount)
        {
            maxEpisodeCount = episodeCount;
            print("maxEpisodeCount : "+maxEpisodeCount);
            dQNAgent.ReplaceTarget();
        }
    }
    public Transform[] rayPoints;
    double[] GetRayDistances()
    {
        double[] distances = new double[rayPoints.Length];

        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = GetRayLength(rayPoints[i]);
            //print(i + " dist : " + distances[i]);

        }
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
        {
            print("jump action");
            print("jump null : "+birdControl==null);
            birdControl.JumpUp();
        }
    }

    void Restart()
    {
        birdsCount--;
        if (birdsCount <= 0)
        {
            birdsCount = 0;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
