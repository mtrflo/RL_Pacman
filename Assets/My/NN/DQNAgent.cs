using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

using Random = UnityEngine.Random;

public class DQNAgent : MonoBehaviour
{
    public static DQNAgent me;
    public NetworkTrainer mainTrainer;
    public NetworkTrainer targetTrainer;

    [Range(0, 1)]
    public float epsilon = 0.8f;//exploit - explore     0-1
    [Range(0, 1)]
    public float gamma = 0.8f;
    private List<Transition> buffer;
    public int replaceTargetCount = 100;
    private int step = 0;
    private void Awake()
    {
        if (me != null)
            Destroy(gameObject);
        else
        {

            DontDestroyOnLoad(this);
            me = this;
        }

    }
    public int ChooseAction(double[] state)
    {
        int action = 0;
        float e = Random.Range(0, 1f);

        if (e > epsilon)
        {
            double[] actionValues = mainTrainer.neuralNetwork.Forward(state);
            action = actionValues.ToList().IndexOf(actionValues.Max());
        }
        else
            action = Random.Range(0, mainTrainer.neuralNetwork.layerSizes.Last());

        return action;
    }

    public void Learn(double[] state, int action, double[] state_, double reward, bool isEnd = false)
    {
        reward = -reward;
        double[] predictedValues = mainTrainer.neuralNetwork.Forward(state);
        double QEval = predictedValues[action];
        double QNext = targetTrainer.neuralNetwork.Forward(state_).Max();
        double QTarget = reward + gamma * QNext;
        if (isEnd)
        {
            QTarget = reward;
        }

        double[] expectedValues = new double[predictedValues.Length];
        for (int i = 0; i < predictedValues.Length; i++)
        {
            expectedValues[i] = predictedValues[i] - (QTarget - QEval);
        }
        expectedValues[action] = QTarget;
        mainTrainer.Learn(state, action, predictedValues, expectedValues);

        if (replaceTargetCount == step)
        {
            targetTrainer.Clone(mainTrainer);
            step = 0;
        }
        step++;
    }
}

class Transition
{
    double[] state;
    int action;
    double[] state_;
    double reward;

    public Transition(double[] state, int action, double[] state_, double reward)
    {
        this.state = state;
        this.action = action;
        this.state_ = state_;
        this.reward = reward;
    }
}