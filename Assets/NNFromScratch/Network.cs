using static System.Math;
using System;
using System.Collections.Generic;
using MonoRL;
namespace MonoRL
{
    [Serializable]
    public class Network
    {
        public double LearningRate = 0.003;
        public Activation.ActivationType hiddenAType, outputAType;
        public List<Layer> Layers = new List<Layer>();

        public ICost Cost;
        public void Init()
        {
            Cost = new Cost.SquaredError();
            Layers.Add(new Layer(3, 4, hiddenAType));
            Layers.Add(new Layer(4, 10, hiddenAType));
            Layers.Add(new Layer(10, 2, outputAType));
        }

        public double[] Forward(double[] inputs)
        {
            for (int i = 0; i < Layers.Count; i++)
                inputs = Layers[i].Forward(inputs);
            return inputs;
        }

        public void Backward(double[] inputs, double[] expectedOutputs)
        {
            Layer outputLayer = Layers[Layers.Count - 1];
            double[] deltas = new double[outputLayer.NodeSize];
            double[] output = Forward(inputs);

            for (int i = 0; i < outputLayer.NodeSize; i++)
                deltas[i] = Cost.CostDerivative(output[i], expectedOutputs[i]);

            inputs = outputLayer.Backward(LearningRate, deltas);
            for (int i = Layers.Count - 2; i >= 0; i--)
            {
                Layer layer = Layers[i];
                inputs = layer.Backward(LearningRate, inputs);
            }
        }
    }

}