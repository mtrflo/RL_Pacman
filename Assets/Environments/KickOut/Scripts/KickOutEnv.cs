using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickOutEnv : MonoBehaviour
{
    public KickerAgent agent;
    public Vector2 epsRange;
    private KickerAgent lastAgent;

    public int count;
    public int myId = 0;
    public static int id;
    private void Awake()
    {
        myId = id;
        id++;
        count = FindObjectOfType<MultiEnviroment>().count;

        lastAgent = transform.GetChild(0).GetComponent<KickerAgent>();
        lastAgent.epsilon = Mathf.Lerp(epsRange.x, epsRange.y, 1f * myId / (1f * count));
    }
    public void Restart()
    {
        if (lastAgent)
            Destroy(lastAgent.gameObject);
        lastAgent = Instantiate(agent, transform);
        lastAgent.GetComponentInChildren<KickerAgent>().env = this;
        lastAgent.env = this;
        lastAgent.epsilon = Mathf.Lerp(epsRange.x, epsRange.y, 1f * myId / (1f * count));
    }
}
