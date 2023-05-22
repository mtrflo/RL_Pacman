using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PacmanAgent : MonoBehaviour
{
    public DPacManController DPacManController;
    public PacMapGenerator pacMapGenerator;
    public DQNAgent dQNAgent;
    private void Start()
    {
        DPacManController.OnMoved += Step;
        Step();
    }

    int lastAction = 0;
    float lastStepReward = 0;



    //Transition
    float[] state;
    int action;
    float[] state_;
    float reward;
    //



    void Step()
    {
        //state
        float[] state_ = ConvertMapToVector(pacMapGenerator.cur_map);
        reward = DPacManController.reward;


        //next_state, reward = make_action()

        if (state != null)
        {
            // print("action : " + action);
            print("reward : " + reward);
            //dQNAgent.Learn(state, action, state_, reward);
        }
        else
            state = state_;
        //action = dQNAgent.ChooseAction(state);
        MakeAction(action);

        state = state_;

        //if(step == 100) {
        //    dQNAgent.Learn();
        //    dQNAgent.ClearTransition();
        //}


    }

    void MakeAction(int action)
    {
        Vector2 to = new Vector2();
        switch (action)
        {
            case 0:
                to.x = 1;
                break;
            case 1:
                to.x = -1;
                break;
            case 2:
                to.y = 1;
                break;
            case 3:
                to.y = -1;
                break;
        }
        DPacManController.MoveToSide(to.x, to.y);
    }

    private float[] ConvertMapToVector(List<List<int>> map)
    {
        int mapWidth = map.Count,
            mapHeight = map[0].Count;
        ;
        float[] vector = new float[mapWidth * mapHeight];
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                vector[i * mapWidth + j] = map[i][j];
            }
        }
        return vector;
    }
}
