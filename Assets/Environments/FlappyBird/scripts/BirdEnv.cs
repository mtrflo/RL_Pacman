using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdEnv : MonoBehaviour
{
    public FlappyBirdAgent head;

    public int count;
    public int myId = 0;
    public static int id;
    private void Awake()
    {
        myId = id;
        id++;
        head.epsilon = Mathf.Lerp(0f, 1f, 1f * myId / (1f * count));
        count = FindObjectOfType<MultiEnviroment>().count;
    }
    public void Restart()
    {
        FlappyBirdAgent bird = Instantiate(head, transform);
        bird.env = this;
        bird.epsilon = Mathf.Lerp(0f, 1f, 1f * myId / (1f * count));
    }
}
