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
    private void Awake()
    {
        dQNAgent = FindObjectOfType<DQNAgent>();
    }
    private void Start()
    {
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
    double[] state;
    IEnumerator ActionMaker()
    {
        while (birdControl.inGame || birdControl.dead)
        {
            ChooseAction();
            yield return new WaitForSeconds(delay);
            if (birdControl.dead)
                break;
        }
        if(birdControl.dead)
            Invoke("Restart", Time.deltaTime * 3);
    }
    void ChooseAction()
    {
        double[] state_ = GetRayDistances();
        if (state == null)
            state = state_;

        if (birdControl.dead)
            reward = terminateReward;
        print("action : " + action);
        print("reward : " + reward);

        dQNAgent.Learn(state, action, state_, reward, birdControl.dead);
        action = dQNAgent.ChooseAction(state);
        MakeAction(action);
        state = state_;
    }
    public Transform[] rayPoints;
    double[] GetRayDistances()
    {
        double[] distances = new double[rayPoints.Length];

        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = GetRayLength(rayPoints[i]);
            print(i + " dist : " + distances[i]);

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
