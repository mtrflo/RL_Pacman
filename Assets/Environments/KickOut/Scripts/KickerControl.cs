using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickerControl : MonoBehaviour
{
    public KickerAgent agent;
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Arena"))
        {
            agent.isPlaying = false;
        }
    }
}
