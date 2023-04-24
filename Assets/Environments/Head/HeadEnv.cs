using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadEnv : MonoBehaviour
{
    public HeadAgent head;

    public int count;
    public int myId = 0;
    public static int id;
    private void Awake()
    {
        myId = id;
        id++;
        head.epsilon = Mathf.Lerp(0f, 1f, 1f* myId / (1f*count) );
        count = FindObjectOfType<MultiEnviroment>().count;
    }
    public void Restart()
    {
        HeadAgent ha = Instantiate(head, transform);
        ha.env = this;
        ha.epsilon = Mathf.Lerp(0f, 1f, 1f * myId / (1f * count));
    }
}