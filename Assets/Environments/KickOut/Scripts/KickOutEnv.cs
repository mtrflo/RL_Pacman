using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickOutEnv : MonoBehaviour
{
    public Vector2 epsRange;
    public EnvCon envCon;
    public EnvCon lastEnvCon;

    public int count;
    public int myId = 0;
    public static int id;
    private void Awake()
    {
        myId = id;
        id++;
        count = FindObjectOfType<MultiEnviroment>().count;
    }
    public void Restart()
    {
        if (lastEnvCon)
            Destroy(lastEnvCon.gameObject);
        lastEnvCon = Instantiate(envCon, transform);
        lastEnvCon.kickOutEnv = this;
        lastEnvCon.epsilon = Mathf.Lerp(epsRange.x, epsRange.y, 1f * myId / (1f * count));
    }
}
