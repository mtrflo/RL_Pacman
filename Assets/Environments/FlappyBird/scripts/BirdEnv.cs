using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdEnv : MonoBehaviour
{
    public FlappyBirdAgent birdAgent;
    [HideInInspector] public FlappyBirdAgent currentBird;
    public Vector2 epsRange;

    public int count;
    public int myId = 0;
    public static int id;
    private void Awake()
    {
        myId = id;
        id++;
        count = FindObjectOfType<MultiEnviroment>().count;
        
        birdAgent.epsilon = Mathf.Lerp(epsRange.x, epsRange.y, 1f * myId / (1f * count));
        currentBird = birdAgent;
    }
    public void Restart()
    {
        name = "0";
        FlappyBirdAgent bird = Instantiate(birdAgent, transform);
        currentBird = bird;
        bird.env = this;
        bird.epsilon = Mathf.Lerp(epsRange.x, epsRange.y, 1f * myId / (1f * count));
    }
}
