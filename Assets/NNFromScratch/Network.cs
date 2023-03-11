using static System.Math;
using System;
using System.Collections.Generic;
using MonoRL;
namespace MonoRL
{

    public class Network
    {
        public List<Layer> Layers = new List<Layer>();
        public double LearningRate = 0.003;
        public ICost Cost;


        public Network(double learningRate = 0.003)
        {
            LearningRate = learningRate;
            Cost = new Cost.SquaredError();
            Layers.Add(new Layer(3, 4, Activation.ActivationType.ReLU));
            Layers.Add(new Layer(4, 10, Activation.ActivationType.ReLU));
            Layers.Add(new Layer(10, 2, Activation.ActivationType.Linear));
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