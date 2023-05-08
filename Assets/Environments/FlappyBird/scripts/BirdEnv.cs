using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdEnv : MonoBehaviour
{
    public FlappyBirdAgent head;
    public Vector2 epsRange;

    public int count;
    public int myId = 0;
    public static int id;
    private void Awake()
    {
        myId = id;
        id++;
        count = FindObjectOfType<MultiEnviroment>().count;
        
        head.epsilon = Mathf.Lerp(epsRange.x, epsRange.y, 1f * myId / (1f * count));
    }
    public void Restart()
    {
        FlappyBirdAgent bird = Instantiate(head, transform);
        bird.env = this;
        bird.epsilon = Mathf.Lerp(epsRange.x, epsRange.y, 1f * myId / (1f * count));
    }
}
