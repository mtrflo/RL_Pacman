using PMT;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class KickerAgent : MonoBehaviour
{
    public float delay;
    WaitForSecondsRealtime wfsr;
    private Transition _Transition = new Transition();
    public DQNAgent rLAgent => DQNAgent.me;
    public float epsilon;
    public int teamID;

    int action;
    List<float> prev_state, current_state;

    public float reward, term_reward, win_reward;


    public bool IsAgent = true;
    public float speed, bounce = 10; // speed of the bot movement
    public Rigidbody rb;
    public Rigidbody enemyRB;
    public bool isPlaying = true;
    public bool win = false;
    public KickOutEnv env;
    public int episodeCount;
    public static int totalEpisodeCount;
    public Arena arena;
    public EnvCon envcon;

    float network_lr;

    void Start()
    {
        network_lr = rLAgent.network.LearningRate;
        epsilon = envcon.epsilon;
        env = envcon.kickOutEnv;
        arena.OnExit += (go) =>
        {
            isPlaying = false;
            win = !(go == rb.gameObject);
        };

        prev_state = new List<float>();
        current_state = new List<float>();
        StartCoroutine(ActionMaker());
    }


    IEnumerator ActionMaker()
    {

        wfsr = new WaitForSecondsRealtime(delay);
        while (true)
        {
            yield return wfsr;
                ChooseAction();
            if (!isPlaying)
                break;
        }

    }
    void ChooseAction()
    {
        float s_reward = isPlaying ? reward : (win ? win_reward : term_reward);
        print("win , "+ win);
        current_state.Clear();

        AddObservation(
            rb.transform.localPosition.x,
            rb.transform.localPosition.z,
            rb.velocity.x,
            rb.velocity.y,
            enemyRB.transform.localPosition.x,
            enemyRB.transform.localPosition.z,
            enemyRB.velocity.x,
            enemyRB.velocity.z
            );

        if (prev_state.Count == 0)
            Utils.CopyTo(current_state, prev_state);

        _Transition.Set(prev_state.ToArray(), action, current_state.ToArray(), s_reward, win);
        Utils.CopyTo(current_state, prev_state);
        action = IsAgent ? rLAgent.SelectAction(prev_state.ToArray(), epsilon) : PlayerChooseAction();
        MakeAction(action);
        //if (IsAgent)
        //    rLAgent.network.LearningRate = network_lr;
        //else
        //    rLAgent.network.LearningRate = network_lr * 10;
        rLAgent.Learn(_Transition);
        episodeCount++;
        totalEpisodeCount++;

        print("s_reward : "+s_reward);
        if (!isPlaying)
            Restart();

    }
    int PlayerChooseAction()
    {
        int action = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
            action = 1;
        if (Input.GetKey(KeyCode.RightArrow))
            action = 2;
        if (Input.GetKey(KeyCode.DownArrow))
            action = 3;
        if (Input.GetKey(KeyCode.UpArrow))
            action = 4;
        //MakeAction(action);
        return action;
        if (!isPlaying)
            Restart();
    }
    void MakeAction(int i)
    {
        float horizontal = 0.0f;
        float vertical = 0.0f;
        switch (i)
        {
            case 1:
                horizontal = -1.0f; break;
            case 2:
                horizontal = 1.0f; break;
            case 3:
                vertical = -1.0f; break;
            case 4:
                vertical = 1.0f; break;
        }

        // move the bot
        Vector3 movement = new Vector3(horizontal, 0.0f, vertical);
        rb.AddForce(movement * speed);
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    print("Trigger exit : " + other.tag);
    //    if (other.CompareTag("Arena"))
    //        isPlaying = false;
    //}
    public void AddObservation(params float[] observation)
    {
        for (int i = 0; i < observation.Length; i++)
        {
            observation[i] = observation[i] / 10;
        }
        current_state.AddRange(observation);
    }

    void Restart()
    {
        env.Restart();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //print("-rb.velocity  : " + (-rb.velocity));
            //print("collision.impulse : " + collision.impulse);
            Vector3 dir = rb.transform.position - enemyRB.transform.position;
            //rb.velocity = collision.rigidbody.velocity;
            //rb.AddForce(dir * ( rb.velocity.magnitude + collision.rigidbody.velocity.magnitude ) * speed * bounce);
            rb.AddForce(collision.impulse * bounce);
            //rb.AddForce(-collision.impulse * 300);
        }
    }
}
