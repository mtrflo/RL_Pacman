using static System.Math;
using System;
using System.Collections.Generic;
using MonoRL;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine;
using Unity.Collections;

[Serializable]
public class Network
{
    public double LearningRate = 0.003;
    public Activation.ActivationType hiddenAType, outputAType;
    public int[] layersSize;

    public List<Layer> Layers = new List<Layer>();

    public ICost Cost;
    public ComputeShader forwardCS;
    public void Init()
    {
        Cost = new Cost.SquaredError();
        if (Layers.Count != 0)
        {
            for (int i = 0; i < Layers.Count - 1; i++)
                Layers[i].SetNonSerializedData(layersSize[i], layersSize[i + 1], hiddenAType);

            Layers.Last().SetNonSerializedData(layersSize[layersSize.Length - 2], layersSize[layersSize.Length - 1], outputAType);
            return;
        }
        for (int i = 0; i < layersSize.Length - 2; i++)
            Layers.Add(new Layer(layersSize[i], layersSize[i + 1], hiddenAType));

        Layers.Add(new Layer(layersSize[layersSize.Length - 2], layersSize[layersSize.Length - 1], outputAType));
    }

    public double[] Forward(double[] inputs)
    {
        NativeArray<double> calc_inputs = new NativeArray<double>(inputs.Length,Allocator.Persistent);
        calc_inputs.CopyFrom(inputs);
        //inputs.CopyTo(calc_inputs, 0);
        for (int i = 0; i < Layers.Count; i++)
            calc_inputs = Layers[i].Forward(calc_inputs);
        double [] calc_i = calc_inputs.ToArray();
        calc_inputs.Dispose();
        return calc_i;
    }

    public void Backward(double[] inputs, double[] expectedOutputs)
    {
        Layer outputLayer = Layers[Layers.Count - 1];
        NativeArray<double> deltas = new NativeArray<double>(outputLayer.NodeSize,Allocator.TempJob);
        double[] output = Forward(inputs);

        for (int i = 0; i < outputLayer.NodeSize; i++)
            deltas[i] = Cost.CostDerivative(output[i], expectedOutputs[i]);

        for (int i = Layers.Count - 1; i >= 0; i--)
            deltas = Layers[i].Backward(deltas);
        deltas.Dispose();
    }

    public void Learn(double[][] batchInputs, double[][] batchExpectedOutputs)
    {
        int batchSize = batchInputs.Length;

        for (int batchIndex = 0; batchIndex < batchSize; batchIndex++)
        {
            double[] inputs = batchInputs[batchIndex];
            double[] expectedOutputs = batchExpectedOutputs[batchIndex];

            Backward(inputs, expectedOutputs);
        }

        for (int i = Layers.Count - 1; i >= 0; i--)
            Layers[i].ApplyGradients(LearningRate, batchSize);
        //
    }
}
