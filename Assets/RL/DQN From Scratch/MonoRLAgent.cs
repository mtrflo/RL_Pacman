using MonoRL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MonoRLAgent : MonoBehaviour
{
    public static MonoRLAgent me;
    public DQNAgent rLAgent;

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
