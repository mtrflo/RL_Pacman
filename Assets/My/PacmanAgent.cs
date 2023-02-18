using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PacmanAgent : MonoBehaviour
{
    public DPacManController DPacManController;
    public PacMapGenerator pacMapGenerator;
    public NetworkTrainer networkTrainer;
    [Range(0,1)]
    public float epsilon = 0.8f;
    private void Start()
    {
        DPacManController.OnMoved += Step;
        Step();
    }
    int lastAction = 0;
    float lastStepReward = 0;
    void Step()
    {
        //state
        double[] inputs = ConvertMapToVector(pacMapGenerator.cur_map);
        lastStepReward = DPacManController.reward;
        print("lastStepReward : " + lastStepReward);
        float eps = Random.Range(0f, 1f);
        lastAction = Random.Range(0, 4);
        if (eps < epsilon)
            lastAction = networkTrainer.ChooseAction(inputs, lastAction, lastStepReward);
        
        print("action : " + lastAction);
        Action(lastAction);
    }

    void Action(int action)
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

    private double[] ConvertMapToVector(List<List<int>> map)
    {
        int mapWidth = map.Count,
            mapHeight = map[0].Count;
        ;
        double[] vector = new double[mapWidth * mapHeight];
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
