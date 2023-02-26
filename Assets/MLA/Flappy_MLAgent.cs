using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Flappy_MLAgent : Agent
{
    public Transform[] rayPoints;
    public BirdControl birdControl;
    
    private float[] state_;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        birdControl.OnDie += End;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        state_[0] = GetRayDistances()[0];
        state_[1] = GetRayDistances()[1];
        state_[2] = rb.velocity.y;
        sensor.AddObservation(state_[0]);
        sensor.AddObservation(state_[1]);
        sensor.AddObservation(state_[2]);
    }
    


    float[] GetRayDistances()
    {
        float[] distances = new float[rayPoints.Length];

        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = GetRayLength(rayPoints[i]);
            //print(i + " dist : " + distances[i]);

        }
        return distances;
    }

    private float GetRayLength(Transform point)
    {
        float dstns = -1;
        RaycastHit2D hit = Physics2D.Raycast(point.transform.position, point.right, 10, ~LayerMask.GetMask("bird"));
        if (hit.collider != null)
        {
            dstns = hit.distance;
        }
        return dstns;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MakeAction(actions.DiscreteActions[0]);
    }
    void MakeAction(int action)
    {
        if (action == 1)
        {
            print("jump action");
            print("jump null : " + birdControl == null);
            birdControl.JumpUp();
        }
        SetReward(0.01f);
    }

    private void End()
    {
        SetReward(-1);
    }
}
