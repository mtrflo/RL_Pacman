using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlappyBirdAgent : MonoBehaviour
{
    public GameMain gameMain;
    public BirdControl birdControl;
    public DQNAgent dQNAgent;

    public float delay;

    public float reward = 0.1f, terminateReward = -1f;
    public static int maxEpisodeCount;
    public int episodeCount;
    public int transitionCount;
    public UdpSocket udpSocket;
    Transition transition;
    private void Awake()
    {
        dQNAgent = FindObjectOfType<DQNAgent>();
        udpSocket = dQNAgent.GetComponent<UdpSocket>();
        transition = new Transition();
    }
    Rigidbody2D rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameMain.OnGameStarted += Started;
        birdControl.OnDie += ChooseAction;
        if (birdControl.inGame)
            Started();
        udpSocket.OnReceived += CA2;
    }

    private void Started()
    {
        StartCoroutine(ActionMaker());
    }
    int action;
    double[] state, state_ = new double[3];
    bool newData = false;
    IEnumerator ActionMaker()
    {
        while (birdControl.inGame || birdControl.dead)
        {
            ChooseAction();
            yield return new WaitForSeconds(delay);
            yield return new WaitWhile(() => newData);
            newData = false;
            if (birdControl.dead)
                break;
        }
        if(birdControl.dead)
            Invoke("Restart", Time.deltaTime * 3);
    }
    void ChooseAction()
    {
        state_[0] = GetRayDistances()[0];
        state_[1] = GetRayDistances()[1];
        state_[2] = rb.velocity.y;
        if (state == null)
            state = state_;

        if (birdControl.dead)
            reward = terminateReward;
        print("action : " + action);
        print("reward : " + reward);
        transition.Set(state, action, state_, reward);
        print("json : "+JsonUtility.ToJson(transition));
        udpSocket.SendData(JsonUtility.ToJson(transition));
        //dQNAgent.Learn(state, action, state_, reward, birdControl.dead);
        

        //if (episodeCount % transitionCount == 0)
        //    dQNAgent.Learn();
    }
    private void CA2(string data)
    {
        print(data);
        action = dQNAgent.ChooseAction(state);
        MakeAction(action);
        state = state_;

        episodeCount++;
        if (maxEpisodeCount < episodeCount)
        {
            maxEpisodeCount = episodeCount;
            print("maxEpisodeCount : " + maxEpisodeCount);
        }
        newData = true;
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
        RaycastHit2D hit = Physics2D.Raycast(point.transform.position, point.right,10,~LayerMask.GetMask("bird"));
        if (hit.collider != null)
        {
            dstns = hit.distance;
        }
        return dstns;
    }

    void MakeAction(int action)
    {
        if(action == 1)
        {
            birdControl.JumpUp();
        }
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
