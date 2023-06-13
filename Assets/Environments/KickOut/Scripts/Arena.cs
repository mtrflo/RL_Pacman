using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
    public KickOutEnv env;
    public Action<GameObject> OnExit;
    private void Awake()
    {
        OnExit = (go) => { };
    }
    private void OnTriggerExit(Collider other)
    {
        //print("arena exit : "+ other.tag);
        if (other.CompareTag("Player"))
        {
            OnExit.Invoke(other.gameObject);
            //env.Restart();
        }
    }
}
