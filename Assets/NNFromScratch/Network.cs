using static System.Math;
using System;
using System.Collections.Generic;
using MonoRL;
using Unity.VisualScripting;
using System.Linq;

namespace MonoRL
{
    [Serializable]
    public class Network
    {
        public double LearningRate = 0.003;
        public Activation.ActivationType hiddenAType, outputAType;
        public int[] layersSize;

        public List<Layer> Layers = new List<Layer>();

        public ICost Cost;
        public void Init()
        {
            Cost = new Cost.SquaredError();
            if (Layers.Count != 0)
            {
                for (int i = 0; i < Layers.Count - 1; i++)
                    Layers[i].Activation = Activation.GetActivationFromType(hiddenAType);

                Layers.Last().Activation = Activation.GetActivationFromType(outputAType);
                return;
            }
            for (int i = 0; i < layersSize.Length - 2; i++)
                Layers.Add(new Layer(layersSize[i], layersSize[i + 1], hiddenAType));

            Layers.Add(new Layer(layersSize[layersSize.Length - 2], layersSize[layersSize.Length - 1], outputAType));
        }

        public double[] Forward(double[] inputs)
        {
            double[] calc_inputs = new double[inputs.Length];
            inputs.CopyTo(calc_inputs, 0);
            for (int i = 0; i < Layers.Count; i++)
                calc_inputs = Layers[i].Forward(calc_inputs);
            return calc_inputs;
        }

        public void Backward(double[] inputs, double[] expectedOutputs)
        {
            Layer outputLayer = Layers[Layers.Count - 1];
            double[] deltas = new double[outputLayer.NodeSize];
            double[] output = Forward(inputs);

            for (int i = 0; i < outputLayer.NodeSize; i++)
                deltas[i] = Cost.CostDerivative(output[i], expectedOutputs[i]);

            for (int i = Layers.Count - 1; i >= 0; i--)
                deltas = Layers[i].Backward(deltas);
        }

        public void Learn(double[][] batchInputs, double[][] batchExpectedOutputs)
        {
            int batchSize = batchInputs.Length;

            for (int batchIndex = 0; batchIndex < batchInputs.Length; batchIndex++)
            {
                double[] inputs = batchInputs[batchIndex];
                double[] expectedOutputs = batchExpectedOutputs[batchIndex];

                Backward(inputs, expectedOutputs);
            }

            for (int i = Layers.Count - 1; i >= 0; i--)
                Layers[i].ApplyGradients(LearningRate, batchSize);
        }
    }
}