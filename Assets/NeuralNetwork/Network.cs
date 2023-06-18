using static System.Math;
using System;
using System.Collections.Generic;
using MonoRL;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEditor.PackageManager.Requests;

[Serializable]
public class Network
{
    public float LearningRate = 0.003f;
    public Activation.ActivationType hiddenAType, outputAType;
    public int[] layersSize;

    public List<Layer> Layers = new List<Layer>();

    public ICost Cost;
    public ComputeShader forwardCS, applyGradsCS;

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
            Layers.Add(new Layer(layersSize[i], layersSize[i + 1],LearningRate, hiddenAType, forwardCS, applyGradsCS));

        Layers.Add(new Layer(layersSize[layersSize.Length - 2], layersSize[layersSize.Length - 1], LearningRate, outputAType, forwardCS, applyGradsCS));
        //
    }
    public float[] Forward(float[] inputs)
    {
        float[] calc_inputs = new float[inputs.Length];
        inputs.CopyTo(calc_inputs, 0);
        for (int i = 0; i < Layers.Count; i++)
            calc_inputs = Layers[i].Forward(calc_inputs);
        return calc_inputs;
        //
    }

    public IEnumerator ForwardGPU(MonoBehaviour mono,float[] inputs, Action<float[]> complete)
    {
        float[] calc_inputs = new float[inputs.Length];
        inputs.CopyTo(calc_inputs, 0);
        for (int i = 0; i < Layers.Count; i++)
        {
            bool done = false;
            mono.StartCoroutine(Layers[i].ForwardGPU(calc_inputs,(cdata) =>
            { 
                calc_inputs = cdata;
                done = true;
            }));
            yield return new WaitUntil(() => done);
        }
        complete.Invoke(calc_inputs);
        //
    }

    public void Backward(float[] inputs, float[] expectedOutputs)
    {
        Layer outputLayer = Layers[Layers.Count - 1];
        float[] deltas = new float[outputLayer.NodeSize];
        float[] output = Forward(inputs);

        for (int i = 0; i < outputLayer.NodeSize; i++)
            deltas[i] = Cost.CostDerivative(output[i], expectedOutputs[i]);

        for (int i = Layers.Count - 1; i >= 0; i--)
            deltas = Layers[i].Backward(deltas);
        //
    }

    public void Backward(int action, float loss)
    {
        Layer outputLayer = Layers[Layers.Count - 1];
        float[] deltas = new float[outputLayer.NodeSize];

        for (int i = 0; i < outputLayer.NodeSize; i++)
            deltas[i] = 0;
        deltas[action] = loss;

        for (int i = Layers.Count - 1; i >= 0; i--)
            deltas = Layers[i].Backward(deltas);

        for (int i = Layers.Count - 1; i >= 0; i--)
            Layers[i].ApplyGradients(LearningRate, 1);
        //
    }

    public void BackwardGPU(MonoBehaviour mono, int action, float loss)
    {
        Layer outputLayer = Layers[Layers.Count - 1];
        float[] deltas = new float[outputLayer.NodeSize];

        for (int i = 0; i < outputLayer.NodeSize; i++)
            deltas[i] = 0;
        deltas[action] = loss;

        for (int i = Layers.Count - 1; i >= 0; i--)
            deltas = Layers[i].Backward(deltas);

        for (int i = Layers.Count - 1; i >= 0; i--)
            mono.StartCoroutine(Layers[i].ApplyGradientsGPU(LearningRate, 1));
        //
    }

    public void Learn(MonoBehaviour mono, float[][] batchInputs, float[][] batchExpectedOutputs)
    {

        int batchSize = batchInputs.Length;

        for (int batchIndex = 0; batchIndex < batchSize; batchIndex++)
        {
            float[] inputs = batchInputs[batchIndex];
            float[] expectedOutputs = batchExpectedOutputs[batchIndex];

            Backward(inputs, expectedOutputs);
        }

        for (int i = Layers.Count - 1; i >= 0; i--)
        {
            Layers[i].ApplyGradients(LearningRate, 1);
            //mono.StartCoroutine(Layers[i].ApplyGradientsGPU(LearningRate, 1));
        }
        //
    }

}