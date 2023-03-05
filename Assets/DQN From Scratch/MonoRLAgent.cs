using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoRLAgent : MonoBehaviour
{
    public static MonoRLAgent me;
    public RLAgent rLAgent;

    private void Awake()
    {
        if (me != null)
        {
            Destroy(gameObject);
            return;
        }
        me = this;
    }
}
