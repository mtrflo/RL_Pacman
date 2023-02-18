using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DQNAgent : MonoBehaviour
{
    public NeuralNetwork neuralNetwork;
    public NetworkTrainer trainer;
    [Range(0,1)]
    public float epsilon = 0.8f;//exploit - explore     0-1
    [Range(0, 1)]
    public float gamma = 0.8f;

    public int ChooseAction(double[] state)
    {
        int action = 0;
        float e = Random.Range(0, 1f);

        if (true)
        {
            double[] actionValues = trainer.neuralNetwork.Forward(state);
            action = actionValues.ToList().IndexOf(actionValues.Max());
        }
        else
            action = Random.Range(0, 4);

        return action;
    }

    public void Learn(double[] state,int action,double[] prev_state, double reward)
    {
        double QEval = trainer.neuralNetwork.Forward(state)[action];
        double QNext = trainer.neuralNetwork.Forward(prev_state).Max();
        double QTarget = reward + gamma * QNext;


        trainer.Learn(state, action, QTarget, QEval);
        // loss = NN.loss(QEval, QTarget)
        // NN.backpropogate()
    }

    
}
