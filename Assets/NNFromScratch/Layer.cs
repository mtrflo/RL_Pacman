using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace MonoRL
{
    [Serializable]
    public class Layer
    {
        [SerializeField]
        public int InputSize;//size
        [SerializeField]
        public int NodeSize;//size

        public List<Weights> Weights;//data
        public double[] Biases;
        public IActivation Activation;

        [SerializeField]
        private double[] _Inputs;//data
        [SerializeField]
        private double[] _Outputs;

        private double[][] _GradW;
        private double[] _GradB;

        public Layer(int inputSize, int nodeSize, Activation.ActivationType activationType)
        {
            InputSize = inputSize;
            NodeSize = nodeSize;
            Activation = MonoRL.Activation.GetActivationFromType(activationType);
            Weights = new List<Weights>();
            Biases = new double[nodeSize];
            _Inputs = new double[inputSize];
            _Outputs = new double[nodeSize];
            _GradW = new double[nodeSize][inputSize];
            _GradB = new double[nodeSize];

            InitializeWeights();
            InitializeBiases();
        }

        public double[] Forward(double[] inputs)
        {
            double[] calculatedOutputs = new double[NodeSize];
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                calculatedOutputs[nodeIndex] = 0;
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                {
                    calculatedOutputs[nodeIndex] += Weights[nodeIndex].weigths[inputIndex] * inputs[inputIndex];
                }
                calculatedOutputs[nodeIndex] += Biases[nodeIndex];
            }

            double[] activatedValues = new double[NodeSize];
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                activatedValues[nodeIndex] = Activation.Activate(calculatedOutputs[nodeIndex]);
            }

            inputs.CopyTo(_Inputs, 0);
            _Outputs = calculatedOutputs;

            return activatedValues;
        }

        public double[] Backward(double[] deltas)
        {
            double[] delta = new double[NodeSize];

            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                delta[nodeIndex] = deltas[nodeIndex] * Activation.Derivative(_Outputs[nodeIndex]);

            UpdateGradients(delta);

            double[] propagatedDelta = new double[InputSize];
            for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
            {
                propagatedDelta[inputIndex] = 0;
                for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                {
                    propagatedDelta[inputIndex] += delta[nodeIndex] * Weights[nodeIndex].weigths[inputIndex];
                }
            }

            return propagatedDelta;
        }

        public void ApplyGradients(double lr, int batchSize)
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                double gradB = _GradB[nodeIndex] / batchSize;
                Biases[nodeIndex] -= lr * gradB;

                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                {
                    double gradW = _GradW[nodeIndex][inputIndex] / batchSize;
                    Weights[nodeIndex].weigths[inputIndex] -= lr * gradW;
                }
            }
        }

        private void UpdateGradients(double[] delta)
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                _GradB[nodeIndex] += delta[nodeIndex];
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                    _GradW[nodeIndex][inputIndex] += delta[nodeIndex] * _Inputs[inputIndex];
            }
        }

        private void InitializeWeights()
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                Weights.Add(new Weights());
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                    Weights[nodeIndex].weigths.Add(UnityEngine.Random.Range(-0.5f, 0.5f));
            }
        }

        private void InitializeBiases()
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                Biases[nodeIndex] = UnityEngine.Random.Range(-0.5f, 0.5f);
        }
    }

}
[Serializable]
public class Weights
{
    public List<double> weigths;
    public Weights()
    {
        weigths = new List<double>();
    }
}