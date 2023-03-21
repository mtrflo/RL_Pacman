using MonoRL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network = MonoRL.Network;
public class MonoRLAgent : MonoBehaviour
{
    public static MonoRLAgent me;
    public SRLAgent rLAgent;

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
