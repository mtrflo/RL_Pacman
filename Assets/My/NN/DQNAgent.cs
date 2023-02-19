using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DQNAgent : MonoBehaviour
{
    public NeuralNetwork neuralNetwork;
    public NetworkTrainer trainer;
    [Range(0, 1)]
    public float epsilon = 0.8f;//exploit - explore     0-1
    [Range(0, 1)]
    public float gamma = 0.8f;

    public int ChooseAction(double[] state)
    {
        int action = 0;
        float e = Random.Range(0, 1f);

        if (e > epsilon)
        {
            double[] actionValues = trainer.neuralNetwork.Forward(state);
            action = actionValues.ToList().IndexOf(actionValues.Max());
        }
        else
            action = Random.Range(0, 4);

        return action;
    }

    public void Learn(double[] state, int action, double[] state_, double reward)
    {
        double[] predictedValues = trainer.neuralNetwork.Forward(state);

        double QEval = predictedValues[action];
        double QNext = trainer.neuralNetwork.Forward(state_).Max();
        double QTarget = reward + gamma * QNext;

        //      predicted   expected
        // 0    QEval0       QEval2    
        // 1    QEval1       QEval2
        // [2]  QEval2       QTarget
        // 3    QEval3       QEval2   

        double[] expectedValues = new double[predictedValues.Length];
        for (int i = 0; i < predictedValues.Length; i++)
        {
            expectedValues[i] = QEval;
        }
        expectedValues[action] = QTarget;

        trainer.Learn(state, action, predictedValues, expectedValues);
        // loss = NN.loss(QEval, QTarget)
        // NN.backpropogate()
    }


}
