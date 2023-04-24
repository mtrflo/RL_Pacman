using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lose : MonoBehaviour
{
    public HeadAgent HeadAgent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Equals("Ball"))
            HeadAgent.falled = true;
    }
}
