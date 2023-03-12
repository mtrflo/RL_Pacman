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
        private double[] _Delta;
        [SerializeField]
        private double[] _Inputs;//data
        [SerializeField]
        private double[] _Outputs;

        public Layer(int inputSize, int nodeSize, Activation.ActivationType activationType)
        {
            InputSize = inputSize;
            NodeSize = nodeSize;
            Activation = MonoRL.Activation.GetActivationFromType(activationType);
            Weights = new List<Weights>();
            Biases = new double[nodeSize];
            _Delta = new double[nodeSize];
            _Inputs = new double[inputSize];
            _Outputs = new double[nodeSize];

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

            inputs.CopyTo(_Inputs,0);
            _Outputs = calculatedOutputs;

            return activatedValues;
        }

        public double[] Backward(double lr, double[] deltas)
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                _Delta[nodeIndex] = deltas[nodeIndex] * Activation.Derivative(_Outputs[nodeIndex]);
            }

            UpdateWeights(lr);
            UpdateBiases(lr);

            double[] propagatedDelta = new double[InputSize];
            for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
            {
                propagatedDelta[inputIndex] = 0;
                for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                {
                    propagatedDelta[inputIndex] += _Delta[nodeIndex] * Weights[nodeIndex].weigths[inputIndex];
                }
            }

            return propagatedDelta;
        }

        private void UpdateWeights(double lr)
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                {
                    double gradW = _Delta[nodeIndex] * _Inputs[inputIndex];
                    Weights[nodeIndex].weigths[inputIndex] -= lr * gradW;
                }
            }
            //Debug.Log("Weights : " + Weights.ToCommaSeparatedString());
        }

        private void UpdateBiases(double lr)
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                double gradB = _Delta[nodeIndex];
                Biases[nodeIndex] -= lr * gradB;
            }

            //Debug.Log("UpdateBiases : " + Biases.ToCommaSeparatedString());
        }

        private void InitializeWeights()
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                Weights.Add(new Weights());
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                    Weights[nodeIndex].weigths.Add(UnityEngine.Random.Range(-0.5f,0.5f));
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