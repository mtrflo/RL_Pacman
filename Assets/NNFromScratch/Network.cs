using static System.Math;
using System;
using System.Collections.Generic;
using MonoRL;
namespace MonoRL
{

    public class Network
    {
        public List<Layer> layers;
        public double learningRate = 0.003;
        public ICost cost;


        public Network()
        {
            this.cost = new Cost.DistanceError();
            this.layers.Add(new Layer(3, 4, Activation.ActivationType.ReLU));
            this.layers.Add(new Layer(4, 10, Activation.ActivationType.ReLU));
            this.layers.Add(new Layer(10, 2, Activation.ActivationType.Linear));
        }

        public double[] Forward(double[] input)
        {
            for (int i = 0; i < this.layers.Count; i++)
            {
                input = this.layers[i].Forward(input);
            }
            double[] output = input;
            return output;
        }

        public void Backward(double[] input, double[] Y)
        {
            Layer outputLayer = this.layers[this.layers.Count - 1];
            double[] deltas = new double[outputLayer.nodes];
            double[] output = this.Forward(input);

            for (int i = 0; i < outputLayer.nodes; i++)
            {
                deltas[i] = this.cost.CostDerivative(output[i], Y[i]);
            }

            input = outputLayer.Backward(this.learningRate, deltas);
            for (int i = this.layers.Count - 2; i >= 0; i--)
            {
                Layer layer = this.layers[i];
                input = layer.Backward(this.learningRate, input);
            }
        }
    }

}