using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
[Serializable]
public class RLAgent
{
    public Activation.ActivationType hiddenActivation;
    public Activation.ActivationType outputActivation;
    public int hiddenSize = 24;
    public int inputSize = 3;
    public int outputSize = 2;
    public int num_hidden_layers = 2;
    float epsilon = 0.8f;
    float gamma = 0.95f;
    List<NNLayer> layers = new List<NNLayer>();
    List<Transition> memory = new List<Transition>();
    public int memorySize = 1000;
    public RLAgent()
    {
        var hact = Activation.GetActivationFromType(hiddenActivation);
        var oact = Activation.GetActivationFromType(outputActivation);
        //memory
        //input layer
        layers.Add(new NNLayer(inputSize,outputSize, hact));
        
        //hidden layer
        for (int i = 0; i < num_hidden_layers; i++)
            layers.Add(new NNLayer(hiddenSize, hiddenSize, hact));
        layers.Add(new NNLayer(hiddenSize, hiddenSize, oact));

    }

    public int SelectAction(double[] observation)
    {
        int action = Random.Range(0, outputSize);

        return action;
    }

    public double[] Forward(double[] observation)
    {
        double[] vals = observation;
        //index nimaga kere bogan ishlatmagan bo'sa
        for (int i = 0; i < layers.Count; i++)
        {
            vals = layers[i].Forward(vals);
        }
        return vals;
    }
    public void Remember(Transition transition)
    {
        if (memorySize < memory.Count)
            memory.RemoveAt(0);
        memory.Append(transition);
    }

    public void ExperienceReplay()
    {
        int batch_size = 20;
        if(memory.Count < batch_size)
            return;

        int[] indicies = new int[batch_size];
        for (int i = 0; i < batch_size; i++)
            indicies[i] = Random.Range(0, memory.Count);

        for (int i = 0; i < batch_size; i++)
        {
            int index = indicies[i];
            Transition transition = memory[index];
            double[] action_values = Forward(transition.state);
            double[] next_action_values = Forward(transition.state_);
            double[] experimental_values = action_values;

            if (transition.isDone)
                experimental_values[transition.action] = -1;
            else
                experimental_values[transition.action] = 1 + gamma * next_action_values.Max();

            Backward(action_values, experimental_values);

        }
        epsilon = epsilon < 0.01f ? epsilon : epsilon * 0.997f;
        foreach (var layer in layers)
        {
            layer.lr = layer.lr < 0.0001f ? layer.lr : layer.lr * 0.99f;
        }
    }

    public void Backward(double[] calculated, double[] experimental)
    {
        double[] delta = new double[calculated.Length];
        for (int i = 0; i < delta.Length; i++)
        {
            delta[i] = calculated[i] - experimental[i];
        }
        for (int i = 0; i < layers.Count; i++)
        {
            delta = layers[i].Backward(delta);
        }
    }
}
